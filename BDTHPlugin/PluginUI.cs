using Dalamud.Interface.Components;
using Dalamud.Logging;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Numerics;

namespace BDTHPlugin
{
  public class PluginUI
  {
    private PluginMemory memory => Plugin.Memory;
    private Configuration configuration => Plugin.Configuration;

    private static float[] identityMatrix =
    {
      1.0f, 0.0f, 0.0f, 0.0f,
      0.0f, 1.0f, 0.0f, 0.0f,
      0.0f, 0.0f, 1.0f, 0.0f,
      0.0f, 0.0f, 0.0f, 1.0f
    };

    private readonly float[] itemMatrix =
    {
      1.0f, 0.0f, 0.0f, 0.0f,
      0.0f, 1.0f, 0.0f, 0.0f,
      0.0f, 0.0f, 1.0f, 0.0f,
      0.0f, 0.0f, 0.0f, 1.0f
    };

    private readonly OPERATION gizmoOperation = OPERATION.TRANSLATE;
    private MODE gizmoMode = MODE.LOCAL;

    // Components for the active item.
    private Vector3 translate = new();
    private Vector3 rotation = new();
    private Vector3 scale = new();

    private float? lockX = null;
    private float? lockY = null;
    private float? lockZ = null;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool visible = false;
    public bool Visible
    {
      get => visible;
      set => visible = value;
    }

    private bool listVisible = false;
    public bool ListVisible
    {
      get => listVisible;
      set => listVisible = value;
    }

    public bool debugVisible = false;

    public bool resetWindow = false;

    private float drag;
    private bool useGizmo;
    private bool doSnap;
    private bool placeAnywhere;

    private bool dummyHousingGoods;
    private bool dummyInventory;
    private bool autoVisible;

    private readonly Vector4 ORANGE_COLOR = new(0.871f, 0.518f, 0f, 1f);
    private readonly Vector4 RED_COLOR = new Vector4(1, 0, 0, 1);

    public PluginUI()
    {
      placeAnywhere = configuration.PlaceAnywhere;
      drag = configuration.Drag;
      useGizmo = configuration.UseGizmo;
      doSnap = configuration.DoSnap;
      autoVisible = configuration.AutoVisible;
    }

    public void Draw()
    {
      try
      {
        DrawGizmo();
        DrawMainWindow();
        DrawHousingList();
        DrawDebug();
      }
      catch (Exception ex)
      {
        PluginLog.LogError(ex, "Error drawing UI");
      }
    }

    private void DrawTooltip(string[] text)
    {
      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();
        foreach (var t in text)
          ImGui.Text(t);
        ImGui.EndTooltip();
      }
    }
    private void DrawTooltip(string text)
    {
      DrawTooltip(new[] { text });
    }

    private void DrawError(string text)
    {
      ImGui.PushStyleColor(ImGuiCol.Text, RED_COLOR);
      ImGui.Text(text);
      ImGui.PopStyleColor();
    }

    public void DrawMainWindow()
    {
      if (!Visible)
        return;

      ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
      ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

      try
      {
        DrawWindowContents();
      }
      catch
      {
      }

      ImGui.PopStyleColor(2);
    }

