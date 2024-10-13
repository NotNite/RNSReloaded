using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class Rem1Fight : CustomFight {
        public Rem1Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_blackear1") { }

        private int Setup(CInstance* self, CInstance* other) {
            int time = 0;
            if (this.scrbp.time(self, other, time)) {
                this.bp.fieldlimit_rectangle(self, other,
                    position: (960, 540),
                    width: 50,
                    height: 50,
                    color: IBattlePatterns.FIELDLIMIT_WHITE,
                    targetMask: 0b1111
                );
                this.bp.tailwind_permanent(self, other);
            }
            time += 2000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.move_position_synced(self, other, duration: 1000, position: (960, 540));
            }

            // While this is repeating, I'd still consider it setup - it only repeats because I can't
            // make it permanent, and the uptime is 100%
            if (this.scrbp.time_repeating(self, other, 1500, 6000)) {
                this.bp.fire_aoe(self, other, 1000, 6000, 14000, .25, [(960, 540)]);
            }

            time += 1000;
            return time;
        }

        private void YeetRepeat(CInstance* self, CInstance* other, int delayStart, int interval) {
            if (this.scrbp.time_repeating(self, other, delayStart, 6000)) {
                this.bp.fire_aoe(self, other,
                    spawnDelay: 2400,
                    eraseDelay: 2900,
                    scale: 2,
                    positions: [(960, 540)]
                );
                this.bp.prscircle(self, other,
                    position: (960, 540),
                    radius: 340,
                    warningDelay: 250,
                    spawnDelay: 3500,
                    warnMsg: 2,
                    doubled: true,
                    numBullets: 40,
                    angle: this.rng.Next(0, 360)
                );
                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                this.bp.cleave(self, other,
                    warningDelay: 500,
                    spawnDelay: 5000,
                    warnMsg: 2,
                    cleaves: [
                        (-45, (int) (*orderBin->Get(0)).Real),
                        (45, (int) (*orderBin->Get(1)).Real),
                        (135, (int) (*orderBin->Get(2)).Real),
                        (-135, (int) (*orderBin->Get(3)).Real),
                    ]
                );
            }
        }
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = this.Setup(self, other);

            time += 2500; // Add a bit of waiting for people to realize what's going on

            this.YeetRepeat(self, other, time, 6000);

            if (this.scrbp.time(self, other, 45000)) {
                this.bp.enrage(self, other, 0, 6000, 6000, false);
            }
            
            return returnValue;
        }
    }
}
