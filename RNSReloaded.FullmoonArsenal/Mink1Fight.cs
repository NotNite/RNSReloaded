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
            base(rnsReloaded, logger, hooks, "bp_wolf_greyeye1") {}

        private int SetupFields(CInstance* self, CInstance* other, int startDelay, int duration, int color) {
            if (this.scrbp.time(self, other, startDelay)) {
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1920 / 2, 1080 / 2),
                    width: 1860,
                    height: 1020,
                    color: color,
                    eraseDelay: duration + 1000
                );
                this.bp.circle_spreads(self, other, radius: 400);
            }

            return duration;
        }
        private void ResetFieldsRepeating(CInstance* self, CInstance* other, int startDelay, int duration, int color) {
            if (this.scrbp.time_repeating(self, other, startDelay, duration)) {
                int minX = int.MaxValue;
                int maxX = int.MinValue;

                int minY = int.MaxValue;
                int maxY = int.MinValue;

                for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                    var playerX = this.utils.GetPlayerVar(i, "distMovePrevX");
                    var playerY = this.utils.GetPlayerVar(i, "distMovePrevY");
                    minX = Math.Min(minX, (int) playerX->Real);
                    maxX = Math.Max(maxX, (int) playerX->Real);
                    minY = Math.Min(minY, (int) playerY->Real);
                    maxY = Math.Max(maxY, (int) playerY->Real);
                }

                int width = maxX - minX;
                int height = maxY - minY;
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (minX + width / 2, minY + height / 2),
                    width: width,
                    height: height,
                    color: color,
                    eraseDelay: duration + 1000
                );
            }
        }

        private void VerticalProjectileRepeating(CInstance* self, CInstance* other, int startDelay, int interval) {
            if (this.scrbp.time_repeating(self, other, startDelay - interval, interval * 2)) {
                this.bp.light_line(self, other, spawnDelay: interval, position: (0, -20), lineAngle: 0, angle: 90, lineLength: 2020, numBullets: 15, type: 0, showWarning: true, spd: 1);
                this.bp.light_line(self, other, warningDelay: interval, spawnDelay: interval * 2, position: (67, -20), lineAngle: 0, angle: 90, lineLength: 2020, numBullets: 15, type: 0, showWarning: true, spd: 1);
            }
        }

        private void DiagonalProjectileRepeating(CInstance* self, CInstance* other, int startDelay, int interval) {
            if (this.scrbp.time_repeating(self, other, startDelay, interval)) {
                this.bp.light_line(self, other, spawnDelay: interval, position: (-20, -20), lineAngle:  0, angle: 45, lineLength: 2020, numBullets: 8, type: 1, showWarning: true, spd: 9);
                this.bp.light_line(self, other, spawnDelay: interval, position: (-20, -20), lineAngle: 90, angle: 45, lineLength: 1180, numBullets: 4, type: 1, showWarning: true, spd: 9);
            }
        }

        private void MoveAndAttack(CInstance* self, CInstance* other, int startDelay, int interval) {
            if (this.scrbp.time_repeating(self, other, startDelay, interval)) {
                this.bp.move_position_synced(self, other, duration: interval / 2, position: (this.rng.Next(100, 1820), this.rng.Next(100, 980)));
                this.bp.dark2_cr_circle(self, other, speed: 4, angle: this.rng.Next(0, 365));
            }
        }

        public override RValue* FightDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            int time = 0;
            time += this.SetupFields(self, other, time, 5000, IBattlePatterns.FIELDLIMIT_WHITE);
            this.ResetFieldsRepeating(self, other, time, 4000, IBattlePatterns.FIELDLIMIT_WHITE);
            this.VerticalProjectileRepeating(self, other, time, 2800);
            this.DiagonalProjectileRepeating(self, other, time, 800);
            this.MoveAndAttack(self, other, time, 2500);

            // Potential projectile summons:
            // bp_dark_<whatever>
            // bp_fire_<whatever>
            // bp_light_<whatever>
            // bp_water_<whatever>

            // Arrow projectiles, lined up horitonally, all moving horizontally
            // angle changes the motion direction, but keeps the line
            //this.bp.fire2_line(self, other, position: (600, 900), numBullets: 10, spd: 2);
            
            //if (this.scrbp.time(self, other, time)) {
            //    this.bp.light_crosswave(self, other, rotation: 45, position: (400, 800));
            //}
            //time += 5000;
            return returnValue;
        }
    }
}
