using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechanicPack;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Runtime.InteropServices;

namespace RnsReloaded.FuzzyMechanicPack {
    // Handles the mechanic that allows color match colors to be changed mid-mechanic
    internal unsafe class ColorMatchSwap : BpGenerator {
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IUtil utils;
        private IBattleScripts scrbp;
        private IHook<ScriptDelegate> colormatchHook;

        public ColorMatchSwap(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) : base(rnsReloaded) {
            this.rnsReloaded = rnsReloaded;
            this.logger = logger;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;

            var colormatchScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_colormatch") - 100000);
            this.colormatchHook =
                hooks.CreateHook<ScriptDelegate>(this.ColormatchDetour, colormatchScript->Functions->Function);
            this.colormatchHook.Activate();
            this.colormatchHook.Enable();
        }

        public void Run(CInstance* self, CInstance* other,
            int numColors = 2,
            int? warningDelay = null,
            int? spawnDelay = null,
            int? matchRadius = null,
            int? setRadius = null,
            int? warnMsg = null,
            int? displayNumber = null,
            (int x, int y)?[]? setCircles = null,
            (int x, int y)?[]? matchCircles = null,
            int[]? targetMask = null,
            int[]? colors = null
        ) {
            RValue[] args = [this.utils.CreateString("type")!.Value, new RValue(1)];
            args = this.add_if_not_null(args, "number", numColors);
            args = this.add_if_not_null(args, "warningDelay", warningDelay);
            args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
            args = this.add_if_not_null(args, "radius", matchRadius);
            args = this.add_if_not_null(args, "amount", setRadius);
            args = this.add_if_not_null(args, "warnMsg", warnMsg);
            args = this.add_if_not_null(args, "displayNumber", displayNumber);
            for (int i = 0; i < numColors; i++) {
                if (setCircles != null && setCircles.Length > i && setCircles[i] != null) {
                    args = this.add_if_not_null(args, "posX_" + i, setCircles[i]!.Value.x);
                    args = this.add_if_not_null(args, "posY_" + i, setCircles[i]!.Value.y);

                }
                if (matchCircles != null && matchCircles.Length > i && matchCircles[i] != null) {
                    args = this.add_if_not_null(args, "offX_" + i, matchCircles[i]!.Value.x);
                    args = this.add_if_not_null(args, "offY_" + i, matchCircles[i]!.Value.y);
                }
                if (targetMask != null && targetMask.Length > i) {
                    args = this.add_if_not_null(args, "orderBin_" + i, targetMask[i]);
                }
                if (colors != null && colors.Length > i) {
                    args = this.add_if_not_null(args, "playerId_" + i, colors[i]);
                }
            }

            this.execute_pattern(self, other, "bp_colormatch", args);
        }

        private CLayer* FindLayer(string layerName = "") {
            var layers = this.rnsReloaded.GetCurrentRoom()->Layers;
            CLayer* curLayer = layers.First;
            CLayer* searchLayer = null;
            while (curLayer != null) {
                // If no layer name, then we want to find the max layer ID
                if (layerName == "") {
                    if (searchLayer == null) {
                        searchLayer = curLayer;
                    } else if (curLayer->ID >= searchLayer->ID) {
                        searchLayer = curLayer;
                    }
                    // If a layer name is provided, break as soon as we find it
                } else {
                    if (Marshal.PtrToStringAnsi((nint) curLayer->Name)! == layerName) {
                        searchLayer = curLayer;
                        break;
                    }
                }
                curLayer = curLayer->Next;
            }
            return searchLayer;
        }
        private CLayerInstanceElement* GetMostRecentObjectFromLayer(string layerName = "") {
            var layers = this.rnsReloaded.GetCurrentRoom()->Layers;
            CLayer* searchLayer = this.FindLayer(layerName);

            if (searchLayer != null) {
                CLayerElementBase* elem = searchLayer->Elements.First;
                CLayerElementBase* maxElem = elem;
                while (elem != null) {
                    if (elem->ID > maxElem->ID) {
                        maxElem = elem;
                    }
                    elem = elem->Next;
                }
                if (maxElem == null) {
                    this.logger.PrintMessage("Layer " + layerName + " has no elements", this.logger.ColorRed);
                }
                return (CLayerInstanceElement*) maxElem;
            } else {
                this.logger.PrintMessage("Failed to find layer with name " + layerName, this.logger.ColorRed);
            }

            return null;
        }

