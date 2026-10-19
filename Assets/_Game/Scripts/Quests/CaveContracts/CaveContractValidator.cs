using System.Collections.Generic;

namespace CindarsHope.Quests.CaveContracts
{
    public class CaveContractValidationIssue
    {
        public string ContractId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class CaveContractValidator
    {
        public List<CaveContractValidationIssue> Validate(CaveContractDefinition contract)
        {
            var issues = new List<CaveContractValidationIssue>();
            if (contract == null) { issues.Add(B(null, "CONTRACT_NULL", "Null contract")); return issues; }

            if (string.IsNullOrEmpty(contract.CaveContractId))
                issues.Add(B(contract.CaveContractId, "CONTRACT_NO_ID", "CaveContractId is empty"));

            if (string.IsNullOrEmpty(contract.QuestId))
                issues.Add(B(contract.CaveContractId, "CONTRACT_NO_QUEST_ID", "QuestId is empty — CaveContract must reference a canonical quest"));

            // Defeat objectives must use stable enemy IDs
            if ((contract.ContractType == CaveContractType.DefeatEnemy ||
                 contract.ContractType == CaveContractType.DefeatEnemyFamily ||
                 contract.ContractType == CaveContractType.DefeatElite) &&
                !contract.UsesStableEnemyId())
                issues.Add(B(contract.CaveContractId, "CONTRACT_DEFEAT_NO_ENEMY_ID",
                    "Defeat contract must have RequiredEnemyId, RequiredEnemyFamilyId, or RequiredBossId"));

            // Boss/Elite require fallback policy
            if (contract.RequiresFallback())
                issues.Add(W(contract.CaveContractId, "CONTRACT_ELITE_NO_FALLBACK",
                    "Elite/Boss contract without fallback policy — rare spawn may softlock player"));

            // Depth contract must have a positive depth
            if (contract.ContractType == CaveContractType.ReachCaveDepth && contract.RequiredCaveAccessDepth <= 0)
                issues.Add(B(contract.CaveContractId, "CONTRACT_DEPTH_ZERO", "ReachCaveDepth contract must have RequiredCaveAccessDepth > 0"));

            // Resource contract needs a ResourceId
            if ((contract.ContractType == CaveContractType.CollectCaveResource) && string.IsNullOrEmpty(contract.RequiredResourceId))
                issues.Add(B(contract.CaveContractId, "CONTRACT_RESOURCE_NO_ID", "CollectCaveResource contract must have RequiredResourceId"));

            // Repeat: unique boss reward cannot repeat
            if (contract.RepeatPolicy.CanRepeat && !contract.RepeatPolicy.UniqueRewardOnFirstOnly &&
                (contract.ContractType == CaveContractType.DefeatBoss || contract.ContractType == CaveContractType.DefeatElite))
                issues.Add(W(contract.CaveContractId, "CONTRACT_BOSS_REPEAT_UNIQUE_REWARD",
                    "Boss/Elite repeat contract should set UniqueRewardOnFirstOnly = true to prevent infinite reward exploit"));

            if (contract.RequiredQuantity <= 0)
                issues.Add(B(contract.CaveContractId, "CONTRACT_ZERO_QUANTITY", "RequiredQuantity must be > 0"));

            return issues;
        }

        private CaveContractValidationIssue B(string id, string code, string msg) =>
            new CaveContractValidationIssue { ContractId = id, Code = code, Message = msg, IsBlocker = true };

        private CaveContractValidationIssue W(string id, string code, string msg) =>
            new CaveContractValidationIssue { ContractId = id, Code = code, Message = msg, IsBlocker = false };
    }
}
