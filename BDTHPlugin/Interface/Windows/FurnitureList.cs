using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace BDTHPlugin.Interface.Windows
{
  public class FurnitureList() : Window("Furnishing List")
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private ulong? _lastActiveItem;
    private byte _renderCount;

    public override void PreDraw()
    {
      // Only allows furnishing list when the housing window is open.
      // Disallows the ability to open furnishing list outdoors.
      IsOpen &= Memory.IsHousingOpen() && !Plugin.IsOutdoors();
    }

    private static void DrawDistance(float distance)
    {
      ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
  
      var distanceString = distance.ToString("F2");
              
      var strWidth = ImGui.CalcTextSize(distanceString).X;
      var containerWidth = ImGui.GetContentRegionAvail().X;

      var padding = ImGui.GetStyle().ItemSpacing.X;
      var x = ImGui.GetCursorPosX() + containerWidth - strWidth - padding;

      ImGui.SetCursorPosX(x);
      ImGui.Text(distanceString);
      ImGui.PopStyleColor();
    }

    private static (string, ushort) GetHousingItem(GameObject gameObject)
    {
      var name = "";
      ushort icon = 0;

      if (Plugin.TryGetYardObject(gameObject.BaseId, out var yardObject))
      {
        name = yardObject.Item.Value.Name.ToString();
        icon = yardObject.Item.Value.Icon;
      }
      else if (Plugin.TryGetFurnishing(gameObject.BaseId, out var furnitureObject))
      {
        name = furnitureObject.Item.Value.Name.ToString();
        icon = furnitureObject.Item.Value.Icon;
      }

      return (name, icon);
    }

    public override unsafe void Draw()
    {
      var fontScale = ImGui.GetIO().FontGlobalScale;
      var hasActiveItem = Memory.HousingStructure->ActiveItem != null;

      SizeConstraints = new WindowSizeConstraints
      {
        MinimumSize = new Vector2(120 * fontScale, 100 * fontScale),
        MaximumSize = new Vector2(400 * fontScale, 1000 * fontScale)
      };

      var sortByDistance = Configuration.SortByDistance;
      if (ImGui.Checkbox("Sort by distance", ref sortByDistance))
      {
        Configuration.SortByDistance = sortByDistance;
        Configuration.Save();
      }

      ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
      ImGui.Separator();
      ImGui.PopStyleVar();

      ImGui.BeginChild("FurnishingList");

      if (Plugin.ObjectTable.LocalPlayer == null)
        return;

      var playerPos = Plugin.ObjectTable.LocalPlayer.Position;

      if (!ImGui.BeginTable("FurnishingListItems", 3))
        return;

      ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 0f);
      ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0f);
      ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.WidthFixed, 0f);
      ImGui.TableHeadersRow();

      try
      {
        if (!Memory.GetFurnishings(out var items, playerPos, sortByDistance))
          return;

        for (var i = 0; i < items.Count; i++)
        {
          var item = items[i];

          ImGui.TableNextRow(ImGuiTableRowFlags.None, 28 * fontScale);
          ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4f, 4f));
          ImGui.TableNextColumn();
          ImGui.AlignTextToFramePadding();

          var (name, icon) = GetHousingItem(items[i]);
          
          // Skip item if we can't find a name or item icon.
          if (name == string.Empty || icon == 0)
            continue;
          
          // The currently selected item.
          var thisActive = hasActiveItem && item.SharedGroupLayoutInstance == Memory.HousingStructure->ActiveItem;
          
          ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2f, 4f));
          if (ImGui.Selectable($"##Item{i}", thisActive, ImGuiSelectableFlags.SpanAllColumns, new(0, 20 * fontScale)))
            Memory.SelectItem((IntPtr)Memory.HousingStructure, (IntPtr)item.SharedGroupLayoutInstance);
          ImGui.PopStyleVar();
          
          if (thisActive)
            ImGui.SetItemDefaultFocus();
          
          // Scroll if the active item has changed from last time.
          if (thisActive && _lastActiveItem != (ulong)Memory.HousingStructure->ActiveItem)
            ImGui.SetScrollHereY();

          var distance = Util.DistanceFromPlayer(item, playerPos);
          
          ImGui.SameLine();
          Plugin.DrawIcon(icon, new Vector2(24 * fontScale, 24 * fontScale));
          
          ImGui.TableNextColumn();
          ImGui.Text(name);

          ImGui.TableNextColumn();
          DrawDistance(distance);

          ImGui.PopStyleVar();
        }
          
        if (_renderCount >= 10)
          _lastActiveItem = (ulong)Memory.HousingStructure->ActiveItem;
        if (_renderCount != 10)
          _renderCount++;
      }
      catch (Exception ex)
      {
        Plugin.Log.Error(ex, ex.Source ?? "No source found");
      }
      finally
      {
        ImGui.EndTable();
        ImGui.EndChild();
      }
    }
  }
}
