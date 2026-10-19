namespace CindarsHope.Core.Events
{
    public readonly struct ItemPurchaseRequestedEvent
    {
        public readonly string ItemId;
        public readonly int Amount;
        public readonly int TotalCost;
        public readonly string SourceId;

        public ItemPurchaseRequestedEvent(string itemId, int amount, int totalCost, string sourceId)
        {
            ItemId = itemId ?? string.Empty;
            Amount = amount;
            TotalCost = totalCost;
            SourceId = sourceId ?? string.Empty;
        }
    }
}
