using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechanicPack;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RnsReloaded.FuzzyMechanicPack {
    // Handles the mechanic that allows players to be buffed whenever they're hit by a specific enemy
    internal unsafe class BulletDelete : BpGenerator {
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IUtil utils;
        private IBattleScripts scrbp;
        private IHook<ScriptDelegate> oobCheckHook;
        private IHook<ScriptDelegate> fieldlimitHook;
        private IHook<ScriptDelegate> patternRemoveHook;
        private IHook<ScriptDelegate> erasePatternHook;
        private IHook<ScriptDelegate> encounterHook;

        public BulletDelete(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) : base(rnsReloaded) {
            this.rnsReloaded = rnsReloaded;
            this.logger = logger;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;

            var oobCheckScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_pattern_bullet_check_oob") - 100000);
            this.oobCheckHook =
                hooks.CreateHook<ScriptDelegate>(this.BoundsCheckDetour, oobCheckScript->Functions->Function);
            this.oobCheckHook.Activate();
            this.oobCheckHook.Enable();

            var fieldlimitScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_fieldlimit_rectangle") - 100000);
            this.fieldlimitHook =
                hooks.CreateHook<ScriptDelegate>(this.FieldlimitDetour, fieldlimitScript->Functions->Function);
            this.fieldlimitHook.Activate();
            this.fieldlimitHook.Enable();

            var encounterScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.encounterHook =
                hooks.CreateHook<ScriptDelegate>(this.EncounterDetour, encounterScript->Functions->Function);
            this.encounterHook.Activate();
            this.encounterHook.Enable();

            var patternRemoveScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_phase_pattern_remove") - 100000);
            this.patternRemoveHook =
                hooks.CreateHook<ScriptDelegate>(this.PatternRemoveDetour, patternRemoveScript->Functions->Function);
            this.patternRemoveHook.Activate();
            this.patternRemoveHook.Enable();

            var erasePatternScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_enemy_erase_patterns") - 100000);
            this.erasePatternHook =
                hooks.CreateHook<ScriptDelegate>(this.ErasePatternDetour, erasePatternScript->Functions->Function);
            this.erasePatternHook.Activate();
            this.erasePatternHook.Enable();
        }

        public void Run(CInstance* self, CInstance* other,
            int? spawnDelay = null,
            int? eraseDelay = null,
            int? midX = null,
            int? midY = null,
            int? width = null,
            int? height = null,
            int? radius = null,
            bool? inverted = null
        ) {
            RValue[] args = [this.utils.CreateString("type")!.Value, new RValue(1)];

            args = this.add_if_not_null(args, "spawnDelay", spawnDelay);
            args = this.add_if_not_null(args, "eraseDelay", eraseDelay);

            args = this.add_if_not_null(args, "x", midX);
            args = this.add_if_not_null(args, "y", midY);
            args = this.add_if_not_null(args, "width", width);
            args = this.add_if_not_null(args, "height", height);
            args = this.add_if_not_null(args, "radius", radius);
            args = this.add_if_not_null(args, "stat", inverted);

            this.execute_pattern(self, other, "bp_fieldlimit_rectangle", args);
        }

        // Dictionary from enemyId -> tracking ID -> bounds checking function to determine if bullet needs deletion
        private Dictionary<int, Dictionary<int, Func<int, int, bool>>> boundsChecks = new Dictionary<int, Dictionary<int, Func<int, int, bool>>>();

        private int globalTrackId = 0;
        private RValue* FieldlimitDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            long type = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "type", new RValue(0)));

            if (type == 1) {
                // API:
                //   type: set this to 1 to enable this variant. 0 will always be default apply_hbs_synced.
                //         Higher values may be used for future variants
                //   spawnDelay: how long until it starts erasing bullets
                //   eraseDelay: how long until it stops erasing bullets. 0 (default) is permanent
                //   x, y: Coordinates for the center of the field
                //   width/height/radius: Either specify width and height for a rectangle, or radius for a circle field
                //   stat: set to 1 if you want to invert the field (delete all bullets outside it instead of inside)

                int spawnDelay = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "spawnDelay", new RValue(0)));
                int eraseDelay = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "eraseDelay", new RValue(0)));

                int midX = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "x", new RValue(1920/2)));
                int midY = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "y", new RValue(1080/2)));
                int width = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "width", new RValue(100)));
                int height = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "height", new RValue(100)));
                int color = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "element", new RValue(0)));
                int radius = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "radius", new RValue(0)));

                bool inverted = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "stat", new RValue(0))) != 0;

                int trackingId = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "trackingId", new RValue(0)));
                int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

                // TODO:
                //   Pass in x, y, width, height for size (maybe optional radius instead too for a circle?)
                //   Fix bounds check to respect size
                //   Pass in element for fieldlimit visual (0 is invisible)
                //     If circle w/ radius, how is this handled?
                //   Pass in `stat` (maybe?) for if it should be inverted (only allow bullets inside it instead of delete bullets inside)

                // Pattern setup
                if (this.scrbp.time(self, other, spawnDelay)) {
                    trackingId = this.globalTrackId++;
                    this.scrbp.sbsv(self, other, "trackingId", new RValue(trackingId));

                    if (!this.boundsChecks.ContainsKey(enemyId)) {
                        this.boundsChecks.Add(enemyId, new Dictionary<int, Func<int, int, bool>>());
                    }

                    this.boundsChecks[enemyId].Add(trackingId, (int x, int y) => {
                        bool inRect =
                            x >= midX - width / 2 &&
                            x <= midX + width /2 &&
                            y >= midY - height / 2 &&
                            y <= midY + height / 2;
                        if (radius > 0) {
                            bool inCircle = Math.Pow(x - midX, 2) + Math.Pow(y - midY, 2) <= Math.Pow(radius, 2);
                            return inverted ^ inCircle;
                        }
                        return inverted ^ inRect;
                    });
                }

                // Pattern removal
                if (eraseDelay > 0 && this.scrbp.time(self, other, eraseDelay)) {
                    this.boundsChecks[enemyId].Remove(trackingId);
                    this.scrbp.end(self, other);
                }
            } else {
                return this.fieldlimitHook.OriginalFunction(self, other, returnValue, argc, argv);
            }
            return returnValue;
        }


        private RValue* BoundsCheckDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            returnValue = this.oobCheckHook.OriginalFunction(self, other, returnValue, argc, argv);
            int bulletX = (int) this.utils.RValueToLong(argv[0]);
            int bulletY = (int) this.utils.RValueToLong(argv[1]);
            if (this.boundsChecks.Any(dict => dict.Value.Any(func => func.Value(bulletX, bulletY)))) {
                returnValue->Real = 1;
            }
            return returnValue;
        }
        private RValue* EncounterDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            this.boundsChecks.Clear();
            return this.encounterHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        private RValue* PatternRemoveDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            int teamId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "teamId"));
            int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            if (teamId == 1) {
                this.boundsChecks.Remove(enemyId);
            }
            return this.patternRemoveHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        private RValue* ErasePatternDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            int teamId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "teamId"));
            int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            if (teamId == 1) {
                this.boundsChecks.Remove(enemyId);
            }
            return this.erasePatternHook.OriginalFunction(self, other, returnValue, argc, argv);
        }
    }

}
