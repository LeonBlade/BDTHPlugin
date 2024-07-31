using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace BDTHPlugin.Game;

public static class Addons
{
  private static int inventoryType = 0;

  public static unsafe AtkUnitBase* HousingLayout => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingLayout", 1);
  public static unsafe AtkUnitBase* HousingGoods => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("HousingGoods", 1);
  public static unsafe AtkUnitBase* InventoryExpansion => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryExpansion", 1);
  public static unsafe AtkUnitBase* InventoryGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0E", 1);
  public static unsafe AtkUnitBase* InventoryGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1E", 1);
  public static unsafe AtkUnitBase* InventoryGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid2E", 1);
  public static unsafe AtkUnitBase* InventoryGrid3E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid3E", 1);
  public static unsafe AtkUnitBase* InventoryLarge => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryLarge", 1);
  public static unsafe AtkUnitBase* Inventory => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("Inventory", 1);
  public static unsafe AtkUnitBase* InventoryGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid0", 1);
  public static unsafe AtkUnitBase* InventoryGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid1", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid0 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid1 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2", 1);
  public static unsafe AtkUnitBase* InventoryGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGrid", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid0E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid0E", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid1E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid1E", 1);
  public static unsafe AtkUnitBase* InventoryEventGrid2E => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryEventGrid2E", 1);
  public static unsafe AtkUnitBase* InventoryGridCrystal => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryGridCrystal", 1);
  public static unsafe AtkUnitBase* InventoryCrystalGrid => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 1);
  public static unsafe AtkUnitBase* InventoryCrystalGrid2 => (AtkUnitBase*)Plugin.GameGui.GetAddonByName("InventoryCrystalGrid", 2);

  public static unsafe bool InventoryVisible
  {
    get => InventoryExpansion != null && InventoryExpansion->IsVisible ||
        InventoryLarge != null && InventoryLarge->IsVisible ||
        Inventory != null && Inventory->IsVisible;

    set
    {
      if (HousingGoods == null || InventoryExpansion == null || InventoryLarge == null || Inventory == null)
        return;

      // Determine which inventory is open assuming it's visible
      if (InventoryExpansion->IsVisible || InventoryLarge->IsVisible || Inventory->IsVisible)
      {
        if (InventoryExpansion->IsVisible) inventoryType = 1;
        else if (InventoryLarge->IsVisible) inventoryType = 2;
        else if (Inventory->IsVisible) inventoryType = 3;
      }

      try
      {
        switch (inventoryType)
        {
          case 1:
            InventoryExpansion->IsVisible = value;
            InventoryGrid0E->IsVisible = value;
            InventoryGrid1E->IsVisible = value;
            InventoryGrid2E->IsVisible = value;
            InventoryGrid3E->IsVisible = value;
            InventoryEventGrid0E->IsVisible = value;
            InventoryEventGrid1E->IsVisible = value;
            InventoryEventGrid2E->IsVisible = value;
            InventoryCrystalGrid2->IsVisible = value;
            break;
          case 2:
            InventoryLarge->IsVisible = value;
            InventoryGrid0->IsVisible = value;
            InventoryGrid1->IsVisible = value;
            InventoryEventGrid0->IsVisible = value;
            InventoryEventGrid1->IsVisible = value;
            InventoryEventGrid2->IsVisible = value;
            InventoryCrystalGrid->IsVisible = value;
            break;
          case 3:
            Inventory->IsVisible = value;
            InventoryGrid->IsVisible = value;
            InventoryGridCrystal->IsVisible = value;
            break;
          default:
            break;
        }
      }
      catch
      {
        Plugin.Log.Error("IsVisible setter not present");
      }
    }
  }

  public static unsafe void ShowFurnishingList(bool state)
  {
    if (HousingGoods != null)
      HousingGoods->IsVisible = state;
  }

  public static void ShowInventory(bool state) => InventoryVisible = state;

  public static unsafe void Dispose()
  {
    try
    {
      // Enable the housing goods menu again.
      if (HousingGoods != null)
        HousingGoods->IsVisible = true;
    }
    catch (Exception ex)
    {
      Plugin.Log.Error(ex, "Error while calling Addons.Dispose()");
    }
  }
}
