using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace RNSReloaded.Interfaces.Structs;

public unsafe interface ILinkedListElement<T> where T : unmanaged {
    T* GetNext();
    T* GetPrev();
}

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

    // TODO: what happens if index out of bounds?
    public void Set(int index, RValue value) {
        RValue[] args = [this, index, value];
        IRNSReloaded.Instance.ExecuteCodeFunction("array_set", null, null, args);
    }

    public bool Set(string key, RValue value) {
        if (this.Type != RValueType.Object) return false;
        RValue[] args = [this.Object->ID, new RValue(key), value];
        IRNSReloaded.Instance.ExecuteCodeFunction("variable_instance_set", null, null, args);
        return true;
    }

    // Thanks C# for making indexing a pointer with a number impossible lol
    public RValue* this[int index] {
        get => this.Get(index);
        set => this.Set(index, *value);
    }
    public RValue* this[string key] {
        get => this.Get(key);
        set => this.Set(key, *value);
    }

    /// Typesafe pointers I guess
    // public ref RValue Get(int index) {
    //     if (this.Type != RValueType.Array) {
    //         throw new NotSupportedException("Only RValues of type Array may be accessed via ints");
    //     }
    //     fixed (RValue* ptr = &this) {
    //         var value = IRNSReloaded.Instance.ArrayGetEntry(ptr, index);
    //         if (value == null) throw new IndexOutOfRangeException($"Failed to find RValue data at index {index}");
    //         return ref *value;
    //     }
    // }

    // public ref RValue Get(string key) {
    //     if (this.Type != RValueType.Object) {
    //         throw new NotSupportedException("Only RValues of type Object may be accessed via strings");
    //     }
    //     var value = IRNSReloaded.Instance.FindValue(this.Object, key);
    //     if (value == null) {
    //         throw new KeyNotFoundException();
    //     }
    //     return ref *value;
    // }


    // public ref RValue this[int index] => ref this.Get(index);
    // public ref RValue this[string key] {
    //     get => ref this.Get(key);
    //     // Why no set access on refs ;-;
    //     // set {
    //     //     try {
    //     //         ref var holder = ref this.Get(key);
    //     //         holder = value;
    //     //     } catch (KeyNotFoundException) {
    //     //         this.Set(key, value);
    //     //     }
    //     // }
    // }

    public int ArrayLength() {
        var length = IRNSReloaded.Instance.ExecuteCodeFunction("array_length", null, null, [this]) ?? 0;
        return (int)length;
    }

    public static RValue? ArrayCreate(int size = 0, RValue? defaultVal = null) {
        RValue[] args = [size];
        if (defaultVal != null) args = [..args, defaultVal.Value];
        var array = IRNSReloaded.Instance.ExecuteCodeFunction("array_create", null, null, args);
        return array;
    }

    public void ArrayPush(params RValue[] values) {
        if (values.Count() == 0) {
            return;
        }
        RValue[] args = [this, values[0]];
        if (values.Count() > 1) {
            args = [..args, ..values[1..]];
        }
        IRNSReloaded.Instance.ExecuteCodeFunction("array_push", null, null, args);
    }

    public RValue ArrayPop() {
        var ret = IRNSReloaded.Instance.ExecuteCodeFunction("array_push", null, null, [this]);
        return ret ?? new RValue();
    }

    public override string ToString() {
        fixed (RValue* ptr = &this) {
            return IRNSReloaded.Instance.GetString(ptr);
        }
    }

    // Technically this should cast ints to uint and return long
    public int ToInt(int defaultValue = 0) {
        return this.Type switch {
            RValueType.Real => (int) this.Real,
            RValueType.Int32 => this.Int32,
            RValueType.Int64 => this.Int32,
            RValueType.Bool => (int) this.Real,
            _ => defaultValue,
        };
    }

    // See INT64_RValue in a YYC decompilation (might need debug symbols)
    private static long RealToLong(double value) {
        if (Double.IsNaN(value)) {
            return 0;
        } else if (Double.IsPositiveInfinity(value)) {
            return long.MaxValue;
        } else if (Double.IsNegativeInfinity(value)) {
            return long.MinValue;
        } else {
            return (int)value;
        }
    }

    public long ToLong(long defaultValue = 0) {
        return this.Type switch {
            RValueType.Real => RealToLong(this.Real),
            RValueType.Int32 => this.Int32,
            RValueType.Int64 => this.Int64,
            RValueType.Bool => RealToLong(this.Real),
            _ => defaultValue,
        };
    }

    public double ToDouble(double defaultValue = 0) {
        return this.Type switch {
            RValueType.Real => this.Real,
            RValueType.Int32 => (double) this.Int32,
            RValueType.Int64 => (double)(int) this.Int64,
            RValueType.Bool => this.Real,
            _ => defaultValue,
        };
    }

    public bool ToBool(bool defaultValue = false) {
        return this.Type switch {
            RValueType.Real => this.Real > 0.5,
            RValueType.Int32 => this.Int32 > 0,
            RValueType.Int64 => this.Int64 > 0,
            RValueType.Bool => this.Real > 0.5,
            _ => defaultValue,
        };
    }

    public Span<RValue> IterateArray() {
        if (this.Type != RValueType.Array) return Span<RValue>.Empty;
        var length = this.ArrayLength();
        unsafe {
            var arrPtr = this.Pointer + 8;
            var arr = (RValue*) *(nint*) arrPtr;
            return new Span<RValue>(arr, length);
        }
    }

    public RValue[] ToArray() {
        if (this.Type != RValueType.Array) return [];
        return this.IterateArray().ToArray();
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

    public RValue(long value) {
        this.Type = RValueType.Int64;
        this.Int64 = value;
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

    public RValue() {
        this.Type = RValueType.Undefined;
        this.Int64 = 0;
        this.Flags = 0;
    }

    public RValue(RValue[] array) {
        this = ArrayCreate() ?? new RValue();
        this.ArrayPush(array);
    }

    public RValue(string value) {
        this = IRNSReloaded.Instance.utils.CreateString(value) ?? new RValue();
    }

    public static implicit operator RValue(CInstance* obj) => new(obj);
    public static implicit operator RValue(int value) => new(value);
    public static implicit operator RValue(long value) => new(value);
    public static implicit operator RValue(double value) => new(value);
    public static implicit operator RValue(bool value) => new(value);
    public static implicit operator RValue(RValue[] value) => new(value);
    public static implicit operator RValue(string value) => new(value);

    public static explicit operator int(RValue value) => value.ToInt();
    public static explicit operator long(RValue value) => value.ToLong();
    public static explicit operator double(RValue value) => value.ToDouble();
    public static explicit operator bool(RValue value) => value.ToBool();

    // TODO: creating other primitives, new objects
    // TODO: instance enumerator
}

// CInstances from functions are laid out in 512 byte chunks at least
[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 512)]
public unsafe struct CInstance : ILinkedListElement<CInstance> {
    // TODO
    // 3 pointers related to vtables (see pdb types)
    fixed byte Padding[176];
    // Alarms probably
    // Should also contain the following at some point
    // CLayer* layer;
    // float collision_space;

