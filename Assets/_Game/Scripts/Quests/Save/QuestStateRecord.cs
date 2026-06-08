using System.Collections.Generic;

namespace CindarsHope.Quests.Save
{
    // Quest save DTO — no Unity refs, only simple types per save-dto-simple-types-only rule
    public class QuestObjectiveStateRecord
    {
        public string ObjectiveId { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsFailed { get; set; }
        public bool IsKnown { get; set; } = true;
    }

    public class QuestChoiceRecord
    {
        public string StepId { get; set; }
        public string ChoiceId { get; set; }
        public int ChosenAtDay { get; set; }
    }

    public class QuestStateRecord
    {
        public string QuestId { get; set; }
        public int State { get; set; } = 0; // serialized QuestStateStatus int
        public string CurrentStepId { get; set; }
        public List<string> CompletedStepIds { get; set; } = new List<string>();
        public List<string> FailedStepIds { get; set; } = new List<string>();
        public List<QuestObjectiveStateRecord> ObjectiveStates { get; set; } = new List<QuestObjectiveStateRecord>();
        public List<string> KnownObjectiveIds { get; set; } = new List<string>();
        public List<string> KnownHints { get; set; } = new List<string>();
        public int StartedAtDay { get; set; }
        public int StartedAtTime { get; set; }
        public int? CompletedAtDay { get; set; }
        public int? ExpiresAtDay { get; set; }
        public bool Tracked { get; set; } = false;
        public bool Discovered { get; set; } = false;
        public string FailureReason { get; set; }
        public List<QuestChoiceRecord> ChoiceHistory { get; set; } = new List<QuestChoiceRecord>();
        public List<string> GrantedRewardIds { get; set; } = new List<string>();
        public List<string> GrantedFlagIds { get; set; } = new List<string>();
        public string RepeatInstanceId { get; set; }
    }
}
