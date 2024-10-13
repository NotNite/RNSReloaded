using Microsoft.VisualBasic;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class RanXin1Fight : CustomFight {
        private IHook<ScriptDelegate> bulletClearHook;
        private IHook<ScriptDelegate> encounterHook;

        private bool enableConsistentDefensive = false;

        public RanXin1Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_bluepaw1", "bp_wolf_redclaw1") {
            var bulletScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_erase_radius") - 100000);
            this.bulletClearHook =
                hooks.CreateHook<ScriptDelegate>(this.BulletClearDetour, bulletScript->Functions->Function);
            this.bulletClearHook.Activate();
            this.bulletClearHook.Enable();

            var encounterScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.encounterHook =
                hooks.CreateHook<ScriptDelegate>(this.EncounterDetour, encounterScript->Functions->Function);
            this.encounterHook.Activate();
            this.encounterHook.Enable();
        }

        private RValue* EncounterDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            this.enableConsistentDefensive = false;
            return this.encounterHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        private RValue* BulletClearDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            if (this.enableConsistentDefensive) {
                var dataId = this.rnsReloaded.FindValue(self, "dataId");
                var dataMap = this.utils.GetGlobalVar("itemData");
                var moveName = dataMap->Get((int) this.utils.RValueToLong(dataId))->Get(0)->Get(0)->ToString();

                argv[2]->Real = 450;
                argv[2]->Type = RValueType.Real;

                // Nerf defender's special bullet clear, since otherwise they're a bit too OP
                if (moveName == "mv_defender_2") {
                    argv[2]->Real = 50;
                }
            }
            returnValue = this.bulletClearHook.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        private int CleaveTopBot(CInstance* self, CInstance* other, int startTime, int width = 300, int warnTime = 4000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.cleave_fixed(self, other, spawnDelay: warnTime, positions: [
                    ((1920/2, 1080/2 + width / 2), 90),
                    ((1920/2, 1080/2 - width / 2), -90),
                ]);
            }
            return warnTime;
        }
        private int CleaveSides(CInstance* self, CInstance* other, int startTime, int width = 300, int warnTime = 4000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.cleave_fixed(self, other, spawnDelay: warnTime, positions: [
                    ((1920/2 + width / 2, 1080/2), 0),
                    ((1920/2 - width / 2, 1080/2), 180),
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

        private int IntroCleave(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            this.CleaveTopBot(self, other, time, warnTime: 6000);
            time += this.CleaveSides(self, other, time, warnTime: 6000);
            time -= 2000; // Overlap a bit
            if (this.scrbp.time(self, other, time)) {                
                this.bp.knockback_circle(self, other, spawnDelay: 2500, kbAmount: 300, position: (1920 / 2, 1080 / 2));
            }
            time += this.XLasers(self, other, time, duration: 4000);
            return time - startTime;
        }

        private int FieldLimitCorners(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            // Fieldlimits 
            if (this.scrbp.time(self, other, time)) {
                List<(int player, double topLeftDist)> playerPos = [(0, double.PositiveInfinity), (0, double.PositiveInfinity), (0, double.PositiveInfinity), (0, double.PositiveInfinity)];

                for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                    double playerX = this.utils.GetPlayerVar(i, "distMovePrevX")->Real / 1920;
                    double playerY = this.utils.GetPlayerVar(i, "distMovePrevY")->Real / 1080;
                    playerPos[i] = (1 << i, playerX + playerY);
                }
                playerPos = playerPos.OrderBy(e => e.topLeftDist).ToList();
                this.playerTargetMasks = playerPos.ConvertAll(val => val.player);

                // x = 30 to x = 960
                // y = 30 to y = 540
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: ((30 + 960) / 2, (30 + 540) / 2),
                    width: 960 - 30,
                    height: 540 - 30,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: playerPos[0].player | playerPos[1].player,
                    eraseDelay: 15000
                );
                // x = 960 to 1890
                // y = 540 to 1050
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: ((1890 + 960) / 2, (1050 + 540) / 2),
                    width: 1890 - 960,
                    height: 1050 - 540,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: playerPos[2].player | playerPos[3].player,
                    eraseDelay: 15000
                );
            }
            time += 500;
            // Daggers
            if (this.scrbp.time_repeat_times(self, other, time, 500, 4)) {
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 2000, position: (0, 0), angle: 90, lineLength: 1920 / 2, numBullets: 12, spd: 18);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 2000, position: (1920 / 2, 1080), angle: -90, lineLength: 1920 / 2, numBullets: 12, spd: 18);
            }
            time += 2000;
            // Spreads + cleave 1
            if (this.scrbp.time(self, other, time)) {
                this.bp.circle_spreads(self, other, spawnDelay: 4000, radius: 150);
            }
            time += this.CleaveSides(self, other, time);
            // Spreads + cleave 2
            if (this.scrbp.time(self, other, time)) {
                this.bp.circle_spreads(self, other, spawnDelay: 2500, radius: 150);
            }
            time += this.CleaveTopBot(self, other, time, warnTime: 2500);
            // Wind cleaves
            if (this.scrbp.time(self, other, time)) {
                this.bp.tailwind(self, other, eraseDelay: 4000);
                this.bp.cleave(self, other, warnMsg: 2, spawnDelay: 4000, cleaves: [
                    (  0, this.playerTargetMasks[0]), // Right cleave
                    ( 90, this.playerTargetMasks[1]), // Bottom cleave
                    (180, this.playerTargetMasks[2]), // Left cleave
                    (-90, this.playerTargetMasks[3]), // Top cleave
                ]);
            }
            time += 5500;
            time += this.XLasers(self, other, time, duration: 2500);
            return time - startTime;
        }

        private int FieldLimitTopBot(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;

            // Field limits
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
                    height: 540 - 40,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: this.playerTargetMasks[0] | this.playerTargetMasks[1],
                    eraseDelay: 31300
                );
                // x = 960 to 1890
                // y = 540 to 1050
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1920 / 2, (1050 + 540) / 2),
                    width: 1920 - 60,
                    height: 1050 - 540,
                    color: IBattlePatterns.FIELDLIMIT_BLUE,
                    targetMask: this.playerTargetMasks[2] | this.playerTargetMasks[3],
                    eraseDelay: 31300
                );
            }
            time += 2500;
            // Dagger walls regular
            if (this.scrbp.time_repeat_times(self, other, time - 1500, 1600, 4)) {
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 13);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, 1080 / 2 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 8, spd: 13);

                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1500, spawnDelay: 2300, position: (1920, 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 13);
                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1500, spawnDelay: 2300, position: (1920, 3 * 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 13);
            }
            time += 1600 * 4;
            // Wind setup
            if (this.scrbp.time(self, other, time)) {
                this.bp.tailwind(self, other, 22700);
                this.bp.ray_single(self, other, spawnDelay: 1500, eraseDelay: 10000, width: 100, position: (-20, 1080 / 2));
            }
            time += 2000;
            // Dagger walls winds
            if (this.scrbp.time_repeat_times(self, other, time - 1500, 2000, 4)) {
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 11);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, 1080 / 2 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 11);

                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1500, spawnDelay: 2500, position: (1920, 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 11);
                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1500, spawnDelay: 2500, position: (1920, 3 * 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 11);
            }
            time += 2000 * 4;
            // Opposite side setup
            if (this.scrbp.time(self, other, time)) {
                this.bp.tailwind(self, other, 11000);
                this.bp.cleave(self, other, warnMsg: 1, spawnDelay: 3000, cleaves: [
                    (  45, this.playerTargetMasks[0]), // Bottom Right cleave
                    ( 135, this.playerTargetMasks[1]), // Bottom Left cleave
                    ( -45, this.playerTargetMasks[2]), // Top Right cleave
                    (-135, this.playerTargetMasks[3]), // Top Left cleave
                ]);
                this.bp.ray_single(self, other, warningDelay: 2000, spawnDelay: 3000, eraseDelay: 11000, width: 100, position: (-20, 1080 / 2));
            }
            time += 3000;
            // Dagger walls winds opposite
            if (this.scrbp.time_repeat_times(self, other, time - 1500, 2200, 4)) {
                this.bp.cleave(self, other, warnMsg: 1, warningDelay: 1500, spawnDelay: 1500 + 2200, cleaves: [
                    (  45, this.playerTargetMasks[0]), // Bottom Right cleave
                    ( 135, this.playerTargetMasks[1]), // Bottom Left cleave
                    ( -45, this.playerTargetMasks[2]), // Top Right cleave
                    (-135, this.playerTargetMasks[3]), // Top Left cleave
                ]);

                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, -15), angle: 180, lineAngle: 90, lineLength: 1130 / 4, numBullets: 10, spd: 9);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: 1500, position: (1920, 1080 / 2 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 9);

                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1100, spawnDelay: 2600, position: (1920, 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1100 / 4, numBullets: 10, spd: 9);
                this.bp.fire2_line(self, other, showWarning: 1, warningDelay: 1100, spawnDelay: 2600, position: (1920, 3 * 1080 / 4 + 15), angle: 180, lineAngle: 90, lineLength: 1130 / 4, numBullets: 10, spd: 9);
            }
            time += 2200 * 4;
            return time - startTime;
        }

        private int KnockbackCleaves(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            if (this.scrbp.time(self, other, time)) {
                this.scrbp.order_random(self, other, false, 1, 1, 1, 1);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                var group0 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                var group1 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;
                var group2 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 2)->Real;
                var group3 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 3)->Real;

                this.bp.knockback_line(self, other, spawnDelay: 3700, kbAmount: 400, position: (-600, 0), horizontal: false, targetMask: group0);
                this.bp.knockback_line(self, other, spawnDelay: 3700, kbAmount: 400, position: (2520, 0), horizontal: false, targetMask: group1);
                this.bp.knockback_line(self, other, spawnDelay: 3700, kbAmount: 400, position: (0, -600), horizontal: true, targetMask: group2);
                this.bp.knockback_line(self, other, spawnDelay: 3700, kbAmount: 400, position: (0, 1780), horizontal: true, targetMask: group3);
            }
            this.CleaveTopBot(self, other, time);
            time += this.CleaveSides(self, other, time);
            return time - startTime;
        }

        private int Windmill(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            const int iterations = 4;
            const int iterationTime = 3000;
            // Windmill + winds
            if (this.scrbp.time(self, other, time)) {
                this.bp.ray_spinfast(self, other,
                    spawnDelay: iterationTime * 2,
                    eraseDelay: (iterationTime + 1) * iterations + 1500,
                    numLasers: 4,
                    position: (1920 / 2, 1080 / 2),
                    width: 100,
                    rot: 45,
                    angle: 90 * (iterations - 1)
                );
                this.bp.tailwind(self, other, eraseDelay: 13500);
            }
            time += iterationTime;
            // Daggers + spreads
            if (this.scrbp.time_repeat_times(self, other, time, iterationTime, iterations)) {
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: iterationTime, position: (0, 0), angle: 90, lineAngle: 0, lineLength: 1920, numBullets: 7, spd: 10);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: iterationTime, position: (0, 1080), angle: -90, lineAngle: 0, lineLength: 1920, numBullets: 7, spd: 10);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: iterationTime, position: (0, 0), angle: 0, lineAngle: 90, lineLength: 1080, numBullets: 5, spd: 10);
                this.bp.fire2_line(self, other, showWarning: 1, spawnDelay: iterationTime, position: (1920, 0), angle: 180, lineAngle: 90, lineLength: 1080, numBullets: 5, spd: 10);

                this.bp.line_spreads_h(self, other, spawnDelay: iterationTime, width: 120);
                this.bp.line_spreads_v(self, other, spawnDelay: iterationTime, width: 120);
            }
            time += iterationTime * iterations + 1500;
            return time - startTime;
        }

        private int spreadTarget1 = 0;
        private int spreadTarget2 = 2;
        private int RotatingCleaves(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            const int iterationTime = 3000;
            const int numIterations = 5;
            // Group generation
            if (this.scrbp.time(self, other, time)) {
                this.playerTargetMasks = new List<int>(this.scrbp.order_random(self, other, false, 1, 1, 1, 1));

                this.bp.cleave(self, other, spawnDelay: iterationTime + 1500, cleaves: [
                    ( -45, this.playerTargetMasks[0]),
                    (-135, this.playerTargetMasks[1]),
                    ( 135, this.playerTargetMasks[2]),
                    (  45, this.playerTargetMasks[3]),
                ]);
            }
            time += 1500;
            // Cleaves + spread/ray generation + projectiles
            if (this.scrbp.time_repeat_times(self, other, time, iterationTime, numIterations)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int iteration = (thisBattleTime - startTime) / iterationTime;
                this.bp.cleave(self, other, spawnDelay: iterationTime, cleaves: [
                    ( -45, this.playerTargetMasks[(0 + iteration) % 4]),
                    (-135, this.playerTargetMasks[(1 + iteration) % 4]),
                    ( 135, this.playerTargetMasks[(2 + iteration) % 4]),
                    (  45, this.playerTargetMasks[(3 + iteration) % 4]),
                ]);

                // Ray creation
                // First iteration shouldn't create ray because no warning
                if (iteration > 0) {
                    if (iteration % 2 == 0) {
                        int player0x = (int) this.utils.GetPlayerVar(this.spreadTarget1, "distMovePrevX")->Real;
                        int player1x = (int) this.utils.GetPlayerVar(this.spreadTarget2, "distMovePrevX")->Real;
                        this.bp.ray_single(self, other,
                            spawnDelay: iterationTime * 3 / 4,
                            eraseDelay: iterationTime * 2,
                            width: 100,
                            angle: 90,
                            position: (player0x, -50)
                        );
                        this.bp.ray_single(self, other,
                            spawnDelay: iterationTime * 3 / 4,
                            eraseDelay: iterationTime * 2,
                            width: 100,
                            angle: 90,
                            position: (player1x, -50)
                        );
                    } else {
                        int player0y = (int) this.utils.GetPlayerVar(this.spreadTarget1, "distMovePrevY")->Real;
                        int player1y = (int) this.utils.GetPlayerVar(this.spreadTarget2, "distMovePrevY")->Real;
                        this.bp.ray_single(self, other,
                            spawnDelay: iterationTime * 3 / 4,
                            eraseDelay: iterationTime * 2,
                            width: 100,
                            angle: 0,
                            position: (-50, player0y)
                        );
                        this.bp.ray_single(self, other,
                            spawnDelay: iterationTime * 3 / 4,
                            eraseDelay: iterationTime * 2,
                            width: 100,
                            angle: 0,
                            position: (-50, player1y)
                        );
                    }

                }
                // Choose which players have to bait
                this.spreadTarget1 = (int) Math.Log2(this.playerTargetMasks[(0 + iteration) % 4]);
                this.spreadTarget2 = (int) Math.Log2(this.playerTargetMasks[(2 + iteration) % 4]);
                // Make sure to not target players that don't exist (ray creation will then crash the game)
                if (this.spreadTarget1 >= this.utils.GetNumPlayers()) {
                    this.spreadTarget1 = 0;
                }
                if (this.spreadTarget2 >= this.utils.GetNumPlayers()) {
                    this.spreadTarget2 = 0;
                }
                // Create spread warnings
                if (iteration != numIterations - 1) {
                    if (iteration % 2 == 0) {
                        this.bp.line_spreads_h(self, other, spawnDelay: iterationTime, width: 100, targetMask: 1 << this.spreadTarget1 | 1 << this.spreadTarget2);
                    } else {
                        this.bp.line_spreads_v(self, other, spawnDelay: iterationTime, width: 100, targetMask: 1 << this.spreadTarget1 | 1 << this.spreadTarget2);
                    }
                }

                // Top projectiles
                this.bp.fire2_line(self, other,
                    warningDelay: 0,
                    showWarning: 1,
                    spawnDelay: iterationTime / 2,
                    position: (0, 0),
                    angle: 45,
                    lineAngle: 0,
                    lineLength: 1920,
                    numBullets: 6,
                    spd: 6 + iteration
                );
                // Bottom
                this.bp.fire2_line(self, other,
                    warningDelay: 0,
                    showWarning: 1,
                    spawnDelay: iterationTime / 2,
                    position: (0, 1080),
                    angle: -135,
                    lineAngle: 0,
                    lineLength: 1920,
                    numBullets: 6,
                    spd: 6 + iteration
                );
                // Right
                this.bp.fire2_line(self, other,
                    warningDelay: 0,
                    showWarning: 1,
                    spawnDelay: iterationTime / 2,
                    position: (1920, 0),
                    angle: 135,
                    lineAngle: 90,
                    lineLength: 1920,
                    numBullets: 4,
                    spd: 6 + iteration
                );
                // Left
                this.bp.fire2_line(self, other,
                    warningDelay: 0,
                    showWarning: 1,
                    spawnDelay: iterationTime / 2,
                    position: (0, 0),
                    angle: -45,
                    lineAngle: 90,
                    lineLength: 1920,
                    numBullets: 4,
                    spd: 6 + iteration
                );
            }

            time += iterationTime * numIterations;
            return time - startTime;
        }

        private void MovementSetup(CInstance* self, CInstance* other, bool isPrimary) {
            if (this.scrbp.time_repeating(self, other, 20000, 15000)) {
                // Painshare
                this.bp.painsplit(self, other, isPrimary);
            }
            if (this.scrbp.time_repeating(self, other, 6000, 15000)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int iteration = (thisBattleTime - 1000) / 12000;
                this.bp.move_position_synced(self, other,
                    duration: 2000,
                    position: ((iteration % 2 == 1) ^ isPrimary ? 240 : 1920 - 240, 1080/2)
                );
            }
        }

        // Blue wolf (patterns)
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            this.MovementSetup(self, other, true);
            int time = 1000;

            time += this.IntroCleave(self, other, time);
            time += this.FieldLimitCorners(self, other, time);
            time += this.FieldLimitTopBot(self, other, time);   // Hard
            time += this.KnockbackCleaves(self, other, time);   // Short
            time += this.RotatingCleaves(self, other, time);    // Medium
            time += 1000;                                       // They probably need a bit of a break
            time += this.KnockbackCleaves(self, other, time);   // Short
            time += this.Windmill(self, other, time);           // Hard
            time += this.KnockbackCleaves(self, other, time);   // Short
            time += this.FieldLimitTopBot(self, other, time);   // Hard

            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other);
            }
            return returnValue;
        }

        // Red wolf (projectiles)
        public override RValue* FightAltDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            this.MovementSetup(self, other, false);
            const int iterations = 10;
            const int cycleTime = 2100;
            for (int i = iterations; i > 3; i--) {
                if (this.scrbp.time_repeating(self, other, 1000 + (iterations - i) * cycleTime, (iterations + 2) * cycleTime)) {
                    double scale = i * 0.5;
                    this.bp.prscircle(self, other, warningDelay: 0, angle: this.rng.Next(0, 360), spawnDelay: cycleTime, radius: (int) (180 * scale), numBullets: i * 3 + 1, speed: 0, position: (1920 / 2, 1080 / 2));
                }
            }
            if (this.scrbp.time(self, other, 1000)) {
                this.enableConsistentDefensive = true;
            }
            return returnValue;
        }
    }
}
