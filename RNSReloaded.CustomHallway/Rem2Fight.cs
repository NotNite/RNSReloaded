using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class Rem2Fight : CustomFight {
        public Rem2Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_blackear2") {}

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 1000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.cleave_fixed(self, other,
                    warningDelay: 0,
                    warnMsg: 0,
                    spawnDelay: 5000,
                    positions: [((1920 / 2, 1080 / 2), 180)]
                );
                this.bp.displaynumbers(self, other,
                    displayNumber: 1,
                    warningDelay: 0,
                    spawnDelay: 4000,
                    positions: [(1920 / 2 - 150, 1080 / 2 - 300)]
                );
                this.bp.cleave_fixed(self, other,
                    warningDelay: 1000,
                    warnMsg: 0,
                    spawnDelay: 4000,
                    positions: [((1920 / 2, 1080 / 2), 0)]
                );
                this.bp.displaynumbers(self, other,
                    displayNumber: 2,
                    warningDelay: 1000,
                    spawnDelay: 5000,
                    positions: [(1920 / 2 + 150, 1080 / 2 - 300)]
                );
            }
            time += 6000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.prscircle(self, other, warnMsg: 2, spawnDelay: 3500, radius: 400, position: (400, 1080 / 2), numBullets: 0);
                this.bp.prscircle(self, other, warningDelay: 800, warnMsg: 2, spawnDelay: 3500, radius: 400, position: (1920 - 400, 1080 / 2), numBullets: 0);

                this.bp.fire_aoe(self, other, warningDelay: 3500, spawnDelay: 3500, eraseDelay: 4000, scale: 2.2, positions: [(300, 1080 / 2), (1920 - 300, 1080 / 2)]);
            }
            time += 4500;

            if (this.scrbp.time(self, other, time)) {
                this.bp.colormatch(self, other, spawnDelay: 4000, radius: 300, targetMask: 0b0001, color: 7);
                this.bp.colormatch(self, other, spawnDelay: 4000, radius: 300, targetMask: 0b0010, color: 12);
                this.bp.colormatch(self, other, spawnDelay: 4000, radius: 300, targetMask: 0b0100, color: 17);
                this.bp.colormatch(self, other, spawnDelay: 4000, radius: 300, targetMask: 0b1000, color: 17);
            }
            time += 5000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage_deco(self, other);
            }
            time += 3000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.ray_spinfast(self, other,
                    position: (1920 / 2, 1080 / 2),
                    rot: 45,
                    angle: 180,
                    numLasers: 4,
                    warningDelay: 0,
                    spawnDelay: 3000,
                    eraseDelay: 7000,
                    width: 150
                );
            }
            time += 8000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.ray_spinfast(self, other,
                    position: (1920 / 2, 1080 / 2),
                    rot: 45,
                    angle: 1,
                    numLasers: 4,
                    warningDelay: 0,
                    spawnDelay: 3000,
                    eraseDelay: 3000,
                    width: 150
                );
                this.bp.ray_spinfast(self, other,
                    position: (1920 / 2, 1080 / 2),
                    rot: 45,
                    angle: -170,
                    numLasers: 4,
                    warningDelay: 3000,
                    spawnDelay: 3000,
                    eraseDelay: 4900,
                    width: 150
                );
            }
            time += 5500;

            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other, spawnDelay: 4000, timeBetween: 1000);
            }
            
            return returnValue;
        }
    }
}
