using Dalamud.Hooking;
using Dalamud.Plugin;
using System;
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

		public IntPtr placeAnywhere;
		public IntPtr wallAnywhere;
		public IntPtr wallmountAnywhere;
		public IntPtr showcaseAnywhereRotate;
		public IntPtr showcaseAnywherePlace;

		public IntPtr housingStructure;
		public IntPtr selectedItem;

		public Vector3 position;
		public Vector3 rotation;

		public IntPtr selectedItemFunc;
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void HousingLoopDelegate(IntPtr housing);
		private readonly Hook<HousingLoopDelegate> housingHook;

		public PluginMemory(DalamudPluginInterface pluginInterface)
		{
			this.pi = pluginInterface;

			this.placeAnywhere = this.pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 4D") + 6;
			this.wallAnywhere = this.pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 80") + 6;
			this.wallmountAnywhere = this.pi.TargetModuleScanner.ScanText("C6 87 73 01 00 00 ?? 48 81 C4 80") + 6;
			this.showcaseAnywhereRotate = this.pi.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 8B");
			this.showcaseAnywherePlace = this.pi.TargetModuleScanner.ScanText("88 87 73 01 00 00 48 83");

			this.selectedItemFunc = this.pi.TargetModuleScanner.ScanText("40 53 48 83 EC 20 48 8B D9 8B 49 08 83 E9 02");
			this.housingHook = new Hook<HousingLoopDelegate>(
				selectedItemFunc, 
				new HousingLoopDelegate(HousingLoopDetour)
			);

			this.housingHook.Enable();

			this.thread = new Thread(new ThreadStart(this.Loop));
			this.thread.Start();
			this.threadRunning = true;
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

				// Kill the hook assuming it's not already dead.
				if (this.housingHook != null)
				{
					this.housingHook.Disable();
					this.housingHook.Dispose();
				}

				// Kind of pointless if I'm just gonna abort the thread but whatever.
				this.threadRunning = false;
				this.thread.Abort();
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, "Error while calling PluginMemory.Dispose()");
			}
		}

		/// <summary>
		/// Housing loop detour.
		/// </summary>
		/// <param name="housing">Housing data structure</param>
		private void HousingLoopDetour(IntPtr housing)
		{
			// Call the original function.
			this.housingHook.Original(housing);

			// Set the housing struct address.
			this.housingStructure = housing;
		}


		/// <summary>
		/// Is the housing menu on.
		/// </summary>
		/// <returns>Boolean state if housing menu is on or off.</returns>
		public bool IsHousingModeOn()
		{
			if (this.housingStructure == IntPtr.Zero)
				return false;

			// Read the tool ID
			var toolID = Marshal.ReadByte(this.housingStructure);

			// Valid territory array.
			var valid = new ushort[]
			{
				345, 346, 347, 386, // The Goblet
				342, 343, 344, 358, // Lavender Beds
				282, 283, 284, 384, // Mist
				649, 650, 651, 652, // Shirogane
			};

			if (!valid.Contains(this.pi.ClientState.TerritoryType))
				return false;

			// Tool ID is set to rotation.
			return toolID == 2;
		}

		/// <summary>
		/// Read the position of the active item.
		/// </summary>
		/// <returns>Vector3 of the position.</returns>
		public Vector3 ReadPosition()
		{
			// Ensure that we're hooked and have the housing structure address.
			if (this.housingStructure == IntPtr.Zero)
				throw new Exception("Housing structure is invalid!");

			// Get the item from the structure to see when it's also invalid.
			var item = Marshal.ReadIntPtr(this.housingStructure + 0x18);

			// Set the selected item.
			this.selectedItem = item;

			// Ensure we have a valid pointer for the item.
			if (item == IntPtr.Zero)
				throw new Exception("No valid item selected!");

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

		/// <summary>
		/// Reads the rotation of the item.
		/// </summary>
		/// <returns></returns>
		public Vector3 ReadRotation()
		{
			// Ensure that we're hooked and have the housing structure address.
			if (this.housingStructure == IntPtr.Zero)
				throw new Exception("Housing structure is invalid!");

			// Get the item from the structure to see when it's also invalid.
			var item = Marshal.ReadIntPtr(this.housingStructure + 0x18);

			// Sets the item.
			this.selectedItem = item;

			// Ensure we have a valid pointer for the item.
			if (item == IntPtr.Zero)
				throw new Exception("No valid item selected!");

			// Rotation offset from the selected item.
			var rotation = item + 0x60;

			// Set up bytes to marshal over the data.
			var bytes = new byte[16];
			// Copy rotation into managed bytes array.
			Marshal.Copy(rotation, bytes, 0, 16);

			// Convert bytes for the quaternion.
			var x = BitConverter.ToSingle(bytes, 0);
			var y = BitConverter.ToSingle(bytes, 4);
			var z = BitConverter.ToSingle(bytes, 8);
			var w = BitConverter.ToSingle(bytes, 12);

			// Return the rotation radian.
			return RotationMath.FromQ(new Quaternion(x, y, z, w));
		}

		/// <summary>
		/// Writes the position vector to memory.
		/// </summary>
		/// <param name="newPosition">Position vector to write.</param>
		public void WritePosition(Vector3 newPosition)
		{
			// Don't write if housing mode isn't on.
			if (!this.IsHousingModeOn())
				return;

			try
			{
				// Get the item from the structure to see when it's also invalid.
				var item = Marshal.ReadIntPtr(this.housingStructure + 0x18);

				// Ensure we have a valid pointer for the item.
				if (item == IntPtr.Zero)
					return;

				// Position offset from the selected item.
				var position = item + 0x50;

				unsafe
				{
					// Write the position to memory.
					*(Vector3*)position = newPosition;
				}
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, "Error occured while writing position!");
			}
		}

		public void WriteRotation(Vector3 newRotation)
		{
			// Don't write if housing mode isn't on.
			if (!this.IsHousingModeOn())
				return;

			try
			{
				// Get the item from the structure to see when it's also invalid.
				var item = Marshal.ReadIntPtr(this.housingStructure + 0x18);

				// Ensure we have a valid pointer for the item.
				if (item == IntPtr.Zero)
					return;

				// Rotation offset from the selected item.
				var x = item + 0x60;
				var y = item + 0x64;
				var z = item + 0x68;
				var w = item + 0x6C;

				// Convert into a quaternion.
				var q = RotationMath.ToQ(newRotation);

				unsafe
				{
					// Write the rotation to memory.
					*(float*)w = q.W;
					*(float*)x = q.X;
					*(float*)y = q.Y;
					*(float*)z = q.Z;
				}
			}
			catch (Exception ex)
			{
				PluginLog.LogError(ex, "Error occured while writing rotation!");
			}
		}

		/// <summary>
		/// Thread loop for reading memory.
		/// </summary>
		public void Loop()
		{
			while (this.threadRunning)
			{
				try
				{
					if (this.IsHousingModeOn())
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
