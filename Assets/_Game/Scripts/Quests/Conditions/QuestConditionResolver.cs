using System.Collections.Generic;

namespace CindarsHope.Quests.Conditions
{
    // Pure C# resolver — no MonoBehaviour, no Unity deps. Side-effect free.
    public class QuestConditionResolver
    {
        public ConditionEvaluationResult EvaluateAll(List<QuestConditionDefinition> conditions, QuestConditionContext ctx)
        {
            if (conditions == null || conditions.Count == 0) return ConditionEvaluationResult.Pass();
            var combined = new ConditionEvaluationResult { Success = true };
            foreach (var c in conditions)
            {
                var r = Evaluate(c, ctx);
                if (!r.Success)
                {
                    combined.Success = false;
                    combined.FailedConditionIds.AddRange(r.FailedConditionIds);
                    combined.KnownFailureReasons.AddRange(r.KnownFailureReasons);
                    combined.HiddenFailureReasons.AddRange(r.HiddenFailureReasons);
                    if (!r.CanShowInQuestLog) combined.CanShowInQuestLog = false;
                    if (!r.CanRetry) combined.CanRetry = false;
                    if (r.SuggestedFallbackId != null) combined.SuggestedFallbackId = r.SuggestedFallbackId;
                }
            }
            return combined;
        }

        public ConditionEvaluationResult Evaluate(QuestConditionDefinition cond, QuestConditionContext ctx)
        {
            if (cond == null) return ConditionEvaluationResult.Fail("null", "Condition is null");
            if (ctx == null) return ConditionEvaluationResult.Fail(cond.ConditionId, "Context is null");

            // Future conditions are always deferred — cannot block main quest on unimplemented systems
            if (cond.IsFutureCondition())
                return ConditionEvaluationResult.Pass();

            bool met = cond.ConditionType switch
            {
                QuestConditionType.QuestCondition => EvaluateQuestFlag(cond, ctx),
                QuestConditionType.PlayerCondition => EvaluatePlayerReputation(cond, ctx),
                QuestConditionType.InventoryCondition => EvaluateInventory(cond, ctx),
                QuestConditionType.WorldCondition => EvaluateWorld(cond, ctx),
                QuestConditionType.TimeCondition => EvaluateTime(cond, ctx),
                QuestConditionType.WeatherCondition => EvaluateWeather(cond, ctx),
                QuestConditionType.LunarCondition => EvaluateLunar(cond, ctx),
                QuestConditionType.NpcCondition => EvaluateNpc(cond, ctx),
                QuestConditionType.DialogueCondition => EvaluateDialogue(cond, ctx),
                QuestConditionType.FarmCondition => EvaluateFarm(cond, ctx),
                QuestConditionType.CaveCondition => EvaluateCave(cond, ctx),
                QuestConditionType.CombatCondition => true, // deferred, no combat snapshot yet
                QuestConditionType.FonteCondition => EvaluateQuestFlag(cond, ctx), // Fonte via flags only
                QuestConditionType.BestiaryCondition => EvaluateBestiary(cond, ctx),
                _ => false
            };

            if (met) return ConditionEvaluationResult.Pass();

            bool isHidden = cond.VisibilityPolicyId == "hidden" || !string.IsNullOrEmpty(cond.KnownFailureHintKey) == false;
            var result = ConditionEvaluationResult.Fail(cond.ConditionId, cond.KnownFailureHintKey ?? $"Condition '{cond.ConditionId}' not met", isHidden);
            if (!string.IsNullOrEmpty(cond.FallbackPolicyId))
                result.SuggestedFallbackId = cond.FallbackPolicyId;
            return result;
        }

        private bool EvaluateQuestFlag(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (c.Operator == QuestConditionOperator.IsNotSet)
                return !ctx.ActiveFlags.ContainsKey(c.TargetId);
            return ctx.ActiveFlags.ContainsKey(c.TargetId);
        }

        private bool EvaluatePlayerReputation(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (!int.TryParse(c.ExpectedValue, out var req)) return false;
            return c.Operator == QuestConditionOperator.GreaterThanOrEqual
                ? ctx.PlayerReputation >= req
                : ctx.PlayerReputation <= req;
        }

        private bool EvaluateInventory(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            ctx.ItemCounts.TryGetValue(c.TargetId, out var count);
            var needed = c.Amount ?? 1;
            return count >= needed;
        }

        private bool EvaluateWorld(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.VisitedLocations.Contains(c.TargetId);

        private bool EvaluateTime(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (!int.TryParse(c.ExpectedValue, out var req)) return true;
            return c.Operator == QuestConditionOperator.GreaterThanOrEqual ? ctx.CurrentDay >= req : ctx.CurrentDay <= req;
        }

        private bool EvaluateWeather(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.CurrentWeather == c.ExpectedValue;

        private bool EvaluateLunar(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.CurrentLunarPhase == c.ExpectedValue;

        private bool EvaluateNpc(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.MetNpcIds.Contains(c.TargetId);

        private bool EvaluateDialogue(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.KnownDialogueIds.Contains(c.TargetId);

        private bool EvaluateFarm(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (!int.TryParse(c.ExpectedValue, out var req)) return true;
            return ctx.FarmLevel >= req;
        }

        private bool EvaluateCave(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (!int.TryParse(c.ExpectedValue, out var req)) return true;
            return ctx.CaveProgress >= req;
        }

        private bool EvaluateBestiary(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.BestiaryKnownIds.Contains(c.TargetId);
    }
}
