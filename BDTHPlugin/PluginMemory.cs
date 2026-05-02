using Dalamud.Game.NativeWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using BDTHPlugin.Anywheres;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using CameraManager = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager;

namespace BDTHPlugin
{
  public class PluginMemory
  {
    private bool _isHousingOpen;

    // Layout and housing module pointers.
    private readonly IntPtr _layoutWorldPtr;
    private readonly IntPtr _housingModulePtr;

    public unsafe LayoutWorld* Layout => (LayoutWorld*)_layoutWorldPtr;
    public unsafe HousingStructure* HousingStructure => Layout->HousingStruct;
    public unsafe HousingModule* HousingModule => _housingModulePtr != IntPtr.Zero ? (HousingModule*)Marshal.ReadIntPtr(_housingModulePtr) : null;
    public static unsafe Camera* Camera => &CameraManager.Instance()->GetActiveCamera()->CameraBase.SceneCamera;

    private static AtkUnitBasePtr HousingLayout => Plugin.GameGui.GetAddonByName("HousingLayout");
    public static bool GamepadMode => !(HousingLayout != null && HousingLayout.IsVisible);

    // Local references to position and rotation to use to free them when an item isn't selected but to keep the UI bound to a reference.
    public Vector3 Position;
    public Vector3 Rotation;

    // Function for selecting an item, usually used when clicking on one in game.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SelectItemDelegate(IntPtr housingStruct, IntPtr item);
    public SelectItemDelegate SelectItem = null!;

    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // public delegate void PlaceHousingItemDelegate(IntPtr item, Vector3 position);
    // private readonly IntPtr placeHousingItemAddress;
    // public PlaceHousingItemDelegate PlaceHousingItem = null!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HousingLayoutModelUpdateDelegate(IntPtr item);
    public HousingLayoutModelUpdateDelegate HousingLayoutModelUpdate = null!;

    public PluginMemory()
    {
      try
      {
        PlaceAnywhere.Initialize();
        ShowcaseAnywhere.Initialize();

        // Pointers for housing structures.
        _layoutWorldPtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B D1 48 8B 0D ?? ?? ?? ?? 48 85 C9 74 0A", 3);
        _housingModulePtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B 05 ?? ?? ?? ?? 8B 52");

        // Read the pointers.
        _layoutWorldPtr = Marshal.ReadIntPtr(_layoutWorldPtr);

        // Select housing item.
        var selectItemAddress = Plugin.TargetModuleScanner.ScanText("48 85 D2 0F 84 ?? ?? ?? ?? 53 41 ?? 48 83 ?? ?? 48 89");
        SelectItem = Marshal.GetDelegateForFunctionPointer<SelectItemDelegate>(selectItemAddress);
        
        // Address for the place item function.
        // placeHousingItemAddress = Plugin.TargetModuleScanner.ScanText("40 53 48 83 EC 20 8B 02 48 8B D9 89 41 50 8B 42 04 89 41 54 8B 42 08 89 41 58 48 83 E9 80");
        // PlaceHousingItem = Marshal.GetDelegateForFunctionPointer<PlaceHousingItemDelegate>(placeHousingItemAddress);
        // Housing item model update.
        
        var housingLayoutModelUpdateAddress = Plugin.TargetModuleScanner.ScanText("48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 48 8B E9 48 8B 49");
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

    /// <summary>
    /// Dispose for the memory functions.
    /// </summary>
    public void Dispose()
    {
      try
      {
                // Disable the place anywhere in case it's on.
                SetPlaceAnywhere(false);
        AtkManager.ShowFurnishingList(true);
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Error while calling PluginMemory.Dispose()");
      }
    }

    /// <summary>
    /// Is the housing menu open.
    /// </summary>
    /// <returns>Boolean state.</returns>
    public unsafe bool IsHousingOpen()
    {
      try
      {
        if (HousingStructure == null)
          return false;

        // Anything other than none means the housing menu is open.
        return HousingStructure->Mode != HousingLayoutMode.None;
      }
      catch
      {
        return false;
      }
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
      return item->Transform.Translation;
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
      return Util.FromQ(item->Transform.Rotation);
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
        item->Transform.Translation = newPosition;
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
        item->Transform.Rotation = Util.ToQ(newRotation);
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
        var lastIsHousingOpen = _isHousingOpen;
        _isHousingOpen = IsHousingOpen();

        // Just perform once when housing is opened
        if (lastIsHousingOpen != _isHousingOpen && _isHousingOpen)
        {
          var config = Plugin.GetConfiguration();
          if (!config.DisplayFurnishingList)
            AtkManager.ShowFurnishingList(false);
          if (!config.DisplayInventory)
            AtkManager.ShowInventory(false);
        }

        if (!CanEditItem())
          return;

        // Don't really need to load position if we're reading it in the UI thread anyway, but leaving it for now for redudency...
        Position = ReadPosition();
        Rotation = ReadRotation();

        // Update the model of active item, the game doesn't do this for wall mounted and outside in rotate mode
        var item = HousingStructure->ActiveItem;
        if (item != null)
          HousingLayoutModelUpdate((IntPtr)item + 0x80);
      }
      catch (PluginException)
      {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, "Unknown exception");
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
      }
    }

