using System;
using System.Numerics;

using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;

using BDTHPlugin.Interface.Components;

namespace BDTHPlugin.Interface.Windows
{
  public class MainWindow : Window
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private static readonly Vector4 RED_COLOR = new(1, 0, 0, 1);

    private readonly Gizmo Gizmo;
    private readonly ItemControls ItemControls = new();

    public bool Reset;

    public MainWindow(Gizmo gizmo) : base(
      "Burning Down the House##BDTH",
      ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize |
      ImGuiWindowFlags.AlwaysAutoResize
    )
    {
      Gizmo = gizmo;
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
      ImGui.BeginGroup();

      var placeAnywhere = Configuration.PlaceAnywhere;
      if (ImGui.Checkbox("Place Anywhere", ref placeAnywhere))
      {
        // Set the place anywhere based on the checkbox state.
        Memory.SetPlaceAnywhere(placeAnywhere);
        Configuration.PlaceAnywhere = placeAnywhere;
        Configuration.Save();
      }
      DrawTooltip("Allows the placement of objects without limitation from the game engine.");

      ImGui.SameLine();

      // Checkbox is clicked, set the configuration and save.
      var useGizmo = Configuration.UseGizmo;
      if (ImGui.Checkbox("Gizmo", ref useGizmo))
      {
        Configuration.UseGizmo = useGizmo;
        Configuration.Save();
      }
      DrawTooltip("Displays a movement gizmo on the selected item to allow for in-game movement on all axis.");

      ImGui.SameLine();

      // Checkbox is clicked, set the configuration and save.
      var doSnap = Configuration.DoSnap;
      if (ImGui.Checkbox("Snap", ref doSnap))
      {
        Configuration.DoSnap = doSnap;
        Configuration.Save();
      }
      DrawTooltip("Enables snapping of gizmo movement based on the drag value set below.");

      ImGui.SameLine();
      if (ImGuiComponents.IconButton(1, Gizmo.Mode == ImGuizmoMode.Local ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
        Gizmo.Mode = Gizmo.Mode == ImGuizmoMode.Local ? ImGuizmoMode.World : ImGuizmoMode.Local;

      DrawTooltip(
      [
        $"Mode: {(Gizmo.Mode == ImGuizmoMode.Local ? "Local" : "World")}",
        "Changes gizmo mode between local and world movement."
      ]);

      ImGui.Separator();

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

      var dummyHousingGoods = PluginMemory.HousingGoods != null && PluginMemory.HousingGoods.IsVisible;
      var dummyInventory = Memory.InventoryVisible;

      if (ImGui.Checkbox("Display in-game list", ref dummyHousingGoods))
      {
        Memory.ShowFurnishingList(dummyHousingGoods);

        Configuration.DisplayFurnishingList = dummyHousingGoods;
        Configuration.Save();
      }
      ImGui.SameLine();

      if (ImGui.Checkbox("Display inventory", ref dummyInventory))
      {
        Memory.ShowInventory(dummyInventory);

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

    private static void DrawTooltip(string[] text)
    {
      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();
        foreach (var t in text)
          ImGui.Text(t);
        ImGui.EndTooltip();
      }
    }

    private static void DrawTooltip(string text)
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
