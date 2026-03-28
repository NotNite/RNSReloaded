using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook.Direct3D11;
using Reloaded.Imgui.Hook.Implementations;
using Reloaded.Imgui.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using RNSReloaded.Interfaces;

namespace RNSReloaded.DamageTracker {
    internal class ImGuiConsumer {
        private const string DEFAULT_ENEMY = "Default";
        private int selectedPlayer = 0;
        private string selectedEnemy = DEFAULT_ENEMY;

        private double? initialPainshare = null;
        private string initialEnemy = "";
        private bool isTreasuresphere = false;

        private IRNSReloaded rnsReloaded;

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
        public ImGuiConsumer(ILogProducer producer, IRNSReloaded rnsReloaded, IReloadedHooks hooks) {
            this.rnsReloaded = rnsReloaded;
            this.Reset();

            SDK.Init(hooks);
            ImguiHook.Create(this.Draw, new ImguiHookOptions() {
                Implementations = [new ImguiHookDx11(), new ImguiHookOpenGL3()],
                EnableViewports = true
            });

            producer.SubscribeAll(
                this.ConsumeDamage,
                this.ConsumeDebuffDamage,
                this.ConsumeNewEnemy,
                this.ConsumeNewFight,
                this.ConsumeHallwayMove,
                this.ConsumeChooseHalls
            );
        }

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

        private void ConsumeChooseHalls(LogChooseHallsElement element) {
            this.Reset();
            this.AddEnemy("Target Dummy", 0);
        }

        private void ConsumeHallwayMove(LogHallwayMoveElement element) {
            this.isTreasuresphere = element.type == NotchType.Chest;
        }

        private void ConsumeNewFight(LogNewFightElement element) {
            this.Reset();
        }

        private void ConsumeNewEnemy(LogNewEnemyElement element) {
            this.AddEnemy(element.enemyKey, element.enemyId);
        }

        private void ConsumeDamage(LogDamageElement element) {
            if (!this.isTreasuresphere) {
                
                // Find nonzero painshare ratio on first enemy and cache it for if it changes to 0 later
                // (Mell changes it to 0 in p2, and we don't want to track summon damage in her p2)
                // (Tassha changes it from 0 to 0.75 with her first set of summons)
                if (!this.initialPainshare.HasValue || this.initialPainshare == 0) {
                    this.initialPainshare = element.painShare;
                }

                var dmgInfo = this.getDamageInfo(element.playerId, element.enemyId, element.hbId);
                dmgInfo.count++;
                dmgInfo.damage += element.damageAmount;
                this.setDamageInfo(element.playerId, element.enemyId, element.hbId, dmgInfo);

                if (this.initialPainshare == 0 || this.enemyIdLookup[element.enemyId] == this.initialEnemy) {
                    dmgInfo = this.damageAmounts[element.playerId][DEFAULT_ENEMY].GetValueOrDefault(element.hbId, new DamageInfo() { count = 0, damage = 0 });
                    dmgInfo.count++;
                    dmgInfo.damage += element.damageAmount;
                    this.damageAmounts[element.playerId][DEFAULT_ENEMY][element.hbId] = dmgInfo;
                }
            }
        }

        private void ConsumeDebuffDamage(LogDebuffDamageElement element) {
            this.ConsumeDamage(new LogDamageElement() {
                playerId = element.playerId,
                enemyId = element.enemyId,
                hbId = -element.debuffId,
                damageAmount = element.damageAmount,
                painShare = element.painShare,
                gameTime = element.gameTime,
            });
        }

        public unsafe void Draw() {
            var open = true;

            var buttonSize = new ImVec2 {
                X = 0,
                Y = 0
            };

            if (ImGui.Begin("Damage", ref open, 0)) {
                for (int i = 0; i < 4; i++) {
                    ImGui.SameLine(0, i == 0 ? 0 : 10);
                    string playerName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "playerName")->Get(0)->Get(i)->ToString();
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
                                hbName = this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "hbsInfo")->Get((int) -key)->Get(0)->ToString();
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
}
