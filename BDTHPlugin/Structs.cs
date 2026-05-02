using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine.Group;

namespace BDTHPlugin
{

  public enum HousingLayoutMode
  {
    None,
    Move,
    Rotate,
    Store,
    Place,
    Remove = 6,
  }

  public enum ItemState
  {
    None = 0,
    Hover,
    SoftSelect,
    Active,
  }

  public enum ItemState2
  {
    None = 0,
    SoftSelect = 3,
    Active = 5,
    Invalid = 6,
  }

  [StructLayout(LayoutKind.Explicit)]
  public unsafe struct HousingObjectManager
  {
    [FieldOffset(0x11260)] public fixed ulong Objects[600];
    [FieldOffset(0x12620)] public GameObject* IndoorActiveObject2;
    [FieldOffset(0x12628)] public GameObject* IndoorHoverObject;
    [FieldOffset(0x12630)] public GameObject* IndoorActiveObject;
    [FieldOffset(0x12638)] public GameObject* OutdoorActiveObject2;
    [FieldOffset(0x12640)] public GameObject* OutdoorHoverObject;
    [FieldOffset(0x12648)] public GameObject* OutdoorActiveObject;
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
    [FieldOffset(0x10)] public SharedGroupLayoutInstance* HoverItem;
    [FieldOffset(0x18)] public SharedGroupLayoutInstance* ActiveItem;
    [FieldOffset(0xB8)] public bool Rotating;
  }
}
