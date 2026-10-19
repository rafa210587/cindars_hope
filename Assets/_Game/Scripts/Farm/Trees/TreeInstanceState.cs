namespace CindarsHope.Farm.Trees
{
    public enum TreeGrowthStage
    {
        Sapling = 0,
        Young = 1,
        Mature = 2,
        Stump = 3,
        Removed = 4,
        Regrowing = 5,
        Reserved = 99
    }

    public class TreeInstanceState
    {
        public string TreeInstanceId { get; set; }
        public string TreeId { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string ZoneId { get; set; }
        public TreeGrowthStage Stage { get; set; } = TreeGrowthStage.Mature;
        public int RemainingHits { get; set; }
        public int LastChoppedDay { get; set; }
        public int NextRegrowthEligibleDay { get; set; }
        public int RandomSeed { get; set; }

        public bool IsCuttable => Stage == TreeGrowthStage.Mature || Stage == TreeGrowthStage.Young;
        public bool IsStump => Stage == TreeGrowthStage.Stump;
    }

    public class TreeChopResult
    {
        public bool Success { get; set; }
        public bool TreeFelled { get; set; }
        public bool BecameStump { get; set; }
        public string[] DroppedItemIds { get; set; } = System.Array.Empty<string>();
        public int[] DroppedQuantities { get; set; } = System.Array.Empty<int>();
        public string FailureReason { get; set; }

        public static TreeChopResult Fail(string reason) =>
            new TreeChopResult { Success = false, FailureReason = reason };

        public static TreeChopResult Hit() =>
            new TreeChopResult { Success = true };

        public static TreeChopResult Felled(string stumpId, string[] items, int[] qtys) =>
            new TreeChopResult { Success = true, TreeFelled = true, BecameStump = !string.IsNullOrEmpty(stumpId), DroppedItemIds = items, DroppedQuantities = qtys };
    }
}
