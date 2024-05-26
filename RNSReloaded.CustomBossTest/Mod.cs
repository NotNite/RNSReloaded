using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RNSReloaded.CustomBossTest {
    public unsafe class Mod : IMod {
        private WeakReference<IRNSReloaded>? rnsReloadedRef;
        private WeakReference<IReloadedHooks>? hooksRef;
        private ILoggerV1 logger = null!;

        private Util? utils;
        private Patterns? patterns;

        private IHook<ScriptDelegate>? addEncounterHook;

        public void Start(IModLoaderV1 loader) {
            this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
            this.hooksRef = loader.GetController<IReloadedHooks>()!;

            this.logger = loader.GetLogger();

            if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                rnsReloaded.OnReady += this.Ready;
            }
        }

        public void Ready() {
            if (
                this.rnsReloadedRef != null
                && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
                && this.hooksRef != null
                && this.hooksRef.TryGetTarget(out var hooks)
            ) {
                rnsReloaded.LimitOnlinePlay();

                this.utils = new Util(this.rnsReloadedRef, this.logger);
                this.patterns = new Patterns(this.rnsReloadedRef, this.utils, this.logger);

                var encounterId = rnsReloaded.ScriptFindId("bp_dragon_granite1_s");
                var encounterScript = rnsReloaded.GetScriptData(encounterId - 100000);

                this.addEncounterHook =
                    hooks.CreateHook<ScriptDelegate>(this.AddEncounterDetour, encounterScript->Functions->Function);
                this.addEncounterHook.Activate();
                this.addEncounterHook.Enable();
            }
        }

        private RValue? scrbp_time_repeating(CInstance* self, CInstance* other, int a, int b) {
            if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                RValue[] args = [new RValue(a), new RValue(b)];
                return this.utils!.RunGMLScript("scrbp_time_repeating", self, other, args);
            }
            return null;
        }

        private RValue* AddEncounterDetour(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
        ) {
            if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
                var result = this.scrbp_time_repeating(self, other, 0, 1000);
                if (result != null && result.Value.Real == 1) {
                    var player_x = this.utils!.GetPlayerVar("distMovePrevX")->Real;
                    var player_y = this.utils!.GetPlayerVar("distMovePrevY")->Real;
                    this.patterns!.bp_fire_aoe(self, other, 1500, 3000, 1.5, 1, player_x, player_y);
                }
            }
            return returnValue;
        }

        public void Suspend() {
            this.addEncounterHook?.Disable();
        }

        public void Resume() {
            this.addEncounterHook?.Enable();
        }

        public bool CanSuspend() => true;

        public void Unload() { }
        public bool CanUnload() => false;

        public Action Disposing => () => { };
    }
}
