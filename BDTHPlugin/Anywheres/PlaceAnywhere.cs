using System;

namespace BDTHPlugin.Anywheres;

public static class PlaceAnywhere
{
    private static IntPtr _placeAnywhere;
    private static IntPtr _wallAnywhere;
    private static IntPtr _wallmountAnywhere;
    
    public static void Initialize()
    {
        _placeAnywhere = Plugin.TargetModuleScanner.ScanText("C6 83 ?? ?? 00 00 00 0F 29 44 24") + 6;
        _wallAnywhere = Plugin.TargetModuleScanner.ScanText("48 85 C0 74 ?? C6 87 ?? ?? 00 00 00") + 11;
        _wallmountAnywhere = Plugin.TargetModuleScanner.ScanText("C6 87 83 01 00 00 00 48 83 C4") + 6;
    }

    public static void SetState(bool enabled)
    {
        // Return out early if we don't have any of the addresses
        if (_placeAnywhere == IntPtr.Zero || _wallAnywhere == IntPtr.Zero || _wallmountAnywhere == IntPtr.Zero)
            return;

        var b = (byte)(enabled ? 1 : 0);

        PluginMemory.WriteProtectedBytes(_placeAnywhere, b);
        PluginMemory.WriteProtectedBytes(_wallAnywhere, b);
        PluginMemory.WriteProtectedBytes(_wallmountAnywhere, b);
    }
}