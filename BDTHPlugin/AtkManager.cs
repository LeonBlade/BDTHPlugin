using System;
using System.Collections.Generic;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace BDTHPlugin
{
  public class AtkManager
  {
    private static InventoryType inventoryType;

    enum InventoryType
    {
      Base,
      Large,
      Expanded
    }

    public static AtkUnitBasePtr HousingGoods => Plugin.GameGui.GetAddonByName("HousingGoods");
    public static AtkUnitBasePtr Inventory => Plugin.GameGui.GetAddonByName("Inventory");
    public static AtkUnitBasePtr InventoryLarge => Plugin.GameGui.GetAddonByName("InventoryLarge");
    public static AtkUnitBasePtr InventoryExpansion => Plugin.GameGui.GetAddonByName("InventoryExpansion");

    private static readonly Dictionary<InventoryType, List<AtkUnitBasePtr>> Atks = new()
    {
      [InventoryType.Base] = [
        Inventory,
        Plugin.GameGui.GetAddonByName("InventoryGrid"),
        Plugin.GameGui.GetAddonByName("InventoryGridCrystal")
      ],
      [InventoryType.Large] = [
        InventoryLarge,
        Plugin.GameGui.GetAddonByName("InventoryGrid0"),
        Plugin.GameGui.GetAddonByName("InventoryGrid1"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid0"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid1"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid2"),
        Plugin.GameGui.GetAddonByName("InventoryCrystalGrid")
      ],
      [InventoryType.Expanded] = [
        InventoryExpansion,
        Plugin.GameGui.GetAddonByName("InventoryGrid0E"),
        Plugin.GameGui.GetAddonByName("InventoryGrid1E"),
        Plugin.GameGui.GetAddonByName("InventoryGrid2E"),
        Plugin.GameGui.GetAddonByName("InventoryGrid3E"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid0E"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid1E"),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid2E"),
        Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 2)
      ]
    };

    public static unsafe bool InventoryVisible
    {
      get => (InventoryExpansion != null && InventoryExpansion.IsVisible) ||
             (InventoryLarge != null && InventoryLarge.IsVisible) ||
             (Inventory != null && Inventory.IsVisible);

      set
      {
        try
        {
          if (HousingGoods == null || InventoryExpansion == null || InventoryLarge == null || Inventory == null)
            return;

          // Determine which inventory is open assuming it's visible
          if (InventoryExpansion.IsVisible || InventoryLarge.IsVisible || Inventory.IsVisible)
          {
            if (InventoryExpansion.IsVisible) inventoryType = InventoryType.Expanded;
            else if (InventoryLarge.IsVisible) inventoryType = InventoryType.Large;
            else if (Inventory.IsVisible) inventoryType = InventoryType.Base;
          }

          Atks[inventoryType].ForEach((atk) => SetVisible(atk, value));
        }
        catch (Exception ex)
        {
          Plugin.Log.Error("Could not set visibility", ex);
        }
      }
    }

    public static unsafe void ShowFurnishingList(bool state)
    {
      if (HousingGoods != null)
        SetVisible(HousingGoods, state);
    }

    private static unsafe void SetVisible(AtkUnitBasePtr ptr, bool visible) => ((AtkUnitBase*)ptr.Address)->IsVisible = visible;

    public static void ShowInventory(bool state) => InventoryVisible = state;
  }
}