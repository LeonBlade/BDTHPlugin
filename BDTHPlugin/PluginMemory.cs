using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager;

using Dalamud.Utility.Signatures;

namespace BDTHPlugin
{
  public class PluginMemory
  {
    private bool isHousingOpen = false;
    private int inventoryType = 0;

    // Pointers to modify assembly to enable place anywhere.
    public IntPtr placeAnywhere;
    public IntPtr wallAnywhere;
    public IntPtr wallmountAnywhere;
    // public IntPtr showcaseAnywhereRotate;
    // public IntPtr showcaseAnywherePlace;

    // Layout and housing module pointers.
    private readonly IntPtr layoutWorldPtr;
    private readonly IntPtr housingModulePtr;

    public unsafe LayoutWorld* Layout => (LayoutWorld*)layoutWorldPtr;
    public unsafe HousingStructure* HousingStructure => Layout->HousingStruct;
    public unsafe HousingModule* HousingModule => housingModulePtr != IntPtr.Zero ? (HousingModule*)Marshal.ReadIntPtr(housingModulePtr) : null;
    public unsafe HousingObjectManager* CurrentManager => HousingModule->GetCurrentManager();
    public unsafe Camera* Camera => &CameraManager.Instance()->GetActiveCamera()->CameraBase.SceneCamera;

    public static unsafe AtkUnitBase* HousingLayout => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingLayout", 1);
    public static unsafe bool GamepadMode => !(HousingLayout != null && HousingLayout->IsVisible);

