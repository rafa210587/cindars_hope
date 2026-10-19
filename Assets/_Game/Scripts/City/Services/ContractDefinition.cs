namespace CindarsHope.City.Services
{
    // Minimal contract adapter — links city service to quest/objective/reward system
    public class ContractDefinition
    {
        public string ContractId { get; set; }
        public string ProviderServiceId { get; set; }
        public string ContractType { get; set; }
        // Adapter ID into quest/objective system (WAVE 09 quest specs)
        public string ObjectiveDefinitionId { get; set; }
        public string RewardTableId { get; set; }
        public int? DeadlineDays { get; set; }
        public int? RequiredReputation { get; set; }
        public int? RequiredCaveProgress { get; set; }
        public int? RequiredFarmLevel { get; set; }
        public ContractRepeatPolicy RepeatPolicy { get; set; } = ContractRepeatPolicy.OneTime;

        public bool HasObjectiveAdapter() => !string.IsNullOrEmpty(ObjectiveDefinitionId);
        public bool HasRewardTable() => !string.IsNullOrEmpty(RewardTableId);
    }
}
