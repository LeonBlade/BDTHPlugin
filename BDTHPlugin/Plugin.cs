using Dalamud.Data.LuminaExtensions;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using ImGuizmoNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

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

        // Sheets used to get housing item info.
        public Dictionary<uint, HousingFurniture> furnitureDict;
        public Dictionary<uint, HousingYardObject> yardObjectDict;

        // Texture dictionary for the housing item icons.
        public readonly Dictionary<ushort, TextureWrap> TextureDictionary = new Dictionary<ushort, TextureWrap>();


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            pi = pluginInterface;

            configuration = pi.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(pi);

            memory = new PluginMemory(pi);
            ui = new PluginUI(this, pi, configuration, memory);

            // Get the excel sheets for furnishings.
            furnitureDict = pi.Data.GetExcelSheet<HousingFurniture>().ToDictionary(row => row.RowId, row => row);
            yardObjectDict = pi.Data.GetExcelSheet<HousingYardObject>().ToDictionary(row => row.RowId, row => row);

            pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the controls for Burning Down the House plugin."
            });

            // Set the ImGui context 
            ImGuizmo.SetImGuiContext(ImGui.GetCurrentContext());

            pi.UiBuilder.OnBuildUi += DrawUI;
        }

        public void Dispose()
        {
            ui.Dispose();

            // Dispose everything in the texture dictionary.
            foreach (var t in TextureDictionary)
                t.Value?.Dispose();
            TextureDictionary.Clear();

            // Dispose for stuff in Plugin Memory class.
            memory.Dispose();

            pi.CommandManager.RemoveHandler(commandName);
            pi.Dispose();
        }

        /// <summary>
        /// Draws icon from game data.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="size"></param>
        public void DrawIcon(ushort icon, Vector2 size)
		{
            if (icon < 65000)
			{
                if (TextureDictionary.ContainsKey(icon))
				{
                    var tex = TextureDictionary[icon];
                    if (tex == null || tex.ImGuiHandle == IntPtr.Zero)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                        ImGui.BeginChild("FailedTexture", size);
                        ImGui.Text(icon.ToString());
                        ImGui.EndChild();
                        ImGui.PopStyleColor();
                    }
                    else
                        ImGui.Image(TextureDictionary[icon].ImGuiHandle, size);
				}
                else
				{
                    ImGui.BeginChild("WaitingTexture", size, true);
                    ImGui.EndChild();

                    TextureDictionary[icon] = null;

                    Task.Run(() =>
                    {
                        try
                        {
                            var iconTex = pi.Data.GetIcon(icon);
                            var tex = pi.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex != null && tex.ImGuiHandle != IntPtr.Zero)
                                TextureDictionary[icon] = tex;
                        }
                        catch 
                        {
                        }
                    });
				}
			}
		}

        private readonly ushort[] outdoors = new ushort[]
        {
            339, // Mist
            340, // The Lavender Beds
            341, // The Goblet
            641  // Shirogane
        };

        public bool IsOutdoors() => outdoors.Contains(pi.ClientState.TerritoryType);

        public bool TryGetFurnishing(uint id, out HousingFurniture furniture) => furnitureDict.TryGetValue(id, out furniture);
        public bool TryGetYardObject(uint id, out HousingYardObject furniture) => yardObjectDict.TryGetValue(id, out furniture);

        private unsafe void OnCommand(string command, string args)
        {
            args = args.Trim().ToLower();

            // Arguments are being passed in.
            if(!string.IsNullOrEmpty(args))
            {
                // Split the arguments into an array.
                var argArray = args.Split(' ');

                // Check valid state for modifying memory.
                var disabled = !(memory.CanEditItem() && memory.HousingStructure->ActiveItem != null);

                // Show/Hide the furnishing list.
                if (argArray.Length == 1)
                {
                    var opt = argArray[0].ToLower();
                    if (opt.Equals("list"))
                    { 
                        // Only allow furnishing list when the housing window is open.
                        if (!memory.IsHousingOpen())
                        {
                            pi.Framework.Gui.Chat.PrintError("Cannot open furnishing list unless housing menu is open.");
                            ui.ListVisible = false;
                            return;
                        }

                        // Disallow the ability to open furnishing list outdoors.
                        if (IsOutdoors())
                        {
                            pi.Framework.Gui.Chat.PrintError("Cannot open furnishing outdoors currently.");
                            ui.ListVisible = false;
                            return;
                        }

                        ui.ListVisible = !ui.ListVisible;
                    }

                    if (opt.Equals("debug"))
                        ui.debugVisible = !ui.debugVisible;
                }

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
                        memory.position.X = x;
                        memory.position.Y = y;
                        memory.position.Z = z;

                        // Write the position.
                        memory.WritePosition(memory.position);

                        // Specifying the rotation as well.
                        if(argArray.Length == 4)
                        {
                            // Parse and write the rotation.
                            memory.rotation.Y = (float)(float.Parse(argArray[3], NumberStyles.Any, CultureInfo.InvariantCulture) * 180 / Math.PI);
                            memory.WriteRotation(memory.rotation);
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
                ui.Visible = !ui.Visible;
            }
        }

        private void DrawUI()
        {
            ui.Draw();
        }
    }
}