    /// CInstance "internals" likely start here

    // Overview of the flags:
    // 0. ?
    // 1. persistent
    // 2. solid?
    // 3. visible
    // 4. not-solid?
    // 5-7. ?
    // 8. solid?
    // 9-31. ?
    // Should have on_ui_layer
    public int instanceFlags;
    public int ID;
    public int objectIndex;
    public int spriteIndex;
    public float sequencePosition;
    public float lastSequencePosition;
    public float sequenceDirection;
    public float imageIndex;
    public float time;
    public float imageSpeed;
    public float scaleX;
    public float scaleY;
    public float rotation;
    public float alpha;
    public int color; // or blend
    public float x;
    public float y;
    public float startx;
    public float starty;
    public float xprevious;
    public float yprevious;
    public float direction;
    public float speed;
    public float friction;
    public float gravity_direction;
    public float gravity;
    public float hspeed;
    public float vspeed;
    // Bounding box
    public float BoxLeft;
    public float BoxTop;
    public float BoxRight;
    public float BoxBottom;
    public fixed int timers[12];
    public Int16 rollbackFrameKilled;
    // 2 padding bytes
    public void* TimelinePath;
    public CCode* initCode;
    public CCode* precreateCode;
    public CObjectGM* oldObject;
    public int layerID;
    public int maskIndex;
    public Int16 mouseOverCount;
    // 2 padding bytes
    public CInstance* Flink;
    public CInstance* Blink;
    fixed long pointers[9];
    public float depth;
    public float Padding6;
    public int Padding7;

