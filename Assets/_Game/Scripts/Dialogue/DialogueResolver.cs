using System.Collections.Generic;
using System.Linq;

namespace CindarsHope.Dialogue
{
    public class DialogueResolveResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string SelectedLineId { get; set; }
        public string SelectedTextKey { get; set; }
        public SpoilerLevel AppliedSpoilerLevel { get; set; }
        public bool WasContextual { get; set; }

        public static DialogueResolveResult Fail(string reason) =>
            new DialogueResolveResult { Success = false, FailureReason = reason };
    }

    public class DialogueResolver
    {
        // Returns the highest-priority contextual line whose condition is met, or falls back to default
        public DialogueResolveResult Resolve(DialogueSetDefinition set, DialogueContext ctx)
        {
            if (set == null) return DialogueResolveResult.Fail("set is null");
            if (ctx == null) return DialogueResolveResult.Fail("context is null");

            // Filter contextual lines whose condition is met, sort by priority desc
            var candidates = set.ContextualLines
                .Where(l => l.Condition == null || l.Condition.IsMet(ctx))
                .OrderByDescending(l => l.Condition?.Priority ?? 0)
                .ToList();

            if (candidates.Count > 0)
            {
                var selected = candidates[0];
                return new DialogueResolveResult
                {
                    Success = true,
                    SelectedLineId = selected.LineId,
                    SelectedTextKey = selected.TextKey,
                    AppliedSpoilerLevel = selected.Condition?.SpoilerLevel ?? SpoilerLevel.None,
                    WasContextual = true
                };
            }

            // Fallback to default
            if (set.DefaultLineTextKeys.Count > 0)
            {
                return new DialogueResolveResult
                {
                    Success = true,
                    SelectedTextKey = set.DefaultLineTextKeys[0],
                    WasContextual = false
                };
            }

            return DialogueResolveResult.Fail($"No lines available for NPC '{set.NpcId}'");
        }
    }
}
