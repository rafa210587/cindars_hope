using System.Collections.Generic;

namespace CindarsHope.Quests.Festivals
{
    public enum FestivalExpiryPolicyType
    {
        ExpireAtFestivalEnd = 0, ExpireAtDayEnd = 1, ExpireAtSpecificTime = 2,
        RemainAvailableForTurnIn = 3, NeverExpireStoryOnly = 4
    }

    public class FestivalQuestDefinition
    {
        public string FestivalQuestId { get; set; }
        public string QuestId { get; set; }
        public string FestivalId { get; set; }
        public int? KnownStartDay { get; set; }
        public int? KnownStartTime { get; set; }
        public int? KnownEndDay { get; set; }
        public int? KnownEndTime { get; set; }
        public List<string> ObjectiveIds { get; set; } = new List<string>();
        public string RewardTableId { get; set; }
        public FestivalExpiryPolicyType ExpiryPolicy { get; set; }
        public string VisibilityPolicyId { get; set; }
        public bool CanBeCompletedAfterFestival { get; set; } = false;
        public bool AffectsEssentialFarmRoutine { get; set; } = false;
        public bool WarningRequired { get; set; } = false;
        public List<string> DebugTags { get; set; } = new List<string>();

        public bool CanExpire() => ExpiryPolicy == FestivalExpiryPolicyType.ExpireAtFestivalEnd
            || ExpiryPolicy == FestivalExpiryPolicyType.ExpireAtDayEnd
            || ExpiryPolicy == FestivalExpiryPolicyType.ExpireAtSpecificTime;

        public bool HasKnownTiming() => KnownStartDay.HasValue || KnownEndDay.HasValue;
    }

    // Pure C# evaluation helper — no Unity refs, no scene state
    public class FestivalQuestAdapter
    {
        // Returns true if the quest should expire given current state
        public bool IsExpired(FestivalQuestDefinition def, int currentDay, int? festivalEndDay, bool festivalEnded)
        {
            if (def == null) return false;

            switch (def.ExpiryPolicy)
            {
                case FestivalExpiryPolicyType.ExpireAtFestivalEnd:
                    return festivalEnded;

                case FestivalExpiryPolicyType.ExpireAtDayEnd:
                    return festivalEndDay.HasValue && currentDay > festivalEndDay.Value;

                case FestivalExpiryPolicyType.ExpireAtSpecificTime:
                    return festivalEndDay.HasValue && currentDay > festivalEndDay.Value;

                case FestivalExpiryPolicyType.RemainAvailableForTurnIn:
                    return false;

                case FestivalExpiryPolicyType.NeverExpireStoryOnly:
                    return false;

                default:
                    return false;
            }
        }

        // Project known festival timing for Quest Log display
        public (int? StartDay, int? EndDay) GetKnownTiming(FestivalQuestDefinition def)
        {
            if (def == null) return (null, null);
            return (def.KnownStartDay, def.KnownEndDay);
        }
    }
}
