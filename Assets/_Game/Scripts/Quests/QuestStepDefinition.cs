using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestObjective
    {
        public string ObjectiveId { get; set; }
        public QuestObjectiveType ObjectiveType { get; set; }
        public string TargetId { get; set; }
        public int RequiredAmount { get; set; } = 1;
        public List<string> ConditionIds { get; set; } = new List<string>();
        public List<string> TriggerIds { get; set; } = new List<string>();
        public string VisibilityPolicyId { get; set; }
        public bool Optional { get; set; } = false;
        public string FailurePolicy { get; set; }
        public string HintTextKey { get; set; }
        public string MapMarkerPolicy { get; set; }

        public bool IsFutureObjective() =>
            ObjectiveType == QuestObjectiveType.FeedAnimalFuture ||
            ObjectiveType == QuestObjectiveType.PetInteractionFuture ||
            ObjectiveType == QuestObjectiveType.CompanionAssignedFuture ||
            ObjectiveType == QuestObjectiveType.WinFestivalActivityFuture;
    }

    public class QuestStepDefinition
    {
        public string StepId { get; set; }
        public string DisplayNameKey { get; set; }
        public string DescriptionKnownKey { get; set; }
        public string DescriptionHiddenKey { get; set; }
        public List<string> ObjectiveIds { get; set; } = new List<string>();
        public CompletionMode CompletionMode { get; set; } = CompletionMode.AllObjectivesRequired;
        public List<string> StartEventIds { get; set; } = new List<string>();
        public List<string> CompleteEventIds { get; set; } = new List<string>();
        public List<string> RewardIds { get; set; } = new List<string>();
        public List<string> NextStepRules { get; set; } = new List<string>();
        public List<string> BranchRules { get; set; } = new List<string>();
        public string VisibilityPolicyId { get; set; }
    }
}
