namespace CindarsHope.Core.Events
{
    public readonly struct ShopStockChangedEvent
    {
        public string ShopId { get; }
        public string ItemId { get; }
        public int CurrentStock { get; }

        public ShopStockChangedEvent(string shopId, string itemId, int currentStock)
        {
            ShopId = shopId;
            ItemId = itemId;
            CurrentStock = currentStock;
        }
    }
}
