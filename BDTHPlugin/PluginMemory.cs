using Dalamud.Hooking;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace BDTHPlugin
{
	public class PluginMemory
	{
		private readonly DalamudPluginInterface pi;

		private readonly Thread thread;
		private bool threadRunning = false;
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

		public unsafe AtkUnitBase* HousingLayout => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("HousingLayout", 1);
		public unsafe bool GamepadMode => !(HousingLayout != null && HousingLayout->IsVisible);

		public unsafe AtkUnitBase* HousingGoods => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("HousingGoods", 1);
		public unsafe AtkUnitBase* InventoryExpansion => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryExpansion", 1);
		public unsafe AtkUnitBase* InventoryGrid0E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid0E", 1);
		public unsafe AtkUnitBase* InventoryGrid1E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid1E", 1);
		public unsafe AtkUnitBase* InventoryGrid2E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid2E", 1);
		public unsafe AtkUnitBase* InventoryGrid3E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid3E", 1);
		public unsafe AtkUnitBase* InventoryLarge => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryLarge", 1);
		public unsafe AtkUnitBase* Inventory => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("Inventory", 1);
		public unsafe AtkUnitBase* InventoryGrid0 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid0", 1);
		public unsafe AtkUnitBase* InventoryGrid1 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid1", 1);
		public unsafe AtkUnitBase* InventoryEventGrid0 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid0", 1);
		public unsafe AtkUnitBase* InventoryEventGrid1 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid1", 1);
		public unsafe AtkUnitBase* InventoryEventGrid2 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid2", 1);
		public unsafe AtkUnitBase* InventoryGrid => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGrid", 1);
		public unsafe AtkUnitBase* InventoryEventGrid0E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid0E", 1);
		public unsafe AtkUnitBase* InventoryEventGrid1E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid1E", 1);
		public unsafe AtkUnitBase* InventoryEventGrid2E => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryEventGrid2E", 1);
		public unsafe AtkUnitBase* InventoryGridCrystal => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryGridCrystal", 1);
		public unsafe AtkUnitBase* InventoryCrystalGrid => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryCrystalGrid", 1);
		public unsafe AtkUnitBase* InventoryCrystalGrid2 => (AtkUnitBase*)pi.Framework.Gui.GetUiObjectByName("InventoryCrystalGrid", 2);

		public unsafe void SetVisible(AtkUnitBase* ui, bool state)
    {
			if (state)
				ui->Flags |= 32;
			else
				ui->Flags = (byte)(ui->Flags & ~32);
		}

		public unsafe bool HousingVisible
    {
			get => HousingGoods->IsVisible;
			set => SetVisible(HousingGoods, value);
		}

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
								SetVisible(InventoryExpansion, true);
								SetVisible(InventoryGrid0E, true);
								SetVisible(InventoryGrid1E, true);
								SetVisible(InventoryGrid2E, true);
								SetVisible(InventoryGrid3E, true);
								SetVisible(InventoryEventGrid0E, true);
								SetVisible(InventoryEventGrid1E, true);
								SetVisible(InventoryEventGrid2E, true);
								SetVisible(InventoryCrystalGrid2, true);
								break;
							case 2:
								SetVisible(InventoryLarge, true);
								SetVisible(InventoryGrid0, true);
								SetVisible(InventoryGrid1, true);
								SetVisible(InventoryEventGrid0, true);
								SetVisible(InventoryEventGrid1, true);
								SetVisible(InventoryEventGrid2, true);
								SetVisible(InventoryCrystalGrid, true);
								break;
							case 3:
								SetVisible(Inventory, true);
								SetVisible(InventoryGrid, true);
								SetVisible(InventoryGridCrystal, true);
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
							SetVisible(InventoryExpansion, false);
							SetVisible(InventoryGrid0E, false);
							SetVisible(InventoryGrid1E, false);
							SetVisible(InventoryGrid2E, false);
							SetVisible(InventoryGrid3E, false);
							SetVisible(InventoryEventGrid0E, false);
							SetVisible(InventoryEventGrid1E, false);
							SetVisible(InventoryEventGrid2E, false);
							SetVisible(InventoryCrystalGrid2, false);
						}

						if (InventoryLarge->Flags == 52)
						{
							inventoryType = 2;
							SetVisible(InventoryLarge, false);
							SetVisible(InventoryGrid0, false);
							SetVisible(InventoryGrid1, false);
							SetVisible(InventoryEventGrid0, false);
							SetVisible(InventoryEventGrid1, false);
							SetVisible(InventoryEventGrid2, false);
							SetVisible(InventoryCrystalGrid, false);
						}

						if (Inventory->Flags == 52)
						{
							inventoryType = 3;
							SetVisible(Inventory, false);
							SetVisible(InventoryGrid, false);
							SetVisible(InventoryGridCrystal, false);
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
		public delegate void SoftSelectDelegate(IntPtr housingStruct, IntPtr item);
		private readonly IntPtr softSelectAddress;
		private readonly Hook<SoftSelectDelegate> SoftSelectHook;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void PlaceHousingItemDelegate(IntPtr item, Vector3 position);
		private readonly IntPtr placeHousingItemAddress;
		public PlaceHousingItemDelegate PlaceHousingItem;

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void HousingLayoutModelUpdateDelegate(IntPtr item, Vector3 position);
		private readonly IntPtr housingLayoutModelUpdateAddress;
		public HousingLayoutModelUpdateDelegate HousingLayoutModelUpdate;

		public PluginMemory(DalamudPluginInterface pluginInterface)
		{
			try
			{
				pi = pluginInterface;

				// Assembly address for asm rewrites.
				placeAnywhere = pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 4D") + 6;
				wallAnywhere = pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 80") + 6;
				wallmountAnywhere = pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 48 81 C4 80") + 6;
				showcaseAnywhereRotate = pi.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 8B");
				showcaseAnywherePlace = pi.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 83");

				// Pointers for housing structures.
				layoutWorldPtr = pi.TargetModuleScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 49 40 E9 ?? ?? ?? ??", 2);
				housingModulePtr = pi.TargetModuleScanner.GetStaticAddressFromSig("40 53 48 83 EC 20 33 DB 48 39 1D ?? ?? ?? ?? 75 2C 45 33 C0 33 D2 B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 11 48 8B C8 E8 ?? ?? ?? ?? 48 89 05 ?? ?? ?? ?? EB 07", 0xA);
				// Read the pointers.
				layoutWorldPtr = Marshal.ReadIntPtr(layoutWorldPtr);
				housingModulePtr = Marshal.ReadIntPtr(housingModulePtr);

				// Matrix address for gizmo usage.
				matrixSingletonAddress = pi.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4c 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??");
				GetMatrixSingleton = Marshal.GetDelegateForFunctionPointer<GetMatrixSingletonDelegate>(matrixSingletonAddress);

				// Select housing item.
				selectItemAddress = pi.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8B CE E8 ?? ?? ?? ?? 48 8B 6C 24 40 48 8B CE");
				SelectItem = Marshal.GetDelegateForFunctionPointer<SelectItemDelegate>(selectItemAddress);

				// Soft select hook.
				softSelectAddress = pi.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 83 3B 05 75 26 48 8B CB") + 9;
				SoftSelectHook = new Hook<SoftSelectDelegate>(softSelectAddress, new SoftSelectDelegate(SoftSelectDetour));
				// SoftSelectHook.Enable();

				// Address for the place item function.
				placeHousingItemAddress = pi.TargetModuleScanner.ScanText("40 53 48 83 EC 20 8B 02 48 8B D9 89 41 50 8B 42 04 89 41 54 8B 42 08 89 41 58 48 83 E9 80");
				PlaceHousingItem = Marshal.GetDelegateForFunctionPointer<PlaceHousingItemDelegate>(placeHousingItemAddress);

				// Housing item model update.
				housingLayoutModelUpdateAddress = pi.TargetModuleScanner.GetStaticAddressFromSig("48 8D 15 ?? ?? ?? ?? 0F 1F 40 00 48 8D 48 F0", 2) + 0x238;
				HousingLayoutModelUpdate = Marshal.GetDelegateForFunctionPointer<HousingLayoutModelUpdateDelegate>(housingLayoutModelUpdateAddress);

				// Thread loop to read active item.
				thread = new Thread(new ThreadStart(Loop));
				thread.Start();
				threadRunning = true;
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

				// Get rid of the hook.
				SoftSelectHook.Disable();
				SoftSelectHook.Dispose();

				// Enable the housing goods menu again.
				if (HousingGoods != null) SetVisible(HousingGoods, true);

				// Kind of pointless if I'm just gonna abort the thread but whatever.
				threadRunning = false;
				thread.Abort();
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

		private void SoftSelectDetour(IntPtr housing, IntPtr item)
		{
			if (item == IntPtr.Zero)
				return;
			SoftSelectHook.Original(housing, item);
			SelectItem(housing, item);
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
				throw new Exception("Housing structure is invalid!");

			// Ensure active item pointer isn't null.
			var item = HousingStructure->ActiveItem;
			if (item == null)
				throw new Exception("No valid item selected!");

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
				throw new Exception("Housing structure is invalid!");

			// Ensure active item pointer isn't null.
			var item = HousingStructure->ActiveItem;
			if (item == null)
				throw new Exception("No valid item selected!");

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
		public unsafe void Loop()
		{
			while (threadRunning)
			{
				try
				{
					if (CanEditItem())
					{
						position = ReadPosition();
						rotation = ReadRotation();
					}

					Thread.Sleep(50);
				}
				catch (Exception)
				{
					position = Vector3.Zero;
					rotation = Vector3.Zero;
				}
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

		/// <summary>
		/// Sets the flag for place anywhere in memory.
		/// </summary>
		/// <param name="state">Boolean state for if you can place anywhere.</param>
		public void SetPlaceAnywhere(bool state)
		{
			// The byte state from boolean.
			var bstate = (byte)(state ? 1 : 0);

			// Write the bytes for place anywhere.
			VirtualProtect(placeAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out var oldProtection);
			Marshal.WriteByte(placeAnywhere, bstate);
			VirtualProtect(placeAnywhere, 1, oldProtection, out _);

			// Write the bytes for wall anywhere.
			VirtualProtect(wallAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			Marshal.WriteByte(wallAnywhere, bstate);
			VirtualProtect(wallAnywhere, 1, oldProtection, out _);

			// Write the bytes for the wall mount anywhere.
			VirtualProtect(wallmountAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			Marshal.WriteByte(wallmountAnywhere, bstate);
			VirtualProtect(wallmountAnywhere, 1, oldProtection, out _);

			// Which bytes to write.
			var showcaseBytes = state ? new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 } : new byte[] { 0x88, 0x87, 0x73, 0x01, 0x00, 0x00 };

			// Write bytes for showcase anywhere (nop or original bytes).
			VirtualProtect(showcaseAnywhereRotate, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			WriteBytes(showcaseAnywhereRotate, showcaseBytes);
			VirtualProtect(showcaseAnywhereRotate, 1, oldProtection, out _);

			// Write bytes for showcase anywhere (nop or original bytes).
			VirtualProtect(showcaseAnywherePlace, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			WriteBytes(showcaseAnywherePlace, showcaseBytes);
			VirtualProtect(showcaseAnywherePlace, 1, oldProtection, out _);
		}

		/// <summary>
		/// Writes a series of bytes.
		/// </summary>
		/// <param name="ptr">Pointer to write to</param>
		/// <param name="bytes">The bytes to write</param>
		private void WriteBytes(IntPtr ptr, byte[] bytes)
		{
			for (var i = 0; i < bytes.Length; i++)
				Marshal.WriteByte(ptr + i, bytes[i]);
		}

		#region Kernel32

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, Protection flNewProtect, out Protection lpflOldProtect);

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
