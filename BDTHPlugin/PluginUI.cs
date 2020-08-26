using ImGuiNET;
using System;
using System.Numerics;

namespace BDTHPlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private float drag = 0.05f;

        private bool placeAnywhere = false;
        private Vector3 position = Vector3.Zero;

        public PluginUI(Configuration configuration)
        {
            this.configuration = configuration;
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

            ImGui.SetNextWindowSize(new Vector2(350, 230), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(350, 230), new Vector2(350, 230));

            if (ImGui.Begin("Burning Down the House", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
            {
                if (ImGui.Checkbox("Place Anywhere", ref this.placeAnywhere))
                {
                    // Set the place anywhere based on the checkbox state.
                    PluginMemory.SetPlaceAnywhere(this.placeAnywhere);
                }

                // When position is moved from the UI.
                if (ImGui.DragFloat3("Position", ref PluginMemory.position, this.drag))
                {
                    PluginMemory.WritePosition(PluginMemory.position);
                }

                ImGui.InputFloat("Drag Amount", ref this.drag);

                if (ImGui.CollapsingHeader("Debug"))
                {
                    ImGui.Text($"Place Anywhere: {PluginMemory.placeAnywhere.ToInt64():X}");
                    ImGui.Text($"Wall Anywhere: {PluginMemory.wallAnywhere.ToInt64():X}");
                    ImGui.Text($"Active Item Func: {PluginMemory.selectedItemFunc.ToInt64():X}");
                    ImGui.Text($"Active Item: {PluginMemory.selectedItem.ToInt64():X}");
                }

            }
            ImGui.End();
        }
    }
}
