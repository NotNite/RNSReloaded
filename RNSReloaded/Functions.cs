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

    public ScriptFindIdDelegate ScriptFindId = null!;
    public ScriptDataDelegate ScriptData = null!;
    public CodeFunctionFindDelegate CodeFunctionFind = null!;
    public GetTheFunctionDelegate GetTheFunction = null!;
    public FindValueDelegate FindValue = null!;
    public ArrayGetEntryDelegate ArrayGetEntry = null!;

    private Utils utils;
    private WeakReference<IStartupScanner> scannerRef;

    public Functions(Utils utils, WeakReference<IStartupScanner> scannerRef) {
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
    }
}
