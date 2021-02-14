using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

namespace BDTHPlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly Configuration configuration;
        private readonly PluginMemory memory;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private float drag = 0.05f;
        private bool placeAnywhere = false;
        private readonly Vector4 orangeColor = new Vector4(0.871f, 0.518f, 0f, 1f);
        private readonly string version;

        public PluginUI(Configuration configuration, PluginMemory memory)
        {
            this.configuration = configuration;
            this.memory = memory;
            var fv = Assembly.GetExecutingAssembly().GetName().Version;
            version = $"{fv.Major}.{fv.Minor}.{fv.Build}";
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
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

            var scale = ImGui.GetIO().FontGlobalScale;
            var size = new Vector2(320 * scale, 280 * scale);

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

                // Disabled if the housing mode isn't on and there isn't a selected item.
                var disabled = !(this.memory.IsHousingModeOn() && this.memory.selectedItem != IntPtr.Zero);

                // Set the opacity based on if housing is on.
                if (disabled)
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, .3f);

                ImGui.PushItemWidth(73f);

                if (ImGui.DragFloat("##xdrag", ref this.memory.position.X, this.drag))
                    memory.WritePosition(this.memory.position);
                ImGui.SameLine(0, 4);
                var xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                if (ImGui.DragFloat("##ydrag", ref this.memory.position.Y, this.drag))
                    memory.WritePosition(this.memory.position);
                ImGui.SameLine(0, 4);
                var yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                if (ImGui.DragFloat("##zdrag", ref this.memory.position.Z, this.drag))
                    memory.WritePosition(this.memory.position);
                ImGui.SameLine(0, 4);
                var zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                ImGui.Text("position");

                if (ImGui.DragFloat("##rydrag", ref this.memory.rotation.Y, this.drag))
                    memory.WriteRotation(this.memory.rotation);
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
                    memory.WritePosition(this.memory.position);
                
                // Move rotation based on which control is being hovered.
                if (ryHover)
                    this.memory.rotation.Y += delta;
                if (ryHover && delta > 0)
                    memory.WriteRotation(this.memory.rotation);

                if (ImGui.InputFloat("x coord", ref this.memory.position.X, this.drag))
                    memory.WritePosition(this.memory.position);
                xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                if (ImGui.InputFloat("y coord", ref this.memory.position.Y, this.drag))
                    memory.WritePosition(this.memory.position);
                yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                if (ImGui.InputFloat("z coord", ref this.memory.position.Z, this.drag))
                    memory.WritePosition(this.memory.position);
                zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                if (ImGui.InputFloat("ry degree", ref this.memory.rotation.Y, this.drag))
                    memory.WriteRotation(this.memory.rotation);
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
                    memory.WritePosition(this.memory.position);

                // Move rotation based on which control is being hovered.
                if (ryHover)
                    this.memory.rotation.Y += delta;
                if (ryHover && delta > 0)
                    memory.WriteRotation(this.memory.rotation);

                ImGui.NewLine();

                if (disabled)
                    ImGui.PopStyleVar();

                // Drag ammount for the inputs.
                ImGui.InputFloat("drag", ref this.drag, 0.05f);
            }
            ImGui.End();

            ImGui.PopStyleColor(2);
        }
    }
}
