using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RnsReloaded.FuzzyMechanicPack;
using RNSReloaded.Interfaces;

namespace RNSReloaded.FullmoonArsenal;

public unsafe class Mod : IMod {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;

    private ColorMatchSwap? colorMatchSwap = null;

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
            this.colorMatchSwap = new ColorMatchSwap(rnsReloaded, this.logger, hooks);
        }
    }

    public void Suspend() {
    }

    public void Resume() {
    }

    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
