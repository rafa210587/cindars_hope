using System.Collections.Generic;
using CindarsHope.Quests.Conditions;

namespace CindarsHope.Quests.Triggers
{
    public class TriggerRouteResult
    {
        public bool Matched { get; set; }
        public string TriggerId { get; set; }
        public List<string> MatchedObjectiveIds { get; set; } = new List<string>();
        public bool WasDeduplicated { get; set; }
        public bool ConditionsBlocked { get; set; }
        public ConditionEvaluationResult ConditionResult { get; set; }
        public string SkipReason { get; set; }

        public static TriggerRouteResult NoMatch(string reason) =>
            new TriggerRouteResult { Matched = false, SkipReason = reason };
    }

    public class QuestTriggerRouter
    {
        private readonly QuestConditionResolver _conditionResolver;
        // tracks: deduplication key → set of processed event ids per objective
        private readonly Dictionary<string, HashSet<string>> _seenDedupeKeys = new Dictionary<string, HashSet<string>>();

        public QuestTriggerRouter(QuestConditionResolver conditionResolver)
        {
            _conditionResolver = conditionResolver;
        }

        public TriggerRouteResult Route(
            QuestEventEnvelope evt,
            QuestTriggerDefinition trigger,
            List<QuestConditionDefinition> requiredConditions,
            QuestConditionContext ctx)
        {
            if (evt == null || trigger == null) return TriggerRouteResult.NoMatch("null input");

            // EventName must match
            if (trigger.EventName != evt.EventName)
                return TriggerRouteResult.NoMatch($"EventName mismatch: trigger={trigger.EventName} event={evt.EventName}");

            // TargetId must match if trigger specifies one
            if (!string.IsNullOrEmpty(trigger.TargetId) && trigger.TargetId != evt.TargetId)
                return TriggerRouteResult.NoMatch($"TargetId mismatch: trigger={trigger.TargetId} event={evt.TargetId}");

            // Deduplication check
            var dedupeKey = $"{trigger.TriggerId}:{evt.GetEffectiveDedupeKey()}";
            if (trigger.DeduplicationPolicy != QuestTriggerDeduplicationPolicy.ManualAllowRepeat)
            {
                if (!_seenDedupeKeys.ContainsKey(dedupeKey))
                    _seenDedupeKeys[dedupeKey] = new HashSet<string>();

                if (_seenDedupeKeys[dedupeKey].Contains(evt.EventId))
                    return new TriggerRouteResult { Matched = false, WasDeduplicated = true, TriggerId = trigger.TriggerId, SkipReason = "deduplicated" };

                _seenDedupeKeys[dedupeKey].Add(evt.EventId);
            }

            // Condition evaluation — side-effect free
            var condResult = _conditionResolver.EvaluateAll(requiredConditions, ctx);
            if (!condResult.Success)
                return new TriggerRouteResult
                {
                    Matched = false, ConditionsBlocked = true,
                    ConditionResult = condResult, TriggerId = trigger.TriggerId,
                    SkipReason = "conditions not met"
                };

            return new TriggerRouteResult
            {
                Matched = true,
                TriggerId = trigger.TriggerId,
                MatchedObjectiveIds = new List<string>(trigger.AppliesToObjectiveIds),
                ConditionResult = condResult
            };
        }

        public void ResetForNewRun()
        {
            _seenDedupeKeys.Clear();
        }
    }
}
