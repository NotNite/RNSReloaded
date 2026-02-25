using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using Reloaded.Imgui.Hook;
using Reloaded.Imgui.Hook.Direct3D11;
using DearImguiSharp;
using Reloaded.Imgui.Hook.Implementations;
namespace RNSReloaded.DamageTracker;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private IHook<ScriptDelegate>? damageHook;
    private IHook<ScriptDelegate>? newFightHook;
    private IHook<ScriptDelegate>? addEnemyHook;
    private IHook<ScriptDelegate>? hallwayMoveHook;
    private IHook<ScriptDelegate>? chooseHallsHook;

    private const string DEFAULT_ENEMY = "Default";
    private struct DamageInfo {
        public long damage;
        public long count;
    }

    // playerId -> normalizedEnemyId -> hbId -> damage info
    private Dictionary<string, Dictionary<long, DamageInfo>>[] damageAmounts = [
        new Dictionary<string, Dictionary<long, DamageInfo>>(),
        new Dictionary<string, Dictionary<long, DamageInfo>>(),
        new Dictionary<string, Dictionary<long, DamageInfo>>(),
        new Dictionary<string, Dictionary<long, DamageInfo>>()
    ];

    private Dictionary<long, string> enemyIdLookup = new Dictionary<long, string>();
    private Dictionary<string, long> enemyNameUses = new Dictionary<string, long>();

    private Dictionary<long, string> hbIdNameOverrides = new Dictionary<long, string>() {
        { 1, "Primary" },
        { 2, "Secondary" },
        { 3, "Special" },
        { 4, "Defensive" }
    };

    private DamageInfo getDamageInfo(long playerId, long enemyId, long hbId) {
        // PlayerID will always exist. Probably.
        var playerDict = this.damageAmounts[playerId];
        if (!playerDict.ContainsKey(this.enemyIdLookup.GetValueOrDefault(enemyId, "UNKNOWN_ENEMY"))) {
            return new DamageInfo() { count = 0, damage = 0 };
        }
        var enemyDict = playerDict[this.enemyIdLookup.GetValueOrDefault(enemyId, "UNKNOWN_ENEMY")];
        if (!enemyDict.ContainsKey(hbId)) {
            return new DamageInfo() { count = 0, damage = 0 };
        }
        return enemyDict[hbId];
    }

    private Dictionary<long, DamageInfo> getHbDamageDict(long playerId, string enemyId) {
        // PlayerID will always exist. Probably.
        var playerDict = this.damageAmounts[playerId];
        if (!playerDict.ContainsKey(enemyId)) {
            return new Dictionary<long, DamageInfo>();
        }
        return playerDict[enemyId];
    }

    private void setDamageInfo(long playerId, long enemyId, long hbId, DamageInfo toSet) {
        // PlayerID will always exist. Probably.
        var playerDict = this.damageAmounts[playerId];
        if (!playerDict.ContainsKey(this.enemyIdLookup.GetValueOrDefault(enemyId, "UNKNOWN_ENEMY"))) {
            playerDict[this.enemyIdLookup.GetValueOrDefault(enemyId, "UNKNOWN_ENEMY")] = new Dictionary<long, DamageInfo>();
        }
        var enemyDict = playerDict[this.enemyIdLookup.GetValueOrDefault(enemyId, "UNKNOWN_ENEMY")];
        enemyDict[hbId] = toSet;
    }

    public void StartEx(IModLoaderV1 loader,IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        
        this.logger = loader.GetLogger();

        this.Reset();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

        if (this.hooksRef != null && this.hooksRef.TryGetTarget(out var hooks)) {
            SDK.Init(hooks);
            ImguiHook.Create(this.Draw, new ImguiHookOptions() {
                Implementations = [new ImguiHookDx11(), new ImguiHookOpenGL3()],
                EnableViewports = true
            });
        }

        this.logger.PrintMessage("Set up discount ACT", this.logger.ColorGreen);
    }

    private bool ready = false;
    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            this.ready = true;

            var damageScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_pattern_deal_damage_enemy_subtract") - 100000);
            this.damageHook =
                hooks.CreateHook<ScriptDelegate>(this.EnemyDamageDetour, damageScript->Functions->Function);
            this.damageHook.Activate();
            this.damageHook.Enable();

            var newFightScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.newFightHook =
                hooks.CreateHook<ScriptDelegate>(this.NewFightDetour, newFightScript->Functions->Function);
            this.newFightHook.Activate();
            this.newFightHook.Enable();

            var addEnemyScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_enemy") - 100000);
            this.addEnemyHook =
                hooks.CreateHook<ScriptDelegate>(this.AddEnemyDetour, addEnemyScript->Functions->Function);
            this.addEnemyHook.Activate();
            this.addEnemyHook.Enable();

            var hallwayMoveScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_move_next") - 100000);
            this.hallwayMoveHook =
                hooks.CreateHook<ScriptDelegate>(this.HallwayMoveDetour, hallwayMoveScript->Functions->Function);
            this.hallwayMoveHook.Activate();
            this.hallwayMoveHook.Enable();

            var chooseHallsScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_choose_halls") - 100000);
            this.chooseHallsHook =
                hooks.CreateHook<ScriptDelegate>(this.ChooseHallsDetour, chooseHallsScript->Functions->Function);
            this.chooseHallsHook.Activate();
            this.chooseHallsHook.Enable();

            // Player applying debuffs or buffs: (note: called even for buffs that already exist)
            // args[0] = player/enemyID
            // args[1] = always 1?
            // args[2] = always undefined?
            // returnValue = did it actually apply maybe?
            // scrbp_add_hbs(0, 1, undefined) -> 1 


            // Phase change, we want to split damage by boss phase (probably) because separate enrage timers
            // scrbp_phase_pattern_remove
        }
    }

    private double? initialPainshare = null;
    private string initialEnemy = "";
    private bool isTreasuresphere = false;

    private RValue* EnemyDamageDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (!this.isTreasuresphere && this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var hbId = rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "hbId"));
            var damage = rnsReloaded.utils.RValueToLong(argv[2]);
            var playerId = rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "playerId"));
            var enemyId = rnsReloaded.utils.RValueToLong(argv[1]);

            // Find nonzero painshare ratio on first enemy and cache it for if it changes to 0 later
            // (Mell changes it to 0 in p2, and we don't want to track summon damage in her p2)
            // (Tassha changes it from 0 to 0.75 with her first set of summons)
            if (!this.initialPainshare.HasValue || this.initialPainshare == 0) {
                var painShare = rnsReloaded.utils.RValueToDouble(rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "playerPainshareRatio")->Get(1)->Get(0));
                this.initialPainshare = painShare;
            }

            var dmgInfo = this.getDamageInfo(playerId, enemyId, hbId);
            dmgInfo.count++;
            dmgInfo.damage += damage;
            this.setDamageInfo(playerId, enemyId, hbId, dmgInfo);

            // hbId of -1 means it's a debuff, so we change the ID to be negative of the debuff ID
            // to avoid collisions with real hbIds
            if (hbId == -1) {
                hbId = -rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "statusId"));
            }

            if (this.initialPainshare == 0 || this.enemyIdLookup[enemyId] == this.initialEnemy) {
                dmgInfo = this.damageAmounts[playerId][DEFAULT_ENEMY].GetValueOrDefault(hbId, new DamageInfo() { count = 0, damage = 0});
                dmgInfo.count++;
                dmgInfo.damage += damage;
                this.damageAmounts[playerId][DEFAULT_ENEMY][hbId] = dmgInfo;
            }
        }

        returnValue = this.damageHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private void Reset() {
        this.initialPainshare = null;
        this.initialEnemy = "";
        // If they selected an enemy, then their selection will be invalid next fight
        // So we set it back to default for a better UX
        this.selectedEnemy = DEFAULT_ENEMY;

        foreach (var dict in this.damageAmounts) {
            dict.Clear();
            // Always add a default option, which stores either damage to the main enemy (if painshare) or total damage (if not)
            // Easier to update it here/when tracking damage than to do the math every display frame.
            dict["Default"] = new Dictionary<long, DamageInfo>();
        }

        this.enemyIdLookup.Clear();
        this.enemyNameUses.Clear();
    }

    private RValue* NewFightDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.Reset();
        returnValue = this.newFightHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private void AddEnemy(string enemyName, long enemyListId) {
        // This causes us to 1-index names, since it's displayed to the player.
        // it's not really an "index" as we don't array off it, so it shouldn't cause off by 1 errors
        var nameId = this.enemyNameUses.GetValueOrDefault(enemyName, 0) + 1;
        this.enemyNameUses[enemyName] = nameId;
        this.enemyIdLookup[enemyListId] = $"{enemyName} ({nameId})";
        if (this.initialEnemy == "") {
            this.initialEnemy = this.enemyIdLookup[enemyListId];
        }
        // Initialize damage dict for enemy, because dict iteration order is based on insertion order
        // and it's really awkward to see enemy (4) before enemy (3) because we hit 4 first.
        foreach (var item in this.damageAmounts) {
            item[this.enemyIdLookup[enemyListId]] = new Dictionary<long, DamageInfo>();
        }
    }

    private RValue* HallwayMoveDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var currentPos = rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "currentPos")) + 1;
            NotchType thisNotchType = (NotchType) rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "notches")->Get((int) currentPos)->Get(0));
            this.isTreasuresphere = thisNotchType == NotchType.Chest;
        }

        returnValue = this.hallwayMoveHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* ChooseHallsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.Reset();
        this.AddEnemy("Target Dummy", 0);

        returnValue = this.chooseHallsHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* AddEnemyDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var enemyRealId = rnsReloaded.utils.RValueToLong(argv[0]);
            // index 0 is key. We use this instead of index 2 (name w/o title) because it's always in english and CN/JP chars don't display in ImGui
            var enemyName = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "enemyData")->Get((int) enemyRealId)->Get(0)->ToString();
            var enemyListId = rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "playerId"));
            this.AddEnemy(enemyName, enemyListId);
        }
        returnValue = this.addEnemyHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private int selectedPlayer = 0;
    private string selectedEnemy = DEFAULT_ENEMY;
    public void Draw() {
        if (!this.ready) return;
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            var open = true;

            var buttonSize = new ImVec2 {
                X = 0,
                Y = 0
            };
            
            if (ImGui.Begin("Damage", ref open, 0)) {
                for (int i = 0; i < 4; i++) {
                    ImGui.SameLine(0, i == 0 ? 0 : 10);
                    string playerName = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "playerName")->Get(0)->Get(i)->ToString();
                    if (this.selectedPlayer == i) {
                        ImGui.Text(playerName);
                    } else {
                        if (ImGui.Button(playerName, buttonSize)) {
                            this.selectedPlayer = i;
                        }
                    }
                }

                if (ImGui.TreeNodeStr("Enemy Select")) {
                    if (this.initialPainshare.HasValue && this.initialPainshare != 0) {
                        ImGui.Text($"First seen Painshare: {this.initialPainshare * 100}%%");
                    }

                    int wrapIndex = 0;
                    foreach (var item in this.damageAmounts[this.selectedPlayer].Keys) {

                        if (wrapIndex % 3 != 0) {
                            ImGui.SameLine(0, 10);
                        }
                        wrapIndex++;

                        if (this.selectedEnemy == item) {
                            ImGui.Text(item);
                        } else if (ImGui.Button(item, buttonSize)) {
                            this.selectedEnemy = item;
                        }
                    }
                    ImGui.TreePop();
                }

                if (ImGui.BeginTable("", 4, 384, buttonSize, 0)) {
                    var orderedKeys = this.getHbDamageDict(this.selectedPlayer, this.selectedEnemy).Keys.ToList();
                    orderedKeys.Sort((long a, long b) => {
                        // Debuffs have negative IDs, so we take their absolute value to sort both after regular HBs and in order
                        if (a < 0) { a = -a; }
                        if (b < 0) { b = -b; }
                        return (int) (a - b);
                    });
                    ImGui.TableNextRow(0, 0);
                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Source");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Damage");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Hits");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("% Total");
                    long totalDamage = this.getHbDamageDict(this.selectedPlayer, this.selectedEnemy).Values.Sum(x => x.damage);
                    foreach (var key in orderedKeys) {
                        // Adjust hBId for multiplayer, since each player gets 14 HBs
                        var adjustedKey = key;
                        if (adjustedKey > 0) {
                            adjustedKey -= this.selectedPlayer * 14;
                        }
                        string hbName = this.hbIdNameOverrides.GetValueOrDefault(adjustedKey, "");                        

                        if (hbName == "") {
                            if (adjustedKey < 0) {
                                // Using -key instead of -adjustedKey is correct here, as debuffs will always be same ID regardless of player
                                hbName = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "hbsInfo")->Get((int) -key)->Get(0)->ToString();
                            } else if (adjustedKey > 4 && adjustedKey < 11) {
                                hbName = "Item #" + (adjustedKey - 4);
                            } else if (adjustedKey >= 11) {
                                hbName = "Potion #" + (adjustedKey - 10);
                            } else {
                                hbName = "HB id " + key;
                            }
                        }
                        var info = this.getHbDamageDict(this.selectedPlayer, this.selectedEnemy)[key];
                        ImGui.TableNextRow(0, 0);
                        ImGui.TableNextColumn();
                        ImGui.Text($"{hbName}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{info.damage}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{info.count}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{Math.Round(100f * info.damage / totalDamage, 1)}%");
                    }
                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }
    }

    public void Suspend() {
    }

    public void Resume() {
    }

    public bool CanSuspend() => false; // Add suspend/resume code and set to true once ready

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
