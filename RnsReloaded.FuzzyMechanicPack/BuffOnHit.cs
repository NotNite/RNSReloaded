using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechanicPack;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RnsReloaded.FuzzyMechanicPack {
    // Handles the mechanic that allows players to be buffed whenever they're hit by a specific enemy
    internal unsafe class BuffOnHit : BpGenerator {
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IUtil utils;
        private IBattleScripts scrbp;
        private IHook<ScriptDelegate> buffHook;
        private IHook<ScriptDelegate> patternRemoveHook;
        private IHook<ScriptDelegate> erasePatternHook;
        private IHook<ScriptDelegate> encounterHook;
        private IHook<ScriptDelegate> hitHook;

        public BuffOnHit(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) : base(rnsReloaded) {
            this.rnsReloaded = rnsReloaded;
            this.logger = logger;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;

            var buffScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("bp_apply_hbs_synced") - 100000);
            this.buffHook =
                hooks.CreateHook<ScriptDelegate>(this.BuffDetour, buffScript->Functions->Function);
            this.buffHook.Activate();
            this.buffHook.Enable();

            var patternRemoveScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_phase_pattern_remove") - 100000);
            this.patternRemoveHook =
                hooks.CreateHook<ScriptDelegate>(this.PatternRemoveDetour, patternRemoveScript->Functions->Function);
            this.patternRemoveHook.Activate();
            this.patternRemoveHook.Enable();

            var encounterScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.encounterHook =
                hooks.CreateHook<ScriptDelegate>(this.EncounterDetour, encounterScript->Functions->Function);
            this.encounterHook.Activate();
            this.encounterHook.Enable();

            var hitScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_pattern_deal_damage_ally") - 100000);
            this.hitHook =
                hooks.CreateHook<ScriptDelegate>(this.OnHitDetour, hitScript->Functions->Function);
            this.hitHook.Activate();
            this.hitHook.Enable();

            var erasePatternScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_enemy_erase_patterns") - 100000);
            this.erasePatternHook =
                hooks.CreateHook<ScriptDelegate>(this.ErasePatternDetour, erasePatternScript->Functions->Function);
            this.erasePatternHook.Activate();
            this.erasePatternHook.Enable();
        }

        public void Run(CInstance* self, CInstance* other,
            string? hbsName = null,
            int? hbsDuration = null,
            int? hbsStrength = null,
            int? targetMask = null,
            int? eraseDelay = null,
            int? timeBetweenBuffs = null
        ) {
            var hbsInfo = this.utils.GetGlobalVar("hbsInfo");
            for (var i = 0; i < this.rnsReloaded.ArrayGetLength(hbsInfo)!.Value.Real; i++) {
                if (hbsInfo->Get(i)->Get(0)->ToString() == hbsName) {
                    RValue[] args = [this.utils.CreateString("type")!.Value, new RValue(1)];
                    args = this.add_if_not_null(args, "hbsIndex", i);
                    args = this.add_if_not_null(args, "hbsDuration", hbsDuration);
                    args = this.add_if_not_null(args, "hbsStrength", hbsStrength);
                    args = this.add_if_not_null(args, "trgBinary", targetMask);
                    args = this.add_if_not_null(args, "eraseDelay", eraseDelay);
                    args = this.add_if_not_null(args, "timeBetween", timeBetweenBuffs);

                    this.execute_pattern(self, other, "bp_apply_hbs_synced", args);
                    break;
                }
            }
        }

        // Dictionary from: enemyId -> trackingId -> array of if players are hit or not
        private Dictionary<int, Dictionary<int, bool[]>> trackingDict = new Dictionary<int, Dictionary<int, bool[]>>();
        private int globalTrackId = 0;
        private RValue* BuffDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            long type = this.utils.RValueToLong(this.scrbp.sbgv(self, other, "type", new RValue(0)));

            if (type == 1) {
                // API:
                //   type: set this to 1 to enable this variant. 0 will always be default apply_hbs_synced.
                //         Higher values may be used for future variants
                //   hbsIndex: the index of the buff to add (string hbsName for Reloaded callers)
                //   hbsDuration: the duration of the buff
                //   hbsStrength: the strength of the buff
                //   trgBinary: which players will get buffs
                //   eraseDelay: how long until the pattern ends and players are no longer buffed
                //   timeBetween: duration that must pass before a player can be re-buffed, to prevent spamming

                int hbsIndex = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "hbsIndex", new RValue(0)));
                int hbsDuration = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "hbsDuration", new RValue(5000)));
                int hbsStrength = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "hbsStrength", new RValue(1)));
                int trgBinary = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "trgBinary", new RValue(127)));
                int eraseDelay = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "eraseDelay", new RValue(0)));
                int timeBetweenBuffs = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "timeBetween", new RValue(1000)));

                int trackingId = (int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "trackingId", new RValue(0)));
                List<int> lastBuffTimes = new List<int>();
                for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                    lastBuffTimes.Add((int) this.utils.RValueToLong(this.scrbp.sbgv(self, other, "lastBuffTime_" + i, new RValue(-timeBetweenBuffs))));
                }

                int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

                var hbsInfo = this.utils.GetGlobalVar("hbsInfo");
                var hbsName = hbsInfo->Get(hbsIndex)->Get(0)->ToString();

                if (this.scrbp.time(self, other, 0)) {
                    trackingId = this.globalTrackId++;
                    this.scrbp.sbsv(self, other, "trackingId", new RValue(trackingId));

                    if (!this.trackingDict.ContainsKey(enemyId)) {
                        this.trackingDict.Add(enemyId, new Dictionary<int, bool[]>());
                    }
                    this.trackingDict[enemyId].Add(trackingId, new bool[this.utils.GetNumPlayers()]);
                }

                // Update loop, called every frame
                if (this.scrbp.time_repeating(self, other, 0, 100)) {
                    int patternTime = (int) this.rnsReloaded.FindValue(self, "patternExTime")->Real;
                    for (int i = 0; i < this.utils.GetNumPlayers(); i++) {
                        // Skip players not targeted by this pattern.
                        if ((trgBinary & (1 << i)) == 0) {
                            continue;
                        }
                        if (this.trackingDict[enemyId][trackingId][i]) {
                            this.trackingDict[enemyId][trackingId][i] = false;
                            // Don't constantly spam rebuffs
                            if (lastBuffTimes[i] <= patternTime - timeBetweenBuffs) {
                                this.rnsReloaded.battlePatterns.apply_hbs_synced(self, other,
                                    delay: 0,
                                    hbsHitDelay: 0,
                                    hbs: hbsName,
                                    hbsDuration:
                                    hbsDuration,
                                    hbsStrength: hbsStrength,
                                    targetMask: 1 << i
                                );
                                this.scrbp.sbsv(self, other, "lastBuffTime_" + i, new RValue(patternTime));
                            }
                        }
                    }
                }

                // End the buff on hit pattern
                if (eraseDelay > 0 && this.scrbp.time(self, other, eraseDelay)) {
                    this.scrbp.end(self, other);
                    this.trackingDict[enemyId].Remove(trackingId);
                }
            } else {
                return this.buffHook.OriginalFunction(self, other, returnValue, argc, argv);
            }
            return returnValue;
        }

        private RValue* OnHitDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            // argv[0]: playerId
            // argv[1]: always 0? No idea
            // returnValue: 0/1 depending if hit or not
            // self.playerId: which mob spawned the bullet
            int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));
            int playerId = (int) this.utils.RValueToLong(argv[0]);
            if (this.trackingDict.ContainsKey(enemyId)) {
                foreach (var dictEntry in this.trackingDict[enemyId].AsEnumerable()) {
                    dictEntry.Value[playerId] = true;
                }
            }
            return this.hitHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        // Clear at the start of each encounter just to be sure
        private RValue* EncounterDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            this.trackingDict.Clear();
            return this.encounterHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        // Called on phase change
        private RValue* PatternRemoveDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            int teamId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "teamId"));
            int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            if (teamId == 1) {
                this.trackingDict.Remove(enemyId);
            }
            return this.patternRemoveHook.OriginalFunction(self, other, returnValue, argc, argv);
        }

        // Called on enemy death
        private RValue* ErasePatternDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            int teamId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "teamId"));
            int enemyId = (int) this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "playerId"));

            if (teamId == 1) {
                this.trackingDict.Remove(enemyId);
            }
            return this.erasePatternHook.OriginalFunction(self, other, returnValue, argc, argv);
        }
    }
}
