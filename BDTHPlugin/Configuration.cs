using Dalamud.Configuration;
using Dalamud.Plugin;
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

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
