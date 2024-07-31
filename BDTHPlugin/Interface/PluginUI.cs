using BDTHPlugin.Interface.Windows;

using Dalamud.Interface.Windowing;

namespace BDTHPlugin.Interface
{
  public class PluginUI
  {
    private readonly WindowSystem Windows = new("BDTH");

    public readonly MainWindow Main;
    public readonly DebugWindow Debug = new();
    public readonly FurnitureList Furniture = new();
    public readonly GroupWindow Group = new();

    private Gizmo gizmo;

    public PluginUI(Gizmo gizmo)
    {
      this.gizmo = gizmo;

      Main = new MainWindow(gizmo);

      Windows.AddWindow(Main);
      Windows.AddWindow(Debug);
      Windows.AddWindow(Furniture);
      Windows.AddWindow(Group);
    }

    public void Draw()
    {
      gizmo.Draw();
      Windows.Draw();
    }
  }
}
