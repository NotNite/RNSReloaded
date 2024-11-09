using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Maxi0Fight : CustomFight {
        public Maxi0Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_tinkerer0_s") {}

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time(self, other, 0)) {
                if (this.utils.GetNumPlayers() != 4) {
                    //this.bp.enrage(self, other, spawnDelay: 3000, timeBetween: 500);
                }
            }

            if (this.scrbp.time_repeating(self, other, 1000, 9000)) {
                this.scrbp.order_random(self, other, false, 2, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                int group1 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                int group2 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;

                this.fzbp.ColormatchSwap(self, other,
                    numColors: 3,
                    spawnDelay: 9000,
                    setRadius: 150,
                    matchRadius: 300,
                    warnMsg: 1,
                    setCircles: [(-500, -500), (1750, 1080-200), (1750, 200)],
                    matchCircles: [(200 + 1920/2, 1080/2), (350, 1080 / 4), (350, 1080 * 3 /4)],
                    targetMask: [0b1111],
                    colors: [IBattlePatterns.COLORMATCH_RED, IBattlePatterns.COLORMATCH_GREEN, IBattlePatterns.COLORMATCH_BLUE]
                );
                // TODO: replace this with the frog bullet
                this.bp.marching_bullet(self, other, spawnDelay: 0, timeBetween: 6300, scale: 350 / 180.0, positions: [
                    (200 + 1920/2, -100),
                    (200 + 1920/2, 1380),
                ]);
                this.bp.fire_aoe(self, other, spawnDelay: 2000, eraseDelay: 9000, scale: 350 / 180.0, positions: [(200 + 1920 / 2, 1080 / 2)]);
                this.bp.thorns_bin(self, other, spawnDelay: 9000, radius: 700, groupMasks: (group1, group2, 0, 0));
            }

            if (this.scrbp.time(self, other, 45000)) {
                this.bp.enrage(self, other, 0);
            }
            
            return returnValue;
        }
    }
}
