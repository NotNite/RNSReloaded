using System.Xml.Linq;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.DoubleTime.Config;

namespace RNSReloaded.DoubleTime;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private IHook<ScriptDelegate>? gameSpeedHook;
    private IHook<ScriptDelegate>? encounterStartHook;

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

            var gameSpeedScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrbp_gamespeed") - 100000);
            this.gameSpeedHook = hooks.CreateHook<ScriptDelegate>(this.gameSpeedDetour, gameSpeedScript->Functions->Function)!;
            this.gameSpeedHook.Activate();
            this.gameSpeedHook.Enable();


            var encounterStartScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.encounterStartHook = hooks.CreateHook<ScriptDelegate>(this.encounterStartDetour, encounterStartScript->Functions->Function)!;
            this.encounterStartHook.Activate();
            this.encounterStartHook.Enable();
        }
    }

    private RValue* gameSpeedDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            string gameSpeedRValueType = rnsReloaded.ExecuteCodeFunction("typeof", null, null, 1, (RValue**) argv).ToString() ?? "none";
            switch (gameSpeedRValueType) {
                case "number":
                    (*argv)->Real = (*argv)->Real * this.config.SpeedMultiplier;
                    break;
                case "int32":
                    (*argv)->Real = (*argv)->Int32 * this.config.SpeedMultiplier;
                    break;
                case "int64":
                    (*argv)->Real = (*argv)->Int64 * this.config.SpeedMultiplier;
                    break;
            }
        }
        returnValue = this.gameSpeedHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private RValue* encounterStartDetour(
     CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.ExecuteScript("scrbp_gamespeed", self, other, [new RValue(1)]);
        }
        returnValue = this.encounterStartHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    public void Resume() { }
    public void Suspend() { }
    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
