using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Globalization;
using System.Numerics;

using BDTHPlugin.Interface;
using Dalamud.Interface.Textures;

namespace BDTHPlugin
{
  public class Plugin : IDalamudPlugin
  {
    private const string commandName = "/bdth";

    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static ISigScanner TargetModuleScanner { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;

    private static Configuration Configuration = null!;
    private static PluginUI Ui = null!;
    private static PluginMemory Memory = null!;

    public static readonly Gizmo Gizmo = new();

    public Plugin()
    {
      Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      Ui = new(Gizmo);
      Memory = new();

      CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
      {
        HelpMessage = "Opens the controls for Burning Down the House plugin."
      });

      // Set the ImGui context 
      ImGuizmo.SetImGuiContext(ImGui.GetCurrentContext());

      PluginInterface.UiBuilder.Draw += Ui.Draw;
      PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
      Condition.ConditionChange += Condition_ConditionChange;
      Framework.Update += Framework_Update;
    }

    public static Configuration GetConfiguration()
    {
      return Configuration;
    }

    public static PluginMemory GetMemory()
    {
      return Memory;
    }

    public static PluginUI GetUi()
    {
      return Ui;
    }

    private void Framework_Update(IFramework framework)
    {
      Memory.Update();
    }

    private void Condition_ConditionChange(ConditionFlag flag, bool value)
    {
      if (Configuration.AutoVisible && flag == ConditionFlag.UsingHousingFunctions)
        Ui.Main.IsOpen = value;
    }

    public void Dispose()
    {
      PluginInterface.UiBuilder.Draw -= Ui.Draw;

      PluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
      Condition.ConditionChange -= Condition_ConditionChange;
      Framework.Update -= Framework_Update;

      // Dispose for stuff in Plugin Memory class.
      Memory.Dispose();

      CommandManager.RemoveHandler(commandName);

      GC.SuppressFinalize(this);
    }

    private void OpenMainUI()
    {
      Ui.Main.IsOpen = true;
    }

    public static void DrawIcon(ushort icon, Vector2 size)
    {
      if (icon < 65000)
      {
        var iconTexture = TextureProvider.GetFromGameIcon(new GameIconLookup(icon));
        ImGui.Image(iconTexture.GetWrapOrEmpty().ImGuiHandle, size);
      }
    }

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
              Ui.Furniture.IsOpen = false;
              return;
            }

            // Disallow the ability to open furnishing list outdoors.
            if (Memory.IsOutdoors())
            {
              Chat.PrintError("Cannot open furnishing outdoors currently.");
              Ui.Furniture.IsOpen = false;
              return;
            }

            Ui.Furniture.Toggle();
          }

          if (opt.Equals("debug"))
            Ui.Debug.Toggle();

          if (opt.Equals("group"))
            Ui.Group.Toggle();

          if (opt.Equals("reset"))
            Ui.Main.Reset = true;
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
            Log.Error(ex, "Error when positioning with command");
          }
        }
      }
      else
      {
        // Hide or show the UI.
        Ui.Main.Toggle();
      }
    }
  }
}
