using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechanicPack;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Runtime.InteropServices;

namespace RnsReloaded.FuzzyMechanicPack {
    // Handles the mechanic that allows color match colors to be changed mid-mechanic
    internal unsafe class Towers : BpGenerator {
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IUtil utils;
        private IBattleScripts scrbp;
        private IHook<ScriptDelegate> colormatchHook;

        public Towers(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) : base(rnsReloaded) {
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
            int playersRequired = 1,
            int? warningDelay = null,
            int? spawnDelay = null,
            int? radius = null,
            int? warnMsg = null,
            int? displayNumber = null,
            int? x = null,
            int? y = null
        ) {
            RValue[] args = [this.utils.CreateString("type")!.Value, new RValue(2)];
            args = this.add_if_not_null(args, "amount", playersRequired);
            args = this.add_if_not_null(args, "warningDelay", warningDelay);
            args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
            args = this.add_if_not_null(args, "radius", radius);
            args = this.add_if_not_null(args, "warnMsg", warnMsg);
            args = this.add_if_not_null(args, "displayNumber", displayNumber);
            args = this.add_if_not_null(args, "x", x);
            args = this.add_if_not_null(args, "y", y);

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

        private RValue* ColormatchDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            long type = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "type", new RValue(0)));

            if (type == 2) {
                // API:
                //   type: set this to 2 to enable this variant. 0 will always be default colormatch.
                //         Higher values may be used for future variants
                //   amount: Number of people required
                //   warningDelay, spawnDelay, radius, warnMsg, displayNumber as normal
                //   x, y: Position of circle

                int numPlayers = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "amount", new RValue(1)));

                int warningDelay   = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warningDelay",  new RValue(0)));
                int spawnDelay     = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "spawnDelay",    new RValue(3000)));
                int radius         = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "radius",        new RValue(400)));
                int warnMsg        = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "warnMsg",       new RValue(0)));
                int displayNumber  = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "displayNumber", new RValue(0)));
                int xPos = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "x", new RValue(0)));
                int yPos = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "y", new RValue(0)));

                // Basic setup. Normal colormatch sets pattern color here too, but we'll be changing that quite often.
                if (this.scrbp.time(self, other, 0)) {
                    this.scrbp.pattern_set_projectile_key(self, other, "pjb_dark", "sfxset_enemy");
                    this.scrbp.pattern_set_drawlayer(self, other, 1);
                }

                // Warning creation
                if (this.scrbp.time(self, other, warningDelay)) {
                    int timeToSpawn = spawnDelay - warningDelay;

                    // Skip everything if it's already time to damage players
                    if (timeToSpawn > 0) {

                        if (displayNumber > 0) {
                            this.scrbp.make_number_warning(self, other, xPos, yPos, displayNumber, timeToSpawn);
                        } else {
                            this.scrbp.warning_msg_pos(self, other, xPos, yPos - radius + 120, "eff_colormatch", warnMsg, timeToSpawn);
                        }

                        this.scrbp.sound_x(self, other, xPos);
                        this.scrbp.sound(self, other, 1, 0);

                        this.scrbp.pattern_set_color_colormatch(self, other, 4);

                        // Create all rings
                        for (int i = 0; i < numPlayers; i++) {
                            int thisRadius = radius + 35 * i;

                            // Create circle
                            this.scrbp.make_warning_colormatch(self, other, xPos, yPos, thisRadius, 69, timeToSpawn);

                            // Background (set to black)
                            var background = this.GetMostRecentObjectFromLayer("BattleWarningMid");
                            var color = this.rnsReloaded.FindValue(background->Instance, "color");
                            color->Real = 0;
                            color->Type = RValueType.Real;

                            // Ring (set both to black)
                            var ring = this.GetMostRecentObjectFromLayer("BattleWarningOver");
                            color = this.rnsReloaded.FindValue(ring->Instance, "color");
                            color->Real = 0xFFFFFF;
                            color->Type = RValueType.Real;

                            var color2 = this.rnsReloaded.FindValue(ring->Instance, "color2");
                            color2->Real = 0xFFFFFF;
                            color2->Type = RValueType.Real;

                            // Inside symbol (set to black)
                            var insideSymbol = this.GetMostRecentObjectFromLayer("BattleWarningUnder");
                            color = this.rnsReloaded.FindValue(insideSymbol->Instance, "color");
                            color->Real = 0xFFFFFF;
                            color->Type = RValueType.Real;

                            this.scrbp.sbsv(self, other, "ring_" + i, new RValue(ring->Instance));
                        }
                    }
                }

                // Update loop, called every frame
                if (this.scrbp.time_repeating(self, other, warningDelay, 100)) {
                    int playersInRing = 0;
                    for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                        int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevX"));
                        int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevY"));
                        if (Math.Pow(playerX - xPos, 2) + Math.Pow(playerY - yPos, 2) < Math.Pow(radius, 2)) {
                            playersInRing++;
                        }
                    }
                    // Update each ring
                    for (int i = 0; i < numPlayers; i++) {
                        var ring = this.scrbp.sbgv(self, other, "ring_" + i, new RValue(0));
                        if (ring.Int64 != 0) {
                            RValue* color = this.rnsReloaded.FindValue(ring.Object, "color");
                            color->Real = i >= playersInRing ? 0 : 0xFFFFFF;
                            color->Type = RValueType.Real;
                        }
                    }
                }

                // Activate
                if (this.scrbp.time(self, other, spawnDelay)) {
                    int playersInRing = 0;
                    for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                        int playerX = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevX"));
                        int playerY = (int) this.utils.RValueToLong(this.utils.GetPlayerVar(i, "distMovePrevY"));
                        if (Math.Pow(playerX - xPos, 2) + Math.Pow(playerY - yPos, 2) < Math.Pow(radius, 2)) {
                            playersInRing++;
                        }
                    }

                    var forceLocal = this.rnsReloaded.FindValue(self, "networkForceLocal");
                    forceLocal->Real = 1.0;
                    forceLocal->Type = RValueType.Bool;

                    // Failed, damage players
                    if (playersInRing < numPlayers) {
                        // Set new trgBinary
                        RValue trgBinName = new RValue(0);
                        this.rnsReloaded.CreateString(&trgBinName, "trgBinary");

                        this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [trgBinName, new RValue(127)]);
                        this.rnsReloaded.ExecuteScript("bpatt_add", self, other, [new RValue(this.rnsReloaded.ScriptFindId("bp_damage_players"))]);
                        // Reset vars for next color
                        this.rnsReloaded.ExecuteScript("bpatt_var_reset", self, other, []);
                    }

                    RValue posX = new RValue(0);
                    RValue posY = new RValue(0);
                    this.rnsReloaded.CreateString(&posX, "posX_0");
                    this.rnsReloaded.CreateString(&posY, "posY_0");
                    this.rnsReloaded.ExecuteScript("bpatt_var", self, other, [posX, new RValue(xPos), posY, new RValue(yPos)]);

                    this.scrbp.make_warning_colormatch_burst(self, other, xPos, yPos, radius, 4);
                    this.scrbp.sound_x(self, other, xPos);
                    this.scrbp.sound(self, other, 1, 1);
                    this.scrbp.end(self, other);
                }
            } else {
                return this.colormatchHook.OriginalFunction(self, other, returnValue, argc, argv);
            }
            return returnValue;
        }
    }
}
