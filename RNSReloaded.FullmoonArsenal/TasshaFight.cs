using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    
    internal unsafe class TasshaFight : CustomFight {

        private IHook<ScriptDelegate> starburstHook;
        private IHook<ScriptDelegate>? jumpCleaveHook;
        private IHook<ScriptDelegate>? bubbleLineHook;

        public TasshaFight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_snowfur0", "bp_wolf_snowfur0_pt2") {
            this.playerRng = new Random();
            // Regular fight = setup
            // pt2 = final phase
            
            var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_wolf_snowfur0_pt3") - 100000);
            this.starburstHook =
                hooks.CreateHook<ScriptDelegate>(this.StarburstPhase, script->Functions->Function);
            this.starburstHook.Activate();
            this.starburstHook.Enable();

            script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_wolf_snowfur0_pt4") - 100000);
            this.jumpCleaveHook =
                hooks.CreateHook<ScriptDelegate>(this.JumpCleavePhase, script->Functions->Function);
            this.jumpCleaveHook.Activate();
            this.jumpCleaveHook.Enable();

            script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_wolf_snowfur0_pt5") - 100000);
            this.bubbleLineHook =
                hooks.CreateHook<ScriptDelegate>(this.BubbleLinePhase, script->Functions->Function);
            this.bubbleLineHook.Activate();
            this.bubbleLineHook.Enable();
        }


        private double myX = 0, myY = 0;
        private int seed;
        private Random playerRng;
        private (double x, double y) posSnapshot = (0, 0);

        private int DashToPlayer(CInstance* self, CInstance* other, int startTime, int target) {
            if (this.scrbp.time(self, other, startTime)) {
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
            return 500;
        }

        private ((double x, double y) pos, int angle) CalculateCleave(double posX, double posY) {
            (double x, double y) vec = (this.myX - posX, this.myY - posY);
            int cleaveAngle = (int) (Math.Atan2(vec.y, vec.x) * 180 / Math.PI) + 180;

            return ((this.myX, this.myY), cleaveAngle);
        }
        private int DashCleave(CInstance* self, CInstance* other, int startTime, int target, bool chain = false) {
            int time = startTime;
            time += this.DashToPlayer(self, other, time, target);
            if (this.scrbp.time(self, other, time)) {
                double playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                double playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;
                this.posSnapshot = (playerX, playerY);
            }
            time += 100;
            if (this.scrbp.time(self, other, time)) {
                var cleave = this.CalculateCleave(this.posSnapshot.x, this.posSnapshot.y);
                this.bp.cleave_fixed(self, other, spawnDelay: 600, positions: [cleave]);
                if (chain) {
                    this.bp.cleave_fixed(self, other, spawnDelay: 600, positions: this.cleaves.ToArray());
                    this.cleaves.Add(cleave);
                }
            }
            time += 600;
            return time - startTime;
        }

        private int DashCleaveWarn(CInstance* self, CInstance* other, int startTime, int target, int warnTime = 1500) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: warnTime, radius: 150, targetMask: 1 << target, position: (this.myX, this.myY));
            }
            return warnTime + this.DashCleave(self, other, startTime + warnTime, target);
        }

        private Dictionary<int, ((double x, double y) pos, int rot)[]> starburstCached = new Dictionary<int, ((double x, double y) pos, int rot)[]>();
        private int StarburstLaser(CInstance* self, CInstance* other, int startTime, int target, int numLasers = 5, int spawnDelay = 3000, int eraseDelay = 5000, (double x, double y)? posOverride = null) {
            if (this.scrbp.time(self, other, startTime)) {
                double playerX, playerY;
                if (posOverride.HasValue) {
                    playerX = posOverride.Value.x;
                    playerY = posOverride.Value.y;
                } else {
                    playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                    playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;
                }

                this.starburstCached[target] = [];
                for (int i = 0; i < numLasers; i++) {
                    int rot = i * 180 / numLasers;
                    var warnDelay = i * 100;

                    double slope = Math.Tan(((double) rot) / 180 * Math.PI);
                    (double x, double y) coords;
                    if (Math.Abs(slope) <= 0.01) {
                        coords = (-50, playerY);
                    } else if (Math.Abs(slope) > 1e5) {
                        coords = (playerX, -50);
                    } else {
                        coords = (playerX - (playerY + 50) / slope, -50);
                    }

                    this.bp.ray_single(self, other,
                        warningDelay: warnDelay,
                        spawnDelay: spawnDelay,
                        eraseDelay: spawnDelay,
                        width: 5,
                        position: coords,
                        angle: rot
                    );
                    this.starburstCached[target] = this.starburstCached[target].Concat([(coords, rot)]).ToArray();
                }
            }
            // We split these lasers up and do this caching because otherwise we send too many patterns
            // in a single frame which tends to break peoples' games
            if (this.scrbp.time(self, other, startTime + spawnDelay)) {
                foreach (var laser in this.starburstCached[target]) {
                    this.bp.ray_single(self, other,
                        warningDelay: 0,
                        spawnDelay: 0,
                        eraseDelay: eraseDelay - spawnDelay,
                        width: 100,
                        position: laser.pos,
                        angle: laser.rot
                    );
                }
            }
            return eraseDelay;
        }

        private int StarburstRotate(CInstance* self, CInstance* other, int startTime, int target, int numLasers = 5, int spawnDelay = 4000, int eraseDelay = 8000, int rot = 90, (double x, double y)? posOverride = null) {
            int time = startTime;
            if (this.scrbp.time(self, other, time)) {
                double playerX = this.utils.GetPlayerVar(target, "distMovePrevX")->Real;
                double playerY = this.utils.GetPlayerVar(target, "distMovePrevY")->Real;
                this.posSnapshot = (playerX, playerY);
                if (posOverride.HasValue) {
                    this.posSnapshot = posOverride.Value;
                }
            }
            this.StarburstLaser(self, other, time, target, numLasers: numLasers, spawnDelay: spawnDelay, eraseDelay: spawnDelay, posOverride: posOverride);
            if (this.scrbp.time(self, other, time + numLasers * 100)) {
                this.bp.ray_spinfast(self, other,
                    warningDelay: 0,
                    spawnDelay: spawnDelay,
                    eraseDelay: spawnDelay,
                    width: 10,
                    angle: rot > 0 ? 1 : -1,
                    position: this.posSnapshot,
                    rot: 0,
                    numLasers: 0,
                    warningRadius: 100
                );
            }
            time += spawnDelay;
            if (this.scrbp.time(self, other, time)) {
                this.bp.ray_spinfast(self, other,
                    warningDelay: 0,
                    spawnDelay: 0,
                    eraseDelay: eraseDelay - spawnDelay,
                    width: 100,
                    angle: rot,
                    position: this.posSnapshot,
                    rot: 0,
                    numLasers: numLasers * 2,
                    warningRadius: 100
                );
            }
            time += eraseDelay - spawnDelay;

            return time - startTime;
        }
        private int BubbleLine(CInstance* self, CInstance* other, int startTime, int bubbleDuration, int skipPercent = 0) {
            int time = startTime;
            bool startLeft = this.rng.Next(0, 2) == 1;
            if (this.scrbp.time(self, other, time)) {
                var x0 = startLeft ? 50 : 1920 - 50;
                var x1 = startLeft ? 1920 - 50 : 50;
                this.bp.move_position_synced(self, other, duration: 1000, position: (x0, 1080/2));
                this.bp.move_position_synced(self, other, spawnDelay: 2000, duration: 667, position: (x1, 1080/2));
                this.bp.move_position_synced(self, other, spawnDelay: 3000, duration: 333, position: (1920 / 2, 1080 / 2));
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
                this.bp.ray_single(self, other, warningDelay: 500, spawnDelay: 3000, eraseDelay: 3000, width: 5, position: (-50, 1080/2));

            }
            time += 3000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.gravity_pull_temporary(self, other, eraseDelay: bubbleDuration, position: (1920/2, 1080/2));
                this.bp.ray_single(self, other, spawnDelay: 0, eraseDelay: bubbleDuration, width: 150, position: (-50, 1080 / 2));
            }
            if (this.scrbp.time_repeat_times(self, other, time, 500, bubbleDuration / 500)) {
                if (this.rng.Next(0, 100) >= skipPercent) {
                    switch (this.rng.Next(0, 3)) {
                        case 0:
                            int x = this.rng.Next(-150, -10);
                            this.bp.light_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: 90, spd: 2, lineLength: 2100, numBullets: this.rng.Next(5, 8), type: 0);
                            this.bp.light_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: -90, spd: 2, lineLength: 2100, numBullets: this.rng.Next(5, 8), type: 0);
                            break;
                        case 1:
                            x = this.rng.Next(-150, -10);
                            this.bp.light_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: 90, spd: 4, lineLength: 2100, numBullets: this.rng.Next(6, 11), type: 1);
                            this.bp.light_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: -90, spd: 4, lineLength: 2100, numBullets: this.rng.Next(6, 11), type: 1);
                            break;
                        case 2:
                            x = this.rng.Next(-150, -10);
                            this.bp.fire2_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: 90, spd: 3, lineLength: 2100, numBullets: this.rng.Next(4, 12));
                            this.bp.fire2_line(self, other, spawnDelay: 0, position: (x, 1080 / 2), angle: -90, spd: 3, lineLength: 2100, numBullets: this.rng.Next(4, 12));
                            break;
                    }
                }
            }
            return time + bubbleDuration - startTime;
        }

        private int BubbleLineRotating(CInstance* self, CInstance* other, int startTime, int bubbleDuration, int rot, int skipPercent = 0) {
            int time = startTime;

            bool startLeft = this.rng.Next(0, 2) == 1;
            if (this.scrbp.time(self, other, startTime)) {
                var x0 = startLeft ? 50 : 1920 - 50;
                var x1 = startLeft ? 1920 - 50 : 50;
                this.bp.move_position_synced(self, other, duration: 1000, position: (x0, 1080 / 2));
                this.bp.move_position_synced(self, other, spawnDelay: 2000, duration: 1000, position: (x1, 1080 / 2));
                this.bp.move_position_synced(self, other, spawnDelay: 3000, duration: 333, position: (1920 / 2, 1080 / 2));
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
                this.bp.ray_single(self, other, warningDelay: 500, spawnDelay: 3000, eraseDelay: 3000, width: 5, position: (-50, 1080 / 2));
                this.bp.ray_spinfast(self, other,
                    warningDelay: 0,
                    spawnDelay: 3000,
                    numLasers: 0,
                    angle: rot > 0 ? 1 : -1,
                    rot: 0,
                    width: 100,
                    eraseDelay: 3000,
                    warningRadius: 100,
                    position: (1920 / 2, 1080 / 2)
                );
            }
            time += 3000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.ray_spinfast(self, other,
                    warningDelay: 0,
                    spawnDelay: 0,
                    numLasers: 2,
                    angle: rot,
                    rot: 0,
                    width: 100,
                    eraseDelay: bubbleDuration,
                    warningRadius: 100,
                    position: (1920 / 2, 1080 / 2)
                );
                this.bp.gravity_pull_temporary(self, other, eraseDelay: bubbleDuration, position: (1920/2, 1080/2));
            }
            if (this.scrbp.time_repeat_times(self, other, time, 500, bubbleDuration / 500)) {
                int thisBattleTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                int thisMechTime = thisBattleTime - time;
                double percentThroughMech = ((float) thisMechTime) / bubbleDuration;
                int rotEdit = (int) (percentThroughMech * rot);

                int lengthAdd = this.rng.Next(0, 200);
                var pos = (1920 / 2 + this.rng.Next(0, 20), 1080 / 2 + this.rng.Next(0, 20));
                if (this.rng.Next(0, 100) >= skipPercent) {
                    switch (this.rng.Next(0, 3)) {
                        case 0:
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit, spd: 2, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(3, 8), type: 0);
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit, spd: 2, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(3, 8), type: 0);

                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit + 180, spd: 2, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(3, 8), type: 0);
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit + 180, spd: 2, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(3, 8), type: 0);
                            break;
                        case 1:
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit, spd: 4, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(5, 9), type: 1);
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit, spd: 4, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(5, 9), type: 1);

                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit + 180, spd: 4, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(5, 9), type: 1);
                            this.bp.light_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit + 180, spd: 4, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(5, 9), type: 1);
                            break;
                        case 2:
                            this.bp.fire2_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit, spd: 3, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(4, 11));
                            this.bp.fire2_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit, spd: 3, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(4, 11));

                            this.bp.fire2_line(self, other, spawnDelay: 0, position: pos, angle: 90 + rotEdit, lineAngle: rotEdit + 180, spd: 3, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(4, 11));
                            this.bp.fire2_line(self, other, spawnDelay: 0, position: pos, angle: -90 + rotEdit, lineAngle: rotEdit + 180, spd: 3, lineLength: 1100 + lengthAdd, numBullets: this.rng.Next(4, 11));
                            break;
                    }
                }
            }

            return time + bubbleDuration - startTime;
        }

        private int VerticalLasers(CInstance* self, CInstance* other, int startTime, int warnDelay = 4000, int eraseDelay = 3000) {
            if (this.scrbp.time(self, other, startTime)) {
                this.bp.ray_multi_v(self, other, spawnDelay: warnDelay, eraseDelay: warnDelay, width: 5, positions: [
                    (1920/6 * -3, -20),
                    (1920/6 * -2, -20),
                    (1920/6 * -1, -20),
                    (1920/6 * 0, -20),
                    (1920/6 * 1, -20),
                    (1920/6 * 2, -20),
                    (1920/6 * 3, -20),
                ]);
            }
            if (this.scrbp.time(self, other, startTime + warnDelay)) {
                this.bp.ray_multi_v(self, other, spawnDelay: 0, eraseDelay: eraseDelay, width: 40, positions: [
                    (1920/6 * -3, -20),
                    (1920/6 * -2, -20),
                    (1920/6 * -1, -20),
                    (1920/6 * 0, -20),
                    (1920/6 * 1, -20),
                    (1920/6 * 2, -20),
                    (1920/6 * 3, -20),
                ]);
            }
            return warnDelay + eraseDelay;
        }

        private int AddWarningThorns(CInstance* self, CInstance* other, int startTime, int spawnDelay = 2000, int interval = 1200) {
            if (this.scrbp.time(self, other, startTime)) {
                this.rng.Shuffle(this.playerTargets);
                this.bp.showorder(self, other, eraseDelay: spawnDelay + interval - 700, timeBetween: interval, orderMasks: (1 << this.playerTargets[0], 1 << this.playerTargets[1], 1 << this.playerTargets[2], 1 << this.playerTargets[3]));
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: spawnDelay + interval - 700, radius: 1, targetMask: 1 << this.playerTargets[0], position: (this.myX, this.myY));
                this.bp.thorns(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: spawnDelay + interval - 700, radius: 1, targetMask: (1 << this.playerTargets[0]) | (1 << this.playerTargets[1]));
                this.bp.thorns(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: spawnDelay + 2 * interval - 700, radius: 1, targetMask: (1 << this.playerTargets[1]) | (1 << this.playerTargets[2]));
                this.bp.thorns(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: spawnDelay + 3 * interval - 700, radius: 1, targetMask: (1 << this.playerTargets[2]) | (1 << this.playerTargets[3]));
            }
            if (this.scrbp.time(self, other, startTime + spawnDelay + interval - 700)) {
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: interval, radius: 1, targetMask: 1 << this.playerTargets[1], position: (this.myX, this.myY));
            }
            if (this.scrbp.time(self, other, startTime + spawnDelay + 2 * interval - 700)) {
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: interval, radius: 1, targetMask: 1 << this.playerTargets[2], position: (this.myX, this.myY));
            }
            if (this.scrbp.time(self, other, startTime + spawnDelay + 3 * interval - 700)) {
                this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: interval, radius: 1, targetMask: 1 << this.playerTargets[3], position: (this.myX, this.myY));
            }
            return spawnDelay;
        }

        private int LimitCut(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            time += this.AddWarningThorns(self, other, time, spawnDelay: 1500);
            time += this.DashCleave(self, other, time, this.playerTargets[0]); // 1200 ms
            time += this.DashToPlayer(self, other, time, this.playerTargets[1]); // 500 ms
            this.StarburstLaser(self, other, time, this.playerTargets[1], spawnDelay: 1100, eraseDelay: 4700);
            time += 1100;
            time += this.DashCleave(self, other, time, this.playerTargets[2]); // 1200 ms
            time += this.DashToPlayer(self, other, time, this.playerTargets[1]); // 500 ms
            time += this.StarburstLaser(self, other, time, this.playerTargets[3], spawnDelay: 1000, eraseDelay: 2000);
            return time - startTime;
        }

        private void StarburstBubble(CInstance* self, CInstance* other, int startTime, int cacheId, bool fromEdge = false) {
            // Choose random x-coord to end, and random y-coord in top half of screen
            (double x, double y) starburstEnd = (this.playerRng.Next(30, 1920 - 30), this.playerRng.Next(40, 1080/2 - 120));
            // Move y-coord to bottom half sometimes. This allows us to have a 120-pixel wide gap around 
            // the center where the starburst can't end up, to ensure it travels far enough
            int distanceMoved = 1080 / 2 - (int) starburstEnd.y;
            if (this.playerRng.Next(0, 2) == 1) {
                starburstEnd.y = 1080 - starburstEnd.y;
            }
            (double x, double y) starburstStart = (starburstEnd.x, 1080 / 2);

            if (fromEdge) {
                starburstStart.y = this.playerRng.Next(0, 2) == 1 ? 0 : 1080;
            }

            if (this.scrbp.time(self, other, startTime)) {
                this.bp.marching_bullet(self, other, spawnDelay: 1500, timeBetween: 1000, scale: 0.2, positions: [starburstStart, starburstEnd]);
            }
            this.StarburstLaser(self, other, startTime + 2500, cacheId, numLasers: 4, spawnDelay: 1000, eraseDelay: 1500, posOverride: starburstEnd);
        }

        private int StartRegularPhase(CInstance* self, CInstance* other, int startTime) {
            if (this.scrbp.time(self, other, startTime)) {
                this.scrbp.phase_pattern_remove(self, other);
                this.scrbp.heal(self, other, 1);

                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2, 1080 / 2));
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
            }
            return 2000;
        }
        private bool PhaseChange(CInstance* self, CInstance* other, double hpThreshold) {
            if (this.scrbp.health_threshold(self, other, hpThreshold)) {
                string phase = "bp_wolf_snowfur0_pt2";
                if (this.phasesRemaining.Count > 0) {
                    phase = this.phasesRemaining.Pop();
                }
                RValue[] args = [new RValue(this.rnsReloaded.ScriptFindId(phase))];
                this.rnsReloaded.ExecuteScript("bpatt_add", self, other, args);
                this.scrbp.end(self, other);
                return true;
            }
            return false;
        }

        int[] playerTargets = [0, 0, 0, 0];
        Stack<string> phasesRemaining = [];
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;
            if (this.scrbp.time(self, other, time)) {
                // 4 player check
                if (this.utils.GetNumPlayers() != 4) {
                    this.bp.enrage(self, other, spawnDelay: 3000, timeBetween: 500);
                }

                this.bp.move_position_synced(self, other, duration: 1000, position: (1920 / 2, 1080 / 2));
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
                this.seed = new Random().Next();
                this.playerTargets = [0, 1, 2, 3];
                // if <4p make sure no crash by just targeting player 0
                this.playerTargets = this.playerTargets.Select(x => x >= this.utils.GetNumPlayers() ? 0 : x).ToArray();
                this.rng.Shuffle(this.playerTargets);
                string[] phases = ["bp_wolf_snowfur0_pt3", "bp_wolf_snowfur0_pt4", "bp_wolf_snowfur0_pt5"];
                this.rng.Shuffle(phases);
                this.phasesRemaining = new Stack<string>(phases);

                this.scrbp.set_special_flags(self, other, IBattleScripts.FLAG_HOLMGANG);
            }
            if (this.scrbp.time_repeating(self, other, 0, 500)) {
                this.PhaseChange(self, other, 1.1);
            }

            return returnValue;
        }

        private int Rem0Callback(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            // Yeet into place
            if (this.scrbp.time(self, other, time)) {
                this.rng.Shuffle(this.playerTargets);
                // Top half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (960, 270), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_RED, targetMask: 1 << this.playerTargets[0], eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_0", hbsDuration: 66000, targetMask: 1 << this.playerTargets[0]);
                // Bottom half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (960, 780), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_BLUE, targetMask: 1 << this.playerTargets[1], eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_2", hbsDuration: 66000, targetMask: 1 << this.playerTargets[1]);

                // Left half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (480, 540), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_YELLOW, targetMask: 1 << this.playerTargets[2], eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_3", hbsDuration: 66000, targetMask: 1 << this.playerTargets[2]);

                // Right half
                this.bp.fieldlimit_rectangle_temporary(self, other, position: (1410, 540), width: 5, height: 5, color: IBattlePatterns.FIELDLIMIT_PURPLE, targetMask: 1 << this.playerTargets[3], eraseDelay: 2000);
                this.bp.apply_hbs_synced(self, other, delay: 0, hbs: "hbs_group_1", hbsDuration: 66000, targetMask: 1 << this.playerTargets[3]);
            }
            time += 3000;

            // Setup the long fieldlimits
            if (this.scrbp.time(self, other, time)) {
                // Top half
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (960, 270),
                    width: 1840,
                    height: 470,
                    color: IBattlePatterns.FIELDLIMIT_RED,
                    targetMask: 1 << this.playerTargets[0],
                    eraseDelay: 66000
                );
                // Bottom half
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (960, 780),
                    width: 1840,
                    height: 470,
                    color: IBattlePatterns.FIELDLIMIT_BLUE,
                    targetMask: 1 << this.playerTargets[1],
                    eraseDelay: 66000
                );
                // Left half
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (480, 540),
                    width: 900,
                    height: 1020,
                    color: IBattlePatterns.FIELDLIMIT_YELLOW,
                    targetMask: 1 << this.playerTargets[2],
                    eraseDelay: 66000
                );
                // Right half
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1410, 540),
                    width: 900,
                    height: 1020,
                    color: IBattlePatterns.FIELDLIMIT_PURPLE,
                    targetMask: 1 << this.playerTargets[3],
                    eraseDelay: 66000
                );
            }
            time += 6000;

            // Color match
            if (this.scrbp.time_repeat_times(self, other, time, 20000, 3)) {
                this.scrbp.order_random(self, other, false, 2, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                var group0 = this.rnsReloaded.ArrayGetEntry(orderBin, 0);
                var group1 = this.rnsReloaded.ArrayGetEntry(orderBin, 1);

                this.bp.colormatch(self, other,
                    warningDelay: 3000,
                    warnMsg: 2,
                    spawnDelay: 19000,
                    radius: 200,
                    targetMask: (int) group0->Real,
                    color: IBattlePatterns.COLORMATCH_BLUE
                );
                this.bp.colormatch(self, other,
                    warningDelay: 3000,
                    warnMsg: 2,
                    spawnDelay: 19000,
                    radius: 200,
                    targetMask: (int) group1->Real,
                    color: IBattlePatterns.COLORMATCH_RED
                );
            }

            for (int i = 0; i < 12; i++) {
                time += this.StarburstLaser(self, other, time, this.playerTargets[i % 4], spawnDelay: 3000, eraseDelay: 5000);
            }
            return time - startTime;
        }
        // "pt3", actual time is ~90-100s
        public RValue* StarburstPhase(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;

            this.playerRng = new Random(this.seed);
            time += this.StartRegularPhase(self, other, time);

            time += this.StarburstLaser(self, other, time, target: this.playerTargets[0], numLasers: 3, spawnDelay: 2000, eraseDelay: 3000);
            time += this.StarburstLaser(self, other, time, target: this.playerTargets[1], numLasers: 4, spawnDelay: 2000, eraseDelay: 3000);
            time += this.StarburstLaser(self, other, time, target: this.playerTargets[2], numLasers: 5, spawnDelay: 2000, eraseDelay: 3000);
            time += this.StarburstLaser(self, other, time, target: this.playerTargets[3], numLasers: 6, spawnDelay: 2000, eraseDelay: 3000);
            
            this.StarburstLaser(self, other, time, target: this.playerTargets[0], numLasers: 3, spawnDelay: 2000, eraseDelay: 3000);
            this.StarburstLaser(self, other, time, target: this.playerTargets[1], numLasers: 3, spawnDelay: 2000, eraseDelay: 3000);
            this.StarburstLaser(self, other, time, target: this.playerTargets[2], numLasers: 3, spawnDelay: 2000, eraseDelay: 3000);
            time += this.StarburstLaser(self, other, time, target: this.playerTargets[3], numLasers: 3, spawnDelay: 2000, eraseDelay: 3000);

            // Rotation stuff
            this.StarburstRotate(self, other, time, 0, rot: 100, posOverride: (1920 / 2, 1080 / 2), numLasers: 3);
            if (this.scrbp.time(self, other, time)) {
                this.rng.Shuffle(this.playerTargets);
                this.bp.prscircle(self, other, spawnDelay: 5000, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 6666, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 8333, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 10000, radius: 550, position: this.posSnapshot);
            }
            time += 2000;

            this.StarburstLaser(self, other, time, this.playerTargets[0], numLasers: 3, spawnDelay: 3500, eraseDelay: 4000);
            time += this.StarburstLaser(self, other, time, this.playerTargets[1], numLasers: 3, spawnDelay: 3500, eraseDelay: 4000);

            this.StarburstRotate(self, other, time, 0, spawnDelay: 2000, eraseDelay: 6000, rot: -100, posOverride: (1920 / 2, 1080 / 2), numLasers: 3);
            if (this.scrbp.time(self, other, time)) {
                this.bp.prscircle(self, other, spawnDelay: 1250, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 2500, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 3750, radius: 550, position: this.posSnapshot);
                this.bp.prscircle(self, other, spawnDelay: 5000, radius: 550, position: this.posSnapshot);
            }
            time += 2000;
            this.StarburstLaser(self, other, time, this.playerTargets[2], numLasers: 3, spawnDelay: 3500, eraseDelay: 4000);
            time += this.StarburstLaser(self, other, time, this.playerTargets[3], numLasers: 3, spawnDelay: 3500, eraseDelay: 4000);

            // Mink windmill callback
            double gameSpeed = 1;
            for (int i = 0; i < 4; i++) {
                gameSpeed += 0.4;
                if (this.scrbp.time(self, other, (int) (time - 500 * gameSpeed))) {
                    this.bp.setgamespeed(self, other, (int) (500 * gameSpeed), gameSpeed);
                }
                if (this.scrbp.time(self, other, time)) {
                    this.rng.Shuffle(this.playerTargets);
                    this.bp.circle_spreads(self, other, radius: 200, spawnDelay: 2000, targetMask: 1 << this.playerTargets[0]);
                    this.bp.clockspot(self, other, warningDelay: 1000, warningDelay2: 6000, spawnDelay: 12000, fanAngle: 20, position: (this.myX, this.myY));
                }
                time += 2000;
                time += this.StarburstRotate(self, other, time, this.playerTargets[0], numLasers: 2, spawnDelay: 6000, eraseDelay: 12000, rot: this.playerRng.Next(0, 2) == 1 ? 180 : -180);
            }

            time += this.Rem0Callback(self, other, time);

            // Speed cooldown
            if (this.scrbp.time(self, other, time)) {
                this.rng.Shuffle(this.playerTargets);
            }
            while (gameSpeed > 1) {
                int spawnDelay = (int) (1000 * gameSpeed);
                int eraseDelay = (int) (1666 * gameSpeed);

                this.StarburstLaser(self, other, time, target: this.playerTargets[0], numLasers: 3, spawnDelay: spawnDelay, eraseDelay: eraseDelay);
                this.StarburstLaser(self, other, time, target: this.playerTargets[1], numLasers: 3, spawnDelay: spawnDelay, eraseDelay: eraseDelay);
                this.StarburstLaser(self, other, time, target: this.playerTargets[2], numLasers: 3, spawnDelay: spawnDelay, eraseDelay: eraseDelay);
                time += this.StarburstLaser(self, other, time, target: this.playerTargets[3], numLasers: 3, spawnDelay: spawnDelay, eraseDelay: eraseDelay);
                gameSpeed -= 0.4;
                if (this.scrbp.time(self, other, (int) (time - 500 * gameSpeed))) {
                    this.bp.setgamespeed(self, other, (int) (500 * gameSpeed), gameSpeed);
                }
            }

            if (this.scrbp.time(self, other, time)) {
                this.bp.setgamespeed(self, other, 500, 1);
                if (!this.PhaseChange(self, other, 0.3)) {
                    this.bp.enrage_deco(self, other);
                }
            }
            time += 6000;
            if (this.scrbp.time_repeating(self, other, time, 2000)) {
                this.PhaseChange(self, other, 0.1);
            }
            this.StarburstLaser(self, other, time, this.playerTargets[0], 4, spawnDelay: 2000, eraseDelay: 20000);
            time += 2000;
            this.StarburstLaser(self, other, time, this.playerTargets[1], 5, spawnDelay: 2000, eraseDelay: 18000);
            time += 2000;
            this.StarburstLaser(self, other, time, this.playerTargets[2], 6, spawnDelay: 2000, eraseDelay: 16000);
            time += 2000;
            this.StarburstLaser(self, other, time, this.playerTargets[3], 7, spawnDelay: 2000, eraseDelay: 14000);
            time += 2000;
            this.StarburstLaser(self, other, time, 0, 8, spawnDelay: 2000, eraseDelay: 12000, posOverride: (1920/2, 1080/2));
            time += 2000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other, spawnDelay: 0, timeBetween: 667);
            }
            return returnValue;
        }

        List<((double x, double y) pos, double rot)> cleaves = new List<((double x, double y) pos, double rot)>();
        // "pt4"
        public RValue* JumpCleavePhase(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            this.playerRng = new Random(this.seed);

            int time = 0;
            time += this.StartRegularPhase(self, other, time);

            time += this.DashCleaveWarn(self, other, time, this.playerRng.Next(0, this.utils.GetNumPlayers()), warnTime: 2000);

            time += 1000;

            time += this.AddWarningThorns(self, other, time, spawnDelay: 3000);
            time += this.DashCleave(self, other, time, this.playerTargets[0]);
            time += this.DashCleave(self, other, time, this.playerTargets[1]);
            time += this.DashCleave(self, other, time, this.playerTargets[2]);
            time += this.DashCleave(self, other, time, this.playerTargets[3]);

            time += 1500;

            if (this.scrbp.time(self, other, time)) {
                this.cleaves.Clear();
                this.cleaves = this.cleaves.Concat([
                    ((1920 - 67, 0), 0),
                    ((67, 0), 180),
                ]).ToList();
                this.bp.cleave_fixed(self, other, warnMsg: 0, spawnDelay: 2500, positions: this.cleaves.ToArray());
            }

            time += 1000;
            time += this.AddWarningThorns(self, other, time);
            time += this.DashCleave(self, other, time, this.playerTargets[0], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[1], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[2], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[3], chain: true);

            time += 2500;
            // Levinstrike summoning
            this.AddWarningThorns(self, other, time, spawnDelay: 0, interval: 5000);

            for (int i = 0; i < 4; i++) {
                if (this.scrbp.time(self, other, time)) {
                    int colormatchTarget = 1 << this.playerTargets[(1 + i) % 4] | 1 << this.playerTargets[(3 + i) % 4];
                    (int, int)[] positions = [(500, 200), (500, 1080 - 200), (1920 - 500, 1080 - 200), (1920 - 500, 200)];
                    this.bp.colormatch2(self, other,
                        spawnDelay: 5000,
                        radius: 150,
                        targetMask: colormatchTarget,
                        color: IBattlePatterns.COLORMATCH_BLUE,
                        position: positions[(this.playerRng.Next(0, positions.Length) + i) % 4]
                    );
                    this.bp.circle_spreads(self, other, spawnDelay: 5000, radius: 850, targetMask: 1 << this.playerTargets[(2 + i) % 4]);
                }
                time += 3800;
                time += this.DashCleave(self, other, time, this.playerTargets[i]);
            }

            time += 2000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.prscircle(self, other, spawnDelay: 3000, radius: 350, position: (1920 / 2, 1080 / 2));
                this.bp.circle_spreads(self, other, spawnDelay: 3000 - 600, radius: 200);
            }
            time += this.AddWarningThorns(self, other, time, spawnDelay: 1800);
            for (int i = 0; i < 8; i++) {
                if (this.scrbp.time(self, other, time + 600)) {
                    this.bp.circle_spreads(self, other, spawnDelay: 1200, radius: 200, warnMsg: 2);
                }
                time += this.DashCleave(self, other, time, this.playerTargets[i % 4]);
                if (i != 7 && this.scrbp.time(self, other, time)) {
                    this.bp.prscircle(self, other, spawnDelay: 1200, radius: 350, position: (1920 / 2, 1080 / 2));
                }
            }

            time += 2000;

            if (this.scrbp.time(self, other, time)) {
                this.cleaves.Clear();
                this.cleaves = this.cleaves.Concat([
                    ((1920 - 67, 0), 0),
                    ((0, 1080 - 67), 90),
                    ((67, 0), 180),
                    ((0, 67), 270),
                ]).ToList();
                this.bp.cleave_fixed(self, other, warnMsg: 0, spawnDelay: 2500, positions: this.cleaves.ToArray());
            }

            time += 1000;
            time += this.AddWarningThorns(self, other, time);
            time += this.DashCleave(self, other, time, this.playerTargets[0], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[1], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[2], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[3], chain: true);
            if (this.scrbp.time(self, other, time)) {
                this.bp.showorder(self, other, eraseDelay: 1200 - 700, timeBetween: 1200, orderMasks: (1 << this.playerTargets[0], 1 << this.playerTargets[1], 1 << this.playerTargets[2], 1 << this.playerTargets[3]));
            }
            time += this.DashCleave(self, other, time, this.playerTargets[0], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[1], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[2], chain: true);
            time += this.DashCleave(self, other, time, this.playerTargets[3], chain: true);

            time += 1500;

            // Start soft enrage
            // Add field limit yeet + jump cleave thing?

            if (this.scrbp.time(self, other, time)) {
                this.cleaves.Clear();
                this.bp.enrage_deco(self, other);
                this.bp.move_position_synced(self, other, duration: 500, position: (1920 / 2, 1080 / 2));
            }
            time += 500;
            for (int i = 0; i < 12; i++) {
                int target = this.playerRng.Next(0, this.utils.GetNumPlayers());
                if (this.scrbp.time(self, other, time)) {
                    if (!this.PhaseChange(self, other, 0.35)) {
                        this.bp.thorns_fixed(self, other, warningDelay: 0, warnMsg: 0, spawnDelay: 1500, radius: 150, targetMask: 1 << target, position: (this.myX, this.myY));
                    }
                }
                time += 1500;
                time += this.DashCleave(self, other, time, target, chain: true);
            }

            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other, warningDelay: 0, spawnDelay: 0, timeBetween: 400);
            }
            return returnValue;
        }

        private int MinkRainstormCallback(CInstance* self, CInstance* other, int startTime) {
            int time = startTime;
            if (this.scrbp.time(self, other, time)) {
                this.bp.fieldlimit_rectangle_temporary(self, other,
                    position: (1920 / 2, 1080 / 2),
                    width: 1740,
                    height: 900,
                    color: IBattlePatterns.FIELDLIMIT_WHITE,
                    eraseDelay: 6000
                );
            }
            if (this.scrbp.time_repeat_times(self, other, time, 5000, 4)) {
                // Nonfunctional spread for just the warning message, so players know when fieldlimit decrease
                // snapshot happens
                this.bp.circle_spreads(self, other, warnMsg: 2, spawnDelay: 5000, radius: 0);
            }
            time += 2000;
            this.BubbleLine(self, other, time, 5000 * 3);
            time += 3000;
            if (this.scrbp.time_repeat_times(self, other, time, 5000, 4)) {
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
                    color: IBattlePatterns.FIELDLIMIT_WHITE,
                    eraseDelay: 6000
                );
            }
            time += 5000 * 2;

            time += this.BubbleLineRotating(self, other, time, 5000 * 2, 180);

            return time - startTime;
        }

        // "pt5"
        public RValue* BubbleLinePhase(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            this.playerRng = new Random(this.seed);

            int time = 0;
            time += this.StartRegularPhase(self, other, time);

            time += this.BubbleLine(self, other, time, 8000);
            time += this.BubbleLineRotating(self, other, time, 8000, this.playerRng.Next(0, 2) == 1 ? 180 : -180);

            // KB players 2+2 to top/bot, add line, resolve get in circle
            time += 1000;
            if (this.scrbp.time(self, other, time)) {
                this.scrbp.order_random(self, other, false, 2, 2);
                var orderBin = this.rnsReloaded.FindValue(self, "orderBin");
                int group1 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 0)->Real;
                int group2 = (int) this.rnsReloaded.ArrayGetEntry(orderBin, 1)->Real;

                this.bp.knockback_line(self, other, spawnDelay: 3500, kbAmount: 800, position: (0, 1080), horizontal: true, targetMask: group1);
                this.bp.knockback_line(self, other, spawnDelay: 3500, kbAmount: 800, position: (0, 0), horizontal: true, targetMask: group2);
            }
            time += 1000;
            this.BubbleLine(self, other, time, 10000);
            time += 3000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.prscircle_follow(self, other, warnMsg: 2, spawnDelay: 6000, doubled: true, numBullets: 35, radius: 400, targetId: this.rng.Next(0, this.utils.GetNumPlayers()));
            }
            time += 10000;

            time += this.MinkRainstormCallback(self, other, time);

            // Stacking them gives double the projectile spam. Is it possible? Maybe not, so the 2nd one skips casting sometimes
            this.BubbleLine(self, other, time, 8000);
            time += this.BubbleLine(self, other, time, 8000, skipPercent: 60);

            this.BubbleLine(self, other, time, 25000);
            time += 3000;
            for (int i = 0; i < 25; i++) {
                if (this.scrbp.time(self, other, time)) {
                    this.bp.ray_single(self, other, spawnDelay: 2000, eraseDelay: 2800, width: 100, position: (120 * i, -20), angle: 90);
                    if (i >= 10) {
                        this.bp.ray_single(self, other, spawnDelay: 2000, eraseDelay: 2800, width: 100, position: (120 * (i - 10), -20), angle: 90);
                    }
                }
                time += 1000;
            }

            // Soft enrage
            if (this.scrbp.time(self, other, time)) {
                if (!this.PhaseChange(self, other, 0.2)) {
                    this.bp.enrage_deco(self, other);
                }
            }
            time += 2000;

            if (this.scrbp.time_repeating(self, other, time, 5000)) {
                this.PhaseChange(self, other, 0.2);
            }
            this.BubbleLine(self, other, time, 20000);
            time += 5000;
            this.BubbleLine(self, other, time, 20000);
            time += 5000;
            this.BubbleLine(self, other, time, 20000);
            time += 5000;
            this.BubbleLine(self, other, time, 20000);
            time += 5000;
            // Hard enrage
            if (this.scrbp.time(self, other, time)) {
                if (!this.PhaseChange(self, other, 0.2)) {
                    this.bp.enrage(self, other, spawnDelay: 0, timeBetween: 667);
                }
            }
            
            return returnValue;
        }

        // "pt2"
        public override RValue* FightAltDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            this.playerRng = new Random(this.seed);

            int time = 0;
            time += this.StartRegularPhase(self, other, time);
            if (this.scrbp.time(self, other, time)) {
                this.scrbp.set_special_flags(self, other, IBattleScripts.FLAG_NO_POSITIONAL);
                // Give players elegy (30% more dmg) to help with DPS checks
                this.bp.apply_hbs_synced(self, other, hbs: "hbs_elegy_0", hbsDuration: 99000, targetMask: 0b1111);
            }

            // Starburst + Cleave
            time += this.LimitCut(self, other, time);
            time += 2000;

            // Bubble + Cleave
            if (this.scrbp.time(self, other, time)) {
                this.rng.Shuffle(this.playerTargets);
            }
            this.DashCleaveWarn(self, other, time + 3500, this.playerTargets[0], warnTime: 3000);
            this.VerticalLasers(self, other, time + 2000, 3000, 3000);
            this.DashCleaveWarn(self, other, time + 7000, this.playerTargets[1], warnTime: 3000);
            this.VerticalLasers(self, other, time + 5500, 3000, 3000);
            time += this.BubbleLine(self, other, time, 9000);
            this.DashCleaveWarn(self, other, time + 3500, this.playerTargets[2], warnTime: 3000);
            this.VerticalLasers(self, other, time + 2000, 3000, 3000);
            this.DashCleaveWarn(self, other, time + 7000, this.playerTargets[3], warnTime: 3000);
            this.VerticalLasers(self, other, time + 5500, 3000, 3000);
            time += this.BubbleLine(self, other, time, 9000);
            time += 2000;

            // Starburst + Bubble
            this.StarburstBubble(self, other, time + 3500, 0);
            this.StarburstBubble(self, other, time + 5100, 1);
            this.StarburstBubble(self, other, time + 6200, 2);
            this.StarburstBubble(self, other, time + 6800, 3);
            this.StarburstBubble(self, other, time + 8000, 4);
            this.StarburstBubble(self, other, time + 8000, 5);

            time += this.BubbleLine(self, other, time, 9000);
            time += 2000;

            // All together now
            if (this.scrbp.time(self, other, time)) {
                this.myX = 1920 / 2;
                this.myY = 1080 / 2;
                this.bp.move_position_synced(self, other, duration: 1000, position: (this.myX, this.myY));
            }
            time += 2000;

            this.AddWarningThorns(self, other, time, spawnDelay: 6000);
            time += 3000;
            if (this.scrbp.time(self, other, time)) {
                this.bp.circle_spreads(self, other, warnMsg: 1, spawnDelay: 4200, radius: 300);
            }
            this.BubbleLine(self, other, time, 5000, skipPercent: 50);
            time += 3100;
            for (int i = 0; i < 4; i++) {
                this.StarburstLaser(self, other, time, this.playerTargets[(2 + i) % 4], spawnDelay: 1200, eraseDelay: 2200);
                this.DashCleave(self, other, time, this.playerTargets[i % 4]);
                time += 1200;
            }
            time += 2000;

            // More advanced mechanics in duos
            // Starburst + Cleave advanced
            time += this.AddWarningThorns(self, other, time);
            for (int i = 0; i < 4; i++) {
                this.StarburstLaser(self, other, time, this.playerTargets[(2 + i) % 4], spawnDelay: 1200, eraseDelay: 1200 * (6 - i));
                this.DashCleave(self, other, time, this.playerTargets[i % 4]);
                time += 1200;
            }
            time += 4000;

            // Bubble + Cleave advanced
            if (this.scrbp.time(self, other, time)) {
                this.cleaves.Clear();
                this.cleaves = this.cleaves.Concat([
                    ((1920 - 67, 0), 0),
                    ((67, 0), 180),
                ]).ToList();
                this.bp.cleave_fixed(self, other, warnMsg: 0, spawnDelay: 2500, positions: this.cleaves.ToArray());
            }
            time += 2000;
            this.BubbleLineRotating(self, other, time, 1200 * 9, this.playerRng.Next(0, 2) == 1 ? 360 : -360);
            time += this.AddWarningThorns(self, other, time, spawnDelay: 3500);
            for (int i = 0; i < 8; i++) {
                this.DashCleave(self, other, time, this.playerTargets[i % 4], chain: i < 4);
                time += 1200;
            }
            time += 2500;

            // Starburst + Bubble advanced
            int lineRot = this.playerRng.Next(0, 2) == 1 ? 180 : -180;
            int bubbleDuration = 10000;
            this.BubbleLineRotating(self, other, time, bubbleDuration, lineRot);
            time += 3000; // Bubble line intro
            this.StarburstBubble(self, other, time +  500, 0, fromEdge: true);
            this.StarburstBubble(self, other, time + 1500, 1, fromEdge: true);
            this.StarburstBubble(self, other, time + 2500, 2, fromEdge: true);
            this.StarburstBubble(self, other, time + 3500, 3, fromEdge: true);
            this.StarburstBubble(self, other, time + 4500, 4, fromEdge: true);
            this.StarburstBubble(self, other, time + 5400, 5, fromEdge: true);
            this.StarburstBubble(self, other, time + 6200, 6, fromEdge: true);
            this.StarburstBubble(self, other, time + 6900, 7, fromEdge: true);
            this.StarburstBubble(self, other, time + 7500, 8, fromEdge: true);
            this.StarburstBubble(self, other, time + 8000, 9, fromEdge: true);
            time += bubbleDuration;
            time += 3000;

            // More advanced mechanics combined in trio
            if (this.scrbp.time(self, other, time)) {
                this.cleaves.Clear();
                this.cleaves = this.cleaves.Concat([
                    ((1920 - 67, 0), 0),
                    ((0, 1080 - 67), 90),
                    ((67, 0), 180),
                    ((0, 67), 270),
                ]).ToList();
                this.bp.cleave_fixed(self, other, warnMsg: 0, spawnDelay: 2500, positions: this.cleaves.ToArray());
            }
            this.BubbleLine(self, other, time, 10000, skipPercent: 30);
            time += this.AddWarningThorns(self, other, time, spawnDelay: 3250, interval: 3000);
            for (int i = 0; i < 8; i++) {
                this.DashCleave(self, other, time, this.playerTargets[i % 4], chain: true);
                this.StarburstLaser(self, other, time + 100, this.playerTargets[i % 4], spawnDelay: 1500, eraseDelay: 2000);
                time += 3000;
            }
            time += 1000;

            if (this.scrbp.time(self, other, time)) {
                this.bp.enrage(self, other);
            }
            return returnValue;
        }
    }
}
