using DearImguiSharp;
using RNSReloaded.Interfaces;
using System.Text.Json.Serialization;

namespace RNSReloaded.DamageTracker {
    internal unsafe class ImGuiBuffConsumer {
        IRNSReloaded rnsReloaded;
        int selectedTarget = 0;
        int selectedSource = -1;
        int enemyLogicalIdIndex = 4;
        bool isTreasuresphere = false;
        long fightStartTime = 0;
        long fightEndTime = 0;

        struct BuffData {
            public long totalTime;
            public int applications;
        }

        // Maps from targetId -> sourceId -> buffKey -> info
        Dictionary<int, Dictionary<int, Dictionary<string, BuffData>>> buffInfo = new Dictionary<int, Dictionary<int, Dictionary<string, BuffData>>>();
        // Maps from unique buff ID -> creation event
        Dictionary<int, LogElementAddBuff> currentBuffs = new Dictionary<int, LogElementAddBuff>();
        // Maps from targetId (or sourceId) -> string name
        Dictionary<int, string> names = new Dictionary<int, string>();
        // Maps from real ID -> logical ID
        Dictionary<int, int> enemyIdMap = new Dictionary<int, int>();

        public ImGuiBuffConsumer(ILogProducer producer, IRNSReloaded rnsReloaded) {
            this.rnsReloaded = rnsReloaded;
            this.Reset(true);

            producer.Subscribe(this.ConsumeAddBuff);
            producer.Subscribe(this.ConsumeRemoveBuff);
            producer.Subscribe(this.ConsumeChooseHalls);
            producer.Subscribe(this.ConsumeHallwayMove);
            producer.Subscribe(this.ConsumeNewEnemy);
            producer.Subscribe(this.ConsumeNewFight);
            producer.Subscribe(this.ConsumeEndFight);
        }

        private void AddEnemy(string name, int id) {
            this.enemyIdMap[id] = this.enemyLogicalIdIndex;
            this.buffInfo[this.enemyLogicalIdIndex] = new Dictionary<int, Dictionary<string, BuffData>>();
            for (int i = -1; i < 4; i++) {
                this.buffInfo[this.enemyLogicalIdIndex][i] = new Dictionary<string, BuffData>();
            }
            this.names[this.enemyLogicalIdIndex] = name;
            this.enemyLogicalIdIndex++;
        }

        private void ConsumeChooseHalls(LogElementChooseHalls elem) {
            this.Reset();
            this.AddEnemy("Target Dummy", 0);
            this.fightStartTime = elem.gameTime;
            this.fightEndTime = 0;
        }

        private void ConsumeHallwayMove(LogElementHallwayMove elem) {
            this.isTreasuresphere = elem.type == NotchType.Chest;
        }

        private void ConsumeNewFight(LogElementNewFight elem) {
            this.Reset();
            // There's just under 2s of load time before you can start hitting
            this.fightStartTime = elem.gameTime + 1833;
            this.fightEndTime = 0;
        }

        private void ConsumeNewEnemy(LogElementNewEnemy elem) {
            this.AddEnemy(elem.enemyKey, elem.enemyId);
        }

        private void ConsumeAddBuff(LogElementAddBuff elem) {
            if (this.isTreasuresphere) {
                return;
            }

            // Handle refreshing buffs, maybe.
            // I still don't know all the refresh types but this should work for base game ones?
            if (this.currentBuffs.ContainsKey(elem.uniqueId)) {
                this.ConsumeRemoveBuff(new LogElementRemoveBuff() {
                    uniqueId = elem.uniqueId,
                    gameTime = elem.gameTime
                });
            }

            this.currentBuffs[elem.uniqueId] = elem;

            var targetId = elem.targetsEnemy ? this.enemyIdMap[elem.targetId] : elem.targetId;
            var buffData = this.buffInfo[targetId][elem.sourceId].GetValueOrDefault(elem.buffName, new BuffData());
            buffData.applications++;

            this.buffInfo[targetId][elem.sourceId][elem.buffName] = buffData;

            var globalData = this.buffInfo[targetId][-1].GetValueOrDefault(elem.buffName, new BuffData());
            globalData.applications++;
            this.buffInfo[targetId][-1][elem.buffName] = globalData;
        }

