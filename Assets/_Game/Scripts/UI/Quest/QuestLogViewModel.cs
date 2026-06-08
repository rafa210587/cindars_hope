using System.Collections.Generic;

namespace CindarsHope.UI.Quest
{
    public class QuestLogEntryViewModel
    {
        public string QuestId { get; set; }
        public string DisplayName { get; set; }
        public string Status { get; set; }
        public int ObjectiveProgress { get; set; }
        public int ObjectiveTarget { get; set; }
        public bool IsMainProgression { get; set; }
        public bool IsHidden { get; set; }

        public float ProgressPercent => ObjectiveTarget > 0 ? (float)ObjectiveProgress / ObjectiveTarget : 0f;
        public bool IsComplete => ObjectiveProgress >= ObjectiveTarget;
    }

    public class QuestLogViewModel
    {
        public List<QuestLogEntryViewModel> ActiveQuests { get; set; } = new();
        public List<QuestLogEntryViewModel> CompletedQuests { get; set; } = new();
        public int SelectedQuestIndex { get; set; } = 0;

        public QuestLogEntryViewModel GetSelectedQuest()
        {
            if (SelectedQuestIndex >= 0 && SelectedQuestIndex < ActiveQuests.Count)
                return ActiveQuests[SelectedQuestIndex];
            return null;
        }
    }
}
