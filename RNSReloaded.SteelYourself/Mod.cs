using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.SteelYourself.Config;
using System.Diagnostics.CodeAnalysis;

namespace RNSReloaded.SteelYourself;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private Dictionary<string, IHook<ScriptDelegate>> hookMap = new Dictionary<string, IHook<ScriptDelegate>>();
    private IHook<ScriptDelegate> outskirtsHook;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>()!;
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

    private void createAndSetHook(IRNSReloaded rnsReloaded, IReloadedHooks hooks, string scriptName, ScriptDelegate detour) {
        var id = rnsReloaded.ScriptFindId(scriptName);
        var script = rnsReloaded.GetScriptData(id - 100000);
        if (script == null) {
            this.logger.PrintMessage("Script " + scriptName + " not found. Hook not created.", this.logger.ColorRed);
            return;
        }
        this.hookMap[scriptName] = hooks.CreateHook<ScriptDelegate>(detour, script->Functions->Function);
        this.hookMap[scriptName].Activate();
        this.hookMap[scriptName].Enable();
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            rnsReloaded.LimitOnlinePlay();

            this.createAndSetHook(rnsReloaded, hooks, "scr_hallwaygen_outskirts", this.OutskirtsDetour);
            this.createAndSetHook(rnsReloaded, hooks, "scr_hallwaygen_outskirts_n", this.OutskirtsDetour);
            this.createAndSetHook(rnsReloaded, hooks, "scr_hallwaygen_geode", this.GeodeDetour);
            this.createAndSetHook(rnsReloaded, hooks, "scr_hallwaygen_toybox", this.ToyboxDetour);
        }
    }

    private RValue* OutskirtsDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.hookMap["scr_hallwaygen_outskirts"]!.OriginalFunction(self, other, returnValue, argc, argv);

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            var encounterName = Enum.GetName(this.config.ForcedEncounter);
            rnsReloaded.utils.setHallway(new List<Notch> {
                new Notch(NotchType.IntroRoom, "", 0, 0),
                new Notch(NotchType.Encounter, encounterName != null ? encounterName : "enc_bird_student0", 0, 0),
                new Notch(NotchType.EndRun, "", 0, 0)
            }, self, rnsReloaded);
        }
        return returnValue;
    }

    private RValue* GeodeDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.hookMap["scr_hallwaygen_geode"]!.OriginalFunction(self, other, returnValue, argc, argv);

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            var encounterName = Enum.GetName(this.config.ForcedEncounter);
            rnsReloaded.utils.setHallway(new List<Notch> {
                new Notch(NotchType.IntroRoom, "", 0, 0),
                new Notch(NotchType.Encounter, encounterName != null ? encounterName : "enc_bird_student0", 0, 0),
                new Notch(NotchType.EndRun, "", 0, 0)
            }, self, rnsReloaded);
        }
        return returnValue;
    }

    private RValue* ToyboxDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.hookMap["scr_hallwaygen_toybox"]!.OriginalFunction(self, other, returnValue, argc, argv);

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            var encounterName = Enum.GetName(this.config.ForcedEncounter);

            rnsReloaded.utils.setHallway(new List<Notch> {
                new Notch(NotchType.ToyboxIntro, "", 0, 0),
                new Notch(NotchType.Encounter, encounterName != null ? encounterName : "enc_bird_student0", 0, 0),
                new Notch(NotchType.EndRun, "", 0, 0)
            }, self, rnsReloaded);
        }
        return returnValue;
    }

    public void Suspend() {}
    public void Resume() { }
    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