        private int GenerateShiftingColor(int time) {
            const int cycleTime = 3000; // Do a full rotation every 6000ms
            time %= cycleTime; 
            int red = 0, green = 0, blue = 0;

            if (time < cycleTime / 3) {
                // linearize between red and green
                red = 255 - (255 * time / (cycleTime / 3));
                green = 255 * time / (cycleTime / 3);
            } else if (time < 2 * cycleTime / 3) {
                time -= cycleTime / 3;
                // linearize between green and blue
                green = 255 - (255 * time / (cycleTime / 3));
                blue = 255 * time / (cycleTime / 3);
            } else {
                time -= 2 * (cycleTime / 3);
                // linearize between blue and red
                blue = 255 - (255 * time / (cycleTime / 3));
                red = 255 * time / (cycleTime / 3);
            }
            return blue + green * 256 + red * 256 * 256;
        }

        private RValue* ColormatchDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            long type = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "type", new RValue(0)));

            if (type == 1) {
                // API:
                //   type: set this to 1 to enable this variant. 0 will always be default colormatch.
                //         Higher values may be used for future variants
                //   number: number of colors to use (up to 4 supported)
                //   warningDelay, spawnDelay, radius, warnMsg, displayNumber as normal
                //         (displayNumber displays on ALL circles, damage and set)
                //   posX_<i>, posY_<i>: coordinates for circle to set color to i
                //   offX_<i>, offY_<i>: coordinates for matching color i (set to -1, -1 to disable)
                //   orderBin_<i>: target mask for color i. There should be no bits enabled in more than one of these
                //   playerId_<i>: color i (used as element, since there's no indexed element variable)
                //   amount: radius of the circles that set your color (since there's no radius2 variable)

                int numColors = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "number", new RValue(2)));

                int warningDelay   = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warningDelay",  new RValue(0)));
                int spawnDelay     = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "spawnDelay",    new RValue(3000)));
                int radius         = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "radius",        new RValue(400)));
                int setRadius      = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "amount",        new RValue(radius / 2)));
                int warnMsg        = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warnMsg",       new RValue(0)));
                int displayNumber  = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "displayNumber", new RValue(0)));

                int[] trgBinary = new int[numColors];
                (int x, int y)[] setCirclePositions = new (int x, int y)[numColors];
                (int x, int y)[] damageCirclePositions = new (int x, int y)[numColors];
                int[] colorIds = new int[numColors];
                for (int i = 0; i < numColors; i++) {
                    trgBinary[i] = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "orderBin_" + i, new RValue(i == 0 ? 127 : 0)));
                    setCirclePositions[i].x = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "posX_" + i, new RValue(1920/ (numColors + 1) * (i + 1))));
                    setCirclePositions[i].y = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "posY_" + i, new RValue(1080/2)));

                    damageCirclePositions[i].x = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "offX_" + i, new RValue(-1)));
                    damageCirclePositions[i].y = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "offY_" + i, new RValue(-1)));

                    colorIds[i] = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "playerId_" + i, new RValue(IBattlePatterns.COLORMATCH_PURPLE + i)));
                }

                // Make sure that no target binaries overlap - each player can only be a single color at once
                int accumOrderBin = 0;
                for (int i = 0; i < numColors; i++) {
                    if ((accumOrderBin & trgBinary[i]) > 0) {
                        this.logger.PrintMessage("Fuzzy's colormatch type 1 error: a player is multiple colors. trgBinary[]=" + string.Join(", ", trgBinary), this.logger.ColorRed);
                        this.scrbp.end(self, other);
                        return returnValue;
                    }
                    accumOrderBin |= trgBinary[i];
                }

                // Basic setup. Normal colormatch sets pattern color here too, but we'll be changing that quite often.
                if (this.scrbp.time(self, other, 0)) {
                    this.scrbp.pattern_set_projectile_key(self, other, "pjb_dark", "sfxset_enemy");
                    this.scrbp.pattern_set_drawlayer(self, other, 1);
                }

                // Warning creation
                if (this.scrbp.time(self, other, warningDelay)) {
                    int timeToSpawn = spawnDelay - warningDelay;
                    int numCircles = 0;
                    int avgPos = 0;

                    // Skip everything if it's already time to damage players
                    if (timeToSpawn > 0) {
                        // Create warnings for color i
                        for (int i = 0; i < numColors; i++) {
                            this.scrbp.pattern_set_color_colormatch(self, other, colorIds[i]);

                            // Create color setting circle - element has a +5 because that causes a stationary circle with the same symbol
                            this.scrbp.make_warning_colormatch(self, other, setCirclePositions[i].x, setCirclePositions[i].y, setRadius, colorIds[i] + 5, timeToSpawn);

                            // Background (set to black)
                            var background = this.GetMostRecentObjectFromLayer("BattleWarningMid");
                            var color = this.rnsReloaded.FindValue(background->Instance, "color");
                            color->Real = 0;
                            color->Type = RValueType.Real;

                            // Ring (set to main color to black, keep secondary color)
                            var ring = this.GetMostRecentObjectFromLayer("BattleWarningOver");
                            color = this.rnsReloaded.FindValue(ring->Instance, "color");

                            var color2 = this.rnsReloaded.FindValue(ring->Instance, "color2");
                            color2->Real = this.utils.RValueToDouble(color);
                            color2->Type = RValueType.Real;

                            color->Real = 0xFFFFFF;
                            color->Type = RValueType.Real;

                            // Inside symbol (set to white)
                            var insideSymbol = this.GetMostRecentObjectFromLayer("BattleWarningUnder");
                            color = this.rnsReloaded.FindValue(insideSymbol->Instance, "color");
                            color->Real = 0xFFFFFF;
                            color->Type = RValueType.Real;

                            this.scrbp.warning_msg_pos(self, other, setCirclePositions[i].x, setCirclePositions[i].y - radius + 120, "eff_colormatch", warnMsg, timeToSpawn);

                            if (displayNumber > 0) {
                                this.scrbp.make_number_warning(self, other, setCirclePositions[i].x, setCirclePositions[i].y, displayNumber, timeToSpawn);
                            }

                            numCircles++;
                            avgPos += setCirclePositions[i].x;

                            // Create color damage circle
                            if (damageCirclePositions[i].x != -1 || damageCirclePositions[i].y != -1) {
                                // A regular display setup, without any weird value changing. Wow, this is so much simpler
                                this.scrbp.make_warning_colormatch(self, other, damageCirclePositions[i].x, damageCirclePositions[i].y, radius, colorIds[i], timeToSpawn);
                                if (displayNumber > 0) {
                                    this.scrbp.make_number_warning(self, other, damageCirclePositions[i].x, damageCirclePositions[i].y, displayNumber, timeToSpawn);
                                }
                                numCircles++;
                                avgPos += damageCirclePositions[i].x;
                            }

                            // Create player rings
                            for (int playerId = 0; playerId < this.utils.GetNumPlayers(); playerId++) {
                                // skipping players not in the accumulated target list so that random players don't accidentally get a color
                                if ((accumOrderBin & (1 << playerId)) > 0) {
                                    this.scrbp.make_warning_colormatch_targ(self, other, playerId, radius, colorIds[i], timeToSpawn);
                                    background = this.GetMostRecentObjectFromLayer("BattleWarningMid");

                                    // Save first generated color background to set later. Rest don't matter since they're just black
                                    if (i == 0) {
                                        this.scrbp.sbsv(self, other, "player_" + playerId + "_bg", new RValue(background->Instance));
                                    } else {
                                        // BG radius to 0 on all but the first one, for clearer display of bg
                                        var rad = this.rnsReloaded.FindValue(background->Instance, "radius");
                                        rad->Real = 0;
                                        rad->Type = RValueType.Real;
                                    }

                                    // Save ALL rings as we'll be setting their radius later on to make them appear/disappear
                                    // We create multiple rings per player instead of changing the colors of one
                                    // because I don't want to have to figure out the exact color values to set them to,
                                    // which will change based off their color setting. Ew, just make it easy on me
                                    ring = this.GetMostRecentObjectFromLayer("BattleEffect");
                                    this.scrbp.sbsv(self, other, "player_" + playerId + "_ring_" + i, new RValue(ring->Instance));

                                    // Since we create multiple circles per player, only count one when figuring out the sound x-coordinate
                                    // and when adding the warning message
                                    if (i == 0) {
                                        numCircles++;
                                        avgPos += (int) this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevX"));
                                        this.scrbp.warning_msg_t(self, other, playerId, "eff_colormatch", warnMsg, timeToSpawn);

                                    }
                                }
                            }
                        }

                        // This will almost always be true, but good to be safe just in case
                        if (numCircles > 0) {
                            this.scrbp.sound_x(self, other, avgPos / numCircles);
                            // Args taken from mino's code. I don't know what they mean.
                            this.scrbp.sound(self, other, 1, 0);
                        }
                    }
                }

                // Update loop, called every frame
                if (this.scrbp.time_repeating(self, other, warningDelay, 100)) {
                    // Update each color
                    for (int i = 0; i < numColors; i++) {
                        // Update each player
                        for (int playerId = 0; playerId < this.utils.GetNumPlayers(); playerId++) {
                            // If player not targeted at all, skip this iteration
                            if ((accumOrderBin & (1 << playerId)) == 0) {
                                continue;
                            }
                            int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevX"));
                            int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevY"));
                            // Check if player is standing in this color change circle. If so, change their color
                            if (Math.Pow(playerX - setCirclePositions[i].x, 2) + Math.Pow(playerY - setCirclePositions[i].y, 2) < Math.Pow(setRadius, 2)) {
                                // Remove player from their current color
                                for (int removeIndex = 0; removeIndex < numColors; removeIndex++) {
                                    trgBinary[removeIndex] &= ~(1 << playerId);
                                    this.scrbp.sbsv(self, other, "orderBin_" + removeIndex, new RValue(trgBinary[removeIndex]));
                                }
                                // Add them to the new color
                                trgBinary[i] |= 1 << playerId;
                                this.scrbp.sbsv(self, other, "orderBin_" + i, new RValue(trgBinary[i]));
                            }

                            var ring = this.scrbp.sbgv(self, other, "player_" + playerId + "_ring_" + i, new RValue(0));
                            if (ring.Int64 != 0) {
                                var rad = this.rnsReloaded.FindValue(ring.Object, "radius");
                                bool isColorActive = (trgBinary[i] & (1 << playerId)) > 0;
                                if (isColorActive) {
                                    rad->Int64 = radius;
                                } else {
                                    // We can't entirely make it not drawn so we set the radius to ridiculously large
                                    rad->Int64 = 20000;
                                }
                                rad->Type = RValueType.Int64;
                            }
                            // Update bg color
                            var bg = this.scrbp.sbgv(self, other, "player_" + playerId + "_bg", new RValue(0));
                            if (bg.Int64 != 0) {
                                var color = this.rnsReloaded.FindValue(bg.Object, "color");
                                int patternTime = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "patternExTime"));
                                color->Int64 = this.GenerateShiftingColor(patternTime);
                                color->Type = RValueType.Int64;
                            }
                        }

                    }
                }

                // Activate
                if (this.scrbp.time(self, other, spawnDelay)) {
                    int avgPos = 0;
                    int numCircles = 0;

                    var forceLocal = this.rnsReloaded.FindValue(self, "networkForceLocal");
                    forceLocal->Real = 1.0;
                    forceLocal->Type = RValueType.Bool;

                    // Used later on when calling bpatt_var to damage players if needed
                    RValue trgBinName = new RValue(0);
                    RValue radiusName = new RValue(0);
                    RValue numPoints = new RValue(0);
                    this.rnsReloaded.CreateString(&trgBinName, "trgBinary");
                    this.rnsReloaded.CreateString(&radiusName, "radius");
                    this.rnsReloaded.CreateString(&numPoints, "numPoints");

                    // Each color is handled like an entirely separate colormatch, except for the sound
                    for (int i = 0; i < numColors; i++) {
                        this.scrbp.pattern_set_color_colormatch(self, other, colorIds[i]);

                        int circlesThisColor = 0;

                        var addBurst = (int x, int y) => {
                            RValue posX = new RValue(0);
                            RValue posY = new RValue(0);
                            this.rnsReloaded.CreateString(&posX, "posX_" + circlesThisColor);
                            this.rnsReloaded.CreateString(&posY, "posY_" + circlesThisColor);
                            this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [posX, new RValue(x), posY, new RValue(y)]);

                            this.scrbp.make_warning_colormatch_burst(self, other, x, y, radius, colorIds[i]);

                            avgPos += x;
                            numCircles++;
                            circlesThisColor++;
                        };

                        // Add burst animations for players, if they have this color
                        for (int playerId = 0; playerId < this.utils.GetNumPlayers(); playerId++) {
                            if ((trgBinary[i] & (1 << playerId)) > 0) {
                                int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevX"));
                                int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevY"));

                                addBurst(playerX, playerY);
                            }
                        }
                        // Add burst animation for damaging circle
                        if (damageCirclePositions[i].x != -1 || damageCirclePositions[i].y != -1) {
                            addBurst(damageCirclePositions[i].x, damageCirclePositions[i].y);
                        }

                        // Setup inverse trgBinary, radius, num circles
                        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [trgBinName, new RValue(127 - trgBinary[i]), radiusName, new RValue(radius), numPoints, new RValue(circlesThisColor)]);
                        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, [new RValue(this.rnsReloaded.ScriptFindId("bp_colormatch_activate"))]);

                        // Set new trgBinary
                        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [trgBinName, new RValue(trgBinary[i])]);
                        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, [new RValue(this.rnsReloaded.ScriptFindId("bp_colormatch_activate_donut"))]);
                        // Reset vars for next color
                        this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
                    }

                    if (numCircles > 0) {
                        this.scrbp.sound_x(self, other, avgPos / numCircles);
                        this.scrbp.sound(self, other, 1, 1);
                    }

                    this.scrbp.end(self, other);
                }
            } else {
                return this.colormatchHook.OriginalFunction(self, other, returnValue, argc, argv);
            }
            return returnValue;
        }
    }
}
