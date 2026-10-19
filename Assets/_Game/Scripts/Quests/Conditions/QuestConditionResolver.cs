using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Quests.Conditions
{
    // Pure C# resolver — no MonoBehaviour, no Unity deps (Debug.Log is the one static exception
    // for one-shot dev logging, per skill observability-and-logging). Side-effect free otherwise.
    public class QuestConditionResolver
    {
        // One-shot dev guard: log at most once per future ConditionType per process,
        // so a repeated Evaluate() call (same quest re-checked every frame/event) does not spam.
        private static readonly HashSet<QuestConditionType> s_loggedFutureConditionTypes = new HashSet<QuestConditionType>();

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

            // Future conditions are always deferred — cannot block main quest on unimplemented systems.
            // This is the correct skip mechanism for genuinely-future condition types (Social/Pet/Companion);
            // see docs/game_rules/quest_rules.md Rule 4 for the closure criterion of each.
            if (cond.IsFutureCondition())
            {
                if (s_loggedFutureConditionTypes.Add(cond.ConditionType))
                {
                    Debug.Log($"[Quest] QuestConditionResolver: ConditionType '{cond.ConditionType}' is a future " +
                        $"condition (no runtime implementation yet) — evaluated as Pass so it never blocks a quest. " +
                        $"See docs/game_rules/quest_rules.md Rule 4 for closure criterion. (one-shot log per type)");
                }
                return ConditionEvaluationResult.Pass();
            }

            // CombatCondition is a real condition type (not future) but has no runtime data source
            // wired to QuestConditionContext today (no context builder populates PlayerCurrentHp/
            // PlayerMaxHp yet — see class doc). Evaluate honestly instead of returning an
            // unconditional Pass: real result when data is present, explicit failure when absent.
            if (cond.ConditionType == QuestConditionType.CombatCondition)
                return EvaluateCombat(cond, ctx);

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

        // CombatCondition compares the player's current HP percentage against ExpectedValue (0-100)
        // using Operator (GreaterThanOrEqual / LessThanOrEqual). Returns an explicit failure — never
        // an implicit Pass — when the context has no combat snapshot (PlayerCurrentHp/PlayerMaxHp null).
        private ConditionEvaluationResult EvaluateCombat(QuestConditionDefinition c, QuestConditionContext ctx)
        {
            if (ctx.PlayerCurrentHp == null || ctx.PlayerMaxHp == null || ctx.PlayerMaxHp <= 0)
            {
                return ConditionEvaluationResult.Fail(c.ConditionId,
                    c.KnownFailureHintKey ?? "Combat snapshot indisponivel: HP do player nao foi fornecido ao QuestConditionContext");
            }

            if (!int.TryParse(c.ExpectedValue, out var expectedPercent))
                return ConditionEvaluationResult.Fail(c.ConditionId, c.KnownFailureHintKey ?? $"Condition '{c.ConditionId}' has an invalid ExpectedValue for CombatCondition");

            var currentPercent = (int)(100f * ctx.PlayerCurrentHp.Value / ctx.PlayerMaxHp.Value);
            var met = c.Operator == QuestConditionOperator.LessThanOrEqual
                ? currentPercent <= expectedPercent
                : currentPercent >= expectedPercent;

            if (met) return ConditionEvaluationResult.Pass();

            bool isHidden = c.VisibilityPolicyId == "hidden" || !string.IsNullOrEmpty(c.KnownFailureHintKey) == false;
            var result = ConditionEvaluationResult.Fail(c.ConditionId, c.KnownFailureHintKey ?? $"Condition '{c.ConditionId}' not met", isHidden);
            if (!string.IsNullOrEmpty(c.FallbackPolicyId))
                result.SuggestedFallbackId = c.FallbackPolicyId;
            return result;
        }

        private bool EvaluateBestiary(QuestConditionDefinition c, QuestConditionContext ctx) =>
            ctx.BestiaryKnownIds.Contains(c.TargetId);
    }
}
