using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace BDTHPlugin.Interface.Windows {
  public class DebugWindow : Window
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    
    public DebugWindow() : base("BDTH Debug")
    {
      
    }

    public unsafe override void Draw()
    {
      ImGui.Text($"Gamepad Mode: {PluginMemory.GamepadMode}");
      ImGui.Text($"CanEditItem: {Memory.CanEditItem()}");
      ImGui.Text($"IsHousingOpen: {Memory.IsHousingOpen()}");
      ImGui.Separator();
      ImGui.Text($"LayoutWorld: {(ulong)Memory.Layout:X}");
      ImGui.Text($"Housing Structure: {(ulong)Memory.HousingStructure:X}");
      ImGui.Text($"Mode: {Memory.HousingStructure->Mode}");
      ImGui.Text($"State: {Memory.HousingStructure->State}");
      ImGui.Text($"State2: {Memory.HousingStructure->State2}");
      ImGui.Text($"Active: {(ulong)Memory.HousingStructure->ActiveItem:X}");
      ImGui.Text($"Hover: {(ulong)Memory.HousingStructure->HoverItem:X}");
      ImGui.Text($"Rotating: {Memory.HousingStructure->Rotating}");
      ImGui.Separator();
      ImGui.Text($"Housing Module: {(ulong)Memory.HousingModule:X}");
      ImGui.Text($"Housing Module: {(ulong)Memory.HousingModule->CurrentTerritory:X}");
      ImGui.Text($"Outdoor Territory: {(ulong)Memory.HousingModule->OutdoorTerritory:X}");
      ImGui.Text($"Indoor Territory: {(ulong)Memory.HousingModule->IndoorTerritory:X}");
      var active = Memory.HousingStructure->ActiveItem;
      if (active != null)
      {
        ImGui.Separator();
        var pos = Memory.HousingStructure->ActiveItem->Position;
        ImGui.Text($"Position: {pos.X}, {pos.Y}, {pos.Z}");
      }
    }
  }
}
