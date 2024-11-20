using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.FuzzyMechanicPack;
using RNSReloaded.FuzzyMechPackInterfaces;
using RNSReloaded.Interfaces;

public unsafe class Mod : IMod, IExports {
    private WeakReference<IRNSReloaded>? rnsReloadedRef;
    private WeakReference<IReloadedHooks>? hooksRef;
    private ILoggerV1 logger = null!;
    private IModLoaderV1 loader = null!;

    private FuzzyMechPack fuzzyMechPack = null!;

    public void Start(IModLoaderV1 loader) {
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>();
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.loader = loader;
        this.logger = loader.GetLogger();
        
        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded) && this.hooksRef.TryGetTarget(out var hooks)) {
            rnsReloaded.OnReady += this.Ready;
            this.fuzzyMechPack = new FuzzyMechPack(rnsReloaded, this.logger, hooks);
            this.loader.AddOrReplaceController<IFuzzyMechPack>(this, this.fuzzyMechPack);
        } else {
            this.logger.PrintMessage("Failed to register Fuzzy Mechanic Pack, was RNS Reloaded found?", this.logger.ColorRed);
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
        }
    }

    public void Suspend() {
    }

    public void Resume() {
    }

    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Type[] GetTypes() => [typeof(IFuzzyMechPack)];


    public Action Disposing => () => { };
}
