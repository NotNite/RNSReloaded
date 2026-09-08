using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Drawing;
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

    private List<string> fakeVanillaMods = [];
    private IHook<ScriptDelegate>? readsheetExternalHook;

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
        if (!this.addScriptHook("scr_readsheet_external", this.ReadsheetExternalDetour, out this.readsheetExternalHook)) {
            this.logger.PrintMessage("RNSReloaded: Failed to hook readsheet_external", Color.Red);
        }
    }

    public void LimitOnlinePlay() {
        this.hooks.LimitOnlinePlay = true;
    }

    // Forces game to behave as if a workshop mod is active
    // Thus turns off unlocks and marks lobby as modded
    // TODO: maybe put the mod in the mods list ingame as well?
    public void registerVanillaMod(string name) {
        this.fakeVanillaMods.Add(name);
    }

    public bool addScriptHook(string name, ScriptDelegate detour, [NotNullWhen(true)] out IHook<ScriptDelegate>? hook) {
        hook = null;
        var script = this.GetScriptData(this.ScriptFindId(name) - 100000);
        if (script == null) return false;
        if (this.hooksRef.TryGetTarget(out var hooks)) {
            hook = hooks.CreateHook<ScriptDelegate>(detour, script->Functions->Function)!;
            hook.Activate();
            return true;
        } else {
            return false;
        }
    }

    public bool addRoutineHook(string name, RoutineDelegate detour, [NotNullWhen(true)] out IHook<RoutineDelegate>? hook) {
        hook = null;
        var func = this.CodeFunctionFind(name);
        if (func == null) return false;
        var routine = this.GetTheFunction(func!.Value);
        if (this.hooksRef.TryGetTarget(out var hooks)) {
            hook = hooks.CreateHook<RoutineDelegate>(detour, (nint)routine.Routine)!;
            hook.Activate();
            return true;
        } else {
            return false;
        }
    }

    private RValue* ReadsheetExternalDetour(CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv) {
        returnValue = this.readsheetExternalHook!.OriginalFunction(self, other, returnValue, argc, argv);
        // Console.WriteLine($"RR: registering {this.fakeVanillaMods.Count} fake mods");
        if (this.fakeVanillaMods.Count == 0) return returnValue;
        var unlockEnabled = this.FindGlobalValue("modUnlockEnabled");
        unlockEnabled->Type = RValueType.Bool;
        unlockEnabled->Real = 0.0;
        var multiActiveCount = this.FindGlobalValue("modsMultiplayerActive");
        multiActiveCount->Real += (double)this.fakeVanillaMods.Count;
        return returnValue;
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

    public RValue* FindGlobalValue(string name) {
        return this.FindValue(this.GetGlobalInstance(), name);
    }

    public RValue* ArrayGetEntry(RValue* array, int index) {
        if (array == null) return null;
        if (array->Type != RValueType.Array) return null;
        unsafe {
            // Dereference the pointer field, then add size_t to get a pointer to the data array
            var arrPtr = array->Pointer + 8;
            var arr = *(nint*)arrPtr;
            // Then it's just a pure array of RValues so we calculate size and return a pointer to the correct index
            return (RValue*) (arr + index * sizeof(RValue));
        }
    }

    public RValue? ArrayGetLength(RValue* array) {
        return this.ExecuteCodeFunction("array_length", null, null, 1, array);
    }

    public string GetString(RValue* value) {
        if (value == null) return "nullptr";
        if (value->Type == RValueType.Unset) return "unset";
        var ptr = this.functions.YYGetString(value, 0);
        return Marshal.PtrToStringUTF8((nint) ptr) ?? "";
    }

    public CRoom* GetCurrentRoom() {
        return this.hooks.CurrentRoom == null
                   ? null
                   : *this.hooks.CurrentRoom;
    }

    public List<string> GetStructKeys(RValue* value) {
        if (value->Type != RValueType.Object) return [];
        RValue[] args = [value->Object];
        RValue arr = IRNSReloaded.Instance.ExecuteCodeFunction("variable_instance_get_names", null, null, args) ?? new RValue([]);
        List<string> ret = [];
        foreach (var key in arr.IterateArray()) {
            ret.Add(key.ToString());
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

    public RValue? ExecuteCodeFunction(string name, CInstance* self, CInstance* other, int argc, RValue* argv) {
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
            return this.ExecuteCodeFunction(name, self, other, arguments.Length, ptr);
        }
    }

    public RValue? ExecuteCodeFunction(string name, RValue[] arguments) {
        return this.ExecuteCodeFunction(name, null, null, arguments);
    }

    public void OnExecuteItWrapper(ExecuteItArguments obj) {
        OnExecuteIt?.Invoke(obj);
    }
}
