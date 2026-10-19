using System;
using System.Collections.Generic;

namespace CindarsHope.Companions
{
    [Serializable]
    public class CompanionAvailabilityState
    {
        public string CompanionId;
        public bool AvailableNow;
        public string UnavailableReason;
        public List<string> AllowedContexts = new List<string>();
        public string CurrentScheduleBlock;
        public InjuryState InjuryState = InjuryState.Healthy;
        public bool StoryLocked;
        public bool QuestConflict;
    }

    public enum AvailabilityReason
    {
        Available,
        LockedByStory,
        LockedByReputation,
        LockedByQuest,
        UnavailableBySchedule,
        UnavailableByInjury,
        UnavailableByFatigue,
        UnavailableByQuestConflict,
        UnavailableByStoryEvent,
        UnavailableByContext,
        Unavailable
    }
}
