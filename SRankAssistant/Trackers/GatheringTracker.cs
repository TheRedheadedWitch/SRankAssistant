using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;

namespace SRankAssistant;

internal static class GatheringTracker
{
    internal static void Initialize()
    {
        SERVICES.GameInventory.ItemAdded += OnItemChangedOrAdded;
        SERVICES.GameInventory.ItemChanged += OnItemChangedOrAdded;
    }

    private static void OnItemChangedOrAdded(GameInventoryEvent type, InventoryEventArgs data)
    {
        LOG.Debug($"GATHERED ITEM - ID: {data.Item.ItemId}, Container: {data.Item.ContainerType.ToString()}, Zone: {SERVICES.ClientState.TerritoryType}");
        SRankCondition? condition = SRankData.GetCondition();
        if (condition == null || condition.Type != SRankConditionType.Gathering) return;
        if (data.Item.ContainerType.ToString() is "Inventory1" or "Inventory2" or "Inventory3" or "Inventory4")
            foreach ((uint goal, uint targetId) in condition.Targets)
                if (data.Item.ItemId == targetId)
                {
                    Globals.tracker.Increment(targetId);
                    break;
                }
    }

    internal static void Dispose()
    {
        SERVICES.GameInventory.ItemAdded -= OnItemChangedOrAdded;
        SERVICES.GameInventory.ItemChanged -= OnItemChangedOrAdded;
    }
}