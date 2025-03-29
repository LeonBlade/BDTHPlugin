using Dalamud.Configuration;
using System;

namespace BDTHPlugin
{
  [Serializable]
  public class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;
    public bool UseGizmo { get; set; } = false;
    public bool DoSnap { get; set; } = false;
    public float Drag { get; set; } = 0.05f;
    public bool SortByDistance { get; set; } = false;
    public bool AutoVisible { get; set; } = true;
    public bool PlaceAnywhere { get; set; } = false;
    public bool DisplayFurnishingList { get; set; } = true;
    public bool DisplayInventory { get; set; } = true;

    public void Save()
    {
      Plugin.PluginInterface.SavePluginConfig(this);
    }
  }
}
