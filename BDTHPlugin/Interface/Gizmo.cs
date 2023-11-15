using System.Numerics;

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
    
    private Matrix4x4 itemMatrix = Matrix4x4.Identity;
    
    public void Draw()
    {
      if (!CanEdit)
        return;
      
      var vp = ImGui.GetMainViewport();
      ImGui.SetNextWindowSize(vp.Size);
      ImGui.SetNextWindowPos(vp.Pos, ImGuiCond.Always);
      ImGui.SetNextWindowViewport(vp.ID);
        
      const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs;
      if (!ImGui.Begin("BDTHGizmo", windowFlags))
        return;

      try
      {
        DrawGizmo(vp.Pos, vp.Size);
      }
      finally
      {
        ImGui.End();
      }
    }

    private unsafe void DrawGizmo(Vector2 pos, Vector2 size)
    {
      var projMatrix = Memory.Camera->RenderCamera->ProjectionMatrix;
      var viewMatrix = Memory.Camera->ViewMatrix;
      viewMatrix.M44 = 1.0f;
      
      ImGuizmo.SetDrawlist();
      
      ImGuizmo.Enable(Memory.HousingStructure->Rotating);
      ImGuizmo.SetID((int)ImGui.GetID("BDTHPlugin"));
      ImGuizmo.SetOrthographic(false);
      
      ImGuizmo.SetRect(pos.X, pos.Y, size.X, size.Y);

      ComposeMatrix();

      var drag = Configuration.Drag;
      var snap = Configuration.DoSnap ? new Vector3(drag, drag, drag) : Vector3.Zero;
      
      var moved = Manipulate(ref viewMatrix.M11, ref projMatrix.M11, OPERATION.TRANSLATE, Mode, ref itemMatrix.M11, ref snap.X);
      if (moved) WriteMatrix();

      ImGuizmo.SetID(-1);
    }

    private void ComposeMatrix()
    {
      try {
        translate = Memory.ReadPosition();
        rotation = Memory.ReadRotation();
        ImGuizmo.RecomposeMatrixFromComponents(ref translate.X, ref rotation.X, ref scale.X, ref itemMatrix.M11);
      }
      catch
      {
        // ignored
      }
    }

    private void WriteMatrix()
    {
      ImGuizmo.DecomposeMatrixToComponents(ref itemMatrix.M11, ref translate.X, ref rotation.X, ref scale.X);
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
