using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.Threading;

namespace BDTHPlugin
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Burning Down the House";

        private const string commandName = "/bdth";

        private DalamudPluginInterface pi;
        private Configuration configuration;
        private PluginUI ui;
        private PluginMemory memory;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;

            this.configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.configuration.Initialize(this.pi);

            this.memory = new PluginMemory(this.pi);
            this.ui = new PluginUI(this.configuration, this.memory);

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the controls for Burning Down the House plugin."
            });

            this.pi.UiBuilder.OnBuildUi += DrawUI;
        }

        public void Dispose()
        {
            this.ui.Dispose();

            // Dispose for stuff in Plugin Memory class.
            this.memory.Dispose();

            this.pi.CommandManager.RemoveHandler(commandName);
            this.pi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.ui.Visible = true;
        }

        private void DrawUI()
        {
            this.ui.Draw();
        }
    }
}
