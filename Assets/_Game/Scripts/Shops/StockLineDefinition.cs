namespace CindarsHope.Shops
{
    public class StockLineDefinition
    {
        public string StockLineId { get; set; }
        public string ItemId { get; set; }
        public StockType StockType { get; set; } = StockType.BaseStock;
        public int MinQuantity { get; set; } = 1;
        public int MaxQuantity { get; set; } = 5;
        public int RestockToQuantity { get; set; } = 5;
        public int RestockVariance { get; set; } = 0;
        public float RestockChance { get; set; } = 1f;
        public string RequiredSeason { get; set; }
        public int RequiredReputation { get; set; } = 0;
        public string RequiredQuestFlag { get; set; }
        public int RequiredCaveProgress { get; set; } = 0;
        public int RequiredFarmLevel { get; set; } = 0;
        public string RequiredStoryFlag { get; set; }
        public int StartDay { get; set; } = -1;
        public int EndDay { get; set; } = -1;
        public int PurchaseLimitPerDay { get; set; } = -1;
        public int PurchaseLimitPerWeek { get; set; } = -1;
        public int PurchaseLimitLifetime { get; set; } = -1;
        public bool IsUniqueStock { get; set; } = false;
        public bool CanBeSoldBackToShop { get; set; } = false;

        public bool IsUnlocked(string currentSeason, int reputation, string questFlag, int caveProgress, int farmLevel, string storyFlag)
        {
            if (!string.IsNullOrEmpty(RequiredSeason) && RequiredSeason != currentSeason) return false;
            if (reputation < RequiredReputation) return false;
            if (!string.IsNullOrEmpty(RequiredQuestFlag) && questFlag != RequiredQuestFlag) return false;
            if (caveProgress < RequiredCaveProgress) return false;
            if (farmLevel < RequiredFarmLevel) return false;
            if (!string.IsNullOrEmpty(RequiredStoryFlag) && storyFlag != RequiredStoryFlag) return false;
            return true;
        }
    }
}