    public CInstance* GetNext() => throw new NotImplementedException();
    public CInstance* GetPrev() => throw new NotImplementedException();
}

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
public unsafe struct LinkedList<T> where T : unmanaged, ILinkedListElement<T> {
    public T* First;
    public T* Last;
    public int Count;

    public Enumerator GetEnumerator() => new Enumerator(this);

    public struct Enumerator {
        private LinkedList<T> list;
        private T* curr;
        private bool first;

        internal Enumerator(LinkedList<T> list) {
            this.list = list;
            this.curr = list.First;
            this.first = true;
        }

        public T* Current => this.curr;

        // Contract says it should be run once before it points to first element
        public bool MoveNext() {
            if (!this.first) {
                this.curr = this.curr->GetNext();
            }
            this.first = false;
            return this.curr != null;
        }

        public void Reset() {
            this.curr = this.list.First;
            this.first = true;
        }

        public void Dispose() { }
    }
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
public unsafe struct CLayer : ILinkedListElement<CLayer> {
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

    public CLayer* GetNext() => this.Next;
    public CLayer* GetPrev() => this.Previous;

    public List<RValue> GetInstances() {
        RValue[] instances = [];
        foreach (var element in this.Elements) {
            if (element->Type != LayerElementType.Instance) { continue; }
            var instance = (CLayerInstanceElement*) element;
            instances = [..instances, instance->Instance];
        }
        return instances.ToList();
    }

    public string GetName() {
        if (this.Name == null) {
            return "Unnamed";
        }
        return Marshal.PtrToStringAnsi((nint) this.Name) ?? "Unnamed";
    }
}

public enum LayerElementType {
    Instance = 2
}

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CLayerElementBase : ILinkedListElement<CLayerElementBase> {
    public LayerElementType Type;
    public int ID;
    public byte RuntimeDataInitialized;
    public byte Unknown; // Unsure what this is but it's set to 1 so probably a byte
    public char* Name;
    public CLayer* Layer;
    public fixed long Padding[2]; // Both seem null
    public CLayerElementBase* Next;
    public CLayerElementBase* Previous;
    // Random 3 bytes of data, then 5 of pack, then another pointer. Who knows what that is though.

    public CLayerElementBase* GetNext() => this.Next;
    public CLayerElementBase* GetPrev() => this.Previous;

    public string GetName() {
        if (this.Name == null) {
            return "Unnamed";
        }
        return Marshal.PtrToStringAnsi((nint) this.Name) ?? "Unnamed";
    }
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
public unsafe struct CObjectGM {
    public readonly char* name; // const in struct.h
    public CObjectGM* parentObject;
    public CHashMap<int, CObjectGM>* childrenMap;
    public CHashMap<int, CEvent>* eventsMap;
    public LinkedList<CInstance> instances;
    public LinkedList<CInstance> instancesRecursive;
    public int flags;
    public int spriteIndex;
    public int depth;
    public int parent;
    public int mask;
    public int ID;
};

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct CEvent {
    public CCode* code;
    public int ownerObjectID;
}

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
