namespace CindarsHope.Inventory
{
    public readonly struct InventoryAddResult
    {
        public bool Success { get; }
        public string ItemId { get; }
        public int RequestedAmount { get; }
        public int AddedAmount { get; }
        public int RemainingAmount => RequestedAmount - AddedAmount;

        public InventoryAddResult(bool success, string itemId, int requestedAmount, int addedAmount)
        {
            Success = success;
            ItemId = itemId ?? string.Empty;
            RequestedAmount = requestedAmount;
            AddedAmount = addedAmount;
        }
    }
}
