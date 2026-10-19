using System.Collections.Generic;

namespace CindarsHope.Quests.Conditions
{
    public class ConditionEvaluationResult
    {
        public bool Success { get; set; }
        public List<string> FailedConditionIds { get; set; } = new List<string>();
        public List<string> KnownFailureReasons { get; set; } = new List<string>();
        public List<string> HiddenFailureReasons { get; set; } = new List<string>();
        public bool CanShowInQuestLog { get; set; } = true;
        public bool CanRetry { get; set; } = true;
        public string SuggestedFallbackId { get; set; }

        public static ConditionEvaluationResult Pass() => new ConditionEvaluationResult { Success = true };

        public static ConditionEvaluationResult Fail(string conditionId, string reason, bool isHidden = false)
        {
            var result = new ConditionEvaluationResult { Success = false };
            result.FailedConditionIds.Add(conditionId);
            if (isHidden) result.HiddenFailureReasons.Add(reason);
            else result.KnownFailureReasons.Add(reason);
            result.CanShowInQuestLog = !isHidden;
            return result;
        }
    }
}
