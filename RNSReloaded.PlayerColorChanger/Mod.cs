using System.Drawing;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;
using RNSReloaded.PlayerColorChanger.Config;

namespace RNSReloaded.PlayerColorChanger;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private IHook<ScriptDelegate>? playerColorSetHook;
    private uint color;

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
        this.ApplyConfig();
    }

    private void ApplyConfig() {
        try {
            var newColor = uint.Parse(this.config.String, System.Globalization.NumberStyles.HexNumber);
            this.color = (newColor & 0xFF00FF00) | ((newColor & 0x00FF0000) >> 16) | ((newColor & 0x000000FF) << 16);
            this.logger.PrintMessage("[Player Color Changer] Color changed to " + this.config.String, Color.White);
        } catch {
            this.logger.PrintMessage("[Player Color Changer] Invalid color!", Color.Red);
        }
    }

    private void ConfigurationUpdated(IUpdatableConfigurable newConfig) {
        this.config = (Config.Config) newConfig;
        this.ApplyConfig();
    }

    public void Ready() {
        if (
            this.rnsReloadedRef != null
            && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)
            && this.hooksRef != null
            && this.hooksRef.TryGetTarget(out var hooks)
        ) {
            var id = rnsReloaded.ScriptFindId("scr_playercolor_set");
            var script = rnsReloaded.GetScriptData(id - 100000);

            this.playerColorSetHook =
                hooks.CreateHook<ScriptDelegate>(this.PlayerColorSetDetour, script->Functions->Function);
            this.playerColorSetHook =
                this.playerColorSetHook?.Activate();
            this.playerColorSetHook?.Enable();
        }
    }

    private RValue* PlayerColorSetDetour(
        CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
    ) {
        returnValue = this.playerColorSetHook!.OriginalFunction(self, other, returnValue, argc, argv);

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue global = rnsReloaded.GetGlobalInstance();
            var clientOwnId = global["clientOwnId"]->Int32;
            var playerCharId = global["playerCharId"]->Get(0)->Get(clientOwnId);
            var allyId = new RValue(self)["allyId"];
            if (allyId->Int32 == playerCharId->Int32) {
                var playerColor = global["playerColor"]->Get(0)->Get(clientOwnId);
                playerColor->Real = this.color;
            }
        }

        return returnValue;
    }

    public void Suspend() => this.playerColorSetHook?.Disable();
    public void Resume() => this.playerColorSetHook?.Enable();
    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