    private unsafe void DrawWindowContents()
    {
      var invalid = memory.HousingStructure->ActiveItem == null
        || PluginMemory.GamepadMode
        || memory.HousingStructure->Mode != HousingLayoutMode.Rotate;
      var fontScale = ImGui.GetIO().FontGlobalScale;
      var size = new Vector2(-1, -1);

      ImGui.SetNextWindowSize(size, ImGuiCond.Always);
      ImGui.SetNextWindowSizeConstraints(size, size);

      if (resetWindow)
      {
        ImGui.SetNextWindowPos(new Vector2(69, 69), ImGuiCond.Always);
        resetWindow = false;
      }

      if (ImGui.Begin($"Burning Down the House##BDTH", ref visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
      {
        ImGui.BeginGroup();

        if (ImGui.Checkbox("Place Anywhere", ref placeAnywhere))
        {
          // Set the place anywhere based on the checkbox state.
          memory.SetPlaceAnywhere(placeAnywhere);
          configuration.PlaceAnywhere = placeAnywhere;
          configuration.Save();
        }
        DrawTooltip("Allows the placement of objects without limitation from the game engine.");

        ImGui.SameLine();

        // Checkbox is clicked, set the configuration and save.
        if (ImGui.Checkbox("Gizmo", ref useGizmo))
        {
          configuration.UseGizmo = useGizmo;
          configuration.Save();
        }
        DrawTooltip("Displays a movement gizmo on the selected item to allow for in-game movement on all axis.");

        ImGui.SameLine();

        // Checkbox is clicked, set the configuration and save.
        if (ImGui.Checkbox("Snap", ref doSnap))
        {
          configuration.DoSnap = doSnap;
          configuration.Save();
        }
        DrawTooltip("Enables snapping of gizmo movement based on the drag value set below.");

        ImGui.SameLine();
        if (ImGuiComponents.IconButton(1, gizmoMode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
          gizmoMode = gizmoMode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;
        DrawTooltip(new[]
        {
          $"Mode: {(gizmoMode == MODE.LOCAL ? "Local" : "World")}",
          "Changes gizmo mode between local and world movement."
        });

        ImGui.Separator();

        if (memory.HousingStructure->Mode == HousingLayoutMode.None)
          DrawError("Enter housing mode to get started");
        else if (PluginMemory.GamepadMode)
          DrawError("Does not support Gamepad");
        else if (memory.HousingStructure->ActiveItem == null || memory.HousingStructure->Mode != HousingLayoutMode.Rotate)
        {
          DrawError("Select a housing item in Rotate mode");
          ImGuiComponents.HelpMarker("Are you doing everything right? Try using the /bdth debug command and report this issue in Discord!");
        }
        else
          DrawItemControls();

        ImGui.Separator();

        // Drag ammount for the inputs.
        if (ImGui.InputFloat("drag", ref drag, 0.05f))
        {
          drag = Math.Min(Math.Max(0.001f, drag), 10f);
          configuration.Drag = drag;
          configuration.Save();
        }
        DrawTooltip("Sets the amount to change when dragging the controls, also influences the gizmo snap feature.");

        dummyHousingGoods = PluginMemory.HousingGoods != null && PluginMemory.HousingGoods->IsVisible;
        dummyInventory = memory.InventoryVisible;

        if (ImGui.Checkbox("Display in-game list", ref dummyHousingGoods))
          if (PluginMemory.HousingGoods != null) PluginMemory.HousingGoods->IsVisible = dummyHousingGoods;
        ImGui.SameLine();
        if (ImGui.Checkbox("Display inventory", ref dummyInventory))
          memory.InventoryVisible = dummyInventory;

        if (ImGui.Button("Open Furnishing List"))
          Plugin.CommandManager.ProcessCommand("/bdth list");
        DrawTooltip(new[]
        {
          "Opens a furnishing list that you can use to sort by distance and click to select objects.",
          "NOTE: Does not currently work outdoors!"
        });

        if (ImGui.Checkbox("Auto Open", ref autoVisible))
        {
          configuration.AutoVisible = autoVisible;
          configuration.Save();
        }
      }
      ImGui.End();
    }

    private unsafe void HandleScrollInput(ref float f)
    {
      if (ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax()))
      {
        var delta = ImGui.GetIO().MouseWheel * drag;
        if (delta != 0)
        {
          f += delta;
          memory.WritePosition(memory.position);
        }
      }
    }
    
    private unsafe bool DrawDrag(string name, ref float f)
    {
      var changed = ImGui.DragFloat(name, ref f, drag);
      ImGui.SameLine(0, 4);
      HandleScrollInput(ref f);
      return changed;
    }

    private unsafe void DrawDragCoord(string name, ref float f)
    {
      if (DrawDrag(name, ref f))
        memory.WritePosition(memory.position);
    }
    
    private unsafe void DrawDragRotate(string name, ref float f)
    {
      if (DrawDrag(name, ref f))
        memory.WriteRotation(memory.rotation);
    }
    
    private unsafe bool DrawInput(string name, ref float f)
    {
      var changed = ImGui.InputFloat(name, ref f, drag);
      HandleScrollInput(ref f);
      ImGui.SameLine();
      return changed;
    }
    
    private unsafe void DrawInputCoord(string name, ref float f)
    {
      if (DrawInput(name, ref f))
        memory.WritePosition(memory.position);
    }

    private unsafe void DrawInputRotate(string name, ref float f)
    {
      if (DrawInput(name, ref f))
        memory.WriteRotation(memory.rotation);
    }

    private unsafe void DrawInputCoord(string name, ref float f, ref float? locked)
    {
      DrawInputCoord(name, ref f);
      if (ImGuiComponents.IconButton((int)ImGui.GetID(name), locked == null ? Dalamud.Interface.FontAwesomeIcon.Unlock : Dalamud.Interface.FontAwesomeIcon.Lock))
        locked = locked == null ? f : null;
    }

    private unsafe void DrawItemControls()
    {
      if (memory.HousingStructure->ActiveItem != null)
      {
        memory.position = memory.ReadPosition();
        // Handle lock logic.
        if (lockX != null)
          memory.position.X = (float)lockX;
        if (lockY != null)
          memory.position.Y = (float)lockY;
        if (lockZ != null)
          memory.position.Z = (float)lockZ;
        memory.WritePosition(memory.position);
      }

      ImGui.BeginGroup();
      {
        ImGui.PushItemWidth(73f);
        {
          DrawDragCoord("##bdth-xdrag", ref memory.position.X);
          DrawDragCoord("##bdth-ydrag", ref memory.position.Y);
          DrawDragCoord("##bdth-zdrag", ref memory.position.Z);
          ImGui.Text("position");

          DrawDragRotate("##bdth-rydrag", ref memory.rotation.Y);
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

      DrawInputCoord("x coord##bdth-x", ref memory.position.X, ref lockX);
      DrawInputCoord("y coord##bdth-y", ref memory.position.Y, ref lockY);
      DrawInputCoord("z coord##bdth-z", ref memory.position.Z, ref lockZ);
      DrawInputRotate("ry degree##bdth-ry", ref memory.rotation.Y);
    }

    public unsafe void DrawGizmo()
    {
      if (!useGizmo)
        return;

      // Disabled if the housing mode isn't on and there isn't a selected item.
      var disabled = !(memory.CanEditItem() && memory.HousingStructure->ActiveItem != null);
      if (disabled)
        return;

      // Just catch errors since the disabled logic above didn't catch it one time.
      try
      {
        translate = memory.ReadPosition();
        rotation = memory.ReadRotation();
        ImGuizmo.RecomposeMatrixFromComponents(ref translate.X, ref rotation.X, ref scale.X, ref itemMatrix[0]);
      }
      catch
      {
      }

      var matrixSingleton = memory.GetMatrixSingleton();
      if (matrixSingleton == IntPtr.Zero)
        return;

      var viewProjectionMatrix = new float[16];

      var rawMatrix = (float*)(matrixSingleton + 0x1B4).ToPointer();
      for (var i = 0; i < 16; i++, rawMatrix++)
        viewProjectionMatrix[i] = *rawMatrix;

      // Gizmo setup.
      ImGuizmo.Enable(!memory.HousingStructure->Rotating);
      ImGuizmo.SetID("BDTHPlugin".GetHashCode());

      ImGuizmo.SetOrthographic(false);

      var vp = ImGui.GetMainViewport();
      ImGui.SetNextWindowSize(vp.Size);
      ImGui.SetNextWindowPos(vp.Pos, ImGuiCond.Always);
      ImGui.SetNextWindowViewport(vp.ID);

      ImGui.Begin("BDTHGizmo", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs);
      ImGui.BeginChild("##BDTHGizmoChild", new Vector2(-1, -1), false, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoInputs);

      ImGuizmo.SetDrawlist();

      ImGuizmo.SetRect(vp.Pos.X, vp.Pos.Y, vp.Size.X, vp.Size.Y);

      var snap = doSnap ? new Vector3(drag, drag, drag) : Vector3.Zero;

      // ImGuizmo.Manipulate(ref viewProjectionMatrix[0], ref identityMatrix[0], gizmoOperation, gizmoMode, ref itemMatrix[0]);
      Manipulate(ref viewProjectionMatrix[0], ref identityMatrix[0], gizmoOperation, gizmoMode, ref itemMatrix[0], ref snap.X);

      ImGuizmo.DecomposeMatrixToComponents(ref itemMatrix[0], ref translate.X, ref rotation.X, ref scale.X);

      memory.WritePosition(translate);

      ImGui.EndChild();
      ImGui.End();
      ImGuizmo.SetID(-1);
    }

    private unsafe void DrawDebug()
    {
      if (!debugVisible)
        return;

      if (ImGui.Begin("BDTH Debug", ref debugVisible))
      {
        ImGui.Text($"Gamepad Mode: {PluginMemory.GamepadMode}");
        ImGui.Text($"CanEditItem: {memory.CanEditItem()}");
        ImGui.Text($"IsHousingOpen: {memory.IsHousingOpen()}");
        ImGui.Separator();
        ImGui.Text($"LayoutWorld: {(ulong)memory.Layout:X}");
        ImGui.Text($"Housing Structure: {(ulong)memory.HousingStructure:X}");
        ImGui.Text($"Mode: {memory.HousingStructure->Mode}");
        ImGui.Text($"State: {memory.HousingStructure->State}");
        ImGui.Text($"State2: {memory.HousingStructure->State2}");
        ImGui.Text($"Active: {(ulong)memory.HousingStructure->ActiveItem:X}");
        ImGui.Text($"Hover: {(ulong)memory.HousingStructure->HoverItem:X}");
        ImGui.Text($"Rotating: {memory.HousingStructure->Rotating}");
        ImGui.Separator();
        ImGui.Text($"Housing Module: {(ulong)memory.HousingModule:X}");
        ImGui.Text($"Housing Module: {(ulong)memory.HousingModule->CurrentTerritory:X}");
        ImGui.Text($"Outdoor Territory: {(ulong)memory.HousingModule->OutdoorTerritory:X}");
        ImGui.Text($"Indoor Territory: {(ulong)memory.HousingModule->IndoorTerritory:X}");
        var active = memory.HousingStructure->ActiveItem;
        if (active != null)
        {
          ImGui.Separator();
          var pos = memory.HousingStructure->ActiveItem->Position;
          ImGui.Text($"Position: {pos.X}, {pos.Y}, {pos.Z}");
        }
      }
      ImGui.End();
    }

    private int FurnishingIndex => memory.GetHousingObjectSelectedIndex();
    private bool sortByDistance = false;
    private ulong? lastActiveItem = null;
    private byte renderCount = 0;

    public unsafe void DrawHousingList()
    {
      if (!listVisible)
        return;

      // Only allow furnishing list when the housing window is open.
      if (!memory.IsHousingOpen())
      {
        listVisible = false;
        return;
      }

      // Disallow the ability to open furnishing list outdoors.
      if (Plugin.IsOutdoors())
      {
        listVisible = false;
        return;
      }

      ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
      ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

      var fontScale = ImGui.GetIO().FontGlobalScale;
      var size = new Vector2(240 * fontScale, 350 * fontScale);

      ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
      ImGui.SetNextWindowSizeConstraints(new Vector2(120 * fontScale, 100 * fontScale), new Vector2(400 * fontScale, 1000 * fontScale));

      if (ImGui.Begin($"Furnishing List", ref listVisible))
      {
        if (ImGui.Checkbox("Sort by distance", ref sortByDistance))
        {
          configuration.SortByDistance = sortByDistance;
          configuration.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
        ImGui.Separator();
        ImGui.PopStyleVar();

        ImGui.BeginChild("FurnishingList");

        if (Plugin.ClientState.LocalPlayer == null)
          return;

        var playerPos = Plugin.ClientState.LocalPlayer.Position;
        // An active item is being selected.
        var hasActiveItem = memory.HousingStructure->ActiveItem != null;

        if (ImGui.BeginTable("FurnishingListItems", 3))
        {
          ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 0f);
          ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0f);
          ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.WidthFixed, 0f);

          try
          {
            if (memory.GetFurnishings(out var items, playerPos, sortByDistance))
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
                var thisActive = hasActiveItem && items[i].Item == memory.HousingStructure->ActiveItem;

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
                if (ImGui.Selectable($"##Item{i}", thisActive, ImGuiSelectableFlags.SpanAllColumns, new(0, 20 * fontScale)))
                  memory.SelectItem((IntPtr)memory.HousingStructure, (IntPtr)items[i].Item);
                ImGui.PopStyleVar();

                if (thisActive)
                  ImGui.SetItemDefaultFocus();

                // Scroll if the active item has changed from last time.
                if (thisActive && lastActiveItem != (ulong)memory.HousingStructure->ActiveItem)
                {
                  ImGui.SetScrollHereY();
                  PluginLog.Log($"{ImGui.GetScrollY()} {ImGui.GetScrollMaxY()}");
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
                lastActiveItem = (ulong)memory.HousingStructure->ActiveItem;
            }
          }
          catch (Exception ex)
          {
            PluginLog.LogError(ex, ex.Source);
          }
          finally
          {
            ImGui.EndTable();
            ImGui.EndChild();
          }
        }
      }

      if (renderCount != 10)
        renderCount++;

      ImGui.End();
      ImGui.PopStyleColor(2);
    }

    // Bypass the delta matrix to just only use snap.
    private static bool Manipulate(ref float view, ref float projection, OPERATION operation, MODE mode, ref float matrix, ref float snap)
    {
      unsafe
      {
        float* localBounds = null;
        float* boundsSnap = null;
        fixed (float* native_view = &view)
        {
          fixed (float* native_projection = &projection)
          {
            fixed (float* native_matrix = &matrix)
            {
              fixed (float* native_snap = &snap)
              {
                byte ret = ImGuizmoNative.ImGuizmo_Manipulate(native_view, native_projection, operation, mode, native_matrix, null, native_snap, localBounds, boundsSnap);
                return ret != 0;
              }
            }
          }
        }
      }
    }
  }
}
