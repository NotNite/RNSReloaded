using System.Runtime.InteropServices;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded;

// ReSharper disable InconsistentNaming
public unsafe class Functions {
    public delegate int ScriptFindIdDelegate(char* name);
    public delegate CScript* ScriptDataDelegate(int id);
    public delegate byte CodeFunctionFindDelegate(char* name, int* id);
    public delegate void GetTheFunctionDelegate(int id, char** name, void** func, int* argumentCount);
    public delegate RValue* FindValueDelegate(CInstance* instance, char* name);
    public delegate RValue* ArrayGetEntryDelegate(nint array, int index);
    public delegate char* YYGetStringDelegate(RValue* value, int unk);
    public delegate int StructGetKeysDelegate(RValue* value, char** keys, int* count);
    public delegate int YYCreateStringDelegate(RValue* value, char* str);
    public delegate RValue* CallScriptFunctionDelegate(CInstance* self, CInstance* other, RValue* result, int num_args, int id, RValue** args, long _, long __);

    public ScriptFindIdDelegate ScriptFindId = null!;
    public ScriptDataDelegate ScriptData = null!;
    public CodeFunctionFindDelegate CodeFunctionFind = null!;
    public GetTheFunctionDelegate GetTheFunction = null!;
    public FindValueDelegate FindValue = null!;
    public ArrayGetEntryDelegate ArrayGetEntry = null!;
    public YYGetStringDelegate YYGetString = null!;
    public StructGetKeysDelegate StructGetKeys = null!;
    public YYCreateStringDelegate YYCreateString = null!;
    public CallScriptFunctionDelegate callScriptFunction = null!;

    private ScanUtils utils;
    private WeakReference<IStartupScanner> scannerRef;

    public Functions(ScanUtils utils, WeakReference<IStartupScanner> scannerRef) {
        this.utils = utils;
        this.scannerRef = scannerRef;

        this.utils.Scan("E8 ?? ?? ?? ?? 83 F8 FF 0F 84 ?? ?? ?? ?? 33 F6",
            addr => { this.ScriptFindId = Marshal.GetDelegateForFunctionPointer<ScriptFindIdDelegate>(addr); });
        this.utils.Scan("E8 ?? ?? ?? ?? EB 39 8B C8",
            addr => { this.ScriptData = Marshal.GetDelegateForFunctionPointer<ScriptDataDelegate>(addr); });
        this.utils.Scan("E8 ?? ?? ?? ?? 44 8B 45 7F",
            addr => { this.CodeFunctionFind = Marshal.GetDelegateForFunctionPointer<CodeFunctionFindDelegate>(addr); });
        this.utils.Scan("3B 0D ?? ?? ?? ?? 7F 3F",
            addr => { this.GetTheFunction = Marshal.GetDelegateForFunctionPointer<GetTheFunctionDelegate>(addr); });
        this.utils.Scan("E8 ?? ?? ?? ?? 48 85 C0 74 9B",
            addr => { this.FindValue = Marshal.GetDelegateForFunctionPointer<FindValueDelegate>(addr); });
        this.utils.Scan("E8 ?? ?? ?? ?? 48 8B D8 EB 4F",
            addr => { this.ArrayGetEntry = Marshal.GetDelegateForFunctionPointer<ArrayGetEntryDelegate>(addr); });
        this.utils.Scan("48 89 5C 24 20 57 48 83 EC 20 48 63 FA",
            addr => { this.YYGetString = Marshal.GetDelegateForFunctionPointer<YYGetStringDelegate>(addr); });
        this.utils.Scan("48 83 EC 38 48 89 74 24 ??",
            addr => { this.StructGetKeys = Marshal.GetDelegateForFunctionPointer<StructGetKeysDelegate>(addr); });
        this.utils.Scan("E8 ?? ?? ?? ?? 8B 7D 87",
            addr => { this.YYCreateString = Marshal.GetDelegateForFunctionPointer<YYCreateStringDelegate>(addr); });
        this.utils.Scan("40 55 41 54 41 55 41 56 41 57 48 83 EC ?? 48 8D 6C 24 ?? 48 89 5D ?? 48 89 75 ?? 48 89 7D ?? 48 8B 05 ?? ?? ?? ?? 48 33 C5 48 89 45 ?? 48 63 45", addr => {
            this.callScriptFunction = Marshal.GetDelegateForFunctionPointer<CallScriptFunctionDelegate>(addr); });
    }
}
