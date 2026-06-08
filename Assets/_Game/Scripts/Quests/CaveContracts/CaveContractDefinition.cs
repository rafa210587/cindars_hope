using System.Collections.Generic;

namespace CindarsHope.Quests.CaveContracts
{
    public enum CaveContractType
    {
        DefeatEnemy = 0, DefeatEnemyFamily = 1, DefeatElite = 2, DefeatBoss = 3,
        CollectCaveResource = 4, ReachCaveDepth = 5, CompleteCaveRun = 6,
        DiscoverWeakness = 7, MapArea = 8, RecoverCorpse = 9, InteractWithCaveObject = 10
    }

    public enum CaveContractRiskTier
    {
        Low = 0, Medium = 1, High = 2, Critical = 3
    }

    public enum CaveContractFallbackPolicy
    {
        None = 0, GuaranteedSpawn = 1, AlternativeEnemy = 2, ExtendDeadline = 3, CancelWithRefund = 4
    }

    public class CaveContractDeadlinePolicy
    {
        public bool HasDeadline { get; set; }
        public int? DaysFromAccept { get; set; }
        public int? SpecificDay { get; set; }
    }

    public class CaveContractRepeatPolicy
    {
        public bool CanRepeat { get; set; }
        public int MaxRepeatCount { get; set; } = 0;
        public bool UniqueRewardOnFirstOnly { get; set; } = true;
    }

    public class CaveContractDefinition
    {
        public string CaveContractId { get; set; }
        public string QuestId { get; set; }
        public string ProviderServiceId { get; set; }
        public CaveContractType ContractType { get; set; }
        public int RequiredCaveAccessDepth { get; set; } = 0;
        public (int Min, int Max) AllowedDepthRange { get; set; }
        public List<string> AllowedBiomeIds { get; set; } = new List<string>();
        public string RequiredEnemyId { get; set; }
        public string RequiredEnemyFamilyId { get; set; }
        public string RequiredBossId { get; set; }
        public string RequiredResourceId { get; set; }
        public string RequiredKnowledgeId { get; set; }
        public int RequiredQuantity { get; set; } = 1;
        public CaveContractDeadlinePolicy DeadlinePolicy { get; set; } = new CaveContractDeadlinePolicy();
        public CaveContractRiskTier RiskTier { get; set; }
        public string RewardTableId { get; set; }
        public CaveContractRepeatPolicy RepeatPolicy { get; set; } = new CaveContractRepeatPolicy();
        public CaveContractFallbackPolicy FallbackPolicy { get; set; } = CaveContractFallbackPolicy.None;
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool UsesStableEnemyId() =>
            !string.IsNullOrEmpty(RequiredEnemyId) || !string.IsNullOrEmpty(RequiredEnemyFamilyId) ||
            !string.IsNullOrEmpty(RequiredBossId);

        public bool RequiresFallback() =>
            (ContractType == CaveContractType.DefeatElite || ContractType == CaveContractType.DefeatBoss)
            && FallbackPolicy == CaveContractFallbackPolicy.None;
    }
}
