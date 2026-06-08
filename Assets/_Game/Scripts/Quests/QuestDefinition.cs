using System.Collections.Generic;

namespace CindarsHope.Quests
{
    public class QuestDefinition
    {
        public string QuestId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<ObjectiveDefinition> Objectives { get; set; } = new List<ObjectiveDefinition>();
        public List<ConditionDefinition> Conditions { get; set; } = new List<ConditionDefinition>();
        public bool IsMainProgression { get; set; }
        public bool IsHidden { get; set; }
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