        private void ConsumeRemoveBuff(LogElementRemoveBuff elem) {
            if (this.isTreasuresphere) {
                return;
            }

            // For some reason, player buffs are removed AFTER changing hallways and starting the new fight
            // And the buff dict will be cleared by then, so we just ignore it when it happens
            if (!this.currentBuffs.ContainsKey(elem.uniqueId)) {
                return;
            }

            var buffInfo = this.currentBuffs[elem.uniqueId];
            this.currentBuffs.Remove(elem.uniqueId);

            var targetId = buffInfo.targetsEnemy ? this.enemyIdMap[buffInfo.targetId] : buffInfo.targetId;

            var buffData = this.buffInfo[targetId][buffInfo.sourceId].GetValueOrDefault(buffInfo.buffName, new BuffData());
            buffData.totalTime += elem.gameTime - buffInfo.gameTime;

            this.buffInfo[targetId][buffInfo.sourceId][buffInfo.buffName] = buffData;

            var globalData = this.buffInfo[targetId][-1].GetValueOrDefault(buffInfo.buffName, new BuffData());
            globalData.totalTime += elem.gameTime - buffInfo.gameTime;
            this.buffInfo[targetId][-1][buffInfo.buffName] = globalData;
        }

        private void ConsumeEndFight(LogElementEndFight elem) {
            this.fightEndTime = elem.gameTime;
        }

        private void Reset(bool tempNames = false) {
            this.buffInfo.Clear();
            this.currentBuffs.Clear();
            this.names.Clear();
            this.enemyIdMap.Clear();
            this.enemyLogicalIdIndex = 4;

            // If they selected an enemy past the first, reset it to the first
            if (this.selectedTarget > 4) {
                this.selectedTarget = 4;
            }

            for (int i = 0; i < 4; i++) {
                string playerName = tempNames ? "Player " + i : this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "playerName")->Get(0)->Get(i)->ToString();
                this.names[i] = playerName;
                this.buffInfo[i] = new Dictionary<int, Dictionary<string, BuffData>>();
                for (int j = -1; j < 4; j++) {
                    this.buffInfo[i][j] = new Dictionary<string, BuffData>();
                }
            }
        }

        public void Draw() {
            var open = true;

            var buttonSize = new ImVec2 {
                X = 0,
                Y = 0
            };

            long fightDuration = this.fightEndTime - this.fightStartTime;
            if (this.fightEndTime == 0) {
                var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));
                fightDuration = gameTime - this.fightStartTime;
            }

            if (ImGui.Begin($"Buffs", ref open, 0)) {
                var seconds = Math.Round(fightDuration / 1000f, 1);
                if (seconds < 60) {
                    ImGui.Text($"Fight Time: {seconds.ToString("0.0")}s");
                } else {
                    ImGui.Text($"Fight Time: {Math.Floor(seconds / 60)}:{(seconds % 60).ToString("00.0")}");
                }
                // Buttons to select each player + enemy (buff target)
                int wrapIndex = 0;
                foreach (var targetId in this.buffInfo.Keys) {
                    if (wrapIndex % 4 != 0) {
                        ImGui.SameLine(0, 10);
                    }
                    string name = this.names[targetId];
                    if (this.selectedTarget == targetId) {
                        ImGui.Text(name);
                    } else {
                        if (ImGui.Button(name, buttonSize)) {
                            this.selectedTarget = targetId;
                        }
                    }
                    wrapIndex++;
                }

                if (ImGui.TreeNodeStr("Source Select")) {
                    // Buttons to select buff source
                    for (int i = -1; i < 4; i++) {
                        if (i >= 0) {
                            ImGui.SameLine(0, 10);
                        }

                        string sourceName = i == -1 ? "All Sources" : this.names[i];
                        if (this.selectedSource == i) {
                            ImGui.Text(sourceName);
                        } else {
                            if (ImGui.Button(sourceName, buttonSize)) {
                                this.selectedSource = i;
                            }
                        }
                    }
                    ImGui.TreePop();
                }

                if (ImGui.BeginTable("", 4, 384, buttonSize, 0)) {
                    ImGui.TableNextRow(0, 0);
                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Buff Name");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Duration");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Applications");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("% Total");

                    if (this.buffInfo.ContainsKey(this.selectedTarget)) {
                        var buffs = this.buffInfo[this.selectedTarget][this.selectedSource];
                        foreach (var key in buffs.Keys) {
                            var buff = buffs[key];
                            var inProgressBuff = this.currentBuffs.ToList().Find(elem => elem.Value.buffName == key);
                            var hasInProgress = !inProgressBuff.Equals(new KeyValuePair<int, LogElementAddBuff>());

                            if (this.fightEndTime == 0 && hasInProgress) {
                                var gameTime = this.rnsReloaded.utils.RValueToLong(this.rnsReloaded.FindValue(this.rnsReloaded.GetGlobalInstance(), "gametime"));
                                buff.totalTime += gameTime - inProgressBuff.Value.gameTime;
                            }

                            ImGui.TableNextRow(0, 0);
                            ImGui.TableNextColumn();
                            ImGui.Text($"{key}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{Math.Round(buff.totalTime / 1000f, 1)}s");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{buff.applications}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{Math.Round(100f * buff.totalTime / fightDuration, 1)}%%");
                        }
                    }
                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }
    }
}
