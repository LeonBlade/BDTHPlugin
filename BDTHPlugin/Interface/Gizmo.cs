using System.Net.WebSockets;
using System.Numerics;
using Dalamud.Interface.Utility;
using ImGuiNET;
using ImGuizmoNET;

namespace BDTHPlugin.Interface
{
  public class Gizmo
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private static unsafe bool CanEdit => Configuration.UseGizmo && Memory.CanEditItem() && Memory.HousingStructure->ActiveItem != null;

    public MODE Mode = MODE.LOCAL;

    private Vector3 translate;
    private Vector3 rotation;
    private Vector3 scale = Vector3.One;

    private Matrix4x4 matrix = Matrix4x4.Identity;

    private ImGuiIOPtr Io;
    private Vector2 Wp;

    public void Draw()
    {
      if (!CanEdit)
        return;

      ImGuiHelpers.ForceNextWindowMainViewport();
      ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(0, 0));

      ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

      const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs;
      if (!ImGui.Begin("BDTHGizmo", windowFlags))
        return;

      Io = ImGui.GetIO();
      ImGui.SetWindowSize(Io.DisplaySize);

      Wp = ImGui.GetWindowPos();

      try
      {
        DrawGizmo(Wp, new Vector2(Io.DisplaySize.X, Io.DisplaySize.Y));
      }
      finally
      {
        ImGui.PopStyleVar();
        ImGui.End();
      }
    }

    private unsafe void DrawGizmo(Vector2 pos, Vector2 size)
    {
      ImGuizmo.BeginFrame();

      var cam = Memory.Camera->RenderCamera;
      var view = Memory.Camera->ViewMatrix;
      var proj = cam->ProjectionMatrix;

      var far = cam->FarPlane;
      var near = cam->NearPlane;
      var clip = far / (far - near);

      proj.M43 = -(clip * near);
      proj.M33 = -((far + near) / (far - near));
      view.M44 = 1.0f;

      ImGuizmo.SetDrawlist();

      ImGuizmo.Enable(Memory.HousingStructure->Rotating);
      ImGuizmo.SetID((int)ImGui.GetID("BDTHPlugin"));
      ImGuizmo.SetOrthographic(false);

      ImGuizmo.SetRect(pos.X, pos.Y, size.X, size.Y);

      ComposeMatrix();

      var snap = Configuration.DoSnap ? new(Configuration.Drag, Configuration.Drag, Configuration.Drag) : Vector3.Zero;

      if (Manipulate(ref view.M11, ref proj.M11, OPERATION.TRANSLATE, Mode, ref matrix.M11, ref snap.X))
        WriteMatrix();

      ImGuizmo.SetID(-1);
    }

    private void ComposeMatrix()
    {
      try
      {
        translate = Memory.ReadPosition();
        rotation = Memory.ReadRotation();
        ImGuizmo.RecomposeMatrixFromComponents(ref translate.X, ref rotation.X, ref scale.X, ref matrix.M11);
      }
      catch
      {
      }
    }

    private void WriteMatrix()
    {
      ImGuizmo.DecomposeMatrixToComponents(ref matrix.M11, ref translate.X, ref rotation.X, ref scale.X);
      Memory.WritePosition(translate);
    }

    private unsafe bool Manipulate(ref float view, ref float proj, OPERATION op, MODE mode, ref float matrix, ref float snap)
    {
      fixed (float* native_view = &view)
      {
        fixed (float* native_proj = &proj)
        {
          fixed (float* native_matrix = &matrix)
          {
            fixed (float* native_snap = &snap)
            {
              return ImGuizmoNative.ImGuizmo_Manipulate(
                native_view,
                native_proj,
                op,
                mode,
                native_matrix,
                null,
                native_snap,
                null,
                null
              ) != 0;
            }
          }
        }
      }
    }
  }
}
