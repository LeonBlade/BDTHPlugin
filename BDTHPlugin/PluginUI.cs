using Dalamud.Interface.Components;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BDTHPlugin
{
	// It is good to have this be disposable in general, in case you ever need it
	// to do any cleanup
	class PluginUI : IDisposable
	{
		private readonly Plugin plugin;
		private readonly DalamudPluginInterface pi;
		private readonly Configuration configuration;
		private readonly PluginMemory memory;

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

		private static float[] testMatrix =
		{
			1.0f, 0.0f, 0.0f, 0.0f,
			0.0f, 1.0f, 0.0f, 0.0f,
			0.0f, 0.0f, 1.0f, 0.0f,
			0.0f, 0.0f, 0.0f, 1.0f
		};

		private readonly OPERATION gizmoOperation = OPERATION.TRANSLATE;
		private MODE gizmoMode = MODE.LOCAL;

		// Components for the active item.
		private Vector3 translate = new Vector3();
		private Vector3 rotation = new Vector3();
		private Vector3 scale = new Vector3();

		// this extra bool exists for ImGui, since you can't ref a property
		private bool visible = false;
		public bool Visible
		{
			get => this.visible;
			set => this.visible = value;
		}

		private bool listVisible = false;
		public bool ListVisible
		{
			get => this.listVisible;
			set => this.listVisible = value;
		}

		private float drag;
		private bool useGizmo;
		private bool doSnap;

		private bool placeAnywhere = false;
		private readonly Vector4 ORANGE_COLOR = new Vector4(0.871f, 0.518f, 0f, 1f);

		public PluginUI(Plugin p, DalamudPluginInterface pi, Configuration configuration, PluginMemory memory)
		{
			this.plugin = p;
			this.pi = pi;
			this.configuration = configuration;
			this.memory = memory;

			this.drag = this.configuration.Drag;
			this.useGizmo = this.configuration.UseGizmo;
			this.doSnap = this.configuration.DoSnap;
		}

		public void Dispose()
		{
		}

		public void Draw()
		{
			this.DrawGizmo();
			this.DrawMainWindow();
			this.DrawHousingList();
		}

		public void DrawMainWindow()
		{
			if (!this.Visible)
			{
				return;
			}

			ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
			ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

			var fontScale = ImGui.GetIO().FontGlobalScale;
			var size = new Vector2(320 * fontScale, 300 * fontScale);

			ImGui.SetNextWindowSize(size, ImGuiCond.Always);
			ImGui.SetNextWindowSizeConstraints(size, size);

			if (ImGui.Begin($"Burning Down the House", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
			{
				ImGui.BeginGroup();

				if (ImGui.Checkbox("Place Anywhere", ref this.placeAnywhere))
				{
					// Set the place anywhere based on the checkbox state.
					this.memory.SetPlaceAnywhere(this.placeAnywhere);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Allows the placement of objects without limitation from the game engine.");
					ImGui.EndTooltip();
				}

				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Gizmo", ref this.useGizmo))
				{
					this.configuration.UseGizmo = this.useGizmo;
					this.configuration.Save();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Displays a movement gizmo on the selected item to allow for in-game movement on all axis.");
					ImGui.EndTooltip();
				}
						
				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Snap", ref this.doSnap))
				{
					this.configuration.DoSnap = this.doSnap;
					this.configuration.Save();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Enables snapping of gizmo movement based on the drag value set below.");
					ImGui.EndTooltip();
				}

				ImGui.SameLine();
				if (ImGuiComponents.IconButton(1, this.gizmoMode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
					this.gizmoMode = this.gizmoMode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text($"Mode: {(this.gizmoMode == MODE.LOCAL ? "Local" : "World")}");
					ImGui.Text("Changes gizmo mode between local and world movement.");
					ImGui.EndTooltip();
				}

				// Disabled if the housing mode isn't on and there isn't a selected item.
				bool disabled = false;
				unsafe
				{
					disabled = !(this.memory.CanEditItem() && this.memory.HousingStructure->ActiveItem != null);
				}

				var io = ImGui.GetIO();
				ImGuizmo.SetRect(0, 0, io.DisplaySize.X, io.DisplaySize.Y);

				// Set the opacity based on if housing is on.
				if (disabled)
					ImGui.PushStyleVar(ImGuiStyleVar.Alpha, .3f);

				ImGui.BeginGroup();

				ImGui.PushItemWidth(73f);

				if (ImGui.DragFloat("##xdrag", ref this.memory.position.X, this.drag))
					this.memory.WritePosition(this.memory.position);
				ImGui.SameLine(0, 4);
				var xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				if (ImGui.DragFloat("##ydrag", ref this.memory.position.Y, this.drag))
					this.memory.WritePosition(this.memory.position);
				ImGui.SameLine(0, 4);
				var yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				if (ImGui.DragFloat("##zdrag", ref this.memory.position.Z, this.drag))
					this.memory.WritePosition(this.memory.position);
				ImGui.SameLine(0, 4);
				var zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				ImGui.Text("position");

				if (ImGui.DragFloat("##rydrag", ref this.memory.rotation.Y, this.drag))
					this.memory.WriteRotation(this.memory.rotation);
				ImGui.SameLine(0, 4);
				var ryHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				ImGui.Text("rotation");

				ImGui.PopItemWidth();

				// Mouse wheel direction.
				var delta = ImGui.GetIO().MouseWheel * this.drag;

				// Move position based on which control is being hovered.
				if (xHover)
					this.memory.position.X += delta;
				if (yHover)
					this.memory.position.Y += delta;
				if (zHover)
					this.memory.position.Z += delta;
				if (xHover || yHover || zHover)
					this.memory.WritePosition(this.memory.position);

				// Move rotation based on which control is being hovered.
				if (ryHover)
					this.memory.rotation.Y += delta;
				if (ryHover && delta > 0)
					this.memory.WriteRotation(this.memory.rotation);

				ImGui.EndGroup(); // End group for the drag section.

				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Click and drag each to move the selected item.");
					ImGui.Text("Change the drag option below to influence how much it moves as you drag.");
					ImGui.EndTooltip();
				}

				if (ImGui.InputFloat("x coord", ref this.memory.position.X, this.drag))
					this.memory.WritePosition(this.memory.position);
				xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				if (ImGui.InputFloat("y coord", ref this.memory.position.Y, this.drag))
					this.memory.WritePosition(this.memory.position);
				yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				if (ImGui.InputFloat("z coord", ref this.memory.position.Z, this.drag))
					this.memory.WritePosition(this.memory.position);
				zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				if (ImGui.InputFloat("ry degree", ref this.memory.rotation.Y, this.drag))
					this.memory.WriteRotation(this.memory.rotation);
				ryHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

				// Mouse wheel direction.
				delta = ImGui.GetIO().MouseWheel * this.drag;

				// Move position based on which control is being hovered.
				if (xHover)
					this.memory.position.X += delta;
				if (yHover)
					this.memory.position.Y += delta;
				if (zHover)
					this.memory.position.Z += delta;
				if (xHover || yHover || zHover)
					this.memory.WritePosition(this.memory.position);

				// Move rotation based on which control is being hovered.
				if (ryHover)
					this.memory.rotation.Y += delta;
				if (ryHover && delta > 0)
					this.memory.WriteRotation(this.memory.rotation);

				ImGui.NewLine();

				if (disabled)
					ImGui.PopStyleVar();

				// Drag ammount for the inputs.
				if (ImGui.InputFloat("drag", ref this.drag, 0.05f))
				{
					this.drag = Math.Min(Math.Max(0.01f, this.drag), 10f);
					this.configuration.Drag = this.drag;
					this.configuration.Save();
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.BeginTooltip();
					ImGui.Text("Sets the amount to change when dragging the controls, also influences the gizmo snap feature.");
					ImGui.EndTooltip();
				}

				if (ImGui.Button("Open Furnishing List"))
				{
					unsafe
					{
						if (!this.memory.IsHousingOpen())
						{
							this.pi.Framework.Gui.Chat.PrintError("Cannot open furnishing list unless housing menu is open.");
							this.listVisible = false;
						}
						else if (this.memory.HousingModule->IsOutdoors())
						{
							this.pi.Framework.Gui.Chat.PrintError("Cannot open furnishing outdoors currently.");
							this.listVisible = false;
						}
						else
							this.listVisible = true;
					}
				}
			}
			ImGui.End();

			ImGui.PopStyleColor(2);
		}

		public void DrawGizmo()
		{
			if (!useGizmo)
				return;

			// Disabled if the housing mode isn't on and there isn't a selected item.
			unsafe
			{
				var disabled = !(this.memory.CanEditItem() && this.memory.HousingStructure->ActiveItem != null);
				if (disabled)
					return;
			}

			// Just catch errors since the disabled logic above didn't catch it one time.
			try
			{
				translate = this.memory.ReadPosition();
				rotation = this.memory.ReadRotation();
				ImGuizmo.RecomposeMatrixFromComponents(ref translate.X, ref rotation.X, ref scale.X, ref itemMatrix[0]);
			}
			catch
			{
			}

			var matrixSingleton = this.memory.GetMatrixSingleton();
			if (matrixSingleton == IntPtr.Zero)
				return;

			var viewProjectionMatrix = new float[16];

			unsafe
			{
				var rawMatrix = (float*)(matrixSingleton + 0x1B4).ToPointer();
				for (var i = 0; i < 16; i++, rawMatrix++)
					viewProjectionMatrix[i] = *rawMatrix;
			}

			// Gizmo setup.
			unsafe
			{
				ImGuizmo.Enable(!this.memory.HousingStructure->Rotating);
			}
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

			var snap = this.doSnap ? new Vector3(this.drag, this.drag, this.drag) : Vector3.Zero;

			// ImGuizmo.Manipulate(ref viewProjectionMatrix[0], ref identityMatrix[0], gizmoOperation, gizmoMode, ref itemMatrix[0]);
			this.Manipulate(ref viewProjectionMatrix[0], ref identityMatrix[0], gizmoOperation, gizmoMode, ref itemMatrix[0], ref snap.X);

			ImGuizmo.DecomposeMatrixToComponents(ref itemMatrix[0], ref translate.X, ref rotation.X, ref scale.X);

			this.memory.WritePosition(translate);

			ImGui.EndChild();
			ImGui.End();
		}

		private int FurnishingIndex => this.memory.GetHousingObjectSelectedIndex();
		private bool sortByDistance = false;
		private ulong? lastActiveItem = null;
		private byte renderCount = 0;

		public unsafe void DrawHousingList()
		{
			if (!this.listVisible)
				return;

			// Only allow furnishing list when the housing window is open.
			if (!this.memory.IsHousingOpen())
			{
				this.listVisible = false;
				return;
			}

			// Disallow the ability to open furnishing list outdoors.
			if (this.memory.HousingModule->IsOutdoors())
			{
				this.listVisible = false;
				return;
			}

			ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ORANGE_COLOR);
			ImGui.PushStyleColor(ImGuiCol.CheckMark, ORANGE_COLOR);

			var fontScale = ImGui.GetIO().FontGlobalScale;
			var size = new Vector2(240 * fontScale, 350 * fontScale);

			ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
			ImGui.SetNextWindowSizeConstraints(new Vector2(120 * fontScale, 100 * fontScale), new Vector2(400 * fontScale, 400 * fontScale));

			if (ImGui.Begin($"Furnishing List", ref this.listVisible))
			{
				if (ImGui.Checkbox("Sort by distance", ref this.sortByDistance))
				{
					this.configuration.SortByDistance = this.sortByDistance;
					this.configuration.Save();
				}

				ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
				ImGui.Separator();
				ImGui.PopStyleVar();

				ImGui.BeginChild("FurnishingList");

				var playerPos = this.pi?.ClientState?.LocalPlayer?.Position;

				if (playerPos.HasValue)
				{
					try
					{
						if (this.memory.GetFurnishings(out var items, playerPos.Value, this.sortByDistance))
						{
							for (var i = 0; i < items.Count; i++)
							{
								var name = "";
								ushort icon = 0;
								if (this.plugin.TryGetYardObject(items[i].HousingRowId, out var yardObject))
								{
									name = yardObject.Item.Value.Name.ToString();
									icon = yardObject.Item.Value.Icon;
								}
								if (this.plugin.TryGetFurnishing(items[i].HousingRowId, out var furnitureObject))
								{
									name = furnitureObject.Item.Value.Name.ToString();
									icon = furnitureObject.Item.Value.Icon;
								}

								// An active item is being selected.
								var hasActiveItem = this.memory.HousingStructure->ActiveItem != null;
								// The currently selected item.
								var thisActive = hasActiveItem && items[i].Item == this.memory.HousingStructure->ActiveItem;

								ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
								if (ImGui.Selectable($"##Item{i}", hasActiveItem && thisActive))
									this.memory.SelectItem((IntPtr)this.memory.HousingStructure, (IntPtr)items[i].Item);

								if (thisActive)
									ImGui.SetItemDefaultFocus();

								// Scroll if the active item has changed from last time.
								if (thisActive && this.lastActiveItem != (ulong)this.memory.HousingStructure->ActiveItem)
								{
									ImGui.SetScrollHereY();
									PluginLog.Log($"{ImGui.GetScrollY()} {ImGui.GetScrollMaxY()}");
								}

								ImGui.SameLine(); this.plugin.DrawIcon(icon, new Vector2(20, 20));
								ImGui.PopStyleVar();
								// var distance = Util.DistanceFromPlayer(items[i], playerPos);

								ImGui.SameLine(); ImGui.Text(name);
								// ImGui.SameLine(); ImGui.Text($"{distance:F2}");
							}

							if (this.renderCount >= 10)
								this.lastActiveItem = (ulong)this.memory.HousingStructure->ActiveItem;
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

			if (this.renderCount != 10)
				this.renderCount++;

			ImGui.End();
			ImGui.PopStyleColor(2);
		}

		// Bypass the delta matrix to just only use snap.
		private bool Manipulate(ref float view, ref float projection, OPERATION operation, MODE mode, ref float matrix, ref float snap)
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
