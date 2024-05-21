using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.PlayerColorChanger;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private IHook<ScriptDelegate>? playerColorSetHook;

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>()!;
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.logger = loader.GetLogger();

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.OnReady += this.Ready;
        }
    }

    public Action Disposing => () => { };

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
                const int color = 0xCB2027; // TODO don't hardcode
                // Convert to 0xBBGGRR
                var converted = (color & 0xFF00FF00) | ((color & 0x00FF0000) >> 16) | ((color & 0x000000FF) << 16);
                playerColor->Real = converted;
            }
        }

        return returnValue;
    }

    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }
    public bool CanUnload() => true;
    public bool CanSuspend() => true;
}
