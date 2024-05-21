using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace RNSReloaded.TextureSwapper;

public class Mod : IMod {
    private WeakReference<IReloadedHooks>? hooksRef;
    private WeakReference<IStartupScanner>? scannerRef;
    private ILoggerV1 logger = null!;

    private delegate long AllocTextureDelegate();
    private delegate nint CreateTextureFromFileDelegate(nint a1, nint a2, nint a3, nint a4, byte a5);

    private IHook<AllocTextureDelegate>? allocTextureHook;
    private IHook<CreateTextureFromFileDelegate>? createTextureFromFileHook;

    private long lastTexture;
    private string dir = null!;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.hooksRef = loader.GetController<IReloadedHooks>()!;
        this.scannerRef = loader.GetController<IStartupScanner>()!;
        this.logger = loader.GetLogger();

        this.dir = ((IModLoader) loader).GetModConfigDirectory(modConfig.ModId);

        var baseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress;
        if (this.scannerRef.TryGetTarget(out var scanner) && this.hooksRef.TryGetTarget(out var hooks)) {
            scanner.AddMainModuleScan("E8 ?? ?? ?? ?? 48 63 E8 84 DB", result => {
                var addr = baseAddr + result.Offset;
                addr += 5 + Marshal.ReadInt32(addr + 1);
                this.allocTextureHook = hooks.CreateHook<AllocTextureDelegate>(this.AllocTextureDetour, addr);
                this.allocTextureHook.Activate();
                this.allocTextureHook.Enable();
            });

            scanner.AddMainModuleScan("E8 ?? ?? ?? ?? F3 0F 10 15 ?? ?? ?? ?? 4C 8B C0", result => {
                var addr = baseAddr + result.Offset;
                addr += 5 + Marshal.ReadInt32(addr + 1);
                this.createTextureFromFileHook =
                    hooks.CreateHook<CreateTextureFromFileDelegate>(this.CreateTextureFromFileDetour, addr);
                this.createTextureFromFileHook.Activate();
                this.createTextureFromFileHook.Enable();
            });
        }
    }

    /*
        GR_Texture_Create calls AllocTexture and then CreateTextureFromFile immediately after.
        Since this texture initialization is called at game startup, the ID (at startup) will match the
        texture ID in the data.win file. CreateTextureFromFile checks the magic of the file it's reading
        so you can just pass in PNG data to it.
    */
    private long AllocTextureDetour() {
        var ret = this.allocTextureHook!.OriginalFunction();
        this.lastTexture = ret - 1; // What?
        return ret;
    }

    private nint CreateTextureFromFileDetour(nint a1, nint a2, nint a3, nint a4, byte a5) {
        var path = Path.Combine(this.dir, $"{this.lastTexture}.png");

        nint ret;
        if (File.Exists(path)) {
            this.lastTexture = -1;

            var data = File.ReadAllBytes(path);
            var mem = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, mem, data.Length);
            ret = this.createTextureFromFileHook!.OriginalFunction(mem, a2, a3, a4, a5);
        } else {
            ret = this.createTextureFromFileHook!.OriginalFunction(a1, a2, a3, a4, a5);
        }

        return ret;
    }

    public void Suspend() {
        this.allocTextureHook?.Disable();
        this.createTextureFromFileHook?.Disable();
    }

    public void Resume() {
        this.allocTextureHook?.Enable();
        this.createTextureFromFileHook?.Enable();
    }

    public bool CanSuspend() => true;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
