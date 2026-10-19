using CindarsHope.City.Schedule;

namespace CindarsHope.City.FarmVisits
{
    public enum FarmVisitType { Social = 0, Service, Gift, Warning, QuestRelated, CompanionRelated, Spouse, Future }

    public class FarmVisitRule
    {
        public string FarmVisitRuleId { get; set; }
        public string NpcId { get; set; }
        public int RequiredRelationshipMin { get; set; } = 0;
        public int? RequiredCityReputation { get; set; }
        public string RequiredQuestFlag { get; set; }
        public string RequiredServiceContract { get; set; }
        public string RequiredMarriageState { get; set; }
        public string RequiredFestival { get; set; }
        public SchedulePeriod? RequiredSchedulePeriod { get; set; }
        public string RequiredSeason { get; set; }
        public string RequiredLunarState { get; set; }
        public int? RequiredCaveProgressMin { get; set; }
        public int? RequiredFarmLevelMin { get; set; }
        public FarmVisitType VisitType { get; set; } = FarmVisitType.Social;
        public int CooldownDays { get; set; } = 7;
        public string DialogueSetId { get; set; }
        public string RewardOrGiftPolicyId { get; set; }
        // Pet-related visits are deferred
        public bool IsPetRelated { get; set; } = false;
    }
}
