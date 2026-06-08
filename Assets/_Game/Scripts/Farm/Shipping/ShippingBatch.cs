using System.Collections.Generic;

namespace CindarsHope.Farm.Shipping
{
    public enum ShippingBatchState { Pending = 0, Processed = 1, FailedPartial = 2 }

    public class ShippingBatch
    {
        public string BatchId { get; set; }
        public int ProcessDay { get; set; }
        public List<PendingShippingEntry> Entries { get; set; } = new List<PendingShippingEntry>();
        public float TotalGold { get; set; } = 0f;
        public ShippingBatchState State { get; set; } = ShippingBatchState.Pending;
    }
}
