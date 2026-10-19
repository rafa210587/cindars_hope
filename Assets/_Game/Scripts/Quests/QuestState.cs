using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestState
    {
        public string QuestId { get; set; }
        // Canonical state (WAVE 09+)
        public QuestStateStatus StateStatus { get; set; } = QuestStateStatus.Unknown;
        // Legacy compat
        public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
        public Dictionary<string, int> ObjectiveProgress { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, bool> ConditionStates { get; set; } = new Dictionary<string, bool>();
        public List<string> CompletedObjectives { get; set; } = new List<string>();
        public List<string> CompletedStepIds { get; set; } = new List<string>();
        public string ActiveStepId { get; set; }
        public bool IsRewardGiven { get; set; }
        public int? StartDay { get; set; }
        public int? CompleteDay { get; set; }
        public List<string> GrantedFlagIds { get; set; } = new List<string>();

        public bool IsTerminal() => StateStatus == QuestStateStatus.Completed ||
                                     StateStatus == QuestStateStatus.Failed ||
                                     StateStatus == QuestStateStatus.Expired ||
                                     StateStatus == QuestStateStatus.HiddenCompleted;
    }

    // Legacy enum preserved for backward compat
    public enum QuestStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        Abandoned = 4
    }
}
