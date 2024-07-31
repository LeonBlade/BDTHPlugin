using BDTHPlugin.Interface.Windows;
using Dalamud.Interface.Components;
using ImGuiNET;
using ImGuizmoNET;

namespace BDTHPlugin.Interface.Components;

public class BasicControls
{
  private static PluginMemory Memory => Plugin.GetMemory();
  private static Configuration Configuration => Plugin.GetConfiguration();
  private void DrawTooltip(string str) => MainWindow.DrawTooltip(str);
  private Gizmo gizmo;

  public BasicControls(Gizmo gizmo)
  {
    this.gizmo = gizmo;
  }

  public unsafe void Draw()
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
    if (ImGuiComponents.IconButton(1, gizmo.Mode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
      gizmo.Mode = gizmo.Mode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;

    MainWindow.DrawTooltip(
    [
      $"Mode: {(gizmo.Mode == MODE.LOCAL ? "Local" : "World")}",
        "Changes gizmo mode between local and world movement."
    ]);

    ImGui.EndGroup();

    ImGui.Separator();
  }
}