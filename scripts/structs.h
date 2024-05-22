struct RValue;
struct CInstance;
struct CPhysicsObject;
struct CSkeletonInstance;
struct YYObjectBase;
struct CObjectGM;
struct CEvent;

struct YYObjectBase_vtable {
  void (*dtor)(YYObjectBase *);
  RValue *(*InternalGetYYVarRef)(YYObjectBase *, int);
};

struct YYObjectBase {
  YYObjectBase_vtable *vtable;
};

struct CHashMapElement_int_CObjectGM_ptr_2 {
  CObjectGM *m_Value;
  int m_Key;
  unsigned __int32 m_Hash;
};

struct CHashMapElement_int_CEvent_ptr_3 {
  CEvent *m_Value;
  int m_Key;
  unsigned __int32 m_Hash;
};

struct CHashMap_int_CObjectGM_ptr_2 {
  __int32 m_CurrentSize;
  __int32 m_UsedCount;
  __int32 m_CurrentMask;
  __int32 m_GrowThreshold;
  CHashMapElement_int_CObjectGM_ptr_2 *m_Elements;
  void (*m_DeleteValue)(int *Key, CObjectGM **Value);
};

struct CHashMap_int_CEvent_ptr_3 {
  __int32 m_CurrentSize;
  __int32 m_UsedCount;
  __int32 m_CurrentMask;
  __int32 m_GrowThreshold;
  CHashMapElement_int_CEvent_ptr_3 *m_Elements;
  void (*m_DeleteValue)(int *Key, CEvent **Value);
};

typedef RValue &(*PFUNC_YYGMLScript)(CInstance *Self, CInstance *Other,
                                     RValue &Result, int ArgumentCount,
                                     RValue **Arguments);
typedef void (*PFUNC_YYGML)(CInstance *Self, CInstance *Other);
typedef void (*PFUNC_RAW)();

struct YYGMLFuncs {
  const char *m_Name;
  union {
    PFUNC_YYGMLScript m_ScriptFunction;
    PFUNC_YYGML m_CodeFunction;
    PFUNC_RAW m_RawFunction;
  };
  void *m_FunctionVariables; // YYVAR
};

enum RValueType : unsigned __int32 {
  VALUE_REAL = 0,
  VALUE_STRING = 1,
  VALUE_ARRAY = 2,
  VALUE_PTR = 3,
  VALUE_VEC3 = 4,
  VALUE_UNDEFINED = 5,
  VALUE_OBJECT = 6,
  VALUE_INT32 = 7,
  VALUE_VEC4 = 8,
  VALUE_VEC44 = 9,
  VALUE_INT64 = 10,
  VALUE_ACCESSOR = 11,
  VALUE_NULL = 12,
  VALUE_BOOL = 13,
  VALUE_ITERATOR = 14,
  VALUE_REF = 15,
  VALUE_UNSET = 0x0ffffff
};

struct RValue {
  union {
    __int32 m_i32;
    __int64 m_i64;
    double m_Real;

    CInstance *m_Object;
    void *m_Pointer;
  };

  unsigned int m_Flags;
  RValueType m_Kind;
};

struct RToken {
  int m_Kind;
  unsigned int m_Type;
  int m_Ind;
  int m_Ind2;
  RValue m_Value;
  int m_ItemNumber;
  RToken *m_Items;
  int m_Position;
};

struct CCode {
  void *vtable;
  CCode *m_Next;
  int m_Kind;
  int m_Compiled;
  const char *m_Str;
  RToken m_Token;
  RValue m_Value;
  void *m_VmInstance;
  void *m_VmDebugInfo;
  char *m_Code;
  const char *m_Name;
  int m_CodeIndex;
  YYGMLFuncs *m_Functions;
  bool m_Watch;
  int m_Offset;
  int m_LocalsCount;
  int m_ArgsCount;
  int m_Flags;
  YYObjectBase *m_Prototype;
};

struct CEvent {
  CCode *m_Code;
  __int32 m_OwnerObjectID;
};

struct LinkedList_CInstance {
  CInstance *m_First;
  CInstance *m_Last;
  __int32 m_Count;
};

struct CPhysicsDataGM {
  float *m_PhysicsVertices;
  bool m_IsPhysicsObject;
  bool m_IsPhysicsSensor;
  bool m_IsPhysicsAwake;
  bool m_IsPhysicsKinematic;
  int m_PhysicsShape;
  int m_PhysicsGroup;
  float m_PhysicsDensity;
  float m_PhysicsRestitution;
  float m_PhysicsLinearDamping;
  float m_PhysicsAngularDamping;
  float m_PhysicsFriction;
  int m_PhysicsVertexCount;
};

struct CObjectGM {
  const char *m_Name;
  CObjectGM *m_ParentObject;
  CHashMap_int_CObjectGM_ptr_2 *m_ChildrenMap;
  CHashMap_int_CEvent_ptr_3 *m_EventsMap;
  CPhysicsDataGM m_PhysicsData;
  LinkedList_CInstance m_Instances;
  LinkedList_CInstance m_InstancesRecursive;
  unsigned __int32 m_Flags;
  __int32 m_SpriteIndex;
  __int32 m_Depth;
  __int32 m_Parent;
  __int32 m_Mask;
  __int32 m_ID;
};

struct YYRECT {
  float m_Left;
  float m_Top;
  float m_Right;
  float m_Bottom;
};

struct CInstanceInternal {
  unsigned __int32 m_InstanceFlags;
  __int32 m_ID;
  __int32 m_ObjectIndex;
  __int32 m_SpriteIndex;
  float m_SequencePosition;
  float m_LastSequencePosition;
  float m_SequenceDirection;
  float m_ImageIndex;
  float m_ImageSpeed;
  float m_ImageScaleX;
  float m_ImageScaleY;
  float m_ImageAngle;
  float m_ImageAlpha;
  unsigned __int32 m_ImageBlend;
  float m_X;
  float m_Y;
  float m_XStart;
  float m_YStart;
  float m_XPrevious;
  float m_YPrevious;
  float m_Direction;
  float m_Speed;
  float m_Friction;
  float m_GravityDirection;
  float m_Gravity;
  float m_HorizontalSpeed;
  float m_VerticalSpeed;
  YYRECT m_BoundingBox;
  int m_Timers[12];
  __int16 m_RollbackFrameKilled;
  void *m_TimelinePath;
  CCode *m_InitCode;
  CCode *m_PrecreateCode;
  CObjectGM *m_OldObject;
  __int32 m_LayerID;
  __int32 m_MaskIndex;
  __int16 m_MouseOverCount;
  CInstance *m_Flink;
  CInstance *m_Blink;
};

struct CInstance : YYObjectBase {
  __int64 m_CreateCounter;
  CObjectGM *m_Object;
  CPhysicsObject *m_PhysicsObject;
  CSkeletonInstance *m_SkeletonAnimation;
  void *m_SequenceInstance;
  CInstanceInternal Members;
};

struct CInstanceBase {
  RValue *m_YYVars;
};
