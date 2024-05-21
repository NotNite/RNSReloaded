using Reloaded.Hooks.Definitions;
using RNSReloaded.Interfaces.Structs;

namespace RNSReloaded.Interfaces;

public unsafe interface IRNSReloaded {
    public event Action? OnReady;
    public static IRNSReloaded Instance = null!;

    public CScript* GetScriptData(int id);
    public int ScriptFindId(string name);
    public int? CodeFunctionFind(string name);
    public RFunctionStringRef GetTheFunction(int id);
    public CInstance* GetGlobalInstance();
    public RValue* FindValue(CInstance* instance, string name);
    public RValue* ArrayGetEntry(RValue* array, int index);
}
