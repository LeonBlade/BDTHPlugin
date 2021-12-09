using Dalamud.Interface.Components;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Numerics;
using Dalamud.Logging;

namespace BDTHPlugin
{
	// It is good to have this be disposable in general, in case you ever need it
	// to do any cleanup
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

		private float drag;
		private bool useGizmo;
		private bool doSnap;

		private bool dummyHousingGoods;
		private bool dummyInventory;
		private bool autoVisible;

		private bool placeAnywhere = false;
		private readonly Vector4 ORANGE_COLOR = new(0.871f, 0.518f, 0f, 1f);

		public PluginUI()
		{
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

		public unsafe void DrawMainWindow()
		{
			if (!Visible)
			{
				return;
			}

			ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
			ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

			var invalid = memory.HousingStructure->ActiveItem == null 
				|| PluginMemory.GamepadMode
				|| memory.HousingStructure->Mode != HousingLayoutMode.Rotate;
			var fontScale = ImGui.GetIO().FontGlobalScale;
			var size = new Vector2(320 * fontScale, (!invalid ? 312 : 170) * fontScale);

			ImGui.SetNextWindowSize(size, ImGuiCond.Always);
			ImGui.SetNextWindowSizeConstraints(size, size);

			if (ImGui.Begin($"Burning Down the House", ref visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
			{
				ImGui.BeginGroup();

				if (ImGui.Checkbox("Place Anywhere", ref placeAnywhere))
				{
					// Set the place anywhere based on the checkbox state.
					memory.SetPlaceAnywhere(placeAnywhere);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Allows the placement of objects without limitation from the game engine.");
					ImGui.EndTooltip();
				}

				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Gizmo", ref useGizmo))
				{
					configuration.UseGizmo = useGizmo;
					configuration.Save();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Displays a movement gizmo on the selected item to allow for in-game movement on all axis.");
					ImGui.EndTooltip();
				}
						
				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Snap", ref doSnap))
				{
					configuration.DoSnap = doSnap;
					configuration.Save();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Enables snapping of gizmo movement based on the drag value set below.");
					ImGui.EndTooltip();
				}

				ImGui.SameLine();
				if (ImGuiComponents.IconButton(1, gizmoMode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
					gizmoMode = gizmoMode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text($"Mode: {(gizmoMode == MODE.LOCAL ? "Local" : "World")}");
					ImGui.Text("Changes gizmo mode between local and world movement.");
					ImGui.EndTooltip();
				}

				ImGui.Separator();

				if (memory.HousingStructure->Mode == HousingLayoutMode.None)
					ImGui.Text("Enter housing mode to get started");
				else if (PluginMemory.GamepadMode)
				{
					ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
					ImGui.Text("Does not support Gamepad");
					ImGui.PopStyleColor();
				}
				else if (memory.HousingStructure->ActiveItem == null || memory.HousingStructure->Mode != HousingLayoutMode.Rotate)
				{
					ImGui.Text("Select a housing item in Rotate mode");
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
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Sets the amount to change when dragging the controls, also influences the gizmo snap feature.");
					ImGui.EndTooltip();
				}

				dummyHousingGoods = PluginMemory.HousingGoods != null && PluginMemory.HousingGoods->IsVisible;
				dummyInventory = memory.InventoryVisible;
 
				if (ImGui.Checkbox("Display in-game list", ref dummyHousingGoods))
					if (PluginMemory.HousingGoods != null) PluginMemory.HousingGoods->IsVisible = dummyHousingGoods;
				ImGui.SameLine();
				if (ImGui.Checkbox("Display inventory", ref dummyInventory))
					memory.InventoryVisible = dummyInventory;

				/*if (ImGui.Button("Open Furnishing List"))
					Plugin.CommandManager.ProcessCommand("/bdth list");
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Opens a furnishing list that you can use to sort by distance and click to select objects.");
					ImGui.Text("NOTE: Does not currently work outdoors!");
					ImGui.EndTooltip();
				}
				*/
				if(ImGui.Checkbox("Auto Open", ref autoVisible))
                {
					configuration.AutoVisible = autoVisible;
					configuration.Save();
               	}

				// ImGui.SameLine();
				// if (ImGui.Button("Place Item"))
				//	 if (memory.CanEditItem() && memory.HousingStructure->ActiveItem != null) memory.PlaceHousingItem((IntPtr)memory.HousingStructure->ActiveItem, memory.position);
			}
			ImGui.End();

			ImGui.PopStyleColor(2);
		}

		private unsafe void DrawItemControls()
		{
			var io = ImGui.GetIO();
			ImGuizmo.SetRect(0, 0, io.DisplaySize.X, io.DisplaySize.Y);

			ImGui.BeginGroup();

			ImGui.PushItemWidth(73f);

			if (ImGui.DragFloat("##xdrag", ref memory.position.X, drag))
				memory.WritePosition(memory.position);
			ImGui.SameLine(0, 4);
			var xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			if (ImGui.DragFloat("##ydrag", ref memory.position.Y, drag))
				memory.WritePosition(memory.position);
			ImGui.SameLine(0, 4);
			var yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			if (ImGui.DragFloat("##zdrag", ref memory.position.Z, drag))
				memory.WritePosition(memory.position);
			ImGui.SameLine(0, 4);
			var zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			ImGui.Text("position");

			if (ImGui.DragFloat("##rydrag", ref memory.rotation.Y, drag))
				memory.WriteRotation(memory.rotation);
			ImGui.SameLine(0, 4);
			var ryHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			ImGui.Text("rotation");

			ImGui.PopItemWidth();

			// Mouse wheel direction.
			var delta = ImGui.GetIO().MouseWheel * drag;

			// Move position based on which control is being hovered.
			if (xHover)
				memory.position.X += delta;
			if (yHover)
				memory.position.Y += delta;
			if (zHover)
				memory.position.Z += delta;
			if (xHover || yHover || zHover)
				memory.WritePosition(memory.position);

			// Move rotation based on which control is being hovered.
			if (ryHover)
				memory.rotation.Y += delta;
			if (ryHover && delta > 0)
				memory.WriteRotation(memory.rotation);

			ImGui.EndGroup(); // End group for the drag section.

			if (ImGui.IsItemHovered())
			{
				ImGui.BeginTooltip();
				ImGui.Text("Click and drag each to move the selected item.");
				ImGui.Text("Change the drag option below to influence how much it moves as you drag.");
				ImGui.EndTooltip();
			}

			if (ImGui.InputFloat("x coord", ref memory.position.X, drag))
				memory.WritePosition(memory.position);
			xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			if (ImGui.InputFloat("y coord", ref memory.position.Y, drag))
				memory.WritePosition(memory.position);
			yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			if (ImGui.InputFloat("z coord", ref memory.position.Z, drag))
				memory.WritePosition(memory.position);
			zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			if (ImGui.InputFloat("ry degree", ref memory.rotation.Y, drag))
				memory.WriteRotation(memory.rotation);
			ryHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

			// Mouse wheel direction.
			delta = ImGui.GetIO().MouseWheel * drag;

			// Move position based on which control is being hovered.
			if (xHover)
				memory.position.X += delta;
			if (yHover)
				memory.position.Y += delta;
			if (zHover)
				memory.position.Z += delta;
			if (xHover || yHover || zHover)
				memory.WritePosition(memory.position);

			// Move rotation based on which control is being hovered.
			if (ryHover)
				memory.rotation.Y += delta;
			if (ryHover && delta > 0)
				memory.WriteRotation(memory.rotation);
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
			ImGuizmo.BeginFrame();

			ImGuizmo.SetOrthographic(false);

			var vp = ImGui.GetMainViewport();
			ImGui.SetNextWindowSize(vp.Size);
			ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
			ImGui.SetNextWindowViewport(vp.ID);

			ImGui.Begin("Gizmo", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs);
			ImGui.BeginChild("##gizmoChild", new Vector2(-1, -1), false, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoInputs);

			ImGuizmo.SetDrawlist();

			var windowWidth = ImGui.GetWindowWidth();
			var windowHeight = ImGui.GetWindowHeight();

			ImGuizmo.SetRect(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y, windowWidth, windowHeight);

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

				var playerPos = Plugin.ClientState.LocalPlayer?.Position;

				if (playerPos.HasValue)
				{
					try
					{
						if (memory.GetFurnishings(out var items, playerPos.Value, sortByDistance))
						{
							for (var i = 0; i < items.Count; i++)
							{
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

								// An active item is being selected.
								var hasActiveItem = memory.HousingStructure->ActiveItem != null;
								// The currently selected item.
								var thisActive = hasActiveItem && items[i].Item == memory.HousingStructure->ActiveItem;

								ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
								if (ImGui.Selectable($"##Item{i}", hasActiveItem && thisActive))
									memory.SelectItem((IntPtr)memory.HousingStructure, (IntPtr)items[i].Item);

								if (thisActive)
									ImGui.SetItemDefaultFocus();

								// Scroll if the active item has changed from last time.
								if (thisActive && lastActiveItem != (ulong)memory.HousingStructure->ActiveItem)
								{
									ImGui.SetScrollHereY();
									PluginLog.Log($"{ImGui.GetScrollY()} {ImGui.GetScrollMaxY()}");
								}

								ImGui.SameLine(); Plugin.DrawIcon(icon, new Vector2(20, 20));
								ImGui.PopStyleVar();
								// var distance = Util.DistanceFromPlayer(items[i], playerPos);

								ImGui.SameLine(); ImGui.Text(name);
								// ImGui.SameLine(); ImGui.Text($"{distance:F2}");
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
