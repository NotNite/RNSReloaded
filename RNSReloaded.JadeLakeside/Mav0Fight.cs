using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe class Mav0Fight : CustomFight {
        public Mav0Fight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks) :
            base(rnsReloaded, fzbp, logger, hooks, "bp_frog_seamstress0") {}

        private int OlympicRingsMech(CInstance* self, CInstance* other, int startTime, int duration) {
            if (this.scrbp.time(self, other, startTime)) {

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
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: this.rng.Next(1, 3), position: (1920 / 2 - 600, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: this.rng.Next(1, 3), position: (1920 / 2 - 200, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: this.rng.Next(1, 3), position: (1920 / 2 + 200, 1080 - 350));
                this.bp.angel_circle(self, other, spawnDelay: duration, radius: 275, number: this.rng.Next(1, 3), position: (1920 / 2 + 600, 1080 - 350));
            }
            return duration + 500;
        }
        public override RValue* FightDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            int time = 0;

            time += 1000;

            time += this.OlympicRingsMech(self, other, time, 7000);
            time += this.OlympicRingsMech(self, other, time, 7000);
            time += this.OlympicRingsMech(self, other, time, 7000);

            return returnValue;
        }
    }
}
