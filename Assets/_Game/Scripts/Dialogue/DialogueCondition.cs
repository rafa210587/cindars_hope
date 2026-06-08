using System.Collections.Generic;
using CindarsHope.City.Schedule;

namespace CindarsHope.Dialogue
{
    public enum SpoilerLevel { None = 0, Minor, Moderate, Major, LoreCritical }
    public enum RepeatPolicy { AlwaysRepeat = 0, OncePerDay, OncePerSeason, OnceOnly, CooldownDays }

    public class DialogueCondition
    {
        public string ConditionId { get; set; }
        public int? RequiredRelationshipMin { get; set; }
        public int? RequiredReputationMin { get; set; }
        public List<string> RequiredQuestFlags { get; set; } = new List<string>();
        public List<string> ForbiddenQuestFlags { get; set; } = new List<string>();
        public List<string> RequiredStoryFlags { get; set; } = new List<string>();
        public List<string> ForbiddenStoryFlags { get; set; } = new List<string>();
        public string RequiredSeason { get; set; }
        public string RequiredWeather { get; set; }
        public string RequiredLunarState { get; set; }
        public SchedulePeriod? RequiredSchedulePeriod { get; set; }
        public string RequiredLocationId { get; set; }
        public int? RequiredCaveProgressMin { get; set; }
        public int? RequiredFarmLevelMin { get; set; }
        public SpoilerLevel SpoilerLevel { get; set; } = SpoilerLevel.None;
        public int Priority { get; set; } = 0;
        public RepeatPolicy RepeatPolicy { get; set; } = RepeatPolicy.AlwaysRepeat;
        public int CooldownDays { get; set; } = 0;

        public bool IsMet(DialogueContext ctx)
        {
            if (ctx == null) return false;
            if (RequiredRelationshipMin.HasValue && ctx.RelationshipValue < RequiredRelationshipMin.Value) return false;
            if (RequiredReputationMin.HasValue && ctx.CityReputation < RequiredReputationMin.Value) return false;
            foreach (var flag in RequiredQuestFlags)
                if (!ctx.ActiveQuestIds.Contains(flag) && !ctx.CompletedQuestIds.Contains(flag)) return false;
            foreach (var flag in ForbiddenQuestFlags)
                if (ctx.ActiveQuestIds.Contains(flag) || ctx.CompletedQuestIds.Contains(flag)) return false;
            foreach (var flag in RequiredStoryFlags)
                if (!ctx.StoryFlags.Contains(flag)) return false;
            foreach (var flag in ForbiddenStoryFlags)
                if (ctx.StoryFlags.Contains(flag)) return false;
            if (RequiredSeason != null && ctx.Season != RequiredSeason) return false;
            if (RequiredWeather != null && ctx.Weather != RequiredWeather) return false;
            if (RequiredLunarState != null && ctx.LunarState != RequiredLunarState) return false;
            if (RequiredSchedulePeriod.HasValue && ctx.CurrentPeriod != RequiredSchedulePeriod.Value) return false;
            if (RequiredLocationId != null && ctx.LocationId != RequiredLocationId) return false;
            if (RequiredCaveProgressMin.HasValue && ctx.CaveProgress < RequiredCaveProgressMin.Value) return false;
            if (RequiredFarmLevelMin.HasValue && ctx.FarmLevel < RequiredFarmLevelMin.Value) return false;
            return true;
        }
    }
}
