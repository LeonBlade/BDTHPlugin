using System.Numerics;
using System.Runtime.InteropServices;

namespace BDTHPlugin
{
  public enum HousingLayoutMode
  {
    None,
    Move,
    Rotate,
    Store,
    Place,
    Remove = 6
  }

  public enum ItemState
  {
    None = 0,
    Hover,
    SoftSelect,
    Active
  }

  public enum ItemState2
  {
    None = 0,
    SoftSelect = 3,
    Active = 5
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingObjectManager
  {
    [FieldOffset(0x8980)] public fixed ulong Objects[400];
    [FieldOffset(0x96E8)] public HousingGameObject* IndoorActiveObject2;
    [FieldOffset(0x96F0)] public HousingGameObject* IndoorHoverObject;
    [FieldOffset(0x96F8)] public HousingGameObject* IndoorActiveObject;
    [FieldOffset(0x9AB8)] public HousingGameObject* OutdoorActiveObject2;
    [FieldOffset(0x9AC0)] public HousingGameObject* OutdoorHoverObject;
    [FieldOffset(0x9AC8)] public HousingGameObject* OutdoorActiveObject;
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingModule
  {
    [FieldOffset(0x0)] public HousingObjectManager* CurrentTerritory;
    [FieldOffset(0x8)] public HousingObjectManager* OutdoorTerritory;
    [FieldOffset(0x10)] public HousingObjectManager* IndoorTerritory;

    public HousingObjectManager* GetCurrentManager()
      => OutdoorTerritory != null ? OutdoorTerritory : IndoorTerritory;
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingGameObject
  {
    [FieldOffset(0x30)] public fixed byte Name[64];
    [FieldOffset(0x80)] public uint HousingRowId;
    [FieldOffset(0xB0)] public float X;
    [FieldOffset(0xB4)] public float Y;
    [FieldOffset(0xB8)] public float Z;
    [FieldOffset(0x108)] public HousingItem* Item;
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct LayoutWorld
  {
    [FieldOffset(0x40)] public HousingStructure* HousingStruct;
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingStructure
  {
    [FieldOffset(0x0)] public HousingLayoutMode Mode;
    [FieldOffset(0x4)] public HousingLayoutMode LastMode;
    [FieldOffset(0x8)] public ItemState State;
    [FieldOffset(0xC)] public ItemState2 State2;
    [FieldOffset(0x10)] public HousingItem* HoverItem;
    [FieldOffset(0x18)] public HousingItem* ActiveItem;
    [FieldOffset(0xB8)] public bool Rotating;
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingItem
  {
    [FieldOffset(0x50)] public Vector3 Position;
    [FieldOffset(0x60)] public Quaternion Rotation;
  }
}
