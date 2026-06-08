namespace CindarsHope.Quests.FarmOrders
{
    // Maps FarmOrderType to canonical QuestObjectiveType event names
    // Uses the event names already defined in QuestEventName (QuestCategoryType.cs)
    public static class FarmOrderObjectiveAdapter
    {
        public static string GetObjectiveType(FarmOrderType orderType) => orderType switch
        {
            FarmOrderType.DeliverItem    => "DeliverItem",
            FarmOrderType.ShipItem       => "ShipItem",
            FarmOrderType.HarvestCrop    => "HarvestCrop",
            FarmOrderType.ProcessItem    => "ProcessItem",
            FarmOrderType.CraftItem      => "CraftItem",
            FarmOrderType.BuildOrUpgrade => "BuildOrUpgrade",
            FarmOrderType.SeasonalDelivery => "DeliverItem",
            _ => "DeliverItem"
        };

        public static string GetTriggerEventName(FarmOrderType orderType) => orderType switch
        {
            FarmOrderType.DeliverItem    => "OnItemDelivered",
            FarmOrderType.ShipItem       => "OnItemShipped",
            FarmOrderType.HarvestCrop    => "OnCropHarvested",
            FarmOrderType.ProcessItem    => "OnItemProcessed",
            FarmOrderType.CraftItem      => "OnItemCrafted",
            FarmOrderType.BuildOrUpgrade => "OnBuildingUpgraded",
            FarmOrderType.SeasonalDelivery => "OnItemDelivered",
            _ => "OnItemDelivered"
        };

        // Shipping-based orders must go through day-transition, not instant delivery
        public static bool RequiresDayTransition(FarmOrderType orderType) =>
            orderType == FarmOrderType.ShipItem;
    }
}
