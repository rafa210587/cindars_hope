using System.Collections.Generic;

namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_28 — one authored line plus its optional gating condition. A null/empty condition (or a
    /// condition with no constrained axis) is always eligible and serves as the fallback line.
    /// </summary>
    [System.Serializable]
    public sealed class ConditionalDialogueLine
    {
        public string Text;
        public DialogueLineCondition Condition;

        public ConditionalDialogueLine() { }

        public ConditionalDialogueLine(string text, DialogueLineCondition condition = null)
        {
            Text = text;
            Condition = condition;
        }

        public int Specificity => Condition?.Specificity ?? 0;

        public bool IsEligible(DialogueConditionContext ctx) =>
            Condition == null || Condition.IsMet(ctx);
    }

    /// <summary>
    /// fable_28 — pure, deterministic selector for a node's conditional line pool. No Unity, no event
    /// bus, no Random: the daily pick is decided by a stable hash of (npcId|day), so the same NPC
    /// repeats the same line all day and may change it the next day. Among eligible lines, the most
    /// specific (most constrained axes) wins; ties break deterministically by the daily hash.
    ///
    /// The fallback contract (CA-3): callers pass a guaranteed non-null fallback line (the existing
    /// node text / RandomLinePool), so selection NEVER returns empty even when no condition matches.
    /// </summary>
    public static class DialogueLineSelector
    {
        // Stable FNV-1a 32-bit hash. Same algorithm as ForageStableHash / CaveLayoutStableHash,
        // re-declared in the NPC namespace so dialogue does not depend on Farm/Cave (project precedent).
        public const string Salt = "fable_28_dialogue_daily_v1";

        public static int StableHash(string value)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in value ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        /// <summary>
        /// Selects the line for <paramref name="npcId"/> on <paramref name="day"/> from
        /// <paramref name="pool"/>, given the world <paramref name="ctx"/>. Returns
        /// <paramref name="fallbackText"/> when the pool is null/empty or no line is eligible.
        /// Deterministic: pure function of (pool contents, ctx, npcId, day).
        /// </summary>
        public static string Select(
            IReadOnlyList<ConditionalDialogueLine> pool,
            DialogueConditionContext ctx,
            string npcId,
            int day,
            string fallbackText)
        {
            if (pool == null || pool.Count == 0)
            {
                return fallbackText;
            }

            // 1) Eligible lines only.
            List<ConditionalDialogueLine> eligible = null;
            int maxSpecificity = -1;
            foreach (var line in pool)
            {
                if (line == null || string.IsNullOrEmpty(line.Text)) continue;
                if (!line.IsEligible(ctx)) continue;
                (eligible ??= new List<ConditionalDialogueLine>()).Add(line);
                if (line.Specificity > maxSpecificity) maxSpecificity = line.Specificity;
            }

            if (eligible == null || eligible.Count == 0)
            {
                return fallbackText;
            }

            // 2) Keep only the most specific eligible lines (the most constrained line wins).
            //    Build the contender list in a stable order (input order) for deterministic tie-break.
            var contenders = new List<ConditionalDialogueLine>();
            foreach (var line in eligible)
            {
                if (line.Specificity == maxSpecificity) contenders.Add(line);
            }

            if (contenders.Count == 1)
            {
                return contenders[0].Text;
            }

            // 3) Deterministic daily tie-break: stable hash of (salt|npcId|day) picks the index.
            //    Same npcId+day => same line all day; the next day generally rotates the pick.
            uint h = (uint)StableHash($"{Salt}|{npcId}|{day}");
            int index = (int)(h % (uint)contenders.Count);
            return contenders[index].Text;
        }
    }
}
