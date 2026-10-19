using System.Collections.Generic;

namespace CindarsHope.Loot
{
    public class RewardTableDefinition
    {
        public string RewardTableId { get; set; }
        public LootSourceType SourceType { get; set; } = LootSourceType.QuestRewards;
        public List<LootEntry> GuaranteedRewards { get; set; } = new List<LootEntry>();
        public List<LootEntry> ChoiceRewards { get; set; } = new List<LootEntry>();
        public List<string> GrantedFlags { get; set; } = new List<string>();
        public List<string> GrantedRecipes { get; set; } = new List<string>();
        public GoldRange GoldRange { get; set; } = new GoldRange();
        public FirstTimeBonus FirstTimeBonus { get; set; }
        public RepeatFarmRules RepeatRules { get; set; } = new RepeatFarmRules();
        public bool IsOneTimeOnly { get; set; } = false;
    }

    public class RewardGrantResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<string> GrantedItemIds { get; set; } = new List<string>();
        public List<int> GrantedQuantities { get; set; } = new List<int>();
        public int GrantedGold { get; set; }
        public List<string> GrantedFlags { get; set; } = new List<string>();
        public List<string> GrantedRecipes { get; set; } = new List<string>();
        public int? GrantedReputation { get; set; }
        public bool FirstTimeRewardConsumed { get; set; }
        public bool RepeatRewardUsed { get; set; }
        public List<string> DebugRolls { get; set; } = new List<string>();

        public static RewardGrantResult Fail(string reason) =>
            new RewardGrantResult { Success = false, FailureReason = reason };

        public static RewardGrantResult Ok() =>
            new RewardGrantResult { Success = true };
    }
}
