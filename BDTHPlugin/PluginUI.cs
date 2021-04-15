using Dalamud.Plugin;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Numerics;

namespace BDTHPlugin
{
	// It is good to have this be disposable in general, in case you ever need it
	// to do any cleanup
	class PluginUI : IDisposable
	{
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
		private readonly MODE gizmoMode = MODE.LOCAL;

		// Components for the active item.
		private Vector3 translate = new Vector3();
		private Vector3 rotation = new Vector3();
		private Vector3 scale = new Vector3();

		// this extra bool exists for ImGui, since you can't ref a property
		private bool visible = false;
		public bool Visible
		{
			get { return this.visible; }
			set { this.visible = value; }
		}

		private float drag;
		private bool useGizmo;
		private bool doSnap;

		private bool placeAnywhere = false;
		private readonly Vector4 orangeColor = new Vector4(0.871f, 0.518f, 0f, 1f);

		public PluginUI(Configuration configuration, PluginMemory memory)
		{
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
			DrawGizmo();
			DrawMainWindow();
		}

		public void DrawMainWindow()
		{
			if (!Visible)
			{
				return;
			}

			ImGui.PushStyleColor(ImGuiCol.TitleBgActive, orangeColor);
			ImGui.PushStyleColor(ImGuiCol.CheckMark, orangeColor);

			var fontScale = ImGui.GetIO().FontGlobalScale;
			var size = new Vector2(320 * fontScale, 280 * fontScale);

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

				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Gizmo", ref this.useGizmo))
				{
					this.configuration.UseGizmo = this.useGizmo;
					this.configuration.Save();
				}
						
				ImGui.SameLine();

				// Checkbox is clicked, set the configuration and save.
				if (ImGui.Checkbox("Snap", ref this.doSnap))
				{
					this.configuration.DoSnap = this.doSnap;
					this.configuration.Save();
				}

				// Disabled if the housing mode isn't on and there isn't a selected item.
				var disabled = !(this.memory.IsHousingModeOn() && this.memory.selectedItem != IntPtr.Zero);

				var io = ImGui.GetIO();
				ImGuizmo.SetRect(0, 0, io.DisplaySize.X, io.DisplaySize.Y);

				// Set the opacity based on if housing is on.
				if (disabled)
					ImGui.PushStyleVar(ImGuiStyleVar.Alpha, .3f);

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
					this.configuration.Drag = this.drag;
					this.configuration.Save();
				}
			}
			ImGui.End();

			ImGui.PopStyleColor(2);
		}

		private void DrawGizmo()
		{
			if (!useGizmo)
				return;

			// Disabled if the housing mode isn't on and there isn't a selected item.
			var disabled = !(this.memory.IsHousingModeOn() && this.memory.selectedItem != IntPtr.Zero);
			if (disabled)
				return;

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
			ImGuizmo.Enable(true);
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
			Manipulate(ref viewProjectionMatrix[0], ref identityMatrix[0], gizmoOperation, gizmoMode, ref itemMatrix[0], ref snap.X);

			ImGuizmo.DecomposeMatrixToComponents(ref itemMatrix[0], ref translate.X, ref rotation.X, ref scale.X);

			this.memory.WritePosition(translate);

			ImGui.EndChild();
			ImGui.End();
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
