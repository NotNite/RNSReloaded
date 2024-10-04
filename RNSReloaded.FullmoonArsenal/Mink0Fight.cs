using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class Mink0Fight : CustomFight {
        public Mink0Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_greyeye0") { }

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time_repeating(self, other, 0, 12000)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int iteration = thisBattleTime / 12000;

                var coords = new List<((double x, double y) coords, int rot, int width)>() {
                    ((iteration * 20, 0), 90, 80 + iteration * 40),
                    ((0, iteration * 5), 0, 45 + iteration * 10),
                    ((1920 - iteration * 40, 1080), -90, 80 + iteration * 40),
                    ((1920, 1080 - iteration * 5), 180, 45 + iteration * 10),
                };

                for (int i = 0; i < coords.Count; i++) {
                    this.bp.ray_single(self, other,
                        warningDelay: 0,
                        spawnDelay: 0,
                        eraseDelay: 12000,
                        width: coords[i].width,
                        position: coords[i].coords,
                        angle: coords[i].rot
                    );
                }

                this.bp.fire_aoe(self, other, spawnDelay: 4000, eraseDelay: 10000, scale: 0.85, positions: [(1920 / 2, 1080 / 2)]);
                this.bp.ray_spinfast(self, other,
                    position: (1920 / 2, 1080 / 2),
                    rot: 45,
                    angle: this.rng.Next(0, 2) == 1 ? 180 : -180,
                    numLasers: 4,
                    warningDelay: 0,
                    spawnDelay: 6000,
                    eraseDelay: 11000,
                    width: 150
                );
                this.bp.cone_spreads(self, other, warnMsg: 2, spawnDelay: 7000, fanAngle: 90, position: (1920 / 2, 1080 / 2));
                this.bp.prsline_h_follow(self, other,
                    warningDelay: 0,
                    warnMsg: 0,
                    doubled: true,
                    spawnDelay: 11500,
                    width: 600,
                    offset: 200,
                    speed: 15,
                    targetId: this.rng.Next(0, this.utils.GetNumPlayers())
                );

            }
            
            // 3x is probably about as fast as things can get without being too crazy
            int time = 0;
            double gameSpeed = 1;
            while (gameSpeed < 2.9) {
                time += (int) (6000 * gameSpeed);
                gameSpeed += 1f/3;

                if (this.scrbp.time(self, other, (int) (time - 500 * gameSpeed))) {
                    this.bp.setgamespeed(self, other, (int) (500 * gameSpeed), gameSpeed);
                }
            }
            // This actually happens ~80s into the run, and ~40s real time
            time += (int) (6000 * gameSpeed);
            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other, 0, (int) (6000 * gameSpeed), (int) (6000 * gameSpeed), false);
            }
            
            return returnValue;
        }
    }
}
