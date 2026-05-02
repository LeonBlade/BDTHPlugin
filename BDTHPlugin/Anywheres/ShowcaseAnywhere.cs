using System;
using System.Linq;

namespace BDTHPlugin.Anywheres;

public static class ShowcaseAnywhere
{
    private static IntPtr _placeAddress;
    private static IntPtr _rotateAddress;

    private static byte[]? _placeOriginal;
    private static byte[]? _rotateOriginal;
    
    private static byte[]? _placeNoop;
    private static byte[]? _rotateNoop;
    
    public static void Initialize()
    {
        // Get the addresses that make up allowing showcases to place anywhere
        _placeAddress = Plugin.TargetModuleScanner.ScanText("C6 87 ?? ?? 00 00 00 48 8B BC 24 ?? ?? ?? ?? 48");
        _rotateAddress = Plugin.TargetModuleScanner.ScanText("88 87 ?? ?? 00 00 0F 28 74 24 ?? 48 8B");

        // Get the original bytes that we're going to noop later
        _placeOriginal = PluginMemory.ReadBytes(_placeAddress, 7);
        _rotateOriginal = PluginMemory.ReadBytes(new IntPtr(_rotateAddress), 6);
        
        // Create the noop bytes
        _placeNoop = Enumerable.Repeat((byte)0x90, _placeOriginal.Length).ToArray();
        _rotateNoop = Enumerable.Repeat((byte)0x90, _rotateOriginal.Length).ToArray();
    }

    public static void SetState(bool state)
    {
        // Didn't find the addresses
        if (_placeAddress == IntPtr.Zero || _rotateAddress == IntPtr.Zero)
            return;

        // Didn't read anything
        if (_placeOriginal == null || _rotateOriginal == null || _placeNoop == null || _rotateNoop == null)
            return;

        // Write the bytes based on what state we're in
        PluginMemory.WriteProtectedBytes(_placeAddress, state ? _placeNoop : _placeOriginal);
        PluginMemory.WriteProtectedBytes(_rotateAddress, state ? _rotateNoop : _rotateOriginal);
    }
}