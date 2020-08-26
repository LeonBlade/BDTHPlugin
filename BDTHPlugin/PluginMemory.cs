using Dalamud.Hooking;
using Dalamud.Plugin;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace BDTHPlugin
{
	public static class PluginMemory
	{
		private static DalamudPluginInterface PluginInterface;

		public static IntPtr placeAnywhere;
		public static IntPtr wallAnywhere;
		public static IntPtr wallmountAnywhere;
		public static IntPtr housingStructure;
		public static IntPtr selectedItem;

		public static Vector3 position;

		public static IntPtr selectedItemFunc;
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		private delegate void SelectedItemDelegate(IntPtr housing, IntPtr item);
		private static Hook<SelectedItemDelegate> selectedItemHook;

		public static void Initialize(DalamudPluginInterface pluginInterface)
		{
			// Set the plugin interface.
			PluginInterface = pluginInterface ?? throw new Exception("Error in MemoryHandler.Initialize: A null plugin interface was passed!");

			// Place Anywhere byte.
			placeAnywhere = pluginInterface.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 4D") + 6;
			// Wall placement anywhere byte.
			wallAnywhere = PluginInterface.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 80") + 6;
			// Wall mounted movement
			wallmountAnywhere = PluginInterface.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 48 81 C4 80") + 6;

			// Active item address, the sig is a call to the function I'm hooking plus 9 byte padding to skip test and a jump which can't be hooked
			// Thanks Wintermute <3
			selectedItemFunc = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? 00 00 48 8B CE E8 ?? ?? 00 00 48 8B 6C 24 ?? 48 8B CE") + 9;
			selectedItemHook = new Hook<SelectedItemDelegate>(selectedItemFunc, new SelectedItemDelegate(ActiveItemDetour));
			selectedItemHook.Enable();
		}

		/// <summary>
		/// Dispose for the memory functions.
		/// </summary>
		public static void Dispose()
		{
			try
			{
				// Disable the place anywhere in case it's on.
				SetPlaceAnywhere(false);

				// Kill the hook assuming it's not already dead.
				if (selectedItemHook != null)
				{
					selectedItemHook.Disable();
					selectedItemHook.Dispose();
				}
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, "Error while calling PluginMemory.Dispose()");
			}
		}

		/// <summary>
		/// The active item func detour.
		/// </summary>
		/// <param name="housing">Housing data structure.</param>
		/// <param name="item">The selected item address</param>
		private static void ActiveItemDetour(IntPtr housing, IntPtr item)
		{
			// Call the original function.
			selectedItemHook.Original(housing, item);

			// Set the housing struct address.
			housingStructure = housing;

			// Set the active item address to the one passed in the function.
			selectedItem = item;
		}


		/// <summary>
		/// Is the housing menu on.
		/// </summary>
		/// <returns>Boolean state if housing menu is on or off.</returns>
		public static bool IsHousingModeOn()
		{
			if (housingStructure == IntPtr.Zero)
				return false;

			// Read the tool ID
			var toolID = Marshal.ReadByte(housingStructure);

			// Tool ID 1 or higher means the user is using housing tool.
			return toolID > 0;
		}

		/// <summary>
		/// Read the position of the active item.
		/// </summary>
		/// <returns>Vector3 of the position.</returns>
		public static Vector3 ReadPosition()
		{
			try
			{
				// Ensure that we're hooked and have the housing structure address.
				if (housingStructure == IntPtr.Zero)
					return Vector3.Zero;

				// Get the item from the structure to see when it's also invalid.
				var item = Marshal.ReadIntPtr(housingStructure + 0x18);

				// Ensure we have a valid pointer for the item.
				if (item == IntPtr.Zero)
					return Vector3.Zero;

				// Position offset from the selected item.
				var position = item + 0x50;

				// Set up bytes to marshal over the data.
				var bytes = new byte[12];
				// Copy position into managed bytes array.
				Marshal.Copy(position, bytes, 0, 12);

				// Convert coords for the vector.
				var x = BitConverter.ToSingle(bytes, 0);
				var y = BitConverter.ToSingle(bytes, 4);
				var z = BitConverter.ToSingle(bytes, 8);

				// Return the position vector.
				return new Vector3(x, y, z);
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, $"Error occured while reading position at {housingStructure:X}.");
			}

			return Vector3.Zero;
		}

		/// <summary>
		/// Writes the position vector to memory.
		/// </summary>
		/// <param name="newPosition">Position vector to write.</param>
		public static void WritePosition(Vector3 newPosition)
		{
			try
			{
				// Get the item from the structure to see when it's also invalid.
				var item = Marshal.ReadIntPtr(housingStructure + 0x18);

				// Ensure we have a valid pointer for the item.
				if (item == IntPtr.Zero)
					return;

				// Position offset from the selected item.
				var position = item + 0x50;

				// Write the position to memory.
				unsafe
				{
					*(Vector3*)position = newPosition;
				}
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, "Error occured while writing position.");
			}
		}

		public static void Loop()
		{
			while (true)
			{
				try
				{
					position = ReadPosition();

					Thread.Sleep(50);
				}
				catch (ThreadAbortException)
				{

				}
			}
		}

		/// <summary>
		/// Sets the flag for place anywhere in memory.
		/// </summary>
		/// <param name="state">Boolean state for if you can place anywhere.</param>
		public static void SetPlaceAnywhere(bool state)
		{
			// The byte state from boolean.
			var bstate = (byte)(state ? 1 : 0);

			// Write the bytes for place anywhere.
			VirtualProtect(placeAnywhere, 1, Protection.PAGE_EXECUTE_READWRITE, out Protection oldProtection);
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
