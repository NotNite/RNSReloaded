using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Mav0Fight : CustomFight {
        public Mav0Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_seamstress0") {}

        const int BETWEEN_MECH_TIME = 500;
        const int bfMidX = 1920 / 2;
        const int bfMidY = 1080 / 2;

        int[] savedGroups = [0, 0, 0, 0];

        private int DiscountPleeMech(CInstance* self, CInstance* other, int startTime, int duration, int numTimes) {
            if (this.scrbp.time_repeat_times(self, other, startTime, duration + BETWEEN_MECH_TIME, numTimes)) {
                this.bp.prscircle(self, other, spawnDelay: duration * 3 / 4, radius: 320, doubled: true, position: (bfMidX, bfMidY));
                const int SET_OFFSET = 290;
                (int x, int y)?[] setCircles = [(bfMidX - SET_OFFSET, bfMidY), (bfMidX + SET_OFFSET, bfMidY), (bfMidX, bfMidY - SET_OFFSET), (bfMidX, bfMidY + SET_OFFSET)];
                this.rng.Shuffle(setCircles);

                const int MATCH_OFFSET = 370;
                (int x, int y)?[] matchCircles = [(bfMidX - MATCH_OFFSET, bfMidY - MATCH_OFFSET), (bfMidX + MATCH_OFFSET, bfMidY - MATCH_OFFSET), (bfMidX - MATCH_OFFSET, bfMidY + MATCH_OFFSET), (bfMidX + MATCH_OFFSET, bfMidY + MATCH_OFFSET)];
                this.rng.Shuffle(matchCircles);

                int[] towerAmounts = [0, 0, 0, 0];
                int[] targetMasks = [0, 0, 0, 0];
                for (int i = 0; i < 4; i++) {
                    towerAmounts[this.rng.Next(0, 4)]++;
                    targetMasks[this.rng.Next(0, 4)] |= 1 << i;
                }

                this.fzbp.ColormatchSwap(self, other, numColors: 4, spawnDelay: duration, matchRadius: 150, setRadius: 110,
                    setCircles: setCircles,
                    matchCircles: matchCircles,
                    targetMask: targetMasks,
                    colors: [IBattlePatterns.COLORMATCH_GREEN, IBattlePatterns.COLORMATCH_RED, IBattlePatterns.COLORMATCH_PURPLE, IBattlePatterns.COLORMATCH_YELLOW]
                );
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 130, number: towerAmounts[0], position: matchCircles[0]);
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 130, number: towerAmounts[1], position: matchCircles[1]);
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 130, number: towerAmounts[2], position: matchCircles[2]);
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 130, number: towerAmounts[3], position: matchCircles[3]);
            }

            return numTimes * (duration + BETWEEN_MECH_TIME);
        }
        private int OlympicRingsMech(CInstance* self, CInstance* other, int startTime, int duration, int numTimes, bool allowThrees) {
            if (this.scrbp.time_repeat_times(self, other, startTime, duration + BETWEEN_MECH_TIME, numTimes)) {

                this.fzbp.ColormatchSwap(self, other,
                    numColors: 3,
                    warningDelay: 0,
                    spawnDelay: duration,
                    setRadius: 190,
                    matchRadius: 190,
                    warnMsg: 1,
                    setCircles: [(1920 - 500, 250), (-500, -500), (500, 250)],
                    matchCircles: [(1920 / 2 - 400, 1080 - 350), (1920 / 2, 1080 - 350), (1920 / 2 + 400, 1080 - 350)],
                    targetMask: [0, 0b1111],
                    colors: [IBattlePatterns.COLORMATCH_GREEN, IBattlePatterns.COLORMATCH_RED, IBattlePatterns.COLORMATCH_BLUE]
                );
                // Currently just 1-2 random
                // If adding 3s:
                //  - with one 3, anything not adjacent must be 1
                //  - with two 3s, 3s must be adjacent and 1s must be rest
                //  - with three 3s, impossible
                int num3s = allowThrees ? this.rng.Next(1, 3) : 0;

                int[] playerCounts = [this.rng.Next(1, 3), this.rng.Next(1, 3), this.rng.Next(1, 3), this.rng.Next(1, 3)];
                if (num3s == 1) {
                    int threeLoc = this.rng.Next(0, 4);
                    playerCounts[threeLoc] = 3;
                    switch (threeLoc) {
                        case 0: // 3xx1
                            playerCounts[2] = 1;
                            playerCounts[3] = 1;
                            break;
                        case 1: // x3x1
                            playerCounts[3] = 1;
                            break;
                        case 2: // 1x3x
                            playerCounts[0] = 1;
                            break;
                        case 3: // 11x3
                            playerCounts[0] = 1;
                            playerCounts[1] = 1;
                            break;
                    }
                } else if (num3s == 2) {
                    // 3s are adjacent, so the first 3 can't be at index 3
                    int threeLocFirst = this.rng.Next(0, 3);
                    // Non-3s must be 1s
                    playerCounts = [1, 1, 1, 1];
                    playerCounts[threeLocFirst] = 3;
                    playerCounts[threeLocFirst + 1] = 3;
                }
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: playerCounts[0], position: (1920 / 2 - 600, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: playerCounts[1], position: (1920 / 2 - 200, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: playerCounts[2], position: (1920 / 2 + 200, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: playerCounts[3], position: (1920 / 2 + 600, 1080 - 350));
            }
            return numTimes * (duration + BETWEEN_MECH_TIME);
        }
        private int OsuMech(CInstance* self, CInstance* other, int startTime) {
            const int CIRCLE_SPEED = 800;
            int time = 0;

            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 2000, number: 4, radius: 250, position: (bfMidX, bfMidY));
            }
            time += 2000 - CIRCLE_SPEED;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.savedGroups = [1, 2, 4, 8];
                this.rng.Shuffle(this.savedGroups);

                this.bp.colormatch3(self, other, targetMask: this.savedGroups[0], color: IBattlePatterns.COLORMATCH_PURPLE, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED, timeExtra: 5000,
                    positions: [
                        (bfMidX - 100, bfMidY - 150),
                        (bfMidX - 300, bfMidY - 300),
                        (bfMidX - 500, bfMidY - 450),
                        (bfMidX - 650, bfMidY - 300),
                        (bfMidX - 800, bfMidY - 150)
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[1], color: IBattlePatterns.COLORMATCH_RED, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED, timeExtra: 5000,
                    positions: [
                        (bfMidX + 100, bfMidY - 150),
                        (bfMidX + 300, bfMidY - 300),
                        (bfMidX + 500, bfMidY - 450),
                        (bfMidX + 650, bfMidY - 300),
                        (bfMidX + 800, bfMidY - 150)
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[2], color: IBattlePatterns.COLORMATCH_GREEN, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED, timeExtra: 5000,
                    positions: [
                        (bfMidX - 100, bfMidY + 150),
                        (bfMidX - 300, bfMidY + 300),
                        (bfMidX - 500, bfMidY + 450),
                        (bfMidX - 650, bfMidY + 300),
                        (bfMidX - 800, bfMidY + 150)
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[3], color: IBattlePatterns.COLORMATCH_YELLOW, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED, timeExtra: 5000,
                    positions: [
                        (bfMidX + 100, bfMidY + 150),
                        (bfMidX + 300, bfMidY + 300),
                        (bfMidX + 500, bfMidY + 450),
                        (bfMidX + 650, bfMidY + 300),
                        (bfMidX + 800, bfMidY + 150)
                    ]
                );
            }
            time += CIRCLE_SPEED * 5;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 3 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX - 800, bfMidY));
                this.bp.angel_circle(self, other, spawnDelay: 3 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX + 800, bfMidY));
            }
            time += CIRCLE_SPEED;

            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[0], color: IBattlePatterns.COLORMATCH_PURPLE, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED * 2, timeExtra: 1000,
                    positions: [
                        (bfMidX - 800, bfMidY - 200),
                        // Tower at -600, 0
                        (bfMidX - 600, bfMidY + 200),
                        // Tower at -400, 0
                        (bfMidX - 400, bfMidY - 200),
                        // Tower at -200, 0
                        (bfMidX - 200, bfMidY + 200),
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[1], color: IBattlePatterns.COLORMATCH_RED, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED * 2, timeExtra: 1000,
                    positions: [
                        (bfMidX + 800, bfMidY - 200),
                        (bfMidX + 600, bfMidY + 200),
                        (bfMidX + 400, bfMidY - 200),
                        (bfMidX + 200, bfMidY + 200),
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[2], color: IBattlePatterns.COLORMATCH_GREEN, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED * 2, timeExtra: 1000,
                    positions: [
                        (bfMidX - 800, bfMidY + 200),
                        (bfMidX - 600, bfMidY - 200),
                        (bfMidX - 400, bfMidY + 200),
                        (bfMidX - 200, bfMidY - 200),
                    ]
                );
                this.bp.colormatch3(self, other, targetMask: this.savedGroups[3], color: IBattlePatterns.COLORMATCH_YELLOW, spawnDelay: CIRCLE_SPEED * 3, radius: 100, timeBetween: CIRCLE_SPEED * 2, timeExtra: 1000,
                    positions: [
                        (bfMidX + 800, bfMidY + 200),
                        (bfMidX + 600, bfMidY - 200),
                        (bfMidX + 400, bfMidY + 200),
                        (bfMidX + 200, bfMidY - 200),
                    ]
                );
            }
            time += CIRCLE_SPEED * 2;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX - 600, bfMidY));
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX + 600, bfMidY));
            }
            time += CIRCLE_SPEED * 2;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX - 400, bfMidY));
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX + 400, bfMidY));
            }
            time += CIRCLE_SPEED * 2;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX - 200, bfMidY));
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 150, number: 2, position: (bfMidX + 200, bfMidY));
            }
            time += CIRCLE_SPEED * 2;
            if (this.scrbp.time(self, other, startTime + time)) {
                this.bp.angel_circle(self, other, spawnDelay: 2 * CIRCLE_SPEED, radius: 250, number: 4, position: (bfMidX, bfMidY));
            }
            time += CIRCLE_SPEED * 2;

            return time + BETWEEN_MECH_TIME;
        }
        private int OrbMech(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.knockback_line(self, other, spawnDelay: 1500, kbAmount: 1500, position: (1920, 0), horizontal: false, kbDuration: 500);
            }
            if (this.scrbp.time_repeat_times(self, other, startTime + 1500, 12000, 2)) {
                this.fzbp.ColormatchSwap(self, other, numColors: 3, spawnDelay: 10000, matchRadius: 160, setRadius: 160, warnMsg: 1,
                    setCircles: [(-500, -500), (1920/2, 1080 / 4), (1920 / 2, 1080 * 3 /4)],
                    matchCircles: [(-1, -1), (100, 1080/2), (1920 - 100, 1080/2)],
                    colors: [IBattlePatterns.COLORMATCH_RED, IBattlePatterns.COLORMATCH_YELLOW, IBattlePatterns.COLORMATCH_GREEN],
                    targetMask: [0b1111]);
                this.bp.angel_circle(self, other, spawnDelay: 10000, radius: 140, number: 2, position: (100, 1080 / 2));
                this.bp.angel_circle(self, other, spawnDelay: 10000, radius: 140, number: 2, position: (1920 - 100, 1080 / 2));

                (int x, int playerId)[] playerLocs = [
                    ((int) this.utils.RValueToLong(this.utils.GetPlayerVar(0, "distMovePrevX")), 0),
                    ((int) this.utils.RValueToLong(this.utils.GetPlayerVar(1, "distMovePrevX")), 1),
                    ((int) this.utils.RValueToLong(this.utils.GetPlayerVar(2, "distMovePrevX")), 2),
                    ((int) this.utils.RValueToLong(this.utils.GetPlayerVar(3, "distMovePrevX")), 3),
                ];
                var sortedLocs = playerLocs.ToList();
                sortedLocs.Sort((a, b) => a.x - b.x);
                this.bp.thorns(self, other, spawnDelay: 12000, radius: 1500, targetMask: 1 << sortedLocs[0].playerId | 1 << sortedLocs[2].playerId);
                this.bp.thorns(self, other, spawnDelay: 12000, radius: 1500, targetMask: 1 << sortedLocs[1].playerId | 1 << sortedLocs[3].playerId);
                this.bp.tether_fixed(self, other, spawnDelay: 11000, radius: 300, eraseDelay: 12000, position: (1920 - 100, 1080 / 2), targetMask: 1 << sortedLocs[0].playerId);
                this.bp.tether_fixed(self, other, spawnDelay: 11000, radius: 300, eraseDelay: 12000, position: (100, 1080 / 2), targetMask: 1 << sortedLocs[3].playerId);
            }
            if (this.scrbp.time_repeat_times(self, other, startTime, 3000, 8)) {
                this.bp.water_moving_ball(self, other, spawnDelay: 800, position: (1920, 1080 / 4), speed: 5, scale: 1.38, angle: 180);
            }
            if (this.scrbp.time_repeat_times(self, other, startTime, 4000, 6)) {
                this.bp.water_moving_ball(self, other, spawnDelay: 1100, position: (1920 + 500, 1080 * 3 / 4), speed: 3, scale: 1.38, angle: 180);
            }
            return 24000 + 4000;
        }


        void execute_pattern(CInstance* self, CInstance* other, string pattern, RValue[] args, bool resetVars = true) {
            this.rnsReloaded.ExecuteScript("bpatt_var", self, other, args);
            args = [new RValue(this.rnsReloaded.ScriptFindId(pattern))];
            this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
            if (resetVars) {
                this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
            }
        }

        private RValue[] add_if_not_null(RValue[] args, string fieldName, int? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }

        private RValue[] add_if_not_null(RValue[] args, string fieldName, bool? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }

        private RValue[] add_if_not_null(RValue[] args, string fieldName, double? value) {
            if (value != null) {
                return args.Concat([this.utils.CreateString(fieldName)!.Value, new RValue(value.Value)]).ToArray();
            }
            return args;
        }

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;

            time += 1000;
            time += this.OrbMech(self, other, time);
            time += 3000;
            time += this.OrbMech(self, other, time);
            time += 3000;
            time += this.OrbMech(self, other, time);

            time += this.DiscountPleeMech(self, other, time, 4500, 3);
            time += 1500;
            time += this.OlympicRingsMech(self, other, time, 6000, 3, false);
            time += this.OsuMech(self, other, time);
            time += 1500;
            time += this.OrbMech(self, other, time);
            time += this.DiscountPleeMech(self, other, time, 4500, 2);
            time += 1500;
            time += this.OlympicRingsMech(self, other, time, 6000, 2, true);

            // Force osu mech after enrage
            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other, spawnDelay: 3000, timeBetween: 500);
            }
            time += this.OsuMech(self, other, time);
            time += this.OsuMech(self, other, time);

            return returnValue;
        }
    }
}
