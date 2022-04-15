using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
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

    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static DataManager Data { get; private set; } = null!;
    [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static Framework Framework { get; private set; } = null!;
    [PluginService] public static ChatGui Chat { get; private set; } = null!;
    [PluginService] public static GameGui GameGui { get; private set; } = null!;
    [PluginService] public static SigScanner TargetModuleScanner { get; private set; } = null!;
    [PluginService] public static Dalamud.Game.ClientState.Conditions.Condition Condition { get; private set; } = null!;

    public static Configuration Configuration;
    public static PluginUI Ui;
    public static PluginMemory Memory;

    // Sheets used to get housing item info.
    public static Dictionary<uint, HousingFurniture> FurnitureDict;
    public static Dictionary<uint, HousingYardObject> YardObjectDict;

    // Texture dictionary for the housing item icons.
    public static readonly Dictionary<ushort, TextureWrap> TextureDictionary = new();

    public Plugin()
    {
      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Memory = new();
      Ui = new();

      FurnitureDict = Data.GetExcelSheet<HousingFurniture>().ToDictionary(row => row.RowId, row => row);
      YardObjectDict = Data.GetExcelSheet<HousingYardObject>().ToDictionary(row => row.RowId, row => row);

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "Opens the controls for Burning Down the House plugin."
      });

      // Set the ImGui context 
      ImGuizmo.SetImGuiContext(ImGui.GetCurrentContext());

      PluginInterface.UiBuilder.Draw += DrawUI;
      Condition.ConditionChange += Condition_ConditionChange;
    }

    private void Condition_ConditionChange(ConditionFlag flag, bool value)
    {
      if (Configuration.AutoVisible && flag == ConditionFlag.UsingHousingFunctions)
      {
        Ui.Visible = value;
      }
    }

    public void Dispose()
    {
      Condition.ConditionChange -= Condition_ConditionChange;
      // Dispose everything in the texture dictionary.
      foreach (var t in TextureDictionary)
        t.Value?.Dispose();
      TextureDictionary.Clear();

      // Dispose for stuff in Plugin Memory class.
      Memory.Dispose();

      CommandManager.RemoveHandler(commandName);

      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Draws icon from game data.
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="size"></param>
    public static void DrawIcon(ushort icon, Vector2 size)
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
              var iconTex = Data.GetIcon(icon);
              var tex = PluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
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

    private static readonly ushort[] outdoors = new ushort[]
    {
      339, // Mist
      340, // The Lavender Beds
      341, // The Goblet
      641, // Shirogane
      979  // Empyreum
    };

    public static bool IsOutdoors() => outdoors.Contains(ClientState.TerritoryType);

    public static bool TryGetFurnishing(uint id, out HousingFurniture furniture) => FurnitureDict.TryGetValue(id, out furniture);
    public static bool TryGetYardObject(uint id, out HousingYardObject furniture) => YardObjectDict.TryGetValue(id, out furniture);

    private unsafe void OnCommand(string command, string args)
    {
      args = args.Trim().ToLower();

      // Arguments are being passed in.
      if (!string.IsNullOrEmpty(args))
      {
        // Split the arguments into an array.
        var argArray = args.Split(' ');

        // Check valid state for modifying memory.
        var disabled = !(Memory.CanEditItem() && Memory.HousingStructure->ActiveItem != null);

        // Show/Hide the furnishing list.
        if (argArray.Length == 1)
        {
          var opt = argArray[0].ToLower();
          if (opt.Equals("list"))
          {
            // Only allow furnishing list when the housing window is open.
            if (!Memory.IsHousingOpen())
            {
              Chat.PrintError("Cannot open furnishing list unless housing menu is open.");
              Ui.ListVisible = false;
              return;
            }

            // Disallow the ability to open furnishing list outdoors.
            if (IsOutdoors())
            {
              Chat.PrintError("Cannot open furnishing outdoors currently.");
              Ui.ListVisible = false;
              return;
            }

            Ui.ListVisible = !Ui.ListVisible;
          }

          if (opt.Equals("debug"))
            Ui.debugVisible = !Ui.debugVisible;
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
            Memory.position.X = x;
            Memory.position.Y = y;
            Memory.position.Z = z;

            // Write the position.
            Memory.WritePosition(Memory.position);

            // Specifying the rotation as well.
            if (argArray.Length == 4)
            {
              // Parse and write the rotation.
              Memory.rotation.Y = (float)(float.Parse(argArray[3], NumberStyles.Any, CultureInfo.InvariantCulture) * 180 / Math.PI);
              Memory.WriteRotation(Memory.rotation);
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
        Ui.Visible = !Ui.Visible;
      }
    }

    private void DrawUI()
    {
      Ui.Draw();
    }
  }
}