    /// <summary>
    /// Get furnishings as they appear in the array in memory.
    /// </summary>
    /// <param name="objects"></param>
    /// <param name="point"></param>
    /// <param name="sortByDistance"></param>
    /// <returns></returns>
    public unsafe bool GetFurnishings(out List<GameObject> objects, Vector3 point, bool sortByDistance = false)
    {
      // Initialize the output to empty for the sake of noticing an error
      objects = [];

      // Exit early due to missing addresses
      if (HousingModule == null || HousingModule->GetCurrentManager() == null || HousingModule->GetCurrentManager()->Objects == null)
        return false;

      // Create a list of game objects and their distance from the player
      var furnishings = new List<(GameObject gameObject, float distance)>();
      // Iterate over array size, max furnishing size (600 as of Dawntrail)
      for (var i = 0; i < 600; i++)
      {
        // Get the object from the array
        var objectAddress = HousingModule->GetCurrentManager()->Objects[i];
        if (objectAddress == 0)
          continue;
        // Cast the address to a GameObject
        var gameObject = *(GameObject*)objectAddress;
        // Add to the objects including distance from player
        furnishings.Add((gameObject, Util.DistanceFromPlayer(gameObject, point)));
      }

      // Pre-sort the objects
      if (sortByDistance)
        furnishings.Sort((obj1, obj2) => obj1.distance.CompareTo(obj2.distance));

      // Set the output variable to just the game objects
      objects = furnishings.Select(obj => obj.gameObject).ToList();

      return true;
    }

    public static byte[] ReadBytes(IntPtr addr, int length)
    {
      var bytes = new byte[length];
      Marshal.Copy(addr, bytes, 0, length);
      return bytes;
    }

    public static void WriteProtectedBytes(IntPtr addr, byte[] b)
    {
      if (addr == IntPtr.Zero)
        return;
      VirtualProtect(addr, 1, Protection.PAGE_EXECUTE_READWRITE, out var oldProtection);
      Marshal.Copy(b, 0, addr, b.Length);
      VirtualProtect(addr, 1, oldProtection, out _);
    }

    public static void WriteProtectedBytes(IntPtr addr, byte b)
    {
      if (addr == IntPtr.Zero)
        return;
      WriteProtectedBytes(addr, [b]);
    }

    /// <summary>
    /// Sets the flag for place anywhere in memory.
    /// </summary>
    /// <param name="state">Boolean state for if you can place anywhere.</param>
    public static void SetPlaceAnywhere(bool state)
    {
      PlaceAnywhere.SetState(state);
      ShowcaseAnywhere.SetState(state);
    }

    #region Kernel32
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

    private enum Protection
    {
      // PAGE_NOACCESS = 0x01,
      // PAGE_READONLY = 0x02,
      // PAGE_READWRITE = 0x04,
      // PAGE_WRITECOPY = 0x08,
      // PAGE_EXECUTE = 0x10,
      // PAGE_EXECUTE_READ = 0x20,
      PAGE_EXECUTE_READWRITE = 0x40,
      // PAGE_EXECUTE_WRITECOPY = 0x80,
      // PAGE_GUARD = 0x100,
      // PAGE_NOCACHE = 0x200,
      // PAGE_WRITECOMBINE = 0x400
    }
    #endregion
  }
}
