using System.Collections.Generic;
using CindarsHope.Quests;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Log
{
    public enum QuestVisibilityState
    {
        Hidden = 0, Discovered = 1, Known = 2, FullyKnown = 3, DebugOnly = 4
    }

    public enum WaitingReason
    {
        None = 0,
        WaitingForTime = 1, WaitingForWeather = 2, WaitingForLunarEvent = 3, WaitingForNPC = 4,
        WaitingForItem = 5, WaitingForCaveDepth = 6, WaitingForCraftingOrProcessing = 7,
        WaitingForFestival = 8, WaitingForStoryFlag = 9, HiddenWaitingReason = 10
    }

    public class QuestVisibilityPolicy
    {
        public string VisibilityPolicyId { get; set; }
        public QuestVisibilityState VisibilityState { get; set; }
        public int AllowedSpoilerTier { get; set; } = 0;
        public bool ShowTitleWhenUnknown { get; set; } = false;
        public bool ShowDescriptionWhenUnknown { get; set; } = false;
        public bool ShowFutureSteps { get; set; } = false;
        public bool ShowHiddenRewards { get; set; } = false;
        public bool ShowSecretTriggers { get; set; } = false;
        public bool ShowUnknownTemporalConditions { get; set; } = false;
        public bool ShowMapMarker { get; set; } = true;
        public List<string> RequiresKnownObjectiveIds { get; set; } = new List<string>();
        public List<string> RequiresKnownHintIds { get; set; } = new List<string>();
        public List<string> RequiresQuestFlags { get; set; } = new List<string>();
        public List<string> ForbiddenQuestFlags { get; set; } = new List<string>();

        public static QuestVisibilityPolicy Default() => new QuestVisibilityPolicy
        { VisibilityPolicyId = "default", VisibilityState = QuestVisibilityState.Known };
    }

    public class QuestObjectiveProjection
    {
        public string ObjectiveId { get; set; }
        public string DisplayText { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOptional { get; set; }
    }

    public class QuestRewardProjection
    {
        public string DisplaySummary { get; set; }
        public bool IsHidden { get; set; }
    }

    public class QuestLogEntryViewModel
    {
        public string QuestId { get; set; }
        public QuestCategory Category { get; set; }
        public string DisplayTitle { get; set; }
        public string DisplaySummary { get; set; }
        public string StateDisplay { get; set; }
        public int Priority { get; set; }
        public bool Tracked { get; set; }
        public bool CanTrack { get; set; }
        public QuestObjectiveProjection KnownCurrentObjective { get; set; }
        public string ProgressText { get; set; }
        public string KnownNpcId { get; set; }
        public string KnownLocationId { get; set; }
        public int? KnownDeadline { get; set; }
        public WaitingReason WaitingReasonValue { get; set; } = WaitingReason.None;
        public string KnownTemporalCondition { get; set; }
        public string KnownRewardSummary { get; set; }
        public bool HasHiddenRewards { get; set; }
        public bool SpoilerSafe { get; set; } = true;
        public int SortKey { get; set; }
    }

    public class QuestDetailViewModel
    {
        public string QuestId { get; set; }
        public string DisplayTitle { get; set; }
        public QuestCategory Category { get; set; }
        public string KnownSummary { get; set; }
        public List<QuestObjectiveProjection> CurrentObjectiveRows { get; set; } = new List<QuestObjectiveProjection>();
        public List<string> KnownHints { get; set; } = new List<string>();
        public string KnownNpc { get; set; }
        public string KnownLocation { get; set; }
        public int? Deadline { get; set; }
        public WaitingReason WaitingReasonValue { get; set; } = WaitingReason.None;
        public List<QuestRewardProjection> KnownRewards { get; set; } = new List<QuestRewardProjection>();
        public string HiddenRewardPlaceholder { get; set; }
        public List<string> CompletedStepHistory { get; set; } = new List<string>();
        public bool CanTrack { get; set; }
        public bool CanUntrack { get; set; }
        public List<string> DebugWarnings { get; set; } = new List<string>();
    }
}
