namespace CindarsHope.City.FarmVisits
{
    public class FarmVisitContext
    {
        public string NpcId { get; set; }
        public int CurrentDay { get; set; }
        public int LastVisitDay { get; set; } = -1;
        public int RelationshipValue { get; set; }
        public int CityReputation { get; set; }
        public string ActiveQuestFlag { get; set; }
        public string ServiceContractId { get; set; }
        public string MarriageState { get; set; }
        public string CurrentFestival { get; set; }
        public City.Schedule.SchedulePeriod CurrentPeriod { get; set; }
        public string CurrentSeason { get; set; }
        public string CurrentLunarState { get; set; }
        public int CaveProgress { get; set; }
        public int FarmLevel { get; set; }
        public bool IsNpcAvailable { get; set; } = true;
        public bool IsConflictingFestivalActive { get; set; } = false;
    }

    public class FarmVisitEligibilityResult
    {
        public bool Eligible { get; set; }
        public string BlockReason { get; set; }
    }

    public class FarmVisitEligibilityResolver
    {
        public FarmVisitEligibilityResult IsEligible(FarmVisitRule rule, FarmVisitContext ctx)
        {
            if (rule == null || ctx == null)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "null input" };

            // Pet visits are deferred
            if (rule.IsPetRelated)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Pet-related visit deferred" };

            // NPC must be available (not in conflicting quest/festival)
            if (!ctx.IsNpcAvailable)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "NPC unavailable" };

            if (ctx.IsConflictingFestivalActive)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Conflicting festival active" };

            // Cooldown check
            if (ctx.LastVisitDay >= 0 && ctx.CurrentDay - ctx.LastVisitDay < rule.CooldownDays)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = $"Cooldown: {rule.CooldownDays - (ctx.CurrentDay - ctx.LastVisitDay)} days remaining" };

            // Relationship minimum
            if (ctx.RelationshipValue < rule.RequiredRelationshipMin)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = $"Relationship {ctx.RelationshipValue} < required {rule.RequiredRelationshipMin}" };

            // Reputation minimum
            if (rule.RequiredCityReputation.HasValue && ctx.CityReputation < rule.RequiredCityReputation.Value)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Insufficient city reputation" };

            // Quest flag
            if (!string.IsNullOrEmpty(rule.RequiredQuestFlag) && ctx.ActiveQuestFlag != rule.RequiredQuestFlag)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = $"Quest flag '{rule.RequiredQuestFlag}' not active" };

            // Service contract
            if (!string.IsNullOrEmpty(rule.RequiredServiceContract) && ctx.ServiceContractId != rule.RequiredServiceContract)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Service contract not active" };

            // Season
            if (!string.IsNullOrEmpty(rule.RequiredSeason) && ctx.CurrentSeason != rule.RequiredSeason)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = $"Wrong season: need {rule.RequiredSeason}" };

            // Lunar state
            if (!string.IsNullOrEmpty(rule.RequiredLunarState) && ctx.CurrentLunarState != rule.RequiredLunarState)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Lunar state mismatch" };

            // Cave progress
            if (rule.RequiredCaveProgressMin.HasValue && ctx.CaveProgress < rule.RequiredCaveProgressMin.Value)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Insufficient cave progress" };

            // Farm level
            if (rule.RequiredFarmLevelMin.HasValue && ctx.FarmLevel < rule.RequiredFarmLevelMin.Value)
                return new FarmVisitEligibilityResult { Eligible = false, BlockReason = "Farm level too low" };

            return new FarmVisitEligibilityResult { Eligible = true };
        }
    }
}
