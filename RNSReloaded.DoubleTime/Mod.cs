using System.Xml.Linq;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.DoubleTime.Config;
using System;

namespace RNSReloaded.DoubleTime;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private static Dictionary<string, IHook<ScriptDelegate>> ScriptHooks = new();

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }

        this.configurator = new Configurator(((IModLoader) loader).GetModConfigDirectory(modConfig.ModId));
        this.config = this.configurator.GetConfiguration<Config.Config>(0);
        this.config.ConfigurationUpdated += this.ConfigurationUpdated;
    }
    private void ConfigurationUpdated(IUpdatableConfigurable newConfig) {
        this.config = (Config.Config) newConfig;
    }
    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            rnsReloaded.LimitOnlinePlay();

            string[] toHook = {
                //"scr_enemy_add_pattern", //this desync cause only host gets this when a treasuresphere breaks?!
                "scrdt_encounter",
                "bp_wolf_steeltooth1",
                "bp_wolf_steeltooth1_enrage",
                "bp_wolf_steeltooth1_pt2",
                "bp_wolf_steeltooth1_pt2_s",
                //"bp_wolf_steeltooth1_pt3",
                //"bp_wolf_steeltooth1_pt3_s",
                "bp_wolf_steeltooth1_pt4",
                "bp_wolf_steeltooth1_pt4_s",
                //"bp_wolf_steeltooth1_pt5",
                //"bp_wolf_steeltooth1_pt5_s",
                "bp_wolf_steeltooth1_pt6",
                "bp_wolf_steeltooth1_pt6_s",
                //"bp_wolf_steeltooth1_pt7",
                //"bp_wolf_steeltooth1_pt7_s",
                "bp_wolf_steeltooth1_pt8",
                "bp_wolf_steeltooth1_pt8_s",
                //"bp_wolf_steeltooth1_pt9",
                //"bp_wolf_steeltooth1_pt9_s",
                "bp_wolf_steeltooth1_s"
            };


            foreach (var hookStr in toHook) {
                RValue* Detour(
                    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
                ) {
                    return this.AddPatternDetour(hookStr, self, other, returnValue, argc, argv);
                }

                var script = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId(hookStr) - 100000);

                var hook = hooks.CreateHook<ScriptDelegate>(Detour, script->Functions->Function)!;

                hook.Activate();
                hook.Enable();

                ScriptHooks[hookStr] = hook;

            }


            // refactor soonTM
            RValue* Detour2(
            CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
            ) {
                return this.AddPatternDetour2(self, other, returnValue, argc, argv);
            }

            var script2 = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_zoom") - 100000);

            var hook2 = hooks.CreateHook<ScriptDelegate>(Detour2, script2->Functions->Function)!;

            hook2.Activate();
            hook2.Enable();

            ScriptHooks["scrbp_zoom"] = hook2;

        }
    }

    private RValue* AddPatternDetour(
        string name, CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        var hook = ScriptHooks[name];
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.ExecuteScript("scrbp_gamespeed", self, other, [new RValue(this.config.SpeedMultiplier)]);
        }
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }
    private RValue* AddPatternDetour2(
     CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
) {
        var hook = ScriptHooks["scrbp_zoom"];
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            if ((*argv)->Real == 1.6) // merran timeslow is 1.6 zoom, 0.3 speed by default
                rnsReloaded.ExecuteScript("scrbp_gamespeed", self, other, [new RValue(0.3 * this.config.SpeedMultiplier)]);
        }
        returnValue = hook.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }
   

    public void Resume() { }
    public void Suspend() { }
    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
