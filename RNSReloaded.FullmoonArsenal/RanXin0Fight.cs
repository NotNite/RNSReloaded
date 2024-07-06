using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;


namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class RanXin0Fight : CustomFight {
        private int[] order = [1, 2, 4, 8];
        const int CYCLE_TIME = 6000;
        private bool delaySpread = false;

        public RanXin0Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_bluepaw0_s", "bp_wolf_redclaw0_s") { }

        private int Setup(CInstance* self, CInstance* other) {
            if (this.scrbp.time(self, other, 0)) {
                // Home position is top left corner
                this.bp.move_position_synced(self, other, 0, false, 2000, (240, 200));

                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                this.order = [
                    (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real,
                    (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real,
                    (int) this.rnsReloaded.ArrayGetEntry(orderBin, 2)->Real,
                    (int) this.rnsReloaded.ArrayGetEntry(orderBin, 3)->Real,
                ];
            }
            return 3000;
        }

        private void ProteanPattern(CInstance* self, CInstance* other, int startTime) {
            // Proteans resolve at 8.8s + 6s * iteration
            if (this.scrbp.time_repeating(self, other, startTime, CYCLE_TIME)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int spreadSize = thisBattleTime / 12000 + 4;
                int warnTime = 1800;
                if (this.delaySpread) {
                    warnTime = 1000;
                    this.delaySpread = false;
                }
                // Bottom left
                this.bp.clockspot(self, other,
                    warningDelay: 500,
                    warningDelay2: 5800 - warnTime,
                    warnMsg: 2,
                    displayNumber: 0,
                    spawnDelay: 5800,
                    radius: 150,
                    fanAngle: spreadSize,
                    position: (240, 1050 - 250)
                );

                // Top right
                this.bp.clockspot(self, other,
                    warningDelay: 2000,
                    warningDelay2: 5800 - warnTime,
                    warnMsg: 2,
                    displayNumber: 0,
                    spawnDelay: 5800,
                    radius: 150,
                    fanAngle: spreadSize,
                    position: (1890 - 325, 200), null
                );

                if (this.utils.GetEnemyHP(1) > 0) {
                    // Move bottom left
                    this.bp.move_position_synced(self, other, spawnDelay: 0, duration: 500, position: (240, 1050 - 250));
                    // Move home
                    this.bp.move_position_synced(self, other, spawnDelay: 500, duration: 500, position: (240, 240));
                    // Move top right
                    this.bp.move_position_synced(self, other, spawnDelay: 1000, duration: 1000, position: (1890 - 325, 200));
                    // Move home
                    this.bp.move_position_synced(self, other, spawnDelay: 2000, duration: 1000, position: (240, 200));
                } else {
                    // Move mid to let people hit it
                    this.bp.move_position_synced(self, other, spawnDelay: 0, duration: 2500, position: (960, 540));
                }
            }

        }

        private int SpreadStack(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.prscircle_follow(self, other, 0, 2, 0, false, CYCLE_TIME + 800, 190, 30, 10, this.rng.Next(0, this.utils.GetNumPlayers()));
                this.bp.circle_spreads(self, other, 1000, 2, CYCLE_TIME + 800, 140, null);
            }
            return CYCLE_TIME;
        }

        private int TasshaCleaves(CInstance* self, CInstance* other, int startTime) {
            const int CLEAVE_TIME = CYCLE_TIME / 3;
            // Start 1 cleave early but delay cleaves for better phase transition setup
            if (this.scrbp.time_repeat_times(self, other, startTime - CLEAVE_TIME, CLEAVE_TIME, 6) && this.utils.GetEnemyHP(1) < 1) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                thisBattleTime -= startTime - CLEAVE_TIME;
                int iteration = thisBattleTime / CLEAVE_TIME;

                this.bp.cleave(self, other,
                    warningDelay: CLEAVE_TIME,
                    spawnDelay: CLEAVE_TIME * 2,
                    warnMsg: 2,
                    cleaves: [
                        (-45, this.order[(0 + iteration) % 4]),
                        (45, this.order[(1 + iteration) % 4]),
                        (135, this.order[(2 + iteration) % 4]),
                        (-135, this.order[(3 + iteration) % 4]),
                    ]
                );
            }

            return CLEAVE_TIME * 6;
        }

        private int YeetCallback(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (960, 540),
                    width: 50,
                    height: 50,
                    color: IBattlePatterns.FIELDLIMIT_WHITE,
                    eraseDelay: 4500
                );
                this.bp.tailwind(self, other, eraseDelay: CYCLE_TIME * 3);

                this.bp.colormatch(self, other,
                    warningDelay: 0,
                    spawnDelay: 4000,
                    radius: 500,
                    targetMask: 0b0011,
                    color: IBattlePatterns.COLORMATCH_RED
                );
                this.bp.colormatch(self, other,
                    warningDelay: 0,
                    spawnDelay: 4000,
                    radius: 500,
                    targetMask: 0b1100,
                    color: IBattlePatterns.COLORMATCH_BLUE
                );
            }

            return CYCLE_TIME;
        }

        private int Cleave1234(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (960, 540),
                    width: 1920,
                    height: 1,
                    color: IBattlePatterns.FIELDLIMIT_WHITE,
                    eraseDelay: CYCLE_TIME
                );

                for (int i = 0; i < 2; i++) {
                    this.bp.cleave_fixed(self, other, spawnDelay: i * 3000 + 2500, positions: [((0, 540), 90)]);
                    this.bp.cleave_fixed(self, other, spawnDelay: i * 3000 + 4000, positions: [((0, 540), -90)]);
                    this.bp.displaynumbers(self, other,
                        displayNumber: i * 2 + 1,
                        warningDelay: 0,
                        spawnDelay: i * 3000 + 2500,
                        positions: [(500 + i * 500, 560 + 150)]
                    );
                    this.bp.displaynumbers(self, other,
                        displayNumber: i * 2 + 2,
                        warningDelay: 0,
                        spawnDelay: i * 3000 + 4000,
                        positions: [(750 + i * 500, 560 - 150)]
                    );
                }
            }

            return CYCLE_TIME;
        }

        private int WindSpreadCircle(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.circle_spreads(self, other,
                    warningDelay: 1000,
                    warnMsg: 2,
                    spawnDelay: CYCLE_TIME,
                    radius: 300
                );
                this.bp.prscircle(self, other,
                    warningDelay: 1000,
                    warnMsg: 2,
                    doubled: true,
                    spawnDelay: CYCLE_TIME,
                    radius: 400,
                    numBullets: 90,
                    speed: 20,
                    position: (960, 540)
                );
            }
            return CYCLE_TIME;
        }

        private void SoftEnrage(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time_repeating(self, other, startTime, 250)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                thisBattleTime -= startTime;
                int difficulty = thisBattleTime / 1500;

                if (thisBattleTime % 500 < 250) {
                    this.bp.circle_spreads(self, other, 0, 0, 750, thisBattleTime / 100 + 200, null);
                }

                if (this.rng.Next(0, Math.Max(4 - difficulty, 1)) == 0) {
                    int x, y, rot;

                    if (this.rng.Next(0, 2) == 1) {
                        x = -20;
                        y = this.rng.Next(200, 800);
                        rot = this.rng.Next(-30, 30);
                    } else {
                        y = -20;
                        x = this.rng.Next(300, 1600);
                        rot = this.rng.Next(-30, -30) + 90;
                    }
                    this.bp.ray_single(self, other,
                        warningDelay: 0,
                        spawnDelay: 1000,
                        eraseDelay: 3000,
                        width: 80 + 3 * difficulty,
                        position: (x, y),
                        angle: rot
                    );
                }

            }

        }
        // Blue wolf
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;

            time += this.Setup(self, other);

            this.ProteanPattern(self, other, time);
            time += 5800;

            // Cycle 1 (spread + stack)
            time += this.SpreadStack(self, other, time);

            // Other enemy is doing stuff for a while
            time += 5 * CYCLE_TIME;

            // 2 cycles
            time += this.TasshaCleaves(self, other, time);
            time += this.YeetCallback(self, other, time);
            time += this.Cleave1234(self, other, time);
            time += this.WindSpreadCircle(self, other, time);
            this.SoftEnrage(self, other, time);

            // 1.5 min enrage. Ideally have ~half this time to clear, in addition to having done *some* damage
            if (this.scrbp.time(self, other, 90 * 1000)) {
                this.bp.enrage(self, other, 0, 6000, 6000, false);
            }
            
            return returnValue;
        }

        private int LaserSpam(CInstance* self, CInstance* other, int delayStart) {
            if (this.scrbp.time(self, other, delayStart)) {
                this.delaySpread = true;
                var centerX = 1310;
                var centerY = 795;

                var leftX = centerX - 1160 / 2;
                var topY = centerY - 510 / 2;
                var rightX = centerX + 1160 / 2;
                var bottomY = centerY + 510 / 2;
                var coords = new List<((double x, double y) coords, int rot, int size)>() {
                    ((centerX + 20, 0), 90, 120),
                    ((0, centerY), 0, 80),
                    ((centerX + 20, 0), 77, 160),
                    ((centerX + 340, 0), 103, 120),
                    ((centerX + 530, 0), 90, 140),
                    ((centerX - centerY, 0), 45, 100),
                    ((centerX - 260, bottomY + 20), -45, 100),
                    ((0, centerY + 70), 4, 90),
                    ((0, centerY - 70), -4, 90),
                    ((centerX - 470, 0), 80, 120),
                    ((centerX - 160, 0), 100, 120),
                    ((leftX + 130, 0), 90, 120),
                    ((rightX + 20, topY + 65), 180 + 15, 65),
                    ((rightX + 20, bottomY - 65), 180 - 15, 65),
                };
                if (this.rng.Next(0, 2) == 1) {
                    coords.AddRange([
                        // Kills top left safespot
                        ((leftX - 220, 0), 75, 130),
                    // Kills bot right safespot (use with top left kill)
                    ((0, centerY + 5), -10, 110),
                ]);
                } else {
                    coords.AddRange([
                        // Kills bot left safespot
                        ((leftX + 200, 0), 105, 120),
                    // Kills top right safespot (use with bot left kill)
                    ((0, centerY - 5), 10, 110),
                ]);
                }

                if (this.rng.Next(0, 2) == 1) {
                    // Kills far right bottom safespot
                    coords.Add(((leftX + 360, -35), 58, 205));
                } else {
                    // Kills far right top safespot
                    coords.Add(((leftX + 640, bottomY + 35), -53, 205));
                }

                // (Rough) safespots
                // [
                //     (leftX + 60, topY + 20),
                //     (leftX + 60, bottomY - 20),
                //     ((centerX + leftX) / 2 + 50, (centerY + topY) / 2 + 50),
                //     ((centerX + leftX) / 2 + 50, (centerY + bottomY) / 2 - 50),
                //     (centerX - 100, bottomY - 20),
                //     (centerX - 100, topY + 20),
                //     (centerX + 400, (centerY + topY) / 2 + 50),
                //     (centerX + 400, (centerY + bottomY) / 2 - 50),
                // ]
                for (int i = 0; i < coords.Count; i++) {
                    // Total of 17 elements, so i = 16 at max
                    var warnDelay = i * 275;

                    this.bp.ray_single(self, other,
                        warningDelay: warnDelay,
                        spawnDelay: CYCLE_TIME - 100,
                        eraseDelay: CYCLE_TIME,
                        width: coords[i].size,
                        position: coords[i].coords,
                        angle: coords[i].rot
                    );
                }
                this.scrbp.order_random(self, other, false, 1, 3);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                this.bp.colormatch(self, other,
                    warningDelay: 0,
                    spawnDelay: CYCLE_TIME,
                    radius: 550,
                    targetMask: (int) orderBin->Get(0)->Real,
                    color: IBattlePatterns.COLORMATCH_BLUE
                );
                this.bp.circle_spreads(self, other, 0, 2, CYCLE_TIME, 150, null);
            }
            return CYCLE_TIME;

        }

        private int SetupAlt(CInstance* self, CInstance* other) {
            if (this.scrbp.time(self, other, 0)) {
                this.bp.move_position_synced(self, other, 0, false, 1000, (1890 - 250, 1050 - 250));
                this.bp.fieldlimit_rectangle(self, other,
                    position: (1315, 795),
                    width: 1160,
                    height: 510,
                    color: IBattlePatterns.FIELDLIMIT_WHITE
                );
            }
            return 2800;
        }

        private int CleavesAlt(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.cleave_fixed(self, other,
                    warningDelay: 0,
                    warnMsg: 0,
                    spawnDelay: CYCLE_TIME - 200,
                    positions: [((1200, 795), 180)]
                );
                this.bp.displaynumbers(self, other,
                    displayNumber: 1,
                    warningDelay: 0,
                    spawnDelay: CYCLE_TIME - 200,
                    positions: [(1100, 395)]
                );
                this.bp.cleave_fixed(self, other,
                    warningDelay: 500,
                    warnMsg: 0,
                    spawnDelay: CYCLE_TIME + 800,
                    positions: [((1200, 795), 0)]
                );
                this.bp.displaynumbers(self, other,
                    displayNumber: 2,
                    warningDelay: 500,
                    spawnDelay: CYCLE_TIME + 800,
                    positions: [(1300, 395)]
                );
            }
            return CYCLE_TIME;
        }

        private int ReverseCircle(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.circle_spreads(self, other, warnMsg: 2, spawnDelay: 4000, radius: 250);
                this.bp.prscircle_follow(self, other,
                    warningDelay: 1000,
                    warnMsg: 2,
                    spawnDelay: 5000,
                    radius: 175,
                    numBullets: 30,
                    targetId: this.rng.Next(0, this.utils.GetNumPlayers())
                );
            }
            return CYCLE_TIME;
        }

        private int PuddleColorMatch(CInstance* self, CInstance* other, int startTime, int cleaveOffset, int color) {
            if (this.scrbp.time(self, other, startTime)) {
                this.scrbp.order_random(self, other, false, 1, 1, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                this.bp.cleave(self, other,
                    warningDelay: 500,
                    spawnDelay: CYCLE_TIME,
                    warnMsg: 2,
                    cleaves: [
                        (0   + cleaveOffset, (int) (*orderBin->Get(0)).Real),
                        (180 + cleaveOffset, (int) (*orderBin->Get(1)).Real),
                    ]
                );
                this.bp.colormatch(self, other,
                    warningDelay: 500,
                    spawnDelay: CYCLE_TIME,
                    radius: 200,
                    targetMask: (int) (*orderBin->Get(2)).Real,
                    color: color
                );
            }
            return CYCLE_TIME;
        }
        private int Rem0Callback(CInstance* self, CInstance* other, int startTime) {
            int time = 0;
            // Puddles get slightly delayed (1s) 
            if (this.scrbp.time_repeat_times(self, other, startTime + 1000, 400, 12)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                thisBattleTime -= startTime + 1000;

                int playerToTarget = (thisBattleTime / 400) % this.utils.GetNumPlayers();

                var player_x = this.utils.GetPlayerVar(playerToTarget, "distMovePrevX")->Real;
                var player_y = this.utils.GetPlayerVar(playerToTarget, "distMovePrevY")->Real;

                // Last until end of cycle 5
                int timeToDespawn = 12000 - thisBattleTime;
                if (timeToDespawn > 2000) {
                    this.bp.fire_aoe(self, other, 0, 1200, timeToDespawn, .35, [(player_x, player_y)]);
                }
            }
            time += this.PuddleColorMatch(self, other, startTime, 0, IBattlePatterns.COLORMATCH_RED);

            if (this.scrbp.time_repeat_times(self, other, startTime + time, 550, 9)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                thisBattleTime -= time;

                int playerToTarget = (thisBattleTime / 400) % this.utils.GetNumPlayers();

                var player_x = this.utils.GetPlayerVar(playerToTarget, "distMovePrevX")->Real;
                var player_y = this.utils.GetPlayerVar(playerToTarget, "distMovePrevY")->Real;

                int timeToDespawn = 6000 - thisBattleTime;
                if (timeToDespawn > 1650) {
                    this.bp.fire_aoe(self, other, 0, 1400, timeToDespawn, .6, [(player_x, player_y)]);
                }
            }
            time += this.PuddleColorMatch(self, other, time + startTime, -90, IBattlePatterns.COLORMATCH_BLUE);
            return time;
        }

        private void SoftEnrageAlt(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time_repeating(self, other, startTime, 500)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                thisBattleTime -= startTime;
                int difficulty = thisBattleTime / 1500;

                this.bp.circle_spreads(self, other, 0, 0, 750, thisBattleTime / 50 + 150, null);
                if (this.rng.Next(0, Math.Max(5 - difficulty, 1)) == 0) {
                    int x, y, rot;

                    if (this.rng.Next(0, 2) == 1) {
                        x = 0;
                        y = this.rng.Next(400, 900);
                        rot = this.rng.Next(-30, 30);
                    } else {
                        y = 0;
                        x = this.rng.Next(800, 1900);
                        rot = this.rng.Next(-30, -30) + 90;
                    }
                    this.bp.ray_single(self, other,
                        warningDelay: 0,
                        spawnDelay: 1000,
                        eraseDelay: 3000,
                        width: 80 + difficulty * 3,
                        position: (x, y),
                        angle: rot
                    );
                }
            }
        }
        // Red wolf
        public override RValue* FightAltDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {

            int time = 0;
            time += this.SetupAlt(self, other);
            time += this.LaserSpam(self, other, time);
            time += this.CleavesAlt(self, other, time);
            time += this.LaserSpam(self, other, time);
            time += this.ReverseCircle(self, other, time);
            // Takes up 2 cycles
            time += this.Rem0Callback(self, other, time);
            time += this.LaserSpam(self, other, time);

            this.SoftEnrageAlt(self, other, time);

            // 1 min enrage (ideally kill ~45s)
            if (this.scrbp.time(self, other, 8800 + 6000 * 7)) {
                this.bp.enrage(self, other, 0, 6000, 6000, false);
            }
            
            return returnValue;
        }
    }
}
