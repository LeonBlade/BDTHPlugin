using ImGuiNET;
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

        public PluginUI(Configuration configuration, PluginMemory memory)
        {
            this.configuration = configuration;
            this.memory = memory;
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

            ImGui.SetNextWindowSize(new Vector2(320, 230), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(320, 230), new Vector2(320, 230));

            if (ImGui.Begin("Burning Down the House", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
            {
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

                // The refs used below swap between the static this.position for a disabled effect.

                // Write the position if we update the values.
                if (ImGui.DragFloat3("position", ref this.memory.position, this.drag))
                    this.memory.WritePosition(this.memory.position);

                // Separate XYZ coordinates.
                if (ImGui.InputFloat("x coord", ref this.memory.position.X, this.drag))
                    this.memory.WritePosition(this.memory.position);
                if (ImGui.InputFloat("y coord", ref this.memory.position.Y, this.drag))
                    this.memory.WritePosition(this.memory.position);
                if (ImGui.InputFloat("z coord", ref this.memory.position.Z, this.drag))
                    this.memory.WritePosition(this.memory.position);

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
