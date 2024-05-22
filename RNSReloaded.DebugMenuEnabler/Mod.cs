using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Memory.Sigscan.Definitions;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.DebugMenuEnabler.Config;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.DebugMenuEnabler;

public class Mod : IMod {
    private WeakReference<IScannerFactory>? scannerRef;
    private WeakReference<IRNSReloaded>? rnsReloadedRef;

    private Configurator configurator = null!;
    private Config.Config config = null!;

    private bool upgradeEnabled;
    private bool encEnabled;
    private bool dialogEnabled;
    private bool itemEnabled;
    private bool item2Enabled;
    private bool statEnabled;

    private nint baseAddr;

    public void StartEx(IModLoaderV1 loader, IModConfigV1 modConfig) {
        this.scannerRef = loader.GetController<IScannerFactory>()!;
        this.rnsReloadedRef = loader.GetController<IRNSReloaded>()!;

        if (this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            rnsReloaded.LimitOnlinePlay();
            rnsReloaded.OnExecuteIt += this.OnExecuteIt;
        }

        this.configurator = new Configurator(((IModLoader) loader).GetModConfigDirectory(modConfig.ModId));
        this.config = this.configurator.GetConfiguration<Config.Config>(0);
        this.config.ConfigurationUpdated += this.ConfigurationUpdated;

        this.upgradeEnabled = this.config.Upgrade;
        this.encEnabled = this.config.Enc;
        this.dialogEnabled = this.config.Dialog;
        this.itemEnabled = this.config.Item;
        this.item2Enabled = this.config.Item;
        this.statEnabled = this.config.Stat;

        this.baseAddr = Process.GetCurrentProcess().MainModule!.BaseAddress;
    }

    private void ConfigurationUpdated(IUpdatableConfigurable newConfig) {
        this.config = (Config.Config) newConfig;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(nint lpAddress, nint dwSize, uint flNewProtect, out uint lpflOldProtect);

    private unsafe void OnExecuteIt(ExecuteItArguments args) {
        var name = Marshal.PtrToStringAnsi((nint) args.Code->Name)!;

        var enc = name == "gml_Object_obj_encDebugMenu_Create_0";
        var dialog = name == "gml_Object_obj_dialogDebugMenu_Create_0";
        var upgrade = name == "gml_Object_obj_upgradeDebugMenu_Create_0";
        var item = name == "gml_Object_obj_itemDebugMenu_Create_0";
        var item2 = name == "gml_Object_obj_itemDebugMenu_Step_0";
        var stat = name == "gml_Object_obj_statDebugMenu_Create_0";

        if (!enc && !dialog && !upgrade && !item && !item2 && !stat) return;

        var isShown = new RValue(args.Self)["isShown"];
        isShown->Type = RValueType.Bool;
        isShown->Real = 1;

        if (this.rnsReloadedRef != null && this.rnsReloadedRef.TryGetTarget(out var rnsReloaded)) {
            RValue global = rnsReloaded.GetGlobalInstance();
            var devMode = global["devMode"];
            devMode->Type = RValueType.Bool;
            devMode->Real = 1;
        }

        bool Nop(string pattern, int offset, int size) {
            var func = args.Code->Functions->Function;
            var rva = (int) (func - this.baseAddr);
            if (this.scannerRef != null && this.scannerRef.TryGetTarget(out var factory)) {
                var scanner = factory.CreateScanner(Process.GetCurrentProcess());
                var result = scanner.FindPattern(pattern, rva);
                if (result.Found) {
                    var addr = this.baseAddr + result.Offset + offset;
                    VirtualProtect(addr, size, 0x40, out var old);
                    for (var i = 0; i < size; i++) Marshal.WriteByte(addr + i, 0x90);
                    VirtualProtect(addr, size, old, out _);
                    return true;
                }
            }

            return false;
        }

        // I don't think it can be expressed how cursed editing a function as you're about to call it is
        if ((enc && this.encEnabled) || (dialog && this.dialogEnabled)) {
            if (Nop("C7 45 ?? ?? ?? ?? ?? 0F 57 C0 F2 0F 11 45 ?? 48 8D 55 BF 48 8B C8 E8 ?? ?? ?? ??", 22, 5)) {
                if (enc) this.encEnabled = false;
                if (dialog) this.dialogEnabled = false;
            }
        }

        if (upgrade && this.upgradeEnabled) {
            if (Nop("C7 45 ?? ?? ?? ?? ?? 0F 57 F6 F2 0F 11 74 24 ?? 48 8D 54 24 ?? 48 8B C8 E8 ?? ?? ?? ??", 24, 5)) {
                this.upgradeEnabled = false;
            }
        }

        if (item && this.itemEnabled) {
            if (Nop("C7 44 24 ?? ?? ?? ?? ?? 0F 57 F6 F2 0F 11 74 24 ?? 48 8D 54 24 ?? 48 8B C8 E8 ?? ?? ?? ??", 25,
                    5)) {
                this.itemEnabled = false;
            }
        }

        if (item2 && this.item2Enabled) {
            if (Nop("C7 45 ?? ?? ?? ?? ?? F2 44 0F 11 45 ?? 48 8D 55 F0 48 8B C8 E8 ?? ?? ?? ??", 20, 5)) {
                this.item2Enabled = false;
            }
        }

        if (stat && this.statEnabled) {
            if (Nop("C7 45 ?? ?? ?? ?? ?? 0F 57 F6 F2 0F 11 75 ?? 48 8D 55 AF 48 8B C8 E8 ?? ?? ?? ??", 22, 5)) {
                this.statEnabled = false;
            }
        }
    }


    public void Suspend() { }
    public void Resume() { }
    public bool CanSuspend() => false;

    public void Unload() { }
    public bool CanUnload() => false;

    public Action Disposing => () => { };
}
