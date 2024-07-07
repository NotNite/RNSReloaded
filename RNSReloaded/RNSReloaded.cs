using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using RNSReloaded.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

// ReSharper disable InconsistentNaming
public unsafe class RNSReloaded : IRNSReloaded, IDisposable {
    public event Action? OnReady;
    public event Action<ExecuteItArguments>? OnExecuteIt;

    private WeakReference<IReloadedHooks> hooksRef;
    private WeakReference<IStartupScanner> scannerRef;
    private ILoggerV1 logger;

    private ScanUtils scanUtils;
    private Hooks hooks;
    private Functions functions;

    public IUtil utils { get; private set; }
    public IBattleScripts battleScripts { get; private set; }
    public IBattlePatterns battlePatterns { get; private set; }

    public RNSReloaded(
        WeakReference<IReloadedHooks> hooksRef,
        WeakReference<IStartupScanner> scannerRef,
        ILoggerV1 logger
    ) {
        this.hooksRef = hooksRef;
        this.scannerRef = scannerRef;
        this.logger = logger;

        this.scanUtils = new ScanUtils(scannerRef, logger);
        this.hooks = new Hooks(this.scanUtils, hooksRef);
        this.functions = new Functions(this.scanUtils, scannerRef);

        this.hooks.OnRunStart += this.OnRunStart;
        this.hooks.OnExecuteIt += this.OnExecuteItWrapper;
        IRNSReloaded.Instance = this;

        this.utils = new Util(this, this.logger);
        this.battleScripts = new BattleScripts(this, this.utils, this.logger);
        this.battlePatterns = new BattlePatterns(this, this.utils, this.logger);
    }

    public void Dispose() {
        this.hooks.OnRunStart -= this.OnRunStart;
        this.hooks.Dispose();

        this.hooksRef = null!;
        this.scannerRef = null!;
        this.logger = null!;
    }

    private void OnRunStart() {
        this.OnReady?.Invoke();
    }

    public void LimitOnlinePlay() {
        this.hooks.LimitOnlinePlay = true;
    }

    public CScript* GetScriptData(int id) {
        return this.functions.ScriptData(id);
    }

    public int ScriptFindId(string name) {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        var ret = this.functions.ScriptFindId((char*) namePtr);
        Marshal.FreeHGlobal(namePtr);
        return ret;
    }

    public int? CodeFunctionFind(string name) {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        var id = 0;
        var ret = this.functions.CodeFunctionFind((char*) namePtr, &id);
        Marshal.FreeHGlobal(namePtr);
        return ret == 1 ? id : null;
    }

    public RFunctionStringRef GetTheFunction(int id) {
        char* name = null;
        void* func = null;
        int argCount = 0;
        this.functions.GetTheFunction(id, &name, &func, &argCount);
        return new RFunctionStringRef {
            Name = name,
            Routine = func,
            ArgumentCount = argCount
        };
    }

    public CInstance* GetGlobalInstance() {
        var id = this.CodeFunctionFind("@@GlobalScope@@")!.Value;
        var funcRef = this.GetTheFunction(id);
        var func = Marshal.GetDelegateForFunctionPointer<RoutineDelegate>((nint) funcRef.Routine);
        var returnValue = new RValue();
        func(&returnValue, null, null, 0, null);
        return returnValue.Object;
    }

    public RValue* FindValue(CInstance* instance, string name) {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        var ret = this.functions.FindValue(instance, (char*) namePtr);
        Marshal.FreeHGlobal(namePtr);
        return ret;
    }

    public RValue* ArrayGetEntry(RValue* array, int index) {
        if (array->Type != RValueType.Array) return null;
        return this.functions.ArrayGetEntry(array->Pointer, index);
    }

    public RValue? ArrayGetLength(RValue* array) {
        return this.ExecuteCodeFunction("array_length", null, null, 1, (RValue**) array);
    }

    public string GetString(RValue* value) {
        if (value == null) return "nullptr";
        if (value->Type == RValueType.Unset) return "unset";
        var ptr = this.functions.YYGetString(value, 0);
        return Marshal.PtrToStringAnsi((nint) ptr)!;
    }

    public CRoom* GetCurrentRoom() {
        return this.hooks.CurrentRoom == null
                   ? null
                   : *this.hooks.CurrentRoom;
    }

    public List<string> GetStructKeys(RValue* value) {
        var ret = new List<string>();
        var count = this.functions.StructGetKeys(value, null, null);
        var keys = new char*[count];

        fixed (char** keysPtr = keys) {
            this.functions.StructGetKeys(value, keysPtr, &count);
            for (var i = 0; i < count; i++) {
                ret.Add(Marshal.PtrToStringAnsi((nint) keys[i])!);
            }
        }

        return ret;
    }

    public void CreateString(RValue* value, string str) {
        var strPtr = Marshal.StringToHGlobalAnsi(str);
        this.functions.YYCreateString(value, (char*) strPtr);
        Marshal.FreeHGlobal(strPtr);
    }

    public RValue? ExecuteScript(string name, CInstance* self, CInstance* other, int argc, RValue** argv) {
        var script = this.ScriptFindId(name);
        if (script == -1) return null;

        var scriptData = this.GetScriptData(script - 100000);
        if (scriptData == null) return null;

        var funcRef = scriptData->Functions->Function;
        var func = Marshal.GetDelegateForFunctionPointer<ScriptDelegate>(funcRef);
        var result = new RValue();
        func(self, other, &result, argc, argv);
        return result;
    }

    public RValue? ExecuteScript(string name, CInstance* self, CInstance* other, RValue[] arguments) {
        fixed (RValue* ptr = arguments) {
            var ptrs = new RValue*[arguments.Length];
            for (var i = 0; i < arguments.Length; i++) ptrs[i] = &ptr[i];
            fixed (RValue** argv = ptrs) {
                return this.ExecuteScript(name, self, other, arguments.Length, argv);
            }
        }
    }

    public RValue? ExecuteCodeFunction(string name, CInstance* self, CInstance* other, int argc, RValue** argv) {
        var id = this.CodeFunctionFind(name);
        if (id == null) return null;

        var funcRef = this.GetTheFunction(id.Value);
        var func = Marshal.GetDelegateForFunctionPointer<RoutineDelegate>((nint) funcRef.Routine);
        if (func == null) return null;
        RValue result;
        func(&result, self, other, argc, argv);
        return result;
    }

    public RValue? ExecuteCodeFunction(string name, CInstance* self, CInstance* other, RValue[] arguments) {
        fixed (RValue* ptr = arguments) {
            var ptrs = new RValue*[arguments.Length];
            for (var i = 0; i < arguments.Length; i++) ptrs[i] = &ptr[i];
            fixed (RValue** argv = ptrs) {
                return this.ExecuteCodeFunction(name, self, other, arguments.Length, argv);
            }
        }
    }

    public void OnExecuteItWrapper(ExecuteItArguments obj) {
        OnExecuteIt?.Invoke(obj);
    }
}
