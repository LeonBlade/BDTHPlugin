using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BDTHPlugin
{
  public class PluginMemory
  {
    private int inventoryType = 0;

    // Pointers to modify assembly to enable place anywhere.
    public IntPtr placeAnywhere;
    public IntPtr wallAnywhere;
    public IntPtr wallmountAnywhere;
    public IntPtr showcaseAnywhereRotate;
    public IntPtr showcaseAnywherePlace;

    // Layout and housing module pointers.
    private readonly IntPtr layoutWorldPtr;
    private readonly IntPtr housingModulePtr;

    public unsafe LayoutWorld* Layout => (LayoutWorld*)layoutWorldPtr;
    public unsafe HousingStructure* HousingStructure => Layout->HousingStruct;
    public unsafe HousingModule* HousingModule => (HousingModule*)housingModulePtr;
    public unsafe HousingObjectManger* CurrentManager => HousingModule->GetCurrentManager();

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
        try
        {
          if (value)
          {
            switch (inventoryType)
            {
              case 1:
                InventoryExpansion->IsVisible = true;
                InventoryGrid0E->IsVisible = true;
                InventoryGrid1E->IsVisible = true;
                InventoryGrid2E->IsVisible = true;
                InventoryGrid3E->IsVisible = true;
                InventoryEventGrid0E->IsVisible = true;
                InventoryEventGrid1E->IsVisible = true;
                InventoryEventGrid2E->IsVisible = true;
                InventoryCrystalGrid2->IsVisible = true;
                break;
              case 2:
                InventoryLarge->IsVisible = true;
                InventoryGrid0->IsVisible = true;
                InventoryGrid1->IsVisible = true;
                InventoryEventGrid0->IsVisible = true;
                InventoryEventGrid1->IsVisible = true;
                InventoryEventGrid2->IsVisible = true;
                InventoryCrystalGrid->IsVisible = true;
                break;
              case 3:
                Inventory->IsVisible = true;
                InventoryGrid->IsVisible = true;
                InventoryGridCrystal->IsVisible = true;
                break;
              default:
                break;
            }
          }
          else
          {
            if (InventoryExpansion->Flags == 52)
            {
              inventoryType = 1;
              InventoryExpansion->IsVisible = false;
              InventoryGrid0E->IsVisible = false;
              InventoryGrid1E->IsVisible = false;
              InventoryGrid2E->IsVisible = false;
              InventoryGrid3E->IsVisible = false;
              InventoryEventGrid0E->IsVisible = false;
              InventoryEventGrid1E->IsVisible = false;
              InventoryEventGrid2E->IsVisible = false;
              InventoryCrystalGrid2->IsVisible = false;
            }

            if (InventoryLarge->Flags == 52)
            {
              inventoryType = 2;
              InventoryLarge->IsVisible = false;
              InventoryGrid0->IsVisible = false;
              InventoryGrid1->IsVisible = false;
              InventoryEventGrid0->IsVisible = false;
              InventoryEventGrid1->IsVisible = false;
              InventoryEventGrid2->IsVisible = false;
              InventoryCrystalGrid->IsVisible = false;
            }

            if (Inventory->Flags == 52)
            {
              inventoryType = 3;
              Inventory->IsVisible = false;
              InventoryGrid->IsVisible = false;
              InventoryGridCrystal->IsVisible = false;
            }
          }
        }
        catch
        {
          PluginLog.LogError("IsVisible setter not present");
        }
      }
    }

    // Local references to position and rotation to use to free them when an item isn't selected but to keep the UI bound to a reference.
    public Vector3 position;
    public Vector3 rotation;

    // Matrix function used for gizmo view projection stuff.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetMatrixSingletonDelegate();
    private readonly IntPtr matrixSingletonAddress;
    public GetMatrixSingletonDelegate GetMatrixSingleton;

    // Function for selecting an item, usually used when clicking on one in game.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SelectItemDelegate(IntPtr housingStruct, IntPtr item);
    private readonly IntPtr selectItemAddress;
    public SelectItemDelegate SelectItem;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PlaceHousingItemDelegate(IntPtr item, Vector3 position);
    private readonly IntPtr placeHousingItemAddress;
    public PlaceHousingItemDelegate PlaceHousingItem;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HousingLayoutModelUpdateDelegate(IntPtr item);
    private readonly IntPtr housingLayoutModelUpdateAddress;
    public HousingLayoutModelUpdateDelegate HousingLayoutModelUpdate;

    public PluginMemory()
    {
      try
      {
        // Assembly address for asm rewrites.
        placeAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 4C") + 6;
        wallAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 80") + 6;
        wallmountAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 48 81 C4 80") + 6;
        showcaseAnywhereRotate = Plugin.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 8B");
        showcaseAnywherePlace = Plugin.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 83");

        // Pointers for housing structures.
        layoutWorldPtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 49 40 E9 ?? ?? ?? ??");
        housingModulePtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("40 53 48 83 EC 20 33 DB 48 39 1D ?? ?? ?? ?? 75 2C 45 33 C0 33 D2 B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 11 48 8B C8 E8 ?? ?? ?? ?? 48 89 05 ?? ?? ?? ?? EB 07");
        // Read the pointers.
        layoutWorldPtr = Marshal.ReadIntPtr(layoutWorldPtr);
        housingModulePtr = Marshal.ReadIntPtr(housingModulePtr);

        // Matrix address for gizmo usage.
        matrixSingletonAddress = Plugin.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4c 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??");
        GetMatrixSingleton = Marshal.GetDelegateForFunctionPointer<GetMatrixSingletonDelegate>(matrixSingletonAddress);

        // Select housing item.
        selectItemAddress = Plugin.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8B CE E8 ?? ?? ?? ?? 48 8B 6C 24 40 48 8B CE");
        SelectItem = Marshal.GetDelegateForFunctionPointer<SelectItemDelegate>(selectItemAddress);

        // Address for the place item function.
        placeHousingItemAddress = Plugin.TargetModuleScanner.ScanText("40 53 48 83 EC 20 8B 02 48 8B D9 89 41 50 8B 42 04 89 41 54 8B 42 08 89 41 58 48 83 E9 80");
        PlaceHousingItem = Marshal.GetDelegateForFunctionPointer<PlaceHousingItemDelegate>(placeHousingItemAddress);

        // Housing item model update.
        housingLayoutModelUpdateAddress = Plugin.TargetModuleScanner.ScanText("48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 48 8B E9 48 8B 49 ??");
        HousingLayoutModelUpdate = Marshal.GetDelegateForFunctionPointer<HousingLayoutModelUpdateDelegate>(housingLayoutModelUpdateAddress);

        if (Plugin.Configuration.PlaceAnywhere)
        {
          SetPlaceAnywhere(Plugin.Configuration.PlaceAnywhere);
        }
      }
      catch (Exception ex)
      {
        PluginLog.LogError(ex, "Error while calling PluginMemory.ctor()");
      }
    }

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
        PluginLog.LogError(ex, "Error while calling PluginMemory.Dispose()");
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
      if (HousingStructure == null)
        return false;

      // Rotate mode only.
      return HousingStructure->Mode == HousingLayoutMode.Rotate;
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
        PluginLog.LogError(ex, "Error occured while writing position!");
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
        PluginLog.LogError(ex, "Error occured while writing rotation!");
      }
    }

    /// <summary>
    /// Thread loop for reading memory.
    /// </summary>
    public unsafe void Update()
    {
      try
      {
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
        PluginLog.LogError(ex, "Unknown exception");
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
      objects = null;

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

    private void WriteProtectedBytes(IntPtr addr, byte[] b)
    {
      VirtualProtect(addr, 1, Protection.PAGE_EXECUTE_READWRITE, out var oldProtection);
      Marshal.Copy(b, 0, addr, b.Length);
      VirtualProtect(addr, 1, oldProtection, out _);
    }

    private void WriteProtectedBytes(IntPtr addr, byte b)
    {
      WriteProtectedBytes(addr, new[] { b });
    }

    /// <summary>
    /// Sets the flag for place anywhere in memory.
    /// </summary>
    /// <param name="state">Boolean state for if you can place anywhere.</param>
    public void SetPlaceAnywhere(bool state)
    {
      // The byte state from boolean.
      var bstate = (byte)(state ? 1 : 0);

      // Write the bytes for place anywhere.
      WriteProtectedBytes(placeAnywhere, bstate);
      WriteProtectedBytes(wallAnywhere, bstate);
      WriteProtectedBytes(wallmountAnywhere, bstate);

      // Which bytes to write.
      var showcaseBytes = state ? new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 } : new byte[] { 0x88, 0x87, 0x73, 0x01, 0x00, 0x00 };

      // Write bytes for showcase anywhere (nop or original bytes).
      WriteProtectedBytes(showcaseAnywhereRotate, showcaseBytes);
      WriteProtectedBytes(showcaseAnywherePlace, showcaseBytes);
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
