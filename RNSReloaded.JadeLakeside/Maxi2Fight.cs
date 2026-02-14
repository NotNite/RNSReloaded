using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Maxi2Fight : CustomFight {

        public Maxi2Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_tinkerer2") { }

        private int[] playerGroups = [1, 2, 4, 8];
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int bfMidX = 1920 / 2;
            int bfMidY = 1080 / 2;

            if (this.scrbp.time(self, other, 0)) {
                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                this.playerGroups[0] = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                this.playerGroups[1] = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;
                this.playerGroups[2] = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 2)->Real;
                this.playerGroups[3] = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 3)->Real;
            }

            const int TOWERS_TIME = 5000;
            const int STACK_TIME = 2500;
            if (this.scrbp.time_repeating(self, other, 1000, TOWERS_TIME + STACK_TIME)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int repeatCount = (thisBattleTime - 1000) / TOWERS_TIME;

                double rotAmount = repeatCount * -Math.PI / 4;

                this.bp.angel_circle(self, other, position: (bfMidX + Math.Sin(rotAmount) * 365, bfMidY + Math.Cos(rotAmount) * 365), spawnDelay: TOWERS_TIME, number: 2, radius: 170, warnMsg: 127);
                this.bp.angel_circle(self, other, position: (bfMidX + Math.Sin(rotAmount + Math.PI) * 365, bfMidY + Math.Cos(rotAmount + Math.PI) * 365), spawnDelay: TOWERS_TIME, number: 2, radius: 170, warnMsg: 127);

                int group1 = this.playerGroups[0] + this.playerGroups[1];
                int group2 = this.playerGroups[2] + this.playerGroups[3];

                this.bp.thorns(self, other, spawnDelay: TOWERS_TIME, targetMask: group1, radius: 680);
                this.bp.thorns(self, other, spawnDelay: TOWERS_TIME, targetMask: group2, radius: 680);

                this.bp.prscircle_follow_bin(self, other, warningDelay: TOWERS_TIME - 1500, spawnDelay: TOWERS_TIME + STACK_TIME, radius: 150, targetMask: 1 << this.rng.Next(4));

                // Shuffle groups to force a tether swap more often than truly random would
                if (this.rng.Next(2) == 1) {
                    // Swap 1 and 2 (AB CD -> AC BD)
                    int temp = this.playerGroups[1];
                    this.playerGroups[1] = this.playerGroups[2];
                    this.playerGroups[2] = temp;
                } else {
                    // Swap 1 and 3 (AB CD -> AD BC)
                    int temp = this.playerGroups[1];
                    this.playerGroups[1] = this.playerGroups[3];
                    this.playerGroups[3] = temp;
                }
            }
            if (this.scrbp.time_repeating(self, other, 1000, 3000)) {
                this.bp.prscircle(self, other, position: (bfMidX, bfMidY), spawnDelay: 3000, radius: 550);
            }

            if (this.scrbp.time_repeating(self, other, 100, 900)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                double rotAmount = (thisBattleTime) / 1000;

                int rotDegrees = (int) (rotAmount * 180 / Math.PI);


                this.bp.light_line(self, other, spawnDelay: 1500, showWarning: true, position: (bfMidX + Math.Sin(rotAmount) * 550, bfMidY + Math.Cos(rotAmount) * 550), lineAngle: -rotDegrees, lineLength: 50, angle: -rotDegrees - 90, spd: 3, numBullets: 3, type: 1);
            }

            if (this.scrbp.time_repeating(self, other, 1000, 800)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                double rotAmount = (-thisBattleTime + 900) / 900;

                int rotDegrees = (int) (rotAmount * 180 / Math.PI);

                this.bp.light_line(self, other, spawnDelay: 1500, showWarning: true, position: (bfMidX + Math.Sin(rotAmount) * 550, bfMidY + Math.Cos(rotAmount) * 550), lineAngle: -rotDegrees, lineLength: 50, angle: -rotDegrees - 90, spd: 4, numBullets: 2, type: 1);
            }


            if (this.scrbp.time(self, other, 49000)) {
                this.bp.enrage(self, other, spawnDelay: 3500, timeBetween: 3500);
            }
            
            return returnValue;
        }
    }
}
