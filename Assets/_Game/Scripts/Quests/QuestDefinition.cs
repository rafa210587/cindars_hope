using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestDefinition
    {
        public string QuestId { get; set; }
        public QuestCategory Category { get; set; } = QuestCategory.Side;
        public string DisplayNameKey { get; set; }
        public string HiddenDisplayNameKey { get; set; }
        public string DescriptionKey { get; set; }
        public int SpoilerTier { get; set; } = 0;
        public List<string> StartConditionIds { get; set; } = new List<string>();
        public List<string> AutoStartTriggerIds { get; set; } = new List<string>();
        public List<string> StepIds { get; set; } = new List<string>();
        public List<string> FailureRuleIds { get; set; } = new List<string>();
        public List<string> ExpiryRuleIds { get; set; } = new List<string>();
        public List<string> RewardIds { get; set; } = new List<string>();
        public List<string> QuestFlagGrantIds { get; set; } = new List<string>();
        public List<string> PrerequisiteQuestIds { get; set; } = new List<string>();
        public List<string> BlockedByQuestIds { get; set; } = new List<string>();
        public string RepeatPolicy { get; set; }
        public bool Trackable { get; set; } = true;
        public string JournalVisibilityPolicyId { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();
        // Legacy compat fields
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<ObjectiveDefinition> Objectives { get; set; } = new List<ObjectiveDefinition>();
        public List<ConditionDefinition> Conditions { get; set; } = new List<ConditionDefinition>();
        public bool IsMainProgression { get; set; }
        public bool IsHidden { get; set; }

        public bool CanExpire() => Category != QuestCategory.Main && ExpiryRuleIds.Count > 0;
    }

    public class ObjectiveDefinition
    {
        public string ObjectiveId { get; set; }
        public string Description { get; set; }
        public int TargetCount { get; set; } = 1;
    }

    public class ConditionDefinition
    {
        public string ConditionId { get; set; }
        public ConditionType Type { get; set; }
        public string TargetId { get; set; }
        public int TargetValue { get; set; }
    }

    public enum ConditionType
    {
        ItemCount = 0,
        DaysPassed = 1,
        SeasonReached = 2,
        LocationVisited = 3,
        NpcMet = 4,
        EventTriggered = 5
    }
}
