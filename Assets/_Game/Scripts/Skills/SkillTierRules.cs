namespace CindarsHope.Skills
{
    // fable_29 — canonical tier gating + dynamic rank cap rules.
    // Source of truth: docs/game_rules/skill_tree_rules.md (Decision 1.1/1.2) and the
    // SKILL_NUMERIC_ADDENDUM §3.4. Pure static logic; no Unity dependency (EditMode-testable).
    //
    // Tier is unlocked by POINTS SPENT IN THE TREE (not global level):
    //   Tier 1 → 0 spent   Tier 2 → 5   Tier 3 → 11   Tier 4 → 18   Tier 5 → 26
    // The DEEPEST unlocked tier of a tree drives the DYNAMIC rank cap of EVERY node in it:
    //   T1 open → cap 2   T2 open → cap 3   T3 open → cap 4   T4/T5 open → cap 5
    public static class SkillTierRules
    {
        // Cumulative points-spent-in-tree thresholds, indexed by tier (1..5).
        // [0] unused. Tier 1 requires 0; Tier 5 requires 26.
        public static readonly int[] TierPointThresholds = { 0, 0, 5, 11, 18, 26 };

        public const int MinTier = 1;
        public const int MaxTier = 5;
        public const int AbsoluteRankCap = 5;

        // Highest tier whose threshold is satisfied by the given points spent in the tree.
        public static int DeepestUnlockedTier(int pointsSpentInTree)
        {
            int deepest = 1;
            for (int tier = MinTier; tier <= MaxTier; tier++)
            {
                if (pointsSpentInTree >= TierPointThresholds[tier])
                    deepest = tier;
            }
            return deepest;
        }

        // Is a node of the given tier purchasable given current points spent in its tree?
        public static bool IsTierUnlocked(int nodeTier, int pointsSpentInTree)
        {
            int t = Clamp(nodeTier);
            return pointsSpentInTree >= TierPointThresholds[t];
        }

        // Dynamic rank cap for ALL nodes in a tree, driven by the deepest unlocked tier.
        // Decision 1.1: T1→2, T2→3, T3→4, T4/T5→5.
        public static int DynamicRankCapForTree(int deepestUnlockedTier)
        {
            int t = Clamp(deepestUnlockedTier);
            switch (t)
            {
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                default: return 5; // tier 4 or 5
            }
        }

        // Convenience: dynamic rank cap given raw points spent in the tree.
        public static int RankCapForPointsInTree(int pointsSpentInTree)
            => DynamicRankCapForTree(DeepestUnlockedTier(pointsSpentInTree));

        private static int Clamp(int tier)
        {
            if (tier < MinTier) return MinTier;
            if (tier > MaxTier) return MaxTier;
            return tier;
        }
    }
}
