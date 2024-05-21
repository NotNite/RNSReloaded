// ReSharper disable InconsistentNaming

using System.Runtime.InteropServices;

namespace RNSReloaded.Interfaces.Structs;

public unsafe delegate RValue* ScriptDelegate(
    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
);

public unsafe delegate void RoutineDelegate(
    RValue* returnValue, CInstance* self, CInstance* other, int argc, RValue** argv
);

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct SLLVMVars {
    [FieldOffset(0x0C)] public int NumGlobalVariables;
    [FieldOffset(0x18)] public YYVAR** Variables;
    [FieldOffset(0x20)] public YYVAR** Functions;
    [FieldOffset(0x28)] public YYVAR** GMLFunctions;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct YYVAR {
    public char* Name;
    public int ID;
}

public enum RValueType : uint {
    Real = 0,
    String = 1,
    Array = 2,
    Pointer = 3,
    Vec33 = 4,
    Undefined = 5,
    Object = 6,
    Int32 = 7,
    Vec4 = 8,
    Matrix = 9,
    Int64 = 10,
    Accessor = 11,
    Null = 12,
    Bool = 13,
    Iterator = 14,
    Ref = 15,
    Unset = 0x0ffffff
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public unsafe struct RValue {
    [FieldOffset(0x0)] public int Int32;
    [FieldOffset(0x0)] public long Int64;
    [FieldOffset(0x0)] public double Real;
    [FieldOffset(0x0)] public CInstance* Object;
    [FieldOffset(0x0)] public nint Pointer;

    [FieldOffset(0x8)] public uint Flags;
    [FieldOffset(0xC)] public RValueType Type;

    public RValue* Get(int index) {
        if (this.Type != RValueType.Array) return null;
        fixed (RValue* ptr = &this) {
            return IRNSReloaded.Instance.ArrayGetEntry(ptr, index);
        }
    }

    public RValue* Get(string key) {
        if (this.Type != RValueType.Object) return null;
        return IRNSReloaded.Instance.FindValue(this.Object, key);
    }

    // Thanks C# for making indexing a pointer with a number impossible lol
    public RValue* this[int index] => this.Get(index);
    public RValue* this[string key] => this.Get(key);

    // Constructors
    public RValue(CInstance* obj) {
        this.Type = RValueType.Object;
        this.Object = obj;
        this.Flags = 0;
    }

    public static implicit operator RValue(CInstance* obj) => new(obj);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CInstance;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CCode;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CScript {
    public nint VTable;
    public CCode* Code;
    public YYGMLFuncs* Functions;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct YYGMLFuncs {
    public char* Name;
    public nint Function;
    public YYVAR* FunctionVariable;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct RFunctionStringRef {
    public char* Name;
    public void* Routine;
    public int ArgumentCount;
}