    public static unsafe AtkUnitBase* HousingGoods => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingGoods", 1);
    public static unsafe AtkUnitBase* InventoryExpansion => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryExpansion", 1);
    public static unsafe AtkUnitBase* InventoryGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0E", 1);
    public static unsafe AtkUnitBase* InventoryGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1E", 1);
    public static unsafe AtkUnitBase* InventoryGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid2E", 1);
    public static unsafe AtkUnitBase* InventoryGrid3E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid3E", 1);
    public static unsafe AtkUnitBase* InventoryLarge => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryLarge", 1);
    public static unsafe AtkUnitBase* Inventory => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("Inventory", 1);
    public static unsafe AtkUnitBase* InventoryGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0", 1);
    public static unsafe AtkUnitBase* InventoryGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2", 1);
    public static unsafe AtkUnitBase* InventoryGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0E", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1E", 1);
    public static unsafe AtkUnitBase* InventoryEventGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2E", 1);
    public static unsafe AtkUnitBase* InventoryGridCrystal => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGridCrystal", 1);
    public static unsafe AtkUnitBase* InventoryCrystalGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 1);
    public static unsafe AtkUnitBase* InventoryCrystalGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 2);

    public unsafe bool InventoryVisible
    {
      get => InventoryExpansion != null && InventoryExpansion->IsVisible ||
          InventoryLarge != null && InventoryLarge->IsVisible ||
          Inventory != null && Inventory->IsVisible;

      set
      {
        if (HousingGoods == null || InventoryExpansion == null || InventoryLarge == null || Inventory == null)
          return;

        // Determine which inventory is open assuming it's visible
        if (InventoryExpansion->IsVisible || InventoryLarge->IsVisible || Inventory->IsVisible)
        {
          if (InventoryExpansion->IsVisible) inventoryType = 1;
          else if (InventoryLarge->IsVisible) inventoryType = 2;
          else if (Inventory->IsVisible) inventoryType = 3;
        }

        try
        {
          switch (inventoryType)
          {
            case 1:
              InventoryExpansion->IsVisible = value;
              InventoryGrid0E->IsVisible = value;
              InventoryGrid1E->IsVisible = value;
              InventoryGrid2E->IsVisible = value;
              InventoryGrid3E->IsVisible = value;
              InventoryEventGrid0E->IsVisible = value;
              InventoryEventGrid1E->IsVisible = value;
              InventoryEventGrid2E->IsVisible = value;
              InventoryCrystalGrid2->IsVisible = value;
              break;
            case 2:
              InventoryLarge->IsVisible = value;
              InventoryGrid0->IsVisible = value;
              InventoryGrid1->IsVisible = value;
              InventoryEventGrid0->IsVisible = value;
              InventoryEventGrid1->IsVisible = value;
              InventoryEventGrid2->IsVisible = value;
              InventoryCrystalGrid->IsVisible = value;
              break;
            case 3:
              Inventory->IsVisible = value;
              InventoryGrid->IsVisible = value;
              InventoryGridCrystal->IsVisible = value;
              break;
            default:
              break;
          }
        }
        catch
        {
          Plugin.Log.Error("IsVisible setter not present");
        }
      }
    }

    // Local references to position and rotation to use to free them when an item isn't selected but to keep the UI bound to a reference.
    public Vector3 position;
    public Vector3 rotation;

    // Function for selecting an item, usually used when clicking on one in game.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SelectItemDelegate(IntPtr housingStruct, IntPtr item);
    private readonly IntPtr selectItemAddress;
    public SelectItemDelegate SelectItem = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PlaceHousingItemDelegate(IntPtr item, Vector3 position);
    private readonly IntPtr placeHousingItemAddress;
    public PlaceHousingItemDelegate PlaceHousingItem = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HousingLayoutModelUpdateDelegate(IntPtr item);
    private readonly IntPtr housingLayoutModelUpdateAddress;
    public HousingLayoutModelUpdateDelegate HousingLayoutModelUpdate = null!;

    public PluginMemory()
    {
      try
      {
        // Assembly address for asm rewrites.
        placeAnywhere = Plugin.TargetModuleScanner.ScanText("C6 ?? ?? ?? 00 00 00 8B FE 48 89") + 6;
        wallAnywhere = Plugin.TargetModuleScanner.ScanText("48 85 C0 74 ?? C6 87 ?? ?? 00 00 00") + 11;
        wallmountAnywhere = Plugin.TargetModuleScanner.ScanText("c6 87 83 01 00 00 00 48 83 c4 ??") + 6;
        // showcaseAnywhereRotate = Plugin.TargetModuleScanner.ScanText("88 87 98 02 00 00 48 8b 9c ?? ?? 00 00 00 4C 8B");
        // showcaseAnywherePlace = Plugin.TargetModuleScanner.ScanText("88 87 98 02 00 00 48 8B");

        // Pointers for housing structures.
        layoutWorldPtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B D1 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 0A", 3);
        housingModulePtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 8B 52");

        // Read the pointers.
        layoutWorldPtr = Marshal.ReadIntPtr(layoutWorldPtr);

        // Select housing item.
        selectItemAddress = Plugin.TargetModuleScanner.ScanText("48 85 D2 0F 84 49 09 00 00 53 41 56 48 83 EC 48 48 89 6C 24 60 48 8B DA 48 89 74 24 70 4C 8B F1");
        SelectItem = Marshal.GetDelegateForFunctionPointer<SelectItemDelegate>(selectItemAddress);

        // Address for the place item function.
        placeHousingItemAddress = Plugin.TargetModuleScanner.ScanText("40 53 48 83 EC 20 8B 02 48 8B D9 89 41 50 8B 42 04 89 41 54 8B 42 08 89 41 58 48 83 E9 80");
        PlaceHousingItem = Marshal.GetDelegateForFunctionPointer<PlaceHousingItemDelegate>(placeHousingItemAddress);

        // Housing item model update.
        housingLayoutModelUpdateAddress = Plugin.TargetModuleScanner.ScanText("48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 48 8B E9 48 8B 49");
        HousingLayoutModelUpdate = Marshal.GetDelegateForFunctionPointer<HousingLayoutModelUpdateDelegate>(housingLayoutModelUpdateAddress);

        var config = Plugin.GetConfiguration();

        if (config.PlaceAnywhere)
          SetPlaceAnywhere(Plugin.GetConfiguration().PlaceAnywhere);
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Error while calling PluginMemory.ctor()");
      }
    }

    public unsafe void ShowFurnishingList(bool state)
    {
      if (HousingGoods != null)
        HousingGoods->IsVisible = state;
    }

    public void ShowInventory(bool state) => InventoryVisible = state;

    /// <summary>
    /// Dispose for the memory functions.
    /// </summary>
    public unsafe void Dispose()
    {
      try
      {
        // Disable the place anywhere in case it's on.
        SetPlaceAnywhere(false);

        // Enable the housing goods menu again.
        if (HousingGoods != null) HousingGoods->IsVisible = true;
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Error while calling PluginMemory.Dispose()");
      }
    }

    public unsafe int GetHousingObjectSelectedIndex()
    {
      for (var i = 0; i < 400; i++)
      {
        if (HousingModule->GetCurrentManager()->Objects[i] == 0)
          continue;
        if ((ulong)HousingModule->GetCurrentManager()->IndoorActiveObject == HousingModule->GetCurrentManager()->Objects[i])
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Is the housing menu open.
    /// </summary>
    /// <returns>Boolean state.</returns>
    public unsafe bool IsHousingOpen()
    {
      if (HousingStructure == null)
        return false;

      // Anything other than none means the housing menu is open.
      return HousingStructure->Mode != HousingLayoutMode.None;
    }

    /// <summary>
    /// Checks if you can edit a housing item, specifically checks that rotate mode is active.
    /// </summary>
    /// <returns>Boolean state if housing menu is on or off.</returns>
    public unsafe bool CanEditItem()
    {
      try
      {
        if (HousingStructure == null)
          return false;

        // Rotate mode only.
        return HousingStructure->Mode == HousingLayoutMode.Rotate;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Read the position of the active item.
    /// </summary>
    /// <returns>Vector3 of the position.</returns>
    public unsafe Vector3 ReadPosition()
    {
      // Ensure that we're hooked and have the housing structure address.
      if (HousingStructure == null)
        throw new PluginException("Housing structure is invalid!");

      // Ensure active item pointer isn't null.
      var item = HousingStructure->ActiveItem;
      if (item == null)
        throw new PluginException("No valid item selected!");

      // Return the position vector.
      return item->Position;
    }

    /// <summary>
    /// Reads the rotation of the item.
    /// </summary>
    /// <returns></returns>
    public unsafe Vector3 ReadRotation()
    {
      // Ensure that we're hooked and have the housing structure address.
      if (HousingStructure == null)
        throw new PluginException("Housing structure is invalid!");

      // Ensure active item pointer isn't null.
      var item = HousingStructure->ActiveItem;
      if (item == null)
        throw new PluginException("No valid item selected!");

      // Return the rotation radian.
      return Util.FromQ(item->Rotation);
    }

    /// <summary>
    /// Writes the position vector to memory.
    /// </summary>
    /// <param name="newPosition">Position vector to write.</param>
    public unsafe void WritePosition(Vector3 newPosition)
    {
      // Don't write if housing mode isn't on.
      if (!CanEditItem())
        return;

      try
      {
        var item = HousingStructure->ActiveItem;
        if (item == null)
          return;

        // Set the position.
        item->Position = newPosition;
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Error occured while writing position!");
      }
    }

    public unsafe void WriteRotation(Vector3 newRotation)
    {
      // Don't write if housing mode isn't on.
      if (!CanEditItem())
        return;

      try
      {
        var item = HousingStructure->ActiveItem;
        if (item == null)
          return;

        // Convert into a quaternion.
        item->Rotation = Util.ToQ(newRotation);
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Error occured while writing rotation!");
      }
    }

    /// <summary>
    /// Thread loop for reading memory.
    /// </summary>
    public unsafe void Update()
    {
      try
      {
        var lastIsHousingOpen = isHousingOpen;
        isHousingOpen = IsHousingOpen();

        // Just perform once when housing is opened
        if (lastIsHousingOpen != isHousingOpen && isHousingOpen)
        {
          Plugin.Log.Info("Housing opened");
          var config = Plugin.GetConfiguration();
          if (!config.DisplayFurnishingList)
            ShowFurnishingList(false);
          if (!config.DisplayInventory)
            ShowInventory(false);
        }

        if (CanEditItem())
        {
          // Don't really need to load position if we're reading it in the UI thread anyway, but leaving it for now for redudency...
          position = ReadPosition();
          rotation = ReadRotation();

          // Update the model of active item, the game doesn't do this for wall mounted and outside in rotate mode
          var item = HousingStructure->ActiveItem;
          if (item != null)
            HousingLayoutModelUpdate((IntPtr)item + 0x80);
        }
      }
      catch (PluginException)
      {
        position = Vector3.Zero;
        rotation = Vector3.Zero;
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Unknown exception");
        position = Vector3.Zero;
        rotation = Vector3.Zero;
      }
    }

    /// <summary>
    /// Get furnishings as they appear in the array in memory.
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public unsafe bool GetFurnishings(out List<HousingGameObject> objects, Vector3 point, bool sortByDistance = false)
    {
      if (sortByDistance == true)
        return GetFurnishingByDistance(out objects, point);

      objects = new List<HousingGameObject>();

      if (HousingModule == null || HousingModule->GetCurrentManager() == null || HousingModule->GetCurrentManager()->Objects == null)
        return false;

      for (var i = 0; i < 400; i++)
      {
        var oPtr = HousingModule->GetCurrentManager()->Objects[i];
        if (oPtr == 0)
          continue;

        objects.Add(*(HousingGameObject*)oPtr);
      }
      return true;
    }

    /// <summary>
    /// Get furnishings and sort by distance to a given point.
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public unsafe bool GetFurnishingByDistance(out List<HousingGameObject> objects, Vector3 point)
    {
      objects = [];

      if (HousingModule == null || HousingModule->GetCurrentManager() == null || HousingModule->GetCurrentManager()->Objects == null)
        return false;

      var tmpObjects = new List<(HousingGameObject gObj, float distance)>();
      objects = new List<HousingGameObject>();
      for (var i = 0; i < 400; i++)
      {
        var oPtr = HousingModule->GetCurrentManager()->Objects[i];
        if (oPtr == 0)
          continue;
        var o = *(HousingGameObject*)oPtr;
        tmpObjects.Add((o, Util.DistanceFromPlayer(o, point)));
      }

      tmpObjects.Sort((obj1, obj2) => obj1.distance.CompareTo(obj2.distance));
      objects = tmpObjects.Select(obj => obj.gObj).ToList();

      return true;
    }

    private static void WriteProtectedBytes(IntPtr addr, byte[] b)
    {
      if (addr == IntPtr.Zero) return;
      VirtualProtect(addr, 1, Protection.PAGE_EXECUTE_READWRITE, out var oldProtection);
      Marshal.Copy(b, 0, addr, b.Length);
      VirtualProtect(addr, 1, oldProtection, out _);
    }

    private static void WriteProtectedBytes(IntPtr addr, byte b)
    {
      if (addr == IntPtr.Zero) return;
      WriteProtectedBytes(addr, [b]);
    }

    /// <summary>
    /// Sets the flag for place anywhere in memory.
    /// </summary>
    /// <param name="state">Boolean state for if you can place anywhere.</param>
    public void SetPlaceAnywhere(bool state)
    {
      if (placeAnywhere == IntPtr.Zero || wallAnywhere == IntPtr.Zero || wallmountAnywhere == IntPtr.Zero)
        return;

      // The byte state from boolean.
      var bstate = (byte)(state ? 1 : 0);

      // Write the bytes for place anywhere.
      WriteProtectedBytes(placeAnywhere, bstate);
      WriteProtectedBytes(wallAnywhere, bstate);
      WriteProtectedBytes(wallmountAnywhere, bstate);

      // Which bytes to write.
      // byte[] showcaseBytes = state ? [0x90, 0x90, 0x90, 0x90, 0x90, 0x90] : [0x88, 0x87, 0x98, 0x02, 0x00, 0x00];

      // // Write bytes for showcase anywhere (nop or original bytes).
      // WriteProtectedBytes(showcaseAnywhereRotate, showcaseBytes);
      // WriteProtectedBytes(showcaseAnywherePlace, showcaseBytes);
    }

    #region Kernel32

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

    public enum Protection
    {
      PAGE_NOACCESS = 0x01,
      PAGE_READONLY = 0x02,
      PAGE_READWRITE = 0x04,
      PAGE_WRITECOPY = 0x08,
      PAGE_EXECUTE = 0x10,
      PAGE_EXECUTE_READ = 0x20,
      PAGE_EXECUTE_READWRITE = 0x40,
      PAGE_EXECUTE_WRITECOPY = 0x80,
      PAGE_GUARD = 0x100,
      PAGE_NOCACHE = 0x200,
      PAGE_WRITECOMBINE = 0x400
    }

    #endregion
  }
}
