using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class Rem0Fight : CustomFight {
        public Rem0Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_blackear0") {}


        private int Setup(CInstance* self, CInstance* other) {
            // Yeet people to initial positions
            if (this.scrbp.time(self, other, 0)) {
                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                int player1 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                int player2 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;
                int player3 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 2)->Real;
                int player4 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 3)->Real;
                this.order = [player1, player2, player3, player4];

                this.scrbp.move_character_absolute(self, other, (30 + 1890) / 2, (30 + 1050) / 2, 2000);
                // Top half
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (960, 270),
                    width: 5,
                    height: 5,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: player1,
                    eraseDelay: 2000
                );
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_0", hbsDuration: 999999, targetMask: player1);
                // Bottom half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (960, 780), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_BLUE, targetMask: player2, eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_2", hbsDuration: 999999, targetMask: player2);

                // Left half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (480, 540), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_YELLOW, targetMask: player3, eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_3", hbsDuration: 999999, targetMask: player3);

                // Right half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (1410, 540), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_PURPLE, targetMask: player4, eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_1", hbsDuration: 999999, targetMask: player4);

            }

            // Setup the perma fieldlimits
            if (this.scrbp.time(self, other, 1000)) {
                // Top half
                this.bp.fieldlimit_rectangle(self, other,
                    position: (960, 270),
                    width: 1840,
                    height: 470,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: this.order[0]
                );
                // Bottom half
                this.bp.fieldlimit_rectangle(self, other,
                    position: (960, 780),
                    width: 1840,
                    height: 470,
                    color: IBattlePatterns.FIELDLIMIT_BLUE,
                    targetMask: this.order[1]
                );
                // Left half
                this.bp.fieldlimit_rectangle(self, other,
                    position: (480, 540),
                    width: 900,
                    height: 1020,
                    color: IBattlePatterns.FIELDLIMIT_YELLOW,
                    targetMask: this.order[2]
                );
                // Right half
                this.bp.fieldlimit_rectangle(self, other,
                    position: (1410, 540),
                    width: 900,
                    height: 1020,
                    color: IBattlePatterns.FIELDLIMIT_PURPLE,
                    targetMask: this.order[3]
                );
            }
            return 3000;
        }
        private void AoeSpamRepeat(CInstance* self, CInstance* other, int delayStart, int cycleTime) {
            if (this.scrbp.time_repeating(self, other, delayStart, 400)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;

                int playerToTarget = (thisBattleTime / 400) % this.utils.GetNumPlayers();
                int moonSize = (thisBattleTime / 1600) % 2;

                var player_x = this.utils.GetPlayerVar(playerToTarget, "distMovePrevX")->Real;
                var player_y = this.utils.GetPlayerVar(playerToTarget, "distMovePrevY")->Real;

                int timeToDespawn = cycleTime - (thisBattleTime % cycleTime);
                int warnTime = 1300 + 400 * moonSize;
                if (timeToDespawn >= warnTime + 400) {
                    this.bp.fire_aoe(self, other, 0, warnTime, cycleTime - (thisBattleTime % cycleTime), .7 + .5 * moonSize, [(player_x, player_y)]);
                }
            }
        }

        private void ColorMatchRepeat(CInstance* self, CInstance* other, int offset, int length) {
            // Appear at very start of each 10s cycle, resolve at end
            if (this.scrbp.time_repeating(self, other, offset, length)) {
                // Shuffle what players get matched with who, in a 2 + 2 pattern randomly
                this.scrbp.order_random(self, other, false, 2, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                var group0 = this.rnsReloaded.ArrayGetEntry(orderBin, 0);
                var group1 = this.rnsReloaded.ArrayGetEntry(orderBin, 1);

                this.bp.colormatch(self, other,
                    warningDelay: 1000,
                    warnMsg: 2,
                    spawnDelay: 9500,
                    radius: 200,
                    targetMask: (int) group0->Real,
                    color: IBattlePatterns.COLORMATCH_BLUE
                );
                this.bp.colormatch(self, other,
                    warningDelay: 1000,
                    warnMsg: 2,
                    spawnDelay: 9500,
                    radius: 200,
                    targetMask: (int) group1->Real,
                    color: IBattlePatterns.COLORMATCH_RED
                );
            }
        }

        private int[] order = [1, 2, 4, 8];
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = this.Setup(self, other);

            this.AoeSpamRepeat(self, other, time, 10000);
            this.ColorMatchRepeat(self, other, 0, 10000);

            if (this.scrbp.time(self, other, 40000)) {
                this.bp.enrage(self, other, 0, 6000, 6000, false);
            }
            
            return returnValue;
        }
    }
}
