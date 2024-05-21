using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;

namespace RNSReloaded;

// ReSharper disable InconsistentNaming
public class Mod : IMod, IExports {
    private IModLoaderV1 loader = null!;
    private ILoggerV1 logger = null!;
    private WeakReference<IReloadedHooks>? hooksRef;
    private WeakReference<IStartupScanner>? scannerRef;
    private RNSReloaded rns = null!;

    public void Start(IModLoaderV1 loader) {
        this.loader = loader;
        this.logger = loader.GetLogger();

        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.scannerRef = loader.GetController<IStartupScanner>()!;

        this.rns = new RNSReloaded(this.hooksRef, this.scannerRef, this.logger);
        this.loader.AddOrReplaceController<IRNSReloaded>(this, this.rns);
    }

    public Action Disposing => () => {
        this.rns.Dispose();
        this.loader.RemoveController<IRNSReloaded>();
        this.hooksRef = null;
        this.scannerRef = null;
    };

    public void Suspend() { }
    public void Resume() { }
    public void Unload() { }
    public bool CanUnload() => true;
    public bool CanSuspend() => false;

    public Type[] GetTypes() => [typeof(IRNSReloaded)];
}
