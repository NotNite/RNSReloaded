using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

// ReSharper disable InconsistentNaming
public unsafe class Hooks : IDisposable {
    public SLLVMVars* LLVMVars = null;
    public bool LimitOnlinePlay = false;

    // Not a great place to put static variables...
    public CRoom** CurrentRoom = null;

    private ScanUtils utils;
    private WeakReference<IReloadedHooks> hooksRef;

    private delegate nint InitLLVMDelegate(SLLVMVars* vars);
    private IHook<InitLLVMDelegate>? initLLVMHook;

    private delegate void RunStartDelegate();
    private IHook<RunStartDelegate>? runStartHook;

    private delegate byte ExecuteItDelegate(
        CInstance* self, CInstance* other, CCode* code, RValue* arguments, int flags
    );

    private IHook<ExecuteItDelegate>? executeItHook;

    private IHook<ScriptDelegate>? protobuildHook;

    public event Action? OnRunStart;
    public event Action<ExecuteItArguments>? OnExecuteIt;

    public Hooks(ScanUtils utils, WeakReference<IReloadedHooks> hooksRef) {
        this.utils = utils;
        this.hooksRef = hooksRef;

        if (this.hooksRef.TryGetTarget(out var hooks)) {
            this.utils.Scan("E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 8B 41 14", addr => {
                this.initLLVMHook = hooks.CreateHook<InitLLVMDelegate>(this.InitLLVMDetour, addr);
                this.initLLVMHook.Enable();
                this.initLLVMHook.Activate();
            });

            this.utils.Scan("48 83 EC ?? 80 3D ?? ?? ?? ?? ?? 75 ?? 33 C9", addr => {
                this.runStartHook = hooks.CreateHook<RunStartDelegate>(this.RunStartDetour, addr);
                this.runStartHook.Enable();
                this.runStartHook.Activate();
            });

            this.utils.Scan("48 8B 15 ?? ?? ?? ?? 48 85 D2 41 0F 95 C6", addr => {
                var offset = Marshal.ReadInt32(addr + 3);
                this.CurrentRoom = (CRoom**) (addr + 7 + offset);
            });

            this.utils.Scan("E8 ?? ?? ?? ?? 0F B6 D8 3C 01", addr => {
                this.executeItHook = hooks.CreateHook<ExecuteItDelegate>(this.ExecuteItDetour, addr);
                this.executeItHook.Enable();
                this.executeItHook.Activate();
            });
        }
    }

    public void Dispose() {
        this.initLLVMHook?.Disable();
        this.runStartHook?.Disable();
        this.protobuildHook?.Disable();
    }

    private nint InitLLVMDetour(SLLVMVars* vars) {
        var orig = this.initLLVMHook!.OriginalFunction(vars);
        this.LLVMVars = vars;
        return orig;
    }

    private void RunStartDetour() {
        this.OnRunStart?.Invoke();

        // We need to set this slightly later so the version variable is set
        if (this.hooksRef.TryGetTarget(out var hooks) && this.LimitOnlinePlay) {
            var id = IRNSReloaded.Instance.ScriptFindId("protobuild_new");
            var script = IRNSReloaded.Instance.GetScriptData(id - 100000);

            this.protobuildHook = hooks.CreateHook<ScriptDelegate>(this.ProtobuildDetour, script->Functions->Function);
            this.protobuildHook.Enable();
            this.protobuildHook.Activate();
        }

        this.runStartHook!.OriginalFunction();
    }

    private RValue* ProtobuildDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        RValue global = IRNSReloaded.Instance.GetGlobalInstance();
        var current = global["onlineVersion"];
        const int separator = 10000;
        if (current->Real <= separator) current->Real += separator;

        returnValue = this.protobuildHook!.OriginalFunction(self, other, returnValue, argc, argv);
        return returnValue;
    }

    private byte ExecuteItDetour(CInstance* self, CInstance* other, CCode* code, RValue* arguments, int flags) {
        this.OnExecuteIt?.Invoke(new ExecuteItArguments {
            Self = self,
            Other = other,
            Code = code,
            Arguments = arguments,
            Flags = flags
        });

        return this.executeItHook!.OriginalFunction(self, other, code, arguments, flags);
    }
}
