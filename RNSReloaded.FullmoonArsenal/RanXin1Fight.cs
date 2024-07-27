using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class RanXin1Fight : CustomFight {

        public RanXin1Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_bluepaw1_s", "bp_wolf_redclaw1_s") { }

        private int CleaveTopBot(CInstance* self, CInstance* other, int startTime, int width = 300, int warnTime = 4000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.cleave_fixed(self, other, spawnDelay: warnTime, positions: [
                    ((0, 1080/2 + width / 2), 90),
                    ((0, 1080/2 - width / 2), -90),
                ]);
            }
            return warnTime;
        }
        private int CleaveSides(CInstance* self, CInstance* other, int startTime, int width = 300, int warnTime = 4000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.cleave_fixed(self, other, spawnDelay: warnTime, positions: [
                    ((1920/2 + width / 2, 0), 0),
                    ((1920/2 - width / 2, 0), 180),
                ]);
            }
            return warnTime;
        }

        private int XLasers(CInstance* self, CInstance* other, int startTime, int duration = 3000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.ray_multi_slice(self, other, spawnDelay: duration / 2, eraseDelay: duration, timeBetween: 0, width: 100, positions: [
                    ((0, 0), 45),
                    ((0, 0), -45)
                ]);
                this.bp.cone_spreads(self, other, spawnDelay: duration, fanAngle: 60, position: (1920 / 2, 1080 / 2));
            }
            return duration;
        }

        private List<int> playerTargetMasks = new List<int>();

        // Blue wolf (patterns)
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 2000;
            
            this.CleaveTopBot(self, other, time, warnTime: 5000);
            time += this.CleaveSides(self, other, time, warnTime: 5000);
            time -= 500; // Overlap a bit
            if (this.scrbp.time(self, other, time)) {
                this.bp.knockback_circle(self, other, spawnDelay: 1200, kbAmount: 300, position: (1920 / 2, 1080 / 2));
            }
            time += this.XLasers(self, other, time);

            if (this.scrbp.time(self, other, time)) {
                List<(int player, double topLeftDist)> playerPos = [(0, double.PositiveInfinity), (0, double.PositiveInfinity), (0, double.PositiveInfinity), (0, double.PositiveInfinity)];

                for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                    double playerX = this.utils.GetPlayerVar(i, "distMovePrevX")->Real / 1920;
                    double playerY = this.utils.GetPlayerVar(i, "distMovePrevY")->Real / 1080;
                    playerPos[i] = (1 << i, playerX + playerY);
                }
                playerPos = playerPos.OrderBy(e=>e.topLeftDist).ToList();
                this.playerTargetMasks = playerPos.ConvertAll(val => val.player);

                // x = 30 to x = 960
                // y = 30 to y = 540
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: ((30+960)/2, (30+540)/2),
                    width: 960 - 30,
                    height: 540 - 30,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: playerPos[0].player | playerPos[1].player,
                    eraseDelay: 15000
                );
                // x = 960 to 1890
                // y = 540 to 1050
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: ((1890+960)/2, (1050+540)/2),
                    width: 1890-960,
                    height: 1050-540,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: playerPos[2].player | playerPos[3].player,
                    eraseDelay: 15000
                );
            }
            time += 500;
            if (this.scrbp.time_repeat_times(self, other, time, 500, 4)) {
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 2000, position: (0, 0), angle: 90, lineLength: 1920 / 2, numBullets: 12, spd: 18);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 2000, position: (1920 / 2, 1080), angle: -90, lineLength: 1920 / 2, numBullets: 12, spd: 18);
            }
            time += 2000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.circle_spreads(self, other, spawnDelay: 4000, radius: 150);
            }
            time += this.CleaveSides(self, other, time);
            if (this.scrbp.time(self, other, time)) {
                this.bp.circle_spreads(self, other, spawnDelay: 2500, radius: 150);
            }
            time += this.CleaveTopBot(self, other, time, warnTime: 2500);

            if (this.scrbp.time(self, other, time)) {
                this.bp.tailwind(self, other, eraseDelay: 4000);
                this.bp.cleave(self, other, warnMsg: 2, spawnDelay: 4000, cleaves: [
                    (  0, this.playerTargetMasks[0]), // Right cleave
                    ( 90, this.playerTargetMasks[1]), // Bottom cleave
                    (180, this.playerTargetMasks[2]), // Left cleave
                    (-90, this.playerTargetMasks[3]), // Top cleave
                ]);
            }
            time += 5000;
            time += this.XLasers(self, other, time, duration: 2000);
            
            if (this.scrbp.time(self, other, time)) {
                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                var group0 = this.rnsReloaded.ArrayGetEntry(orderBin, 0);
                var group1 = this.rnsReloaded.ArrayGetEntry(orderBin, 1);
                var group2 = this.rnsReloaded.ArrayGetEntry(orderBin, 2);
                var group3 = this.rnsReloaded.ArrayGetEntry(orderBin, 3);
                this.playerTargetMasks = [(int) group0->Real, (int) group1->Real, (int) group2->Real, (int) group3->Real];
                // x = 30 to x = 1920 - 30
                // y = 30 to y = 540
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1920 / 2, (30 + 540) / 2),
                    width: 1920 - 60,
                    height: 540 - 30,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: this.playerTargetMasks[0] | this.playerTargetMasks[1],
                    eraseDelay: 21100
                );
                // x = 960 to 1890
                // y = 540 to 1050
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1920 / 2, (1050 + 540) / 2),
                    width: 1920 - 60,
                    height: 1050 - 540,
                    color: IBattlePatterns.FIELDLIMIT_BLUE,
                    targetMask: this.playerTargetMasks[2] | this.playerTargetMasks[3],
                    eraseDelay: 21100
                );
            }
            time += 2500;
            if (this.scrbp.time_repeat_times(self, other, time, 1600, 4)) {
                this.bp.fire2_line(self, other, spawnDelay: 0, position: (1920, 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
                this.bp.fire2_line(self, other, spawnDelay: 0, position: (1920, 1080/2 + 10), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);

                this.bp.fire2_line(self, other, spawnDelay: 800, position: (1920, 1080/4 + 20), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
                this.bp.fire2_line(self, other, spawnDelay: 800, position: (1920, 3*1080/4 + 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
            }
            time += 1600 * 4;
            if (this.scrbp.time(self, other, time)) {
                this.bp.tailwind(self, other, 11000);
                this.bp.cleave(self, other, spawnDelay: 3000, cleaves: [
                    (  45, this.playerTargetMasks[0]), // Bottom Right cleave
                    ( 135, this.playerTargetMasks[1]), // Bottom Left cleave
                    ( -45, this.playerTargetMasks[2]), // Top Right cleave
                    (-135, this.playerTargetMasks[3]), // Top Left cleave
                ]);
                this.bp.ray_single(self, other, warningDelay: 2000, spawnDelay: 3000, eraseDelay: 20000, width: 100, position: (-20, 1080 / 2));
            }
            time += 3000;
            if (this.scrbp.time_repeat_times(self, other, time, 2000, 4)) {
                this.bp.cleave(self, other, spawnDelay: 2000, cleaves: [
                    (  45, this.playerTargetMasks[0]), // Bottom Right cleave
                    ( 135, this.playerTargetMasks[1]), // Bottom Left cleave
                    ( -45, this.playerTargetMasks[2]), // Top Right cleave
                    (-135, this.playerTargetMasks[3]), // Top Left cleave
                ]);

                this.bp.fire2_line(self, other, spawnDelay: 0, position: (1920, 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
                this.bp.fire2_line(self, other, spawnDelay: 0, position: (1920, 1080 / 2 + 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);

                this.bp.fire2_line(self, other, spawnDelay: 1000, position: (1920, 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
                this.bp.fire2_line(self, other, spawnDelay: 1000, position: (1920, 3 * 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1080 / 4, numBullets: 8, spd: 12);
            }
            time += 2000 * 4;

            // TODO: knockback into cleave, randomized direction per player so you gotta stand (pretty much a TP mechanic)
            //   (intermediary to re-randomize positions before next field limit)

            // XLasers, but rotating? Maybe with bullets or something to dodge at the same time? Or use as downtime to help bullet clear setups

            return returnValue;
        }

        // Red wolf (projectiles)
        public override RValue* FightAltDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            const int iterations = 12;
            const int cycleTime = 1700;
            for (int i = iterations; i > 0; i--) {
                if (this.scrbp.time_repeating(self, other, 1000 + (iterations - i) * cycleTime, iterations * cycleTime)) {
                    double scale = i * 0.4;
                    this.bp.prscircle(self, other, warningDelay: 0, angle: this.rng.Next(0, 360), spawnDelay: cycleTime, radius: (int) (180 * scale), numBullets: i * 4, speed: 0, position: (1920 / 2, 1080 / 2));
                }
            }

            if (this.scrbp.time_repeating(self, other, 20000, 15000)) {
                // Painshare
            }
            return returnValue;
        }
    }
}
