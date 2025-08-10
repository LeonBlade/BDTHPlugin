using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Components;

using Dalamud.Bindings.ImGui;

namespace BDTHPlugin.Interface.Components
{
  public class ItemControls
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private static float Drag => Configuration.Drag;

    private float? lockX;
    private float? lockY;
    private float? lockZ;

    private Vector3? copyPosition;
    private float? copyRotation;

    private Vector4 RED = new(1, .2f, .2f, 1);
    private Vector4 GREEN = new(.2f, 1, .2f, 1);
    private Vector4 BLUE = new(.2f, .2f, 1, 1);

    public unsafe void Draw()
    {
      if (Memory.HousingStructure->ActiveItem != null)
      {
        Memory.position = Memory.ReadPosition();
        // Handle lock logic.
        if (lockX != null)
          Memory.position.X = (float)lockX;
        if (lockY != null)
          Memory.position.Y = (float)lockY;
        if (lockZ != null)
          Memory.position.Z = (float)lockZ;
        Memory.WritePosition(Memory.position);
      }

      ImGui.BeginGroup();
      {
        ImGui.PushItemWidth(73f);
        {
          DrawDragCoord("##bdth-xdrag", ref Memory.position.X);
          DrawDragCoord("##bdth-ydrag", ref Memory.position.Y);
          DrawDragCoord("##bdth-zdrag", ref Memory.position.Z);
          ImGui.Text("position");

          DrawDragRotate("##bdth-rydrag", ref Memory.rotation.Y);
          ImGui.Text("rotation");
        }
        ImGui.PopItemWidth();
      }
      ImGui.EndGroup();

      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();
        ImGui.Text("Click and drag each to move the selected item.");
        ImGui.Text("Change the drag option below to influence how much it moves as you drag.");
        ImGui.EndTooltip();
      }

      ImGui.SameLine();
      ImGui.BeginGroup();
      {
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
        {
          copyPosition = Memory.position;
          copyRotation = Memory.rotation.Y;
        }
        if (ImGui.IsItemHovered())
          ImGui.SetTooltip("Copy Position & Rotation");

        ImGui.BeginDisabled(copyPosition == null || copyRotation == null);
        {
          if (ImGuiComponents.IconButton(FontAwesomeIcon.Paste) && copyPosition != null && copyRotation != null)
          {
            Memory.WritePosition(copyPosition.Value);
            Memory.WriteRotation(Memory.rotation with { Y = copyRotation.Value });
          }
          if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Paste Position & Rotation");
        }
        ImGui.EndDisabled();
      }

      ImGui.EndGroup();

      DrawInputCoord("x coord##bdth-x", ref Memory.position.X, ref lockX, RED);
      DrawInputCoord("y coord##bdth-y", ref Memory.position.Y, ref lockY, GREEN);
      DrawInputCoord("z coord##bdth-z", ref Memory.position.Z, ref lockZ, BLUE);
      DrawInputRotate("ry degree##bdth-ry", ref Memory.rotation.Y);
    }

    private void HandleScrollInput(ref float f)
    {
      if (ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax()))
      {
        var delta = ImGui.GetIO().MouseWheel * Drag;
        if (delta != 0)
        {
          f += delta;
          Memory.WritePosition(Memory.position);
        }
      }
    }

    private bool DrawDrag(string name, ref float f)
    {
      var changed = ImGui.DragFloat(name, ref f, Drag);
      ImGui.SameLine(0, 4);
      HandleScrollInput(ref f);
      return changed;
    }

    private void DrawDragCoord(string name, ref float f)
    {
      if (DrawDrag(name, ref f))
        Memory.WritePosition(Memory.position);
    }

    private void DrawDragRotate(string name, ref float f)
    {
      if (DrawDrag(name, ref f))
        Memory.WriteRotation(Memory.rotation);
    }

    private bool DrawInput(string name, ref float f)
    {
      var changed = ImGui.InputFloat(name, ref f, Drag);
      HandleScrollInput(ref f);
      ImGui.SameLine();
      return changed;
    }

    private void DrawInputCoord(string name, ref float f)
    {
      if (DrawInput(name, ref f))
        Memory.WritePosition(Memory.position);
    }

    private void DrawInputRotate(string name, ref float f)
    {
      if (DrawInput(name, ref f))
        Memory.WriteRotation(Memory.rotation);
    }

    static void DrawCircle(Vector4 color)
    {
      ImGui.SameLine();
      var pos = ImGui.GetCursorScreenPos();
      var center = pos.Y + ImGui.GetTextLineHeight() / 2f + 3f;
      ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(pos.X + 12, center), 8, ImGui.GetColorU32(color));
      ImGui.Dummy(new Vector2(24, 24));
    }

    private void DrawInputCoord(string name, ref float f, ref float? locked, Vector4 color)
    {
      DrawInputCoord(name, ref f);
      if (ImGuiComponents.IconButton((int)ImGui.GetID(name), locked == null ? FontAwesomeIcon.Unlock : FontAwesomeIcon.Lock))
        locked = locked == null ? f : null;
      DrawCircle(color);
    }
  }
}
