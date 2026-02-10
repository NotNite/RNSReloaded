using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace RNSReloaded.Interfaces.Structs;

public unsafe delegate RValue* ScriptDelegate(
    CInstance* self, CInstance* other, RValue* returnValue, int argc, RValue** argv
);

public unsafe delegate void RoutineDelegate(
    RValue* returnValue, CInstance* self, CInstance* other, int argc, RValue* argv
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

[StructLayout(LayoutKind.Explicit, Pack = 8)]
public unsafe struct RValue {
    [FieldOffset(0x0)] public int Int32;
    [FieldOffset(0x0)] public long Int64;
    [FieldOffset(0x0)] public double Real;
    [FieldOffset(0x0)] public bool Bool;
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

    public override string ToString() {
        fixed (RValue* ptr = &this) {
            return IRNSReloaded.Instance.GetString(ptr);
        }
    }

    // Constructors
    public RValue(CInstance* obj) {
        this.Type = RValueType.Object;
        this.Object = obj;
        this.Flags = 0;
    }

    public RValue(int value) {
        this.Type = RValueType.Int32;
        this.Int32 = value;
        this.Flags = 0;
    }

    public RValue(double value) {
        this.Type = RValueType.Real;
        this.Real = value;
        this.Flags = 0;
    }

    public RValue(bool value) {
        this.Type = RValueType.Bool;
        this.Real = value ? 1.0 : 0;
        this.Flags = 0;
    }

    public static implicit operator RValue(CInstance* obj) => new(obj);

    // TODO: creating other primitives, new objects, and new arrays
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CInstance; // TODO

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CScript {
    public nint VTable;
    public CCode* Code;
    public YYGMLFuncs* Functions;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct YYGMLFuncs {
    public char* Name;
    public nint Function;
    public YYVAR* FunctionVariable;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct RFunctionStringRef {
    public char* Name;
    public void* Routine;
    public int ArgumentCount;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LinkedList<T> where T : unmanaged {
    public T* First;
    public T* Last;
    public int Count;

    // TODO: enumerator
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CArrayStructure<T> where T : unmanaged {
    public int Length;
    public T* Data;

    public T this[int index] {
        get => this.Data[index];
        set => this.Data[index] = value;
    }

    // TODO: enumerator
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CHashMap<K, V> where K : unmanaged where V : unmanaged {
    public int Size;
    public int UsedCount;
    public int CurrentMask;
    public int GrowThreshold;
    public CHashMapElement<K, V>* Elements;
    public delegate* unmanaged<K*, V*, void> DeleteValue;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CHashMapElement<K, V> where K : unmanaged where V : unmanaged {
    public V* Value;
    public K Key;
    public uint Hash;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CRoom {
    public int LastTile;
    public CRoom* InstanceHandle;
    public char* Caption;
    public int Speed;
    public int Width;
    public int Height;
    public byte Persistent;
    public byte EnableViews;
    public byte ClearScreen;
    public byte ClearDisplayBuffer;
    public fixed long Views[8];
    public char* LegacyCode;
    public CCode* CodeObject;
    public byte HasPhysicsWorld;
    public int PhysicsGravityX;
    public int PhysicsGravityY;
    public float PhysicsPixelToMeters;
    public LinkedList<CInstance> ActiveInstances;
    public LinkedList<CInstance> InactiveInstances;
    public CInstance* MarkedFirst;
    public CInstance* MarkedLast;
    public int* CreationOrderList;
    public int CreationOrderListSize;
    public YYRoom* WadRoom;
    // These 4 commented fields probably aren't the correct ones that were removed
    // But also IDK what the right ones are and this causes Name and Layers to align properly sooooo
    //public nint WadBaseAddress;
    public CPhysicsWorld* PhysicsWorld;
    public int TileCount;
    public CArrayStructure<RTile> Tiles;
    //public YYRoomTiles* WadTiles;
    //public YYRoomInstances* WadInstances;
    public char* Name;
    //public byte IsDuplicate;
    public LinkedList<CLayer> Layers;
    public CHashMap<int, CLayer> LayerLookup;
    public CHashMap<int, CLayerElementBase> LayerElementLookup;
    public CLayerElementBase* LastElementLookedUp;
    public CHashMap<int, CLayerInstanceElement> InstanceElementLookup;
    public int* SequenceInstanceIDs;
    public int SequenceInstanceIDCount;
    public int SequenceInstanceIDMax;
    public int* EffectLayerIDs;
    public int EffectLayerIDCount;
    public int EffectLayerIDMax;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CLayer {
    public int ID;
    public int Depth;
    public float XOffset;
    public float YOffset;
    public float HorizontalSpeed;
    public float VerticalSpeed;
    public byte Visible;
    public byte Deleting;
    public byte Dynamic;

    // Not sure 100% sure where this should be added or what it's used for, but this causes Name to align properly
    // And the previous bytes seem accurate too, so it's most likely here.
    // They also seem to be mostly null with the occasional int32 1 thrown in (nullptr x3, int32 0, int32 1, int32 1, int32 0)
    public fixed long Padding[5];

    public char* Name;
    public RValue BeginScript;
    public RValue EndScript;
    public byte EffectEnabled;
    public byte EffectPendingEnabled;
    public RValue Effect;
    public CLayerEffectInfo* InitialEffectInfo;
    public int ShaderID;

    public LinkedList<CLayerElementBase> Elements;
    public CLayer* Next;
    public CLayer* Previous;
    public nint GCProxy;
}

public enum LayerElementType {
    Instance = 2
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CLayerElementBase {
    public LayerElementType Type;
    public int ID;
    public byte RuntimeDataInitialized;
    public char* Name;
    public CLayer* Layer;
    public CLayerElementBase* Next;
    public CLayerElementBase* Previous;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CLayerInstanceElement {
    public CLayerElementBase Base;
    public int InstanceID;
    public CInstance* Instance;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CCode {
    public nint VTable;
    public CCode* Next;
    public int Kind;
    public int Compiled;
    public char* Str;
    public RToken Token;
    public RValue Value;
    public nint VMInstance;
    public nint VMDebugInfo;
    public char* Code;
    public char* Name;
    public int CodeIndex;
    public YYGMLFuncs* Functions;
    public byte Watch;
    public int Offset;
    public int LocalsCount;
    public int ArgsCount;
    public int Flags;
    public nint Prototype;
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CBackGM;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CView;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct YYRoom;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CPhysicsWorld;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct RTile;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct YYRoomTiles;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct YYRoomInstances;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CLayerEffectInfo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct RToken {
    public int Kind;
    public uint Type;
    public int Ind;
    public int Ind2;
    public RValue Value;
    public int ItemNumber;
    public RToken* Items;
    public int Position;
};

public unsafe struct ExecuteItArguments {
    public CInstance* Self;
    public CInstance* Other;
    public CCode* Code;
    public RValue* Arguments;
    public int Flags;
}
