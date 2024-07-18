using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe class RanXin1Fight : CustomFight {

        public RanXin1Fight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, logger, hooks, "bp_wolf_bluepaw1_s", "bp_wolf_redclaw1_s") { }

        private int Setup(CInstance* self, CInstance* other) {
            if (this.scrbp.time(self, other, 0)) {
                this.bp.fieldlimit_rectangle(self, other, position: (300, 300)              , width: 10, height: 10, color: IBattlePatterns.FIELDLIMIT_WHITE, targetMask: 0);
                this.bp.fieldlimit_rectangle(self, other, position: (1920-300, 300)         , width: 10, height: 10, color: IBattlePatterns.FIELDLIMIT_WHITE, targetMask: 0);
                this.bp.fieldlimit_rectangle(self, other, position: (300, 1080-300)         , width: 10, height: 10, color: IBattlePatterns.FIELDLIMIT_WHITE, targetMask: 0);
                this.bp.fieldlimit_rectangle(self, other, position: (1920 - 300, 1080 - 300), width: 10, height: 10, color: IBattlePatterns.FIELDLIMIT_WHITE, targetMask: 0);
                this.bp.move_position_synced(self, other, duration: 2000, position: (1920/2, 1080/2));
            }
            return 2000;
        }

        private int StartCycle(CInstance* self, CInstance* other, int delayTime) {
            if (this.scrbp.time(self, other, delayTime)) {
                
            }
            return 0;
        }
        // Blue wolf
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;
            time += this.Setup(self, other);

            time += this.StartCycle(self, other, time);

            return returnValue;
        }

        // Red wolf
        public override RValue* FightAltDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.scrbp.time(self, other, 0)) {
                this.bp.move_position_synced(self, other, spawnDelay: 0, duration: 500, position: (1920 - 300, 300));
            }
            if (this.scrbp.time_repeating(self, other, 2000, 12000)) {
                this.bp.move_position_synced(self, other, spawnDelay: 0, duration: 3000, position: (1920-300, 300));
                this.bp.move_position_synced(self, other, spawnDelay: 4000, duration: 1000, position: (1920 - 300, 1080 - 300));
                this.bp.move_position_synced(self, other, spawnDelay: 6000, duration: 3000, position: (300, 1080-300));
                this.bp.move_position_synced(self, other, spawnDelay: 10000, duration: 1000, position: (300, 300));
            }
            return returnValue;
        }
    }
}
