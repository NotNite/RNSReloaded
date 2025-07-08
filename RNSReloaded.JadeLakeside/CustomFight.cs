using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.JadeLakeside {
    internal unsafe abstract class CustomFight {
        protected ILoggerV1 logger;
        protected IRNSReloaded rnsReloaded;
        protected IUtil utils;
        protected IBattleScripts scrbp;
        protected IBattlePatterns bp;
        protected IFuzzyMechPack fzbp;

        private IHook<ScriptDelegate> scriptHook;
        private IHook<ScriptDelegate> scriptHookSingle;
        private IHook<ScriptDelegate>? scriptAltHook;
        private IHook<ScriptDelegate>? scriptAltHookSingle;

        protected Random rng;
        public CustomFight(IRNSReloaded rnsReloaded, IFuzzyMechPack fzbp, ILoggerV1 logger, IReloadedHooks hooks, string fightName, string fightAltName = "") {
            this.rnsReloaded = rnsReloaded;
            this.utils = rnsReloaded.utils;
            this.scrbp = rnsReloaded.battleScripts;
            this.bp = rnsReloaded.battlePatterns;
            this.fzbp = fzbp;
            this.logger = logger;

            var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightName) - 100000);
            this.scriptHook =
                hooks.CreateHook<ScriptDelegate>(this.FightDetour, script->Functions->Function);
            this.scriptHook.Activate();
            this.scriptHook.Enable();

            var scriptSingle = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightName + "_s") - 100000);
            if (scriptSingle != null) {
                this.scriptHookSingle = hooks.CreateHook<ScriptDelegate>(this.FightDetour, scriptSingle->Functions->Function);
                this.scriptHookSingle.Activate();
                this.scriptHookSingle.Enable();
            }

            if (fightAltName != "") {
                var scriptAlt = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightAltName) - 100000);
                this.scriptAltHook =
                    hooks.CreateHook<ScriptDelegate>(this.FightAltDetour, scriptAlt->Functions->Function);
                this.scriptAltHook.Activate();
                this.scriptAltHook.Enable();

                var scriptAltSingle = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(fightAltName + "_s") - 100000);
                if (scriptAltSingle != null) {
                    this.scriptAltHookSingle =
                        hooks.CreateHook<ScriptDelegate>(this.FightAltDetour, scriptAltSingle->Functions->Function);
                    this.scriptAltHookSingle.Activate();
                    this.scriptAltHookSingle.Enable();
                }
            }

            this.rng = new Random();
        }

        public abstract RValue* FightDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv);
        public virtual RValue* FightAltDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) { return returnValue; }
    }
}
