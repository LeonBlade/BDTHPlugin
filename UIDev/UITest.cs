using ImGuiNET;
using ImGuiScene;
using System;
using System.Numerics;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup hereThe 

            scene.OnBuildUI += Draw;

            this.Visible = true;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();

            if (!Visible)
            {
                this.scene.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool placeAnywhere = false;
        private Vector3 position = Vector3.Zero;
        private bool testBool = false;

        private float drag = 0.05f;
        private string testing = "";
        private int test;

        private bool disabled = false;
        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(350, 220), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(350, 220), new Vector2(350, 220));

            if (ImGui.Begin("Burning Down the House", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize))
            {
                if (ImGui.Checkbox("Place Anywhere", ref this.placeAnywhere))
                {
                    this.testBool = this.placeAnywhere;
                }

                ImGui.SameLine(); ImGui.Checkbox("Disable", ref this.disabled);

                //if (this.disabled)
                //    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, .3f);

                // ImGui.DragFloat3("position", ref this.position, this.drag);

                ImGui.PushItemWidth(73f);

                ImGui.DragFloat("##xdrag", ref this.position.X, this.drag); ImGui.SameLine(0, 4);
                var xHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                ImGui.DragFloat("##ydrag", ref this.position.Y, this.drag); ImGui.SameLine(0, 4);
                var yHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                ImGui.DragFloat("##zdrag", ref this.position.Z, this.drag); ImGui.SameLine(0, 4);
                var zHover = ImGui.IsMouseHoveringRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());

                ImGui.PopItemWidth();

                ImGui.Text("position");

                // Mouse wheel direction.
                var delta = ImGui.GetIO().MouseWheel * this.drag;

                // Move position based on which control is being hovered.
                if (xHover)
                    this.position.X += delta;
                if (yHover)
                    this.position.Y += delta;
                if (zHover)
                    this.position.Z += delta;

                ImGui.InputFloat("x coord", ref this.position.X, this.drag);
                ImGui.InputFloat("y coord", ref this.position.Y, this.drag);
                ImGui.InputFloat("z coord", ref this.position.Z, this.drag);

                ImGui.NewLine();

                ImGui.InputFloat("drag", ref this.drag, 0.05f);

                //if (this.disabled)
                //    ImGui.PopStyleVar();
            }
            ImGui.End();
        }

        #endregion
    }
}
