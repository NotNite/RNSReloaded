using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Maxi0Fight : CustomFight {
        public Maxi0Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_tinkerer0") {}

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time(self, other, 0)) {
                if (this.utils.GetNumPlayers() != 4) {
                    this.bp.enrage(self, other, spawnDelay: 3000, timeBetween: 500);
                }
                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2 + 200, 1080 / 2));
            }

            if (this.scrbp.time_repeating(self, other, 1000, 9500)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int warningDelay = thisBattleTime < 2000 ? 0 : 1500;

                this.scrbp.order_random(self, other, false, 2, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                int group1 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                int group2 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;

                this.fzbp.ColormatchSwap(self, other,
                    numColors: 3,
                    warningDelay: warningDelay,
                    spawnDelay: 10800,
                    setRadius: 150,
                    matchRadius: 300,
                    warnMsg: 1,
                    setCircles: [(-500, -500), (1750, 1080-200), (1750, 200)],
                    matchCircles: [(200 + 1920/2, 1080/2), (350, 1080 / 4), (350, 1080 * 3 /4)],
                    targetMask: [0b1111],
                    colors: [IBattlePatterns.COLORMATCH_RED, IBattlePatterns.COLORMATCH_GREEN, IBattlePatterns.COLORMATCH_BLUE]
                );
                bool ballDown = this.rng.Next(0, 2) == 1;
                this.bp.water_moving_ball(self, other, warningDelay: warningDelay, spawnDelay: 1500, scale: 350 / 180.0, speed: 3, position: (200 + 1920/2, ballDown ? -100 : 1180), angle: ballDown ? 90 : -90);
                this.bp.fire_aoe(self, other, warningDelay: warningDelay, spawnDelay: 3500, eraseDelay: 10800, scale: 350 / 180.0, positions: [(200 + 1920 / 2, 1080 / 2)]);
                this.bp.thorns_bin(self, other, warningDelay: warningDelay, spawnDelay: 10800, radius: 550, groupMasks: (group1, group2, 0, 0));
            }

            if (this.scrbp.time(self, other, 45000)) {
                this.bp.enrage(self, other, 0);
            }
            
            return returnValue;
        }
    }
}
