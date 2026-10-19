using System.Collections.Generic;

namespace CindarsHope.Enemy.Drops
{
    public class EnemyDropProfile
    {
        public string EnemyDropProfileId { get; set; }
        public string EnemyId { get; set; }
        public string EnemyFamilyId { get; set; }
        public int NativeFloorMin { get; set; } = 1;
        public int NativeFloorMax { get; set; } = 5;
        public List<string> BiomeIds { get; set; } = new List<string>();
        public List<string> VariantTags { get; set; } = new List<string>();
        public string CommonMaterialTableId { get; set; }
        public string ThematicComponentTableId { get; set; }
        public string RareComponentTableId { get; set; }
        public string GoldRangeProfileId { get; set; }
        public string XPRewardProfileId { get; set; }
        public bool NoNormalLoot { get; set; } = false;
        public List<string> DebugTags { get; set; } = new List<string>();
    }

    public class EliteDropProfile
    {
        public string EliteDropProfileId { get; set; }
        public string BaseEnemyFamilyId { get; set; }
        public string GuaranteedOrNearGuaranteedComponentItemId { get; set; }
        public float GuaranteedComponentChance { get; set; } = 0.90f;
        public string RareDropTableId { get; set; }
        public float BlueprintDropChance { get; set; } = 0.10f;
        public float SpecialGearDropChance { get; set; } = 0.05f;
        public float GoldBonusMultiplier { get; set; } = 1.5f;
        public float XPBonusMultiplier { get; set; } = 1.5f;
    }
}
