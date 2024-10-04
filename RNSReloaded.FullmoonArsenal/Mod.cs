using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace RNSReloaded.FullmoonArsenal;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private IHook<ScriptDelegate>? outskirtsHook;
    private IHook<ScriptDelegate>? outskirtsHookN;
    private IHook<ScriptDelegate>? arsenalHook;
    private IHook<ScriptDelegate>? pinnacleHook;
    private IHook<ScriptDelegate>? chooseHallsHook;
    private IHook<ScriptDelegate>? healHook;
    private IHook<ScriptDelegate>? moveNextHook;

    private Random rng = new Random();
    private List<CustomFight> fights = [];

    private int notchCount = -1;

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive) {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles()) {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive) {
            foreach (DirectoryInfo subDir in dirs) {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    private void CopyItemMod() {
        // Copy over item mod to game folder
        // We assume that this environment variable is actually correct
        DirectoryInfo sourceDir = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%RELOADEDIIMODS%"));
        string path = Path.Combine(sourceDir.FullName, @"RNSReloaded.FullmoonArsenal\ItemMod");
        CopyDirectory(path, @".\Mods\ArsenalHealItem", true);
        // Enable the item mod in save file
        string modSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"RabbitSteel\SaveFileNonSynced\modconfig.ini");
        try {
            string[] enabledMods = File.ReadLines(modSavePath).ToArray();
            bool modInFile = false;
            for(int i = 0; i < enabledMods.Count(); i++) {
                string line = enabledMods[i];
                // If there's already an entry in save for this mod, replace it with the enabled version
                if (line.StartsWith("ArsenalHealItem=")) {
                    enabledMods[i] = "ArsenalHealItem=\"1.000000\"";
                    modInFile = true;
                }
            }
            // If no entry, add it
            if (!modInFile) {
                enabledMods = enabledMods.Concat(["ArsenalHealItem=\"1.000000\""]).ToArray();
            }
            File.WriteAllLines(modSavePath, enabledMods);

        } catch {
            File.WriteAllLines(modSavePath, [
                "[Enable]",
                "ArsenalHealItem=\"1.000000\""
            ]);
        }

    }

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        
        this.logger = loader.GetLogger();
        
        this.CopyItemMod();
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            rnsReloaded.LimitOnlinePlay();
            this.fights = [
                new Rem0Fight  (rnsReloaded, this.logger, hooks),
                new Rem1Fight (rnsReloaded, this.logger, hooks),
                new RanXin0Fight (rnsReloaded, this.logger, hooks),
                new Rem2Fight (rnsReloaded, this.logger, hooks),
                new Mink0Fight (rnsReloaded, this.logger, hooks),
                new RanXin1Fight (rnsReloaded, this.logger, hooks),
                new Mink1Fight (rnsReloaded, this.logger, hooks),
                new TasshaFight(rnsReloaded, this.logger, hooks)
            ];

            var outskirtsScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwaygen_outskirts") - 100000);
            this.outskirtsHook =
                hooks.CreateHook<ScriptDelegate>(this.OutskirtsDetour, outskirtsScript->Functions->Function);
            this.outskirtsHook.Activate();
            this.outskirtsHook.Enable();

            var outskirtsScriptN = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwaygen_outskirts_n") - 100000);
            this.outskirtsHookN =
                hooks.CreateHook<ScriptDelegate>(this.OutskirtsDetour, outskirtsScriptN->Functions->Function);
            this.outskirtsHookN.Activate();
            this.outskirtsHookN.Enable();

            var arsenalScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwaygen_arsenal") - 100000);
            this.arsenalHook =
                hooks.CreateHook<ScriptDelegate>(this.ArsenalDetour, arsenalScript->Functions->Function);
            this.arsenalHook.Activate();
            this.arsenalHook.Enable();

            var pinnacleScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwaygen_pinnacle") - 100000);
            this.pinnacleHook =
                hooks.CreateHook<ScriptDelegate>(this.PinnacleDetour, pinnacleScript->Functions->Function);
            this.pinnacleHook.Activate();
            this.pinnacleHook.Enable();

            var chooseHallsScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_choose_halls") - 100000);
            this.chooseHallsHook =
                hooks.CreateHook<ScriptDelegate>(this.ChooseHallsDetour, chooseHallsScript->Functions->Function);
            this.chooseHallsHook.Activate();
            this.chooseHallsHook.Enable();

            var healScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("ipat_heal_light") - 100000);
            this.healHook =
                hooks.CreateHook<ScriptDelegate>(this.HealDetour, healScript->Functions->Function);
            this.healHook.Activate();
            this.healHook.Enable();

            var moveNextScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_hallwayprogress_move_next") - 100000);
            this.moveNextHook =
                hooks.CreateHook<ScriptDelegate>(this.MoveNextDetour, moveNextScript->Functions->Function);
            this.moveNextHook.Activate();
            this.moveNextHook.Enable();
        }
    }

    private bool IsReady(
        [MaybeNullWhen(false), NotNullWhen(true)] out IRNSReloaded rnsReloaded
    ) {
        if (this.rnsReloadedRef != null) {
            this.rnsReloadedRef.TryGetTarget(out var rnsReloadedRef);
            rnsReloaded = rnsReloadedRef;
            return rnsReloaded != null;
        }
        rnsReloaded = null;
        return false;
    }

    private RValue* HealDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        if (this.IsReady(out var rnsReloaded)) {
            int playerId = (int) rnsReloaded.utils.RValueToLong(rnsReloaded.FindValue(self, "playerId"));
            this.logger.PrintMessage("Player ID: " + playerId, this.logger.ColorGreen);

            var playerTrinket = rnsReloaded.utils.GetGlobalVar("playerTrinket");
            int trinketId = (int) rnsReloaded.utils.RValueToLong(playerTrinket->Get(playerId));
            // They have a floof equipped, so we want to suppress healing
            if (trinketId >= 19 && trinketId <= 22) {
                return returnValue;
            }
        }
        // self.playerId
        returnValue = this.healHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private readonly static int[] ENEMY_LEVELS = [
        0, // Target dummy
        20, // - Testing
        30, // Chicken Tendies
        20, // Fieldlimit YEET
        28, // Boss 1
        0, // Skipped due to hall transition
        15, // Troll
        35, // Mink Electric Windmill
        18, // Mink rainstorm
        1, // Boss 2
        0, // Skipped due to hall transition
        0, // Pinnacle cutscene
        1, // Tassha!
        0, // End
    ];
    private RValue* MoveNextDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        if (this.IsReady(out var rnsReloaded)) {
            returnValue = this.moveNextHook!.OriginalFunction(self, other, returnValue, argc, argv);
            this.notchCount++;
            RValue* enemyLevel = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "enemyLevel");
            enemyLevel->Real = ENEMY_LEVELS[this.notchCount];
            enemyLevel->Type = RValueType.Real;
        }
        return returnValue;
    }

    private RValue* ChooseHallsDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        returnValue = this.chooseHallsHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.IsReady(out var rnsReloaded)) {
            this.notchCount = -1; // Reset enemy level tracking

            var hallkey = rnsReloaded.FindValue(self, "hallkey");
            rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 0), "hw_outskirts");
            rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 1), "hw_arsenal");
            rnsReloaded.CreateString(rnsReloaded.ArrayGetEntry(hallkey, 2), "hw_pinnacle");

            var enemyData = rnsReloaded.utils.GetGlobalVar("enemyData");
            for (var i = 0; i < rnsReloaded.ArrayGetLength(enemyData)!.Value.Real; i++) {
                string enemyName = enemyData->Get(i)->Get(0)->ToString();
                if (enemyName == "en_wolf_blackear") {
                    enemyData->Get(i)->Get(9)->Real = 420;
                } else if (enemyName == "en_wolf_greyeye") {
                    enemyData->Get(i)->Get(9)->Real = 300;
                } else if (enemyName == "en_wolf_snowfur") {
                    enemyData->Get(i)->Get(9)->Real = 420;
                }
            }

            // Code to give players a modded item that guarantees 1 HP heal after each combat
            // It's here to make sure that the variables exist, as they won't during regular setup
            rnsReloaded.utils.GetGlobalVar("testItemNum")->Real = 1;
            rnsReloaded.utils.GetGlobalVar("testItemNum")->Type = RValueType.Real;
            var testItemArr = rnsReloaded.utils.GetGlobalVar("testItem");
            var arrName = testItemArr->Get(0);
            rnsReloaded.CreateString(arrName, "it_fullmoon_heal");
        }
        return returnValue;
    }

    private RValue* OutskirtsDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.outskirtsHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.IsReady(out var rnsReloaded)) {
            rnsReloaded.utils.setHallway(new List<Notch> {
                new Notch(NotchType.IntroRoom, "", 0, 0),
                // Temp for testing because I'm too lazy to steel yourself lol
                new Notch(NotchType.Encounter, "enc_wolf_greyeye1", 0, 0),

                new Notch(NotchType.Encounter, "enc_wolf_blackear0", 0, 0),
                new Notch(NotchType.Encounter, "enc_wolf_blackear1", 0, 0),
                new Notch(NotchType.Boss, "enc_wolf_bluepaw0", 0, Notch.BOSS_FLAG)
            }, self, rnsReloaded);
        }
        return returnValue;
    }

    private RValue* ArsenalDetour(
    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
) {
        returnValue = this.arsenalHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.IsReady(out var rnsReloaded)) {
            rnsReloaded.utils.setHallway(new List<Notch> {
                new Notch(NotchType.Encounter, "enc_wolf_blackear2", 0, 0),
                new Notch(NotchType.Encounter, "enc_wolf_greyeye0", 0, 0),
                new Notch(NotchType.Encounter, "enc_wolf_greyeye1", 0, 0),
                new Notch(NotchType.Boss, "enc_wolf_bluepaw1", 0, Notch.BOSS_FLAG)
            }, self, rnsReloaded);
        }
        return returnValue;
    }

    private RValue* PinnacleDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.pinnacleHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.IsReady(out var rnsReloaded)) {
            rnsReloaded.utils.setHallway(new List<Notch> {
                // Cutscene needed for music to play, sadly. Kind of awkward just adding it though
                new Notch(NotchType.PinnacleCutscene, "", 0, 0),
                new Notch(NotchType.FinalBoss, "enc_wolf_snowfur0", 0, Notch.FINAL_BOSS_FLAG),
                new Notch(NotchType.EndRun, "", 0, 0)
            }, self, rnsReloaded);
        }
        return returnValue;
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
