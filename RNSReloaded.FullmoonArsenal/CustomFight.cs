using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.FullmoonArsenal {
    internal unsafe abstract class CustomFight {
        protected ILoggerV1 logger;
        protected IRNSReloaded rnsReloaded;
        protected IUtil utils;
        protected IBattleScripts scrbp;
        protected IBattlePatterns bp;

        private IHook<ScriptDelegate> scriptHook;
        private IHook<ScriptDelegate>? scriptAltHook;

        protected Random rng;
        public CustomFight(IRNSReloaded rnsReloaded, ILoggerV1 logger, IReloadedHooks hooks, string fightName, string fightAltName = "") {
            this.rnsReloaded = rnsReloaded;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;
            this.bp = rnsReloaded.battlePatterns;
            this.logger = logger;
            var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightName) - 100000);
            this.scriptHook =
                hooks.CreateHook<ScriptDelegate>(this.FightDetour, script->Functions->Function);
            this.scriptHook.Activate();
            this.scriptHook.Enable();

            if (fightAltName != "") {
                var scriptAlt = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightAltName) - 100000);
                this.scriptAltHook =
                    hooks.CreateHook<ScriptDelegate>(this.FightAltDetour, scriptAlt->Functions->Function);
                this.scriptAltHook.Activate();
                this.scriptAltHook.Enable();
            }

            this.rng = new Random();
        }

        public abstract RValue* FightDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv);
        public virtual RValue* FightAltDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) { return returnValue; }
    }
}
