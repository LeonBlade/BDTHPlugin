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

    public static unsafe AtkUnitBasePtr HousingGoods => Plugin.GameGui.GetAddonByName("HousingGoods", 1);
    public static unsafe AtkUnitBasePtr Inventory => Plugin.GameGui.GetAddonByName("Inventory", 1);
    public static unsafe AtkUnitBasePtr InventoryLarge => Plugin.GameGui.GetAddonByName("InventoryLarge", 1);
    public static unsafe AtkUnitBasePtr InventoryExpansion => Plugin.GameGui.GetAddonByName("InventoryExpansion", 1);

    private static readonly unsafe Dictionary<InventoryType, List<AtkUnitBasePtr>> Atks = new()
    {
      [InventoryType.Base] = [
        Inventory,
        Plugin.GameGui.GetAddonByName("InventoryGrid", 1),
        Plugin.GameGui.GetAddonByName("InventoryGridCrystal", 1)
      ],
      [InventoryType.Large] = [
        InventoryLarge,
        Plugin.GameGui.GetAddonByName("InventoryEventGrid0", 1),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid1", 1),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid2", 1),
        Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 1)
      ],
      [InventoryType.Expanded] = [
        InventoryExpansion,
        Plugin.GameGui.GetAddonByName("InventoryGrid0E", 1),
        Plugin.GameGui.GetAddonByName("InventoryGrid1E", 1),
        Plugin.GameGui.GetAddonByName("InventoryGrid2E", 1),
        Plugin.GameGui.GetAddonByName("InventoryGrid3E", 1),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid0E", 1),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid1E", 1),
        Plugin.GameGui.GetAddonByName("InventoryEventGrid2E", 1),
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