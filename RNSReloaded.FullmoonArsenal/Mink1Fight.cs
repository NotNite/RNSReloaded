using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal struct FieldNode {
        public int playerId;
        public int endTime;
    }
    internal unsafe class Mink1Fight : CustomFight {
        public Mink1Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_greyeye1_s") {}

        private FieldNode[][] fieldGrid = [
            [new FieldNode(), new FieldNode(), new FieldNode()],
            [new FieldNode(), new FieldNode(), new FieldNode()],
            [new FieldNode(), new FieldNode(), new FieldNode()],
        ];

        private readonly int[] fieldColors = [
            IBattlePatterns.FIELDLIMIT_PURPLE,
            IBattlePatterns.FIELDLIMIT_RED,
            IBattlePatterns.FIELDLIMIT_BLUE,
            IBattlePatterns.FIELDLIMIT_GREEN
        ];

        private void SetField(CInstance* self, CInstance* other, int nodeId, int playerId, int startTime, int duration) {
            int index1 = nodeId / 3;
            int index2 = nodeId % 3;
            playerId = playerId < 4 ? playerId : -1;
            this.fieldGrid[index1][index2].playerId = playerId < 4 ? playerId : -1;
            this.fieldGrid[index1][index2].endTime = startTime + duration;

            this.bp.fieldlimit_rectangle_temporary(self, other,
                position: (440 + index1 * 520, 250 + index2 * 290),
                width: 5,
                height: 5,
                color: playerId >= 0 ? this.fieldColors[playerId] : IBattlePatterns.FIELDLIMIT_WHITE,
                targetMask: playerId >= 0 ? 1 << playerId : 0,
                eraseDelay: playerId >= 0 ? duration + 800 : FIELD_CHANGE_SPEED
            );
        }

        private int FindExpiringField(int battleTime) {
            for (int i = 0; i < this.fieldGrid.Length; i++) {
                for (int j = 0; j < this.fieldGrid[i].Length; j++) {
                    if (this.fieldGrid[i][j].endTime <= battleTime) {
                        return i * 3 + j;
                    }
                }
            }
            return 0;
        }

        private int FindEmptyField() {
            for (int i = 0; i < this.fieldGrid.Length; i++) {
                for (int j = 0; j < this.fieldGrid[i].Length; j++) {
                    if (this.fieldGrid[i][j].playerId < 0) {
                        return i * 3 + j;
                    }
                }
            }
            return 0;
        }

        private void SwapFields(CInstance* self, CInstance* other, int fieldIndex0, int fieldIndex1, int time) {
            var field0 = this.fieldGrid[fieldIndex0 / 3][fieldIndex0 % 3];
            var field1 = this.fieldGrid[fieldIndex1 / 3][fieldIndex1 % 3];
            var player0 = field0.playerId;
            var player1 = field1.playerId;
            this.SetField(self, other, fieldIndex0, player1, time, FIELD_CHANGE_SPEED * 8);
            this.SetField(self, other, fieldIndex1, player0, time, FIELD_CHANGE_SPEED * 8);
        }

        private const int FIELD_CHANGE_SPEED = 7000;
        public override RValue* FightDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            if (this.scrbp.time(self, other, 0)) {
                int[] nodes = Enumerable.Range(0, 9).ToArray();
                int[] durations = Enumerable.Range(0, 8).ToArray();
                this.rng.Shuffle(nodes);
                this.rng.Shuffle(durations);
                int numPlayers = this.utils.GetNumPlayers();
                for (int i = 0; i < durations.Length; i++) {
                    this.SetField(self, other, nodes[i], i / 2, 0, FIELD_CHANGE_SPEED * (durations[i] + 1));
                }
                this.SetField(self, other, nodes[8], -1, 0, FIELD_CHANGE_SPEED);
            }

            if (this.scrbp.time_repeating(self, other, FIELD_CHANGE_SPEED, FIELD_CHANGE_SPEED)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                this.SwapFields(self, other, this.FindExpiringField(thisBattleTime), this.FindEmptyField(), thisBattleTime);
            }
            return returnValue;
        }
    }
}
