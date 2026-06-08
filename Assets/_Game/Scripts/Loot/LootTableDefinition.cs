using System.Collections.Generic;

namespace CindarsHope.Loot
{
    public class GoldRange
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 0;
        public bool IsEmpty => Max <= 0;
    }

    public class QualityRollProfile
    {
        public float ChanceQ1 { get; set; } = 0f;
        public float ChanceQ2 { get; set; } = 0f;
        public float ChanceQ3 { get; set; } = 0f;
        public float ChanceQ4 { get; set; } = 0f;
        public bool IsSeeded { get; set; } = false;
    }

    public class RarityRollProfile
    {
        public float ChanceUncommon { get; set; } = 0f;
        public float ChanceRare { get; set; } = 0f;
        public float ChanceEpic { get; set; } = 0f;
        public float ChanceLegendary { get; set; } = 0f;
    }

    public class QuantityRollProfile
    {
        public int BaseMin { get; set; } = 1;
        public int BaseMax { get; set; } = 1;
        public float BonusMultiplier { get; set; } = 1f;
    }

    public class FirstTimeBonus
    {
        public List<string> BonusItemIds { get; set; } = new List<string>();
        public int BonusGold { get; set; } = 0;
        public List<string> GrantedFlags { get; set; } = new List<string>();
        public bool IsConsumed { get; set; } = false;
    }

    public class RepeatFarmRules
    {
        public bool AllowRepeat { get; set; } = true;
        public float RepeatDropRateMultiplier { get; set; } = 1f;
        public int CooldownDays { get; set; } = 0;
        public int MaxGrantsPerDay { get; set; } = -1;
    }

    public class PityRules
    {
        public bool Enabled { get; set; } = false;
        public int PityThreshold { get; set; } = 10;
        public string PityGrantItemId { get; set; }
        public string AlternativeRecipeId { get; set; }
        public string LateFallbackQuestId { get; set; }
    }

    public class LootTableDefinition
    {
        public string LootTableId { get; set; }
        public LootSourceType SourceType { get; set; } = LootSourceType.Unknown;
        public List<string> AllowedBiomes { get; set; } = new List<string>();
        public List<string> AllowedFloorRanges { get; set; } = new List<string>();
        public List<string> RequiredProgressFlags { get; set; } = new List<string>();
        public List<string> RequiredReputationFlags { get; set; } = new List<string>();
        public List<string> RequiredStoryFlags { get; set; } = new List<string>();
        public List<LootEntry> GuaranteedDrops { get; set; } = new List<LootEntry>();
        public List<LootEntry> WeightedDrops { get; set; } = new List<LootEntry>();
        public List<LootEntry> RareDrops { get; set; } = new List<LootEntry>();
        public List<LootEntry> UniqueDrops { get; set; } = new List<LootEntry>();
        public GoldRange GoldRange { get; set; } = new GoldRange();
        public QualityRollProfile QualityRollProfile { get; set; } = new QualityRollProfile();
        public RarityRollProfile RarityRollProfile { get; set; } = new RarityRollProfile();
        public QuantityRollProfile QuantityRollProfile { get; set; } = new QuantityRollProfile();
        public FirstTimeBonus FirstTimeBonus { get; set; }
        public RepeatFarmRules RepeatFarmRules { get; set; } = new RepeatFarmRules();
        public PityRules PityRules { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();
    }
}
