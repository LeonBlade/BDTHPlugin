using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Logging;

namespace BDTHPlugin
{
	public class PluginMemory
	{
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

		public unsafe LayoutWorld* Layout => (LayoutWorld*)this.layoutWorldPtr;
		public unsafe HousingStructure* HousingStructure => this.Layout->HousingStruct;
		public unsafe HousingModule* HousingModule => (HousingModule*)this.housingModulePtr;
		public unsafe HousingObjectManger* CurrentManager => this.HousingModule->GetCurrentManager();

		public unsafe AtkUnitBase* HousingLayout => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingLayout", 1);
		public unsafe bool GamepadMode => !(this.HousingLayout != null && this.HousingLayout->IsVisible);

		public unsafe AtkUnitBase* HousingGoods => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingGoods", 1);
		public unsafe AtkUnitBase* InventoryExpansion => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryExpansion", 1);
		public unsafe AtkUnitBase* InventoryGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0E", 1);
		public unsafe AtkUnitBase* InventoryGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1E", 1);
		public unsafe AtkUnitBase* InventoryGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid2E", 1);
		public unsafe AtkUnitBase* InventoryGrid3E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid3E", 1);
		public unsafe AtkUnitBase* InventoryLarge => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryLarge", 1);
		public unsafe AtkUnitBase* Inventory => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("Inventory", 1);
		public unsafe AtkUnitBase* InventoryGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0", 1);
		public unsafe AtkUnitBase* InventoryGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1", 1);
		public unsafe AtkUnitBase* InventoryEventGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0", 1);
		public unsafe AtkUnitBase* InventoryEventGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1", 1);
		public unsafe AtkUnitBase* InventoryEventGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2", 1);
		public unsafe AtkUnitBase* InventoryGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid", 1);
		public unsafe AtkUnitBase* InventoryEventGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0E", 1);
		public unsafe AtkUnitBase* InventoryEventGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1E", 1);
		public unsafe AtkUnitBase* InventoryEventGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2E", 1);
		public unsafe AtkUnitBase* InventoryGridCrystal => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGridCrystal", 1);
		public unsafe AtkUnitBase* InventoryCrystalGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 1);
		public unsafe AtkUnitBase* InventoryCrystalGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 2);

		public unsafe bool InventoryVisible
		{
			get => this.InventoryExpansion != null && this.InventoryExpansion->IsVisible ||
					this.InventoryLarge != null && this.InventoryLarge->IsVisible ||
					this.Inventory != null && this.Inventory->IsVisible;

			set
			{
				if (this.HousingGoods == null || this.InventoryExpansion == null || this.InventoryLarge == null || this.Inventory == null)
					return;

				if (value)
				{
					switch (inventoryType)
					{
						case 1:
							this.InventoryExpansion->IsVisible = true;
							this.InventoryGrid0E->IsVisible = true;
							this.InventoryGrid1E->IsVisible = true;
							this.InventoryGrid2E->IsVisible = true;
							this.InventoryGrid3E->IsVisible = true;
							this.InventoryEventGrid0E->IsVisible = true;
							this.InventoryEventGrid1E->IsVisible = true;
							this.InventoryEventGrid2E->IsVisible = true;
							this.InventoryCrystalGrid2->IsVisible = true;
							break;
						case 2:
							this.InventoryLarge->IsVisible = true;
							this.InventoryGrid0->IsVisible = true;
							this.InventoryGrid1->IsVisible = true;
							this.InventoryEventGrid0->IsVisible = true;
							this.InventoryEventGrid1->IsVisible = true;
							this.InventoryEventGrid2->IsVisible = true;
							this.InventoryCrystalGrid->IsVisible = true;
							break;
						case 3:
							this.Inventory->IsVisible = true;
							this.InventoryGrid->IsVisible = true;
							this.InventoryGridCrystal->IsVisible = true;
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
						this.InventoryExpansion->IsVisible = false;
						this.InventoryGrid0E->IsVisible = false;
						this.InventoryGrid1E->IsVisible = false;
						this.InventoryGrid2E->IsVisible = false;
						this.InventoryGrid3E->IsVisible = false;
						this.InventoryEventGrid0E->IsVisible = false;
						this.InventoryEventGrid1E->IsVisible = false;
						this.InventoryEventGrid2E->IsVisible = false;
						this.InventoryCrystalGrid2->IsVisible = false;
					}

					if (InventoryLarge->Flags == 52)
					{
						inventoryType = 2;
						this.InventoryLarge->IsVisible = false;
						this.InventoryGrid0->IsVisible = false;
						this.InventoryGrid1->IsVisible = false;
						this.InventoryEventGrid0->IsVisible = false;
						this.InventoryEventGrid1->IsVisible = false;
						this.InventoryEventGrid2->IsVisible = false;
						this.InventoryCrystalGrid->IsVisible = false;
					}

					if (Inventory->Flags == 52)
					{
						inventoryType = 3;
						this.Inventory->IsVisible = false;
						this.InventoryGrid->IsVisible = false;
						this.InventoryGridCrystal->IsVisible = false;
					}
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

		public PluginMemory()
		{
			try
			{
				// Assembly address for asm rewrites.
				this.placeAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 4D") + 6;
				this.wallAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 80") + 6;
				this.wallmountAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 48 81 C4 80") + 6;
				this.showcaseAnywhereRotate = Plugin.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 8B");
				this.showcaseAnywherePlace = Plugin.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 83");

				// Pointers for housing structures.
				this.layoutWorldPtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 49 40 E9 ?? ?? ?? ??", 2);
				this.housingModulePtr = Plugin.TargetModuleScanner.GetStaticAddressFromSig("40 53 48 83 EC 20 33 DB 48 39 1D ?? ?? ?? ?? 75 2C 45 33 C0 33 D2 B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 11 48 8B C8 E8 ?? ?? ?? ?? 48 89 05 ?? ?? ?? ?? EB 07", 0xA);
				// Read the pointers.
				this.layoutWorldPtr = Marshal.ReadIntPtr(this.layoutWorldPtr);
				this.housingModulePtr = Marshal.ReadIntPtr(this.housingModulePtr);

				// Matrix address for gizmo usage.
				this.matrixSingletonAddress = Plugin.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4c 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??");
				this.GetMatrixSingleton = Marshal.GetDelegateForFunctionPointer<GetMatrixSingletonDelegate>(this.matrixSingletonAddress);

				// Select housing item.
				this.selectItemAddress = Plugin.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 48 8B CE E8 ?? ?? ?? ?? 48 8B 6C 24 40 48 8B CE");
				this.SelectItem = Marshal.GetDelegateForFunctionPointer<SelectItemDelegate>(this.selectItemAddress);

				// Soft select hook.
				this.softSelectAddress = Plugin.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 83 3B 05 75 26 48 8B CB") + 9;
				this.SoftSelectHook = new Hook<SoftSelectDelegate>(this.softSelectAddress, new SoftSelectDelegate(SoftSelectDetour));
				// this.SoftSelectHook.Enable();

				// Address for the place item function.
				this.placeHousingItemAddress = Plugin.TargetModuleScanner.ScanText("40 53 48 83 EC 20 8B 02 48 8B D9 89 41 50 8B 42 04 89 41 54 8B 42 08 89 41 58 48 83 E9 80");
				this.PlaceHousingItem = Marshal.GetDelegateForFunctionPointer<PlaceHousingItemDelegate>(this.placeHousingItemAddress);

				// Housing item model update.
				this.housingLayoutModelUpdateAddress = Plugin.TargetModuleScanner.GetStaticAddressFromSig("48 8D 15 ?? ?? ?? ?? 0F 1F 40 00 48 8D 48 F0", 2) + 0x238;
				this.HousingLayoutModelUpdate = Marshal.GetDelegateForFunctionPointer<HousingLayoutModelUpdateDelegate>(this.housingLayoutModelUpdateAddress);

				// Thread loop to read active item.
				this.thread = new Thread(new ThreadStart(this.Loop));
				this.thread.Start();
				this.threadRunning = true;
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
				this.SetPlaceAnywhere(false);

				// Get rid of the hook.
				this.SoftSelectHook.Disable();
				this.SoftSelectHook.Dispose();

				// Enable the housing goods menu again.
				if (this.HousingGoods != null) this.HousingGoods->IsVisible = true;

				// Kind of pointless if I'm just gonna abort the thread but whatever.
				this.threadRunning = false;
				this.thread.Abort();
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
				if (this.HousingModule->GetCurrentManager()->Objects[i] == 0)
					continue;
				if ((ulong)this.HousingModule->GetCurrentManager()->IndoorActiveObject == this.HousingModule->GetCurrentManager()->Objects[i])
					return i;
			}
			return -1;
		}

		private void SoftSelectDetour(IntPtr housing, IntPtr item)
		{
			if (item == IntPtr.Zero)
				return;
			this.SoftSelectHook.Original(housing, item);
			this.SelectItem(housing, item);
		}

		/// <summary>
		/// Is the housing menu open.
		/// </summary>
		/// <returns>Boolean state.</returns>
		public unsafe bool IsHousingOpen()
		{
			if (this.HousingStructure == null)
				return false;

			// Anything other than none means the housing menu is open.
			return this.HousingStructure->Mode != HousingLayoutMode.None;
		}

		/// <summary>
		/// Checks if you can edit a housing item, specifically checks that rotate mode is active.
		/// </summary>
		/// <returns>Boolean state if housing menu is on or off.</returns>
		public unsafe bool CanEditItem()
		{
			if (this.HousingStructure == null)
				return false;

			// Rotate mode only.
			return this.HousingStructure->Mode == HousingLayoutMode.Rotate;
		}

		/// <summary>
		/// Read the position of the active item.
		/// </summary>
		/// <returns>Vector3 of the position.</returns>
		public unsafe Vector3 ReadPosition()
		{
			// Ensure that we're hooked and have the housing structure address.
			if (this.HousingStructure == null)
				throw new Exception("Housing structure is invalid!");

			// Ensure active item pointer isn't null.
			var item = this.HousingStructure->ActiveItem;
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
			if (this.HousingStructure == null)
				throw new Exception("Housing structure is invalid!");

			// Ensure active item pointer isn't null.
			var item = this.HousingStructure->ActiveItem;
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
			if (!this.CanEditItem())
				return;

			try
			{
				var item = this.HousingStructure->ActiveItem;
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
			if (!this.CanEditItem())
				return;

			try
			{
				var item = this.HousingStructure->ActiveItem;
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
			while (this.threadRunning)
			{
				try
				{
					if (this.CanEditItem())
					{
						this.position = this.ReadPosition();
						this.rotation = this.ReadRotation();
					}

					Thread.Sleep(50);
				}
				catch (Exception)
				{
					this.position = Vector3.Zero;
					this.rotation = Vector3.Zero;
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
				return this.GetFurnishingByDistance(out objects, point);

			objects = new List<HousingGameObject>();

			if (this.HousingModule == null || this.HousingModule->GetCurrentManager() == null || this.HousingModule->GetCurrentManager()->Objects == null)
				return false;

			for (var i = 0; i < 400; i++)
			{
				var oPtr = this.HousingModule->GetCurrentManager()->Objects[i];
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

			if (this.HousingModule == null || this.HousingModule->GetCurrentManager() == null || this.HousingModule->GetCurrentManager()->Objects == null)
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
			VirtualProtect(this.placeAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out var oldProtection);
			Marshal.WriteByte(this.placeAnywhere, bstate);
			VirtualProtect(this.placeAnywhere, 1, oldProtection, out _);

			// Write the bytes for wall anywhere.
			VirtualProtect(this.wallAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			Marshal.WriteByte(this.wallAnywhere, bstate);
			VirtualProtect(this.wallAnywhere, 1, oldProtection, out _);

			// Write the bytes for the wall mount anywhere.
			VirtualProtect(this.wallmountAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			Marshal.WriteByte(this.wallmountAnywhere, bstate);
			VirtualProtect(this.wallmountAnywhere, 1, oldProtection, out _);

			// Which bytes to write.
			var showcaseBytes = state ? new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 } : new byte[] { 0x88, 0x87, 0x73, 0x01, 0x00, 0x00 };

			// Write bytes for showcase anywhere (nop or original bytes).
			VirtualProtect(this.showcaseAnywhereRotate, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			WriteBytes(this.showcaseAnywhereRotate, showcaseBytes);
			VirtualProtect(this.showcaseAnywhereRotate, 1, oldProtection, out _);

			// Write bytes for showcase anywhere (nop or original bytes).
			VirtualProtect(this.showcaseAnywherePlace, 1, Protection.PAGE_EXECUTE_READWRITE, out oldProtection);
			WriteBytes(this.showcaseAnywherePlace, showcaseBytes);
			VirtualProtect(this.showcaseAnywherePlace, 1, oldProtection, out _);
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
