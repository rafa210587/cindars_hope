namespace CindarsHope.Loot
{
    public enum LootEntryQualityPolicy { None = 0, Fixed, RollFromProfile, SeededFromSnapshot }
    public enum LootEntryRarityPolicy { None = 0, Fixed, RollFromProfile }

    public class LootEntry
    {
        public string EntryId { get; set; }
        public string ItemId { get; set; }
        public int QuantityMin { get; set; } = 1;
        public int QuantityMax { get; set; } = 1;
        public int Weight { get; set; } = 1;
        public float DropChance { get; set; } = 1f;
        public LootEntryQualityPolicy QualityPolicy { get; set; } = LootEntryQualityPolicy.None;
        public LootEntryRarityPolicy RarityPolicy { get; set; } = LootEntryRarityPolicy.None;
        public string RequiredFlag { get; set; }
        public string ForbiddenFlag { get; set; }
        public bool FirstTimeOnly { get; set; } = false;
        public bool Repeatable { get; set; } = true;
        public bool IsUniqueReward { get; set; } = false;
        public bool IsLoreReward { get; set; } = false;
        public bool IsProgressionCritical { get; set; } = false;

        public bool IsProtected => IsUniqueReward || IsLoreReward || FirstTimeOnly;
    }
}
