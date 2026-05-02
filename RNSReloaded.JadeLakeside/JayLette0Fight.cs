using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class JayLette0Fight : CustomFight {
        private IHook<ScriptDelegate> bulletClearHook;
        private IHook<ScriptDelegate> encounterHook;

        private bool enableConsistentDefensive = false;

        public JayLette0Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_musician0", "bp_frog_songstress0") {
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

                // Veeeeery small bullet clear radius
                argv[2]->Real = 150;
                argv[2]->Type = RValueType.Real;

                // Nerf defender's special bullet clear, since otherwise they're a bit too OP
                if (moveName == "mv_defender_2") {
                    argv[2]->Real = 50;
                }
            }
            returnValue = this.bulletClearHook.OriginalFunction(self, other, returnValue, argc, argv);
            return returnValue;
        }

        const int bfMidX = 1920 / 2;
        const int bfMidY = 1080 / 2;

        int[] savedGroups = [0, 0, 0, 0];

        const int RADIUS = 500;
        private (int x, int y, bool isMoving) Movement(CInstance* self, CInstance* other, int startDelay, int cycleTime, int restTime, int midX, int midY, int restTheta) {
            if (this.scrbp.time(self, other, 0)) {
                this.enableConsistentDefensive = true;
                this.bp.move_position_synced(self, other, spawnDelay: 0, duration: 500, position: (midX + (Math.Cos(restTheta * Math.PI / 180) * RADIUS), midY + (Math.Sin(restTheta * Math.PI / 180) * RADIUS)));
            }
            // This is done purely so `patternExTime` populates
            if (this.scrbp.time_repeating(self, other, 0, 1)) { }

            const int CYCLES = 50;
            const int rad = 360 / CYCLES;

            long thisBattleTime = this.utils.RValueToLong(this.rnsReloaded.FindValue(self, "patternExTime")) + 500;
            long totalCycle = restTime + cycleTime;
            long perMoveTime = cycleTime / CYCLES;
            long thisRepeatTime = (thisBattleTime - startDelay) % totalCycle;

            // If in movement part of cycle, move
            long thisIteration = 0;
            if (thisRepeatTime < cycleTime) {
                thisIteration = thisRepeatTime / perMoveTime + 1;
            }

            var x = midX + (int) (Math.Cos((thisIteration * rad + restTheta) * Math.PI / 180) * RADIUS);
            var y = midY + (int) (Math.Sin((thisIteration * rad + restTheta) * Math.PI / 180) * RADIUS);
            if (this.scrbp.time_repeating(self, other, startDelay - 500, cycleTime / CYCLES)) {
                this.bp.move_position_synced(self, other, spawnDelay: 500, duration: (int) (perMoveTime * 1.1), resetAnim: true, position: (x, y));
            }

            return (x, y, thisIteration > 0);
        }

        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            var pos = this.Movement(self, other, 1000, 4000, 4000, bfMidX - RADIUS / 2, bfMidY, 0);

            if (this.scrbp.time_repeating(self, other, 500, 1000)) {
                this.bp.prscircle(self, other, warningDelay: 500, spawnDelay: 1250, doubled: true, radius: RADIUS, position: (bfMidX, bfMidY));
            }

            if (this.scrbp.time_repeating(self, other, 500, 400)) {
                if (pos.isMoving) {
                    this.bp.water2_line(self, other,
                        spawnDelay: 500,
                        position: (pos.x, pos.y),
                        lineLength: 20,
                        numBullets: 2,
                        spd: 2,
                        angle: 180 + (int) (Math.Atan2(pos.y - bfMidY, pos.x - bfMidX) * 180 / Math.PI)
                    );
                }
            }
            if (this.scrbp.time_repeating(self, other, 500, 500)) {
                if (!pos.isMoving) {
                    var playerId = this.rng.Next(this.utils.GetNumPlayers());
                    var playerX = this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevX"));
                    var playerY = this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevY"));
                    this.bp.fire2_line(self, other,
                        spawnDelay: 500,
                        position: (pos.x, pos.y),
                        lineLength: 20,
                        numBullets: 2,
                        spd: 5,
                        angle: 180 + (int) (Math.Atan2(pos.y - playerY, pos.x - playerX) * 180 / Math.PI)
                    );
                }
            }

            if (this.scrbp.time(self, other, 45000)) {
                this.bp.enrage(self, other, spawnDelay: 5000, timeBetween: 10000);
            }
            return returnValue;
        }

        public override unsafe RValue* FightAltDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
            var pos = this.Movement(self, other, 5000, 4000, 4000, bfMidX + RADIUS / 2, bfMidY, 180);
            if (this.scrbp.time_repeating(self, other, 1250, 1000)) {
                this.bp.circle_spreads(self, other, warningDelay: 500, spawnDelay: 1500, radius: 130);
            }

            if (this.scrbp.time_repeating(self, other, 500, 400)) {
                if (pos.isMoving) {
                    this.bp.water2_line(self, other,
                        spawnDelay: 500,
                        position: (pos.x, pos.y),
                        lineLength: 20,
                        numBullets: 2,
                        spd: 1,
                        angle: 180 + (int) (Math.Atan2(pos.y - bfMidY, pos.x - bfMidX) * 180 / Math.PI));
                }
            }
            if (this.scrbp.time_repeating(self, other, 500, 1000)) {
                if (!pos.isMoving) {
                    var playerId = this.rng.Next(this.utils.GetNumPlayers());
                    var playerX = this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevX"));
                    var playerY = this.utils.RValueToLong(this.utils.GetPlayerVar(playerId, "distMovePrevY"));
                    this.bp.fire_aoe(self, other, warningDelay: 500, spawnDelay: 2000, eraseDelay: 4500, scale: 0.8, positions: [(playerX, playerY)]);
                }
            }

            if (this.scrbp.time(self, other, 55000)) {
                this.bp.enrage(self, other, spawnDelay: 5000, timeBetween: 5000);
            }

            return returnValue;
        }
    }
}
