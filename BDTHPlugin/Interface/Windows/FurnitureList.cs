using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using Dalamud.Bindings.ImGui;

namespace BDTHPlugin.Interface.Windows
{
  public class FurnitureList : Window
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private ulong? lastActiveItem;
    private byte renderCount;

    public FurnitureList() : base("Furnishing List")
    {

    }

    public override void PreDraw()
    {
      // Only allows furnishing list when the housing window is open.
      // Disallows the ability to open furnishing list outdoors.
      IsOpen &= Memory.IsHousingOpen() && !Plugin.IsOutdoors();
    }

    public unsafe override void Draw()
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

      if (Plugin.ClientState.LocalPlayer == null)
        return;

      var playerPos = Plugin.ClientState.LocalPlayer.Position;
      // An active item is being selected.
      // var hasActiveItem = Memory.HousingStructure->ActiveItem != null;

      if (ImGui.BeginTable("FurnishingListItems", 3))
      {
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 0f);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0f);
        ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.WidthFixed, 0f);

        try
        {
          if (Memory.GetFurnishings(out var items, playerPos, sortByDistance))
          {
            for (var i = 0; i < items.Count; i++)
            {
              ImGui.TableNextRow(ImGuiTableRowFlags.None, 28 * fontScale);
              ImGui.TableNextColumn();
              ImGui.AlignTextToFramePadding();

              var name = "";
              ushort icon = 0;

              if (Plugin.TryGetYardObject(items[i].HousingRowId, out var yardObject))
              {
                name = yardObject.Item.Value.Name.ToString();
                icon = yardObject.Item.Value.Icon;
              }

              if (Plugin.TryGetFurnishing(items[i].HousingRowId, out var furnitureObject))
              {
                name = furnitureObject.Item.Value.Name.ToString();
                icon = furnitureObject.Item.Value.Icon;
              }

              // Skip item if we can't find a name or item icon.
              if (name == string.Empty || icon == 0)
                continue;

              // The currently selected item.
              var thisActive = hasActiveItem && items[i].Item == Memory.HousingStructure->ActiveItem;

              ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
              if (ImGui.Selectable($"##Item{i}", thisActive, ImGuiSelectableFlags.SpanAllColumns, new(0, 20 * fontScale)))
                Memory.SelectItem((IntPtr)Memory.HousingStructure, (IntPtr)items[i].Item);
              ImGui.PopStyleVar();

              if (thisActive)
                ImGui.SetItemDefaultFocus();

              // Scroll if the active item has changed from last time.
              if (thisActive && lastActiveItem != (ulong)Memory.HousingStructure->ActiveItem)
              {
                ImGui.SetScrollHereY();
              }

              ImGui.SameLine();
              Plugin.DrawIcon(icon, new Vector2(24 * fontScale, 24 * fontScale));
              var distance = Util.DistanceFromPlayer(items[i], playerPos);

              ImGui.TableNextColumn();
              ImGui.SetNextItemWidth(-1);
              ImGui.Text(name);

              ImGui.TableNextColumn();
              ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
              ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - ImGui.CalcTextSize(distance.ToString("F2")).X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
              ImGui.Text($"{distance:F2}");
              ImGui.PopStyleColor();
            }

            if (renderCount >= 10)
              lastActiveItem = (ulong)Memory.HousingStructure->ActiveItem;
            if (renderCount != 10)
              renderCount++;
          }
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
}
