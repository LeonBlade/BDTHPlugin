using System;
using System.Numerics;

using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using BDTHPlugin.Interface.Components;
using BDTHPlugin.Game;

namespace BDTHPlugin.Interface.Windows
{
  public class MainWindow : Window
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private static readonly Vector4 RED_COLOR = new(1, 0, 0, 1);

    private readonly Gizmo Gizmo;
    private readonly BasicControls BasicControls;
    private readonly ItemControls ItemControls = new();

    public bool Reset;

    public MainWindow(Gizmo gizmo) : base(
      "Burning Down the House##BDTH",
      ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize |
      ImGuiWindowFlags.AlwaysAutoResize
    )
    {
      Gizmo = gizmo;
      BasicControls = new BasicControls(Gizmo);
    }

    public override void PreDraw()
    {
      if (Reset)
      {
        Reset = false;
        ImGui.SetNextWindowPos(new Vector2(69, 69), ImGuiCond.Always);
      }
    }

    public unsafe override void Draw()
    {
      BasicControls.Draw();

      if (Memory.HousingStructure->Mode == HousingLayoutMode.None)
        DrawError("Enter housing mode to get started");
      else if (PluginMemory.GamepadMode)
        DrawError("Does not support Gamepad");
      else if (Memory.HousingStructure->ActiveItem == null || Memory.HousingStructure->Mode != HousingLayoutMode.Rotate)
      {
        DrawError("Select a housing item in Rotate mode");
        ImGuiComponents.HelpMarker("Are you doing everything right? Try using the /bdth debug command and report this issue in Discord!");
      }
      else
        ItemControls.Draw();

      ImGui.Separator();

      // Drag amount for the inputs.
      var drag = Configuration.Drag;
      if (ImGui.InputFloat("drag", ref drag, 0.05f))
      {
        drag = Math.Min(Math.Max(0.001f, drag), 10f);
        Configuration.Drag = drag;
        Configuration.Save();
      }
      DrawTooltip("Sets the amount to change when dragging the controls, also influences the gizmo snap feature.");

      var dummyHousingGoods = Addons.HousingGoods != null && Addons.HousingGoods->IsVisible;
      var dummyInventory = Addons.InventoryVisible;

      if (ImGui.Checkbox("Display in-game list", ref dummyHousingGoods))
      {
        Addons.ShowFurnishingList(dummyHousingGoods);

        Configuration.DisplayFurnishingList = dummyHousingGoods;
        Configuration.Save();
      }
      ImGui.SameLine();

      if (ImGui.Checkbox("Display inventory", ref dummyInventory))
      {
        Addons.ShowInventory(dummyInventory);

        Configuration.DisplayInventory = dummyInventory;
        Configuration.Save();
      }

      if (ImGui.Button("Open Furnishing List"))
        Plugin.CommandManager.ProcessCommand("/bdth list");
      DrawTooltip(
      [
        "Opens a furnishing list that you can use to sort by distance and click to select objects.",
        "NOTE: Does not currently work outdoors!"
      ]);

      var autoVisible = Configuration.AutoVisible;
      if (ImGui.Checkbox("Auto Open", ref autoVisible))
      {
        Configuration.AutoVisible = autoVisible;
        Configuration.Save();
      }
    }

    public static void DrawTooltip(string[] text)
    {
      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();
        foreach (var t in text)
          ImGui.Text(t);
        ImGui.EndTooltip();
      }
    }

    public static void DrawTooltip(string text)
    {
      DrawTooltip([text]);
    }

    private static void DrawError(string text)
    {
      ImGui.PushStyleColor(ImGuiCol.Text, RED_COLOR);
      ImGui.Text(text);
      ImGui.PopStyleColor();
    }
  }
}
