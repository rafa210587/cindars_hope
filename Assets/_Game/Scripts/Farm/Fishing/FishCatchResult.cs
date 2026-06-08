namespace CindarsHope.Farm.Fishing
{
    public class FishCatchResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string FishItemId { get; set; }
        public int Quantity { get; set; }
        public FishRarity Rarity { get; set; }
        public int StaminaCostApplied { get; set; }
        public int FatigueCostApplied { get; set; }
        public bool InventoryFull { get; set; }

        public static FishCatchResult Fail(string reason) =>
            new FishCatchResult { Success = false, FailureReason = reason };
    }
}
