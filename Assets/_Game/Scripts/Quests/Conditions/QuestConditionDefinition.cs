using System.Collections.Generic;

namespace CindarsHope.Quests.Conditions
{
    public class QuestConditionDefinition
    {
        public string ConditionId { get; set; }
        public QuestConditionType ConditionType { get; set; }
        public string TargetId { get; set; }
        public QuestConditionOperator Operator { get; set; } = QuestConditionOperator.IsSet;
        public string ExpectedValue { get; set; }
        public int? Amount { get; set; }
        public string VisibilityPolicyId { get; set; }
        public string KnownFailureHintKey { get; set; }
        public bool IsCriticalPath { get; set; } = false;
        public string FallbackPolicyId { get; set; }
        public List<string> DebugTags { get; set; } = new List<string>();

        // Future conditions must not have runtime logic
        public bool IsFutureCondition() => ConditionType == QuestConditionType.SocialConditionFuture ||
                                            ConditionType == QuestConditionType.PetConditionFuture ||
                                            ConditionType == QuestConditionType.CompanionConditionFuture;
    }
}
