using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    
    internal unsafe class TasshaFight : CustomFight {
        public TasshaFight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_snowfur0_s") { }


        private double myX = 0, myY = 0;

        private int DashCleave(CInstance* self, CInstance* other, int startTime, int target) {
            int time = 0;
            if (this.scrbp.time(self, other, startTime + time)) {
                double playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                double playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;

                // If more than 150 pixels away, then move
                (double x, double y) vec = (this.myX - playerX, this.myY - playerY);
                double vecMag = Math.Sqrt(vec.x * vec.x + vec.y * vec.y);

                if (vecMag > 100) {
                    if (playerX == this.myX) {
                        this.myX++;
                    }
                    (double x, double y) vec_u = (vec.x / vecMag, vec.y / vecMag);

                    this.bp.move_position_synced(self, other, duration: 500, position: (playerX + vec_u.x * 100, playerY + vec_u.y * 100));
                    this.myX = playerX + vec_u.x * 100;
                    this.myY = playerY + vec_u.y * 100;
                }

            }
            time += 500;

            if (this.scrbp.time(self, other, startTime + time)) {
                double playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                double playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;
                (double x, double y) vec = (this.myX - playerX, this.myY - playerY);
                int cleaveAngle = (int) (Math.Atan2(vec.y, vec.x) * 180 / Math.PI) + 180;

                this.bp.cleave_fixed(self, other, spawnDelay: 600, positions: [((this.myX, this.myY), cleaveAngle)]);
            }
            time += 600;
            return time;
        }

        private int DashCleaveWarn(CInstance* self, CInstance* other, int startTime, int target) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: 1500, radius: 150, targetMask: 1 << target, position: (this.myX, this.myY));
            }
            return 1500 + this.DashCleave(self, other, startTime + 1500, target);
        }

        private int StarburstLaser(CInstance* self, CInstance* other, int startTime, int target, int numLasers = 5, int eraseDelay = 5000) {
            if (this.scrbp.time(self, other, startTime)) {
                double playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                double playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;

                for (int i = 0; i < numLasers; i++) {
                    int rot = i * 180 / numLasers;
                    var warnDelay = i * 100;

                    double slope = Math.Tan(((double) rot) / 180 * Math.PI);
                    (double x, double y) coords;
                    if (slope == 0) {
                        coords = (-50, playerY);
                    } else if (Math.Abs(slope) > 1e5) {
                        coords = (playerX, -50);
                    } else {
                        coords = (playerX - (playerY + 50) / slope, -50);
                    }
                    this.logger.PrintMessage("Slope " + slope, this.logger.ColorRed);
                    this.logger.PrintMessage("Coords " + coords, this.logger.ColorRed);

                    this.bp.ray_single(self, other,
                        warningDelay: warnDelay,
                        spawnDelay: 9999,
                        eraseDelay: 3000,
                        width: 5,
                        position: coords,
                        angle: rot
                    );
                    this.bp.ray_single(self, other,
                        warningDelay: 3000,
                        spawnDelay: 3000,
                        eraseDelay: eraseDelay,
                        width: 100,
                        position: coords,
                        angle: rot
                    );
                }
            }
            return eraseDelay;
        }

        private int BubbleLine(CInstance* self, CInstance* other, int startTime, int bubbleDuration) {
            int time = 0;
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.ray_single(self, other, warningDelay: 500, spawnDelay: 3000, eraseDelay: 3000, width: 5, position: (-50, 1080/2));
            }
            time += 3000;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.ray_single(self, other, spawnDelay: 0, eraseDelay: bubbleDuration, width: 150, position: (-50, 1080 / 2));
                this.bp.gravity_pull_temporary(self, other, eraseDelay: bubbleDuration);
            }
            if (this.scrbp.time_repeat_times(self, other, startTime + time, 500, bubbleDuration / 500)) {
                // Spawn Bubbles moving up/down? Somehow?
                // bp_frog_moving_ball maybe?
                // Projectiles? 
            }
            return time + bubbleDuration;
        }
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;
            if (this.scrbp.time(self, other, time)) {
                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2, 1080 / 2));
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
            }
            time += 1000;

            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);
            time += this.BubbleLine(self, other, time, 10000);

            return returnValue;
        }
    }
}
