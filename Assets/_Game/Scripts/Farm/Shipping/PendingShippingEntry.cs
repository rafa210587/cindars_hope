namespace CindarsHope.Farm.Shipping
{
    public enum ShippingEntryState { Pending = 0, Processed = 1, Cancelled = 2, Invalid = 3 }

    public class PendingShippingEntry
    {
        public string ShippingEntryId { get; set; }
        public string SellPointId { get; set; }
        public string ItemId { get; set; }
        public string ItemInstanceId { get; set; }
        public int Quantity { get; set; }
        public int QualityTier { get; set; } = 0;
        public float BaseValueSnapshot { get; set; } = 0f;
        public float PricePreview { get; set; } = 0f;
        public int DepositedDay { get; set; }
        public int ProcessOnDay { get; set; }
        public ShippingEntryState State { get; set; } = ShippingEntryState.Pending;
        public string ProcessedPaymentId { get; set; }
        public bool IsPending => State == ShippingEntryState.Pending;
        public bool IsProcessed => State == ShippingEntryState.Processed;
    }
}
