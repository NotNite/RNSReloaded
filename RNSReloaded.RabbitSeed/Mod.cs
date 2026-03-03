using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.RabbitSeed.Config;

namespace RNSReloaded.RabbitSeed;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private IHook<ScriptDelegate>? initAdventureHook;
    private IHook<ScriptDelegate>? langStringHook;
    private IHook<ScriptDelegate>? encounterHook;

    private bool isFirstEncounter = true;

    public void StartEx(IModLoaderV1 loader,IModConfigV1 modConfig) {
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
            if (this.config.shouldSetSeed) {
                rnsReloaded.LimitOnlinePlay();
            }
            var initScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_init_adventure_map") - 100000);
            this.initAdventureHook =
                hooks.CreateHook<ScriptDelegate>(this.initAdventureDetour, initScript->Functions->Function);
            this.initAdventureHook.Activate();

            var langStringScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scr_lang_string") - 100000);
            this.langStringHook =
                hooks.CreateHook<ScriptDelegate>(this.langStringDetour, langStringScript->Functions->Function);
            this.langStringHook.Activate();

            var encScript = rnsReloaded.GetScriptData(rnsReloaded.ScriptFindId("scrdt_encounter") - 100000);
            this.encounterHook =
                hooks.CreateHook<ScriptDelegate>(this.encounterDetour, encScript->Functions->Function);
            this.encounterHook.Activate();
        }
    }

    private RValue* initAdventureDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        this.logger.PrintMessage("Initializing adventure map", this.logger.ColorGreen);
        returnValue = this.initAdventureHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded) && this.config.shouldSetSeed) {
            var newSeed = this.config.mapSeed;
            this.logger.PrintMessage($"Changing map seed to {newSeed}", this.logger.ColorGreen);
            var mapSeedR = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "mapSeed");
            mapSeedR->Type = RValueType.Real;
            mapSeedR->Real = newSeed;

            this.isFirstEncounter = true;
        }
        return returnValue;
    }

    private RValue* langStringDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.langStringHook!.OriginalFunction(self, other, returnValue, argc, argv);
        var key = argv[0]->ToString();
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded)
                && (key == "vd_subtitle" || key == "vd_subtitle2")) {
            var mapSeedR = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "mapSeed");
            rnsReloaded.CreateString(returnValue,  $"MAP SEED: {mapSeedR->ToString()}");
            return returnValue;
        } else {
            return returnValue;
        }
    }

    private RValue* encounterDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.encounterHook!.OriginalFunction(self, other, returnValue, argc, argv);
        if (this.rnsReloadedRef!.TryGetTarget(out var rnsReloaded) && this.config.shouldChatSeed) {
            if (this.isFirstEncounter) {
                this.isFirstEncounter = false;
                var mapSeedR = rnsReloaded.FindValue(rnsReloaded.GetGlobalInstance(), "mapSeed");
                var message = rnsReloaded.utils.CreateString($"Using map seed: {mapSeedR->ToString()}")!.Value;
                rnsReloaded.ExecuteScript("scr_chat_add_mesage_system", null, null, [message]);
            }
        }
        return returnValue;
    }

    public void Suspend() {
        this.initAdventureHook?.Disable();
    }

    public void Resume() {
        this.initAdventureHook?.Enable();
    }

    public bool CanSuspend() => false; // Add suspend/resume code and set to true once ready

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
