using System.Collections.Generic;
using CindarsHope.Quests.Conditions;

namespace CindarsHope.Quests.Triggers
{
    public class QuestTriggerDefinition
    {
        public string TriggerId { get; set; }
        public QuestTriggerType TriggerType { get; set; }
        public string EventName { get; set; }
        public string TargetId { get; set; }
        public string AmountSource { get; set; }
        public List<string> AppliesToObjectiveIds { get; set; } = new List<string>();
        public List<string> RequiredConditionIds { get; set; } = new List<string>();
        public QuestTriggerDeduplicationPolicy DeduplicationPolicy { get; set; } = QuestTriggerDeduplicationPolicy.ByObjectiveCompletion;
        public QuestRetroactivePolicy RetroactivePolicy { get; set; } = QuestRetroactivePolicy.NotAllowed;
        public List<string> DebugTags { get; set; } = new List<string>();
    }
}
