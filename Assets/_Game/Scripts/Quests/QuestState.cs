using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestState
    {
        public string QuestId { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
        public Dictionary<string, int> ObjectiveProgress { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, bool> ConditionStates { get; set; } = new Dictionary<string, bool>();
        public List<string> CompletedObjectives { get; set; } = new List<string>();
        public bool IsRewardGiven { get; set; }
    }

    public enum QuestStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        Abandoned = 4
    }
}
