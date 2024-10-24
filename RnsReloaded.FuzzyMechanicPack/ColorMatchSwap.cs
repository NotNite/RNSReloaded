using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RnsReloaded.FuzzyMechanicPack {
    // Handles the mechanic that allows color match colors to be changed mid-mechanic
    internal unsafe class ColorMatchSwap {
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IUtil utils;
        private IBattleScripts scrbp;

        // Key is the pointer value of the self object of the color match.
        private Dictionary<nint, int> playerStates;

        private IHook<ScriptDelegate> bulletClearHook;
        private IHook<ScriptDelegate> colormatchHook;

        public ColorMatchSwap(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) {
            this.rnsReloaded = rnsReloaded;
            this.logger = logger;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;

            this.playerStates = new Dictionary<nint, int>();

            var bulletScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_erase_radius") - 100000);
            this.bulletClearHook =
                hooks.CreateHook<ScriptDelegate>(this.BulletClearDetour, bulletScript->Functions->Function);
            this.bulletClearHook.Activate();
            this.bulletClearHook.Enable();

            var colormatchScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_colormatch") - 100000);
            this.colormatchHook =
                hooks.CreateHook<ScriptDelegate>(this.ColormatchDetour, colormatchScript->Functions->Function);
            this.colormatchHook.Activate();
            this.colormatchHook.Enable();
        }

        private RValue* BulletClearDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            var dataId = this.rnsReloaded.FindValue(self, "dataId");
            var dataMap = this.utils.GetGlobalVar("itemData");
            var moveName = dataMap->Get((int) this.utils.RValueToLong(dataId))->Get(0)->Get(0)->ToString();

            if (moveName != "mv_defender_2") {
                this.logger.PrintMessage("Wow defensived!", this.logger.ColorRed);

                var layers = this.rnsReloaded.GetCurrentRoom()->Layers;

                var layer = layers.First;
                var maxLayer = layers.First;
                while (layer != null) {
                    if (layer->ID > maxLayer->ID) { //Marshal.PtrToStringAnsi((nint) layer->Name)! == "BattleWarningMid") {
                        maxLayer = layer;
                    } else {
                        layer = layer->Next;
                    }
                }

                if (maxLayer != null) {
                    this.logger.PrintMessage("Yay found layer " + maxLayer->ID, this.logger.ColorRed);
                    CLayerElementBase* elem = maxLayer->Elements.First;
                    if (elem != null) {
                        var instance = (CLayerInstanceElement*) elem;
                        var instanceValue = new RValue(instance->Instance);

                        var color = this.rnsReloaded.FindValue(instance->Instance, "color");
                        color->Real = 0;
                        color->Type = RValueType.Real;
                    }
                }
            }

            returnValue = this.bulletClearHook.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
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
        private RValue* ColormatchDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            long type = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "type", new RValue(0)));

            if (type == 1 || true) {
                int warningDelay   = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warningDelay",  new RValue(0)));
                int spawnDelay     = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "spawnDelay",    new RValue(3000)));
                int radius         = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "radius",        new RValue(400)));
                int trgBinary      = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "trgBinary",     new RValue(0b1111)));
                int warnMsg        = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warnMsg",       new RValue(0)));
                int element        = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "element",       new RValue(IBattlePatterns.COLORMATCH_PURPLE)));
                int hasFixed       = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "hasFixed",      new RValue(0)));
                int xPos           = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "x",             new RValue(0)));
                int yPos           = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "y",             new RValue(0)));
                int displayNumber  = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "displayNumber", new RValue(0)));

                int numColors = 2;
                if (this.scrbp.time(self, other, 0)) {
                    this.scrbp.pattern_set_projectile_key(self, other, "pjb_dark", "sfxset_enemy");
                    this.scrbp.pattern_set_drawlayer(self, other, 1);

                    this.scrbp.pattern_set_color_colormatch(self, other, element);

                    this.scrbp.sbsv(self, other, "trgBinary2", new RValue(0));
                }
                if (this.scrbp.time(self, other, warningDelay)) {
                    int timeToSpawn = spawnDelay - warningDelay;
                    int numCircles = 0;
                    int avgPos = 0;

                    if (timeToSpawn > 0) {
                        if (hasFixed != 0) {
                            // Set starting element to 99
                            this.scrbp.make_warning_colormatch(self, other, xPos, yPos, radius, 99, timeToSpawn);

                            // Background (set to black)
                            var background = this.GetMostRecentObjectFromLayer("BattleWarningMid");
                            
                            var color = this.rnsReloaded.FindValue(background->Instance, "color");
                            color->Real = 0;
                            color->Type = RValueType.Real;

                            // Ring (set to bright white)
                            var ring = this.GetMostRecentObjectFromLayer("BattleWarningOver");
                            
                            color = this.rnsReloaded.FindValue(ring->Instance, "color");
                            color->Real = 0;
                            color->Type = RValueType.Real;

                            var color2 = this.rnsReloaded.FindValue(ring->Instance, "color2");
                            color2->Real = 0xFFFFFF;
                            color2->Type = RValueType.Real;

                            // Inside symbol (remove entirely)
                            var insideSymbol = this.GetMostRecentObjectFromLayer("BattleWarningUnder");
                            var alpha = this.rnsReloaded.FindValue(insideSymbol->Instance, "alpha");
                            alpha->Real = 0;
                            alpha->Type = RValueType.Real;

                            this.scrbp.warning_msg_pos(self, other, xPos, yPos - radius + 120, "eff_colormatch", warnMsg, timeToSpawn);

                            if (displayNumber > 0) {
                                this.scrbp.make_number_warning(self, other, xPos, yPos, displayNumber, timeToSpawn);
                            }

                            numCircles++;
                            avgPos += xPos;
                        }

                        for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                            // Player is targeted
                            if ((trgBinary & (1 << i)) > 0) {
                                for (int colorIndex = 0; colorIndex < numColors; colorIndex++) {
                                    this.scrbp.pattern_set_color_colormatch(self, other, element + colorIndex);

                                    this.scrbp.make_warning_colormatch_targ(self, other, i, 20000 + colorIndex * 10 + i, element + colorIndex, timeToSpawn);
                                    var background = this.GetMostRecentObjectFromLayer("BattleWarningMid");

                                    var color = this.rnsReloaded.FindValue(background->Instance, "color");
                                    color->Real = 0;
                                    color->Type = RValueType.Real;

                                    var rad = this.rnsReloaded.FindValue(background->Instance, "radius");
                                    rad->Real = colorIndex == 0 ? radius : 0;
                                    rad->Type = RValueType.Real;

                                    if (colorIndex == 0) {
                                        this.scrbp.sbsv(self, other, "player_" + i + "_bg", new RValue(background->Instance));
                                    }
                                    // TODO: figure out what's going on here
                                    // The new layer isn't added until after colormatch resolves
                                    var ring = this.GetMostRecentObjectFromLayer();

                                    color = this.rnsReloaded.FindValue(ring->Instance, "color");
                                    color->Real = 0;
                                    color->Type = RValueType.Real;
                                }

                                this.scrbp.warning_msg_t(self, other, i, "eff_colormatch", warnMsg, timeToSpawn);

                                numCircles++;
                                avgPos += (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevX"));
                            }
                        }

                        if (numCircles > 0) {
                            this.scrbp.sound_x(self, other, avgPos / numCircles);
                            // Args taken from mino's code. I don't know what they mean.
                            this.scrbp.sound(self, other, 1, 0);
                        }
                    }
                }
                // Update loop
                if (this.scrbp.time_repeating(self, other, warningDelay + 100, 1000)) {
                    this.scrbp.pattern_set_color_colormatch(self, other, 1);

                    this.scrbp.warning_msg_pos(self, other, xPos, yPos + radius - 120, "Colors Swapping!", 1, 1000);
                    int trgBinary2 = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "trgBinary2", new RValue(0)));
                    for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                        int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevX"));
                        int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevY"));

                        // Update bg flashing
                        var bg = this.scrbp.sbgv(self, other, "player_" + i + "_bg", new RValue(0));
                        if (bg.Int64 != 0) {
                            var color = this.rnsReloaded.FindValue(bg.Object, "color");
                            color->Int64 = (this.utils.RValueToLong(color) * 10 + 7) % 0xFFFFFF;
                            color->Type = RValueType.Int64;
                        }

                        // If player is inside fixed ring, swap their color
                        if ((trgBinary & (1 << i)) > 0 && (Math.Pow(playerX - xPos, 2) + Math.Pow(playerY - yPos, 2) < Math.Pow(radius, 2))) {
                            trgBinary2 ^= 1 << i;
                            this.scrbp.sbsv(self, other, "trgBinary2", new RValue(trgBinary2));
                        }

                        for (int colorIndex = 0; colorIndex < numColors; colorIndex++) {
                            var ring = this.scrbp.sbgv(self, other, "player_" + i + "_ring_" + colorIndex, new RValue(0));
                            if (ring.Int64 != 0) {
                                var rad = this.rnsReloaded.FindValue(ring.Object, "radius");
                                bool isColorSwapped = (trgBinary2 & (1 << i)) > 0;
                                if (colorIndex == 0) {
                                    if (isColorSwapped) {
                                        rad->Int64 = 19999;
                                    } else {
                                        rad->Int64 = radius;
                                    }
                                } else {
                                    if (isColorSwapped) {
                                        rad->Int64 = radius;
                                    } else {
                                        rad->Int64 = 19999;
                                    }
                                }
                                rad->Type = RValueType.Int64;
                            } else {
                                CLayer* layer = this.FindLayer();
                                CLayerElementBase* elem = layer->Elements.First;

                                while (elem != null) {
                                    CLayerInstanceElement* instance = (CLayerInstanceElement*) elem;
                                    RValue* rad = this.rnsReloaded.FindValue(instance->Instance, "radius");
                                    if (rad != null && this.utils.RValueToLong(rad) == 20000 + colorIndex * 10 + i) {
                                        this.scrbp.sbsv(self, other, "player_" + i + "_ring_" + colorIndex, new RValue(instance->Instance));
                                        rad->Int64 = radius;
                                        rad->Type = RValueType.Int64;
                                        break;
                                    }
                                    elem = elem->Next;
                                }
                            }
                        }
                    }
                }
                if (this.scrbp.time(self, other, spawnDelay)) {
                    int avgPos = 0;
                    int numCircles = 0;

                    var forceLocal = this.rnsReloaded.FindValue(self, "networkForceLocal");
                    forceLocal->Real = 1.0;
                    forceLocal->Type = RValueType.Bool;

                    int inverseTarget = 127 - trgBinary;

                    for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                        var addBurst = (int x, int y, int element) => {
                            RValue posX = new RValue(0);
                            RValue posY = new RValue(0);
                            this.rnsReloaded.CreateString(&posX, "posX_" + numCircles);
                            this.rnsReloaded.CreateString(&posY, "posY_" + numCircles);
                            this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [posX, new RValue(x), posY, new RValue(y)]);

                            this.scrbp.make_warning_colormatch_burst(self, other, x, y, radius, element);
                            avgPos += x;
                            numCircles++;
                        };

                        if ((trgBinary & (1 << i)) > 0) {
                            int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevX"));
                            int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevY"));

                            addBurst(playerX, playerY, element);
                        }

                        if (hasFixed != 0 && false) {
                            // TOOD: remove this for final version
                            addBurst(xPos, yPos, 1);

                            var ring = this.GetMostRecentObjectFromLayer("BattleWarningMid");
                            var color = this.rnsReloaded.FindValue(ring->Instance, "color");
                            color->Real = 0;
                            color->Type = RValueType.Real;
                        }

                        if (numCircles > 0) {
                            this.scrbp.sound_x(self, other, avgPos / numCircles);
                            this.scrbp.sound(self, other, 1, 1);
                        }

                    }
                    // TODO: make these call for each color, with the correct trgBinary values

                    // Setup inverse trgBinary
                    RValue trgBinName = new RValue(0);
                    RValue radiusName = new RValue(0);
                    RValue numPoints = new RValue(0);
                    this.rnsReloaded.CreateString(&trgBinName, "trgBinary");
                    this.rnsReloaded.CreateString(&radiusName, "radius");
                    this.rnsReloaded.CreateString(&numPoints, "numPoints");
                    this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [trgBinName, new RValue(inverseTarget), radiusName, new RValue(radius), numPoints, new RValue(numCircles)]);
                    
                    this.rnsReloaded.ExecuteScript("bpatt_add", self, other, [new RValue(this.rnsReloaded.ScriptFindId("bp_colormatch_activate"))]);

                    // Set new trgBinary
                    this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [trgBinName, new RValue(trgBinary)]);
                    this.rnsReloaded.ExecuteScript("bpatt_add", self, other, [new RValue(this.rnsReloaded.ScriptFindId("bp_colormatch_activate_donut"))]);
                    this.scrbp.end(self, other);
                }
            } else {
                return this.colormatchHook.OriginalFunction(self, other, returnValue, argc, argv);
            }
            return returnValue;
        }
    }
}
