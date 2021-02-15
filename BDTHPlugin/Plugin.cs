using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Globalization;
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
            args = args.Trim().ToLower();

            // Arguments are being passed in.
            if(!string.IsNullOrEmpty(args))
            {
                // Split the arguments into an array.
                var argArray = args.Split(' ');

                // Check valid state for modifying memory.
                var disabled = !(this.memory.IsHousingModeOn() && this.memory.selectedItem != IntPtr.Zero);

                // Position or rotation values are being passed in, and we're not disabled.
                if (argArray.Length >= 3 && !disabled)
                {
                    try
                    {
                        // Parse the coordinates into floats.
                        var x = float.Parse(argArray[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        var y = float.Parse(argArray[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        var z = float.Parse(argArray[2], NumberStyles.Any, CultureInfo.InvariantCulture);

                        // Set the position in the memory object.
                        this.memory.position.X = x;
                        this.memory.position.Y = y;
                        this.memory.position.Z = z;

                        // Write the position.
                        this.memory.WritePosition(this.memory.position);

                        // Specifying the rotation as well.
                        if(argArray.Length == 4)
                        {
                            // Parse and write the rotation.
                            this.memory.rotation.Y = (float)(double.Parse(argArray[3]) * 180 / Math.PI);
                            this.memory.WriteRotation(this.memory.rotation);
                        }
                    }
                    catch (Exception ex)
					{
                        PluginLog.LogError(ex, "Error when positioning with command");
					}
                }
            }
            else
            {
                // Hide or show the UI.
                this.ui.Visible = !this.ui.Visible;
            }
        }

        private void DrawUI()
        {
            this.ui.Draw();
        }
    }
}
