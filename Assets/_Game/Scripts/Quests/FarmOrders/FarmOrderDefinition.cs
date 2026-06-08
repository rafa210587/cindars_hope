using System.Collections.Generic;

namespace CindarsHope.Quests.FarmOrders
{
    public enum FarmOrderType
    {
        DeliverItem = 0, ShipItem = 1, HarvestCrop = 2, ProcessItem = 3,
        CraftItem = 4, BuildOrUpgrade = 5, SeasonalDelivery = 6
    }

    public enum FarmOrderDeadlinePolicyType
    {
        NoDeadline = 0, EndOfDay = 1, DaysFromAccept = 2,
        SeasonEnd = 3, FestivalEnd = 4, SpecificDay = 5
    }

    public enum FarmOrderRepeatPolicyType
    {
        Never = 0, DailyTable = 1, WeeklyTable = 2,
        SeasonalTable = 3, NpcRequestPool = 4, ShopRequestPool = 5, ManualStoryOnly = 6
    }

    public enum QualityAcceptancePolicyType
    {
        AcceptAnyQuality = 0, MinimumQuality = 1,
        ExactQuality = 2, BonusForHigherQuality = 3, RejectLowerQuality = 4
    }

    public enum FarmOrderFailurePolicy
    {
        NoConsequence = 0, SmallRelationshipPenaltyFuture = 1, ExpireOnly = 2
    }

    public enum EconomyRiskTag
    {
        None = 0, MediumRisk = 1, HighRisk = 2, RequiresBalanceReview = 3
    }

    public class RequiredOrderItem
    {
        public string ItemId { get; set; }
        public int RequiredAmount { get; set; }
        public int? MinimumQuality { get; set; }
        public bool AcceptHigherQuality { get; set; } = true;
        public bool AcceptAnyQuality { get; set; } = false;
        public bool ConsumeOnDelivery { get; set; } = true;
        public bool ProtectFromShippingWhenTracked { get; set; } = false;
    }

    public class FarmOrderDeadlinePolicy
    {
        public FarmOrderDeadlinePolicyType PolicyType { get; set; }
        public int? DaysFromAccept { get; set; }
        public int? SpecificDay { get; set; }
    }

    public class FarmOrderRepeatPolicy
    {
        public FarmOrderRepeatPolicyType PolicyType { get; set; }
        public int MaxRepeatCount { get; set; } = 0;
        public string RepeatTableId { get; set; }
    }

    public class FarmOrderDefinition
    {
        public string FarmOrderId { get; set; }
        public string QuestId { get; set; }
        public string RequesterNpcId { get; set; }
        public string RequesterServiceId { get; set; }
        public FarmOrderType OrderType { get; set; }
        public List<RequiredOrderItem> RequiredItems { get; set; } = new List<RequiredOrderItem>();
        public QualityAcceptancePolicyType AcceptedQualityPolicy { get; set; }
        public string RequiredSeason { get; set; }
        public FarmOrderDeadlinePolicy DeadlinePolicy { get; set; } = new FarmOrderDeadlinePolicy();
        public FarmOrderRepeatPolicy RepeatPolicy { get; set; } = new FarmOrderRepeatPolicy();
        public string RewardTableId { get; set; }
        public int ReputationReward { get; set; } = 0;
        public FarmOrderFailurePolicy FailurePolicy { get; set; }
        public EconomyRiskTag EconomyRiskTag { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool CanExpire() => DeadlinePolicy != null &&
            DeadlinePolicy.PolicyType != FarmOrderDeadlinePolicyType.NoDeadline;

        public bool CanRepeat() => RepeatPolicy != null &&
            RepeatPolicy.PolicyType != FarmOrderRepeatPolicyType.Never;
    }
}
