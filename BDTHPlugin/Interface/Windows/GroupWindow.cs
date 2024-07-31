using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuizmoNET;

namespace BDTHPlugin.Interface.Windows;

public class GroupWindow : Window
{
  private static PluginMemory Memory => Plugin.GetMemory();

  private readonly List<IntPtr> items = [];

  public unsafe GroupWindow() : base("Furniture Group")
  {
    Plugin.Gizmo.GizmoChanged += GizmoChanged;
  }

  private void GizmoChanged(object? sender, GizmoChangeArgs e)
  {
    Plugin.Log.Info($"position: {e.position}");
    if (IsOpen && Memory.IsHousingOpen() && !Memory.IsItemSelected())
      MoveItems(e.position);
  }

  private unsafe void AddItem()
  {
    items.Add((IntPtr)Memory.HousingStructure->ActiveItem);
  }

  private unsafe void MoveItems(Vector3 delta)
  {
    if (!Memory.CanEditItem())
      return;

    try
    {
      foreach (var item in items)
      {
        var pos = ((HousingItem*)item)->Position;
        pos += delta;
        ((HousingItem*)item)->Position = pos;
        Memory.HousingLayoutModelUpdate(item + 0x80);
      }
    }
    catch
    {
    }
  }

  public override void Draw()
  {
    ImGui.Text("Create new group");

    ImGui.Separator();

    if (ImGui.Button("Add item"))
      AddItem();
    if (ImGui.Button("Clear group"))
      items.Clear();

    ImGui.Separator();

    foreach (var item in items)
      ImGui.Text(item.ToString("X"));

    ImGui.Separator();

    // unsafe
    // {
    //   if (items.Count > 0)
    //   {
    //     var item = (HousingItem*)items[0];
    //     Plugin.Gizmo.SetTransform(item->Position, Util.FromQ(item->Rotation));
    //   }
    // }
  }
}