using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RnsReloaded.FuzzyMechanicPack;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FuzzyMechanicPack {
    internal unsafe class FuzzyMechPack : IFuzzyMechPack {
        public event Action? OnReady;
        public event Action<ExecuteItArguments>? OnExecuteIt;

        private ColorMatchSwap? colorMatchSwap;
        private BuffOnHit? buffOnHit;
        private IRNSReloaded rnsReloaded;
        private ILoggerV1 logger;
        private IReloadedHooks hooks;

        public FuzzyMechPack(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks) {
            this.rnsReloaded = rnsReloaded;
            this.logger = logger;
            this.hooks = hooks;

            IFuzzyMechPack.Instance = this;

            this.rnsReloaded.OnReady += this.Ready;
        }

        public void Dispose() {
            // TODO
        }

        private void Ready() {
            this.colorMatchSwap = new ColorMatchSwap(this.rnsReloaded, this.logger, this.hooks);
            this.buffOnHit = new BuffOnHit(this.rnsReloaded, this.logger, this.hooks);
            this.logger.PrintMessage("Fuzzy's Mechanic Pack set up!", this.logger.ColorGreen);
        }

        public void ColormatchSwap(CInstance* self, CInstance* other,
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
            this.colorMatchSwap?.Run(self, other, numColors, warningDelay, spawnDelay, matchRadius, setRadius, warnMsg, displayNumber, setCircles, matchCircles, targetMask, colors);
        }

        public void BuffOnHit(CInstance* self, CInstance* other,
            string? hbsName = null,
            int? hbsDuration = null,
            int? hbsStrength = null,
            int? targetMask = null,
            int? eraseDelay = null,
            int? timeBetweenBuffs = null
        ) {
            this.buffOnHit?.Run(self, other, hbsName, hbsDuration, hbsStrength, targetMask, eraseDelay, timeBetweenBuffs);
        }
    }
}
