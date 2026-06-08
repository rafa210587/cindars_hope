using System.Collections.Generic;

namespace CindarsHope.Quests.Rewards
{
    public enum QuestRewardType
    {
        Gold = 0, Item = 1, Recipe = 2, ToolUnlock = 3, EquipmentUnlock = 4, SpellUnlock = 5,
        SkillPoint = 6, SkillTreeUnlock = 7,
        RelationshipFuture = 80,
        KnowledgeUnlock = 10, BestiaryEntryUnlock = 11,
        FonteUpgrade = 20, LivingWaterCharge = 21,
        QuestFlagGrant = 30, QuestFlagClear = 31,
        AreaUnlock = 40, CaveDepthUnlock = 41, ShopUnlock = 42, ShopStockUnlock = 43,
        DialogueUnlock = 44, NpcScheduleUnlock = 45, FestivalUnlock = 46,
        CompanionUnlockFuture = 81, PetUnlockFuture = 82, SocialUnlockFuture = 83
    }

    public enum RewardIdempotencyPolicy
    {
        TrackByRewardId = 0,
        TrackByFlagId = 1,
        AllowRepeat = 2,
        AtomicOnce = 3
    }

    public enum RewardFailurePolicy
    {
        FailAndNotify = 0,
        OverflowDropToGround = 1,
        RetryOnNextSession = 2,
        SkipSilently = 3
    }

    public class QuestRewardDefinition
    {
        public string RewardId { get; set; }
        public QuestRewardType RewardType { get; set; }
        public string TargetId { get; set; }
        public int Quantity { get; set; } = 1;
        public string QualityPolicy { get; set; }
        public string VisibilityPolicyId { get; set; }
        public int SpoilerTier { get; set; } = 0;
        public bool RequiresStrongConfirmation { get; set; } = false;
        public RewardIdempotencyPolicy IdempotencyPolicy { get; set; } = RewardIdempotencyPolicy.TrackByRewardId;
        public RewardFailurePolicy FailurePolicy { get; set; } = RewardFailurePolicy.FailAndNotify;
        public string GrantedFlagId { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool IsFutureReward() => RewardType == QuestRewardType.RelationshipFuture ||
                                         RewardType == QuestRewardType.CompanionUnlockFuture ||
                                         RewardType == QuestRewardType.PetUnlockFuture ||
                                         RewardType == QuestRewardType.SocialUnlockFuture;

        // FonteUpgrade/LivingWaterCharge cannot be applied via generic quest reward without adapter
        public bool RequiresFonteAdapter() => RewardType == QuestRewardType.FonteUpgrade ||
                                               RewardType == QuestRewardType.LivingWaterCharge;
    }
}
