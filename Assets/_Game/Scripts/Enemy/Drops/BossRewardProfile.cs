using System.Collections.Generic;

namespace CindarsHope.Enemy.Drops
{
    public class BossRewardProfile
    {
        public string BossRewardProfileId { get; set; }
        public string BossId { get; set; }
        public int BossGateLevel { get; set; } = 1;
        public string FirstTimeRewardTableId { get; set; }
        public string RepeatRewardTableId { get; set; }
        public List<string> RequiredStoryFlags { get; set; } = new List<string>();
        public List<string> GrantedStoryFlags { get; set; } = new List<string>();
        public string GrantedCheckpoint { get; set; }
        public bool UnlocksLevel101Access { get; set; } = false;
        public List<string> LoreRewardIds { get; set; } = new List<string>();
        public string BossRewardConsumedFlag { get; set; }
    }

    public class BossDefeatState
    {
        public string BossId { get; set; }
        public bool IsDefeated { get; set; } = false;
        public bool FirstTimeRewardConsumed { get; set; } = false;
        public int DefeatCount { get; set; } = 0;
        public int LastDefeatDay { get; set; } = -1;
    }

    public class DefeatRewardContext
    {
        public string EnemyId { get; set; }
        public string EnemyFamilyId { get; set; }
        public List<string> VariantTags { get; set; } = new List<string>();
        public int CaveLevel { get; set; } = 1;
        public string BiomeId { get; set; }
        public bool IsElite { get; set; } = false;
        public bool IsBoss { get; set; } = false;
        public string BossId { get; set; }
        public int RunSeed { get; set; }
        public string SnapshotId { get; set; }
        public List<string> PlayerProgressFlags { get; set; } = new List<string>();
        public BossDefeatState BossDefeatState { get; set; }
    }
}
