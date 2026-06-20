using System.Collections.Generic;
using CindarsHope.Skills;

namespace CindarsHope.Player
{
    /// <summary>
    /// fable_39 — INFERRED player class (decision Q9.1). The player NEVER chooses a class:
    /// a display-only title plus a tiny identity bonus is DERIVED, read-only and deterministic,
    /// from how skill points are distributed across the five trees that
    /// <see cref="SkillTreeManager"/> already persists.
    ///
    /// This is a PURE function (static, no Unity reference, no GameObject.Find/FindObjectOfType,
    /// no GameEventBus, no saved state). The runtime recompute hook
    /// (<see cref="InferredClassRuntime"/>) feeds it the current per-tree point counts and exposes
    /// the result; this class owns only the rules and the title/bonus tables.
    ///
    /// Rules (spec CA-1..CA-4 / player_rules.md Rule 8):
    ///   - dominant = the tree with the most points, ONLY IF it has >= 6 points AND >= 40% of total;
    ///   - hybrid if the second tree has >= 70% of the first's points -> composite title + HALF of each bonus;
    ///   - otherwise "Colono" (no dominant tree, NO bonus), including ties.
    /// Micro-bonuses are identity, not power — never above 5%.
    /// </summary>
    public static class PlayerClassInference
    {
        // ── Thresholds (spec) ────────────────────────────────────────────────────
        public const int MinDominantPoints = 6;
        public const float MinDominantShare = 0.40f;   // >= 40% of total spent
        public const float HybridSecondaryRatio = 0.70f; // 2nd >= 70% of 1st => hybrid

        // ── Identity micro-bonus magnitudes (NEVER above 5%) ─────────────────────
        // Warrior +3% posture dealt, Hunter +3% crit chance, Mystic -3% mana cost,
        // Farmer +5% stamina outside the cave, Bond +3% friendship gained.
        // Hybrids get HALF of each (dominant + secondary).
        public const float WarriorPostureBonus = 0.03f;
        public const float HunterCritBonus = 0.03f;
        public const float MysticManaCostReduction = 0.03f;
        public const float FarmerStaminaOutOfCaveBonus = 0.05f;
        public const float BondFriendshipBonus = 0.03f;

        /// <summary>The identity axis a tree's micro-bonus acts on (one per pure tree).</summary>
        public enum BonusAxis
        {
            None = 0,
            PostureDealt,        // Warrior  (Melee)
            CritChance,          // Hunter   (Ranged)
            ManaCostReduction,   // Mystic   (Magic)
            StaminaOutOfCave,    // Farmer   (Survival)
            FriendshipGained     // Bond     (Crafting)
        }

        /// <summary>Localization id for the "no class" title (Colono).</summary>
        public const string ColonoTitleId = "ui.class.colono";

        /// <summary>
        /// One identity bonus entry: which axis and how much (already halved for hybrids).
        /// Magnitude is always &lt;= 5% by construction (table values capped, hybrid halves).
        /// </summary>
        public readonly struct ClassBonus
        {
            public readonly BonusAxis Axis;
            public readonly float Value;

            public ClassBonus(BonusAxis axis, float value)
            {
                Axis = axis;
                Value = value;
            }

            public bool IsEmpty => Axis == BonusAxis.None || Value == 0f;
        }

        /// <summary>
        /// Immutable inferred-class profile. Pure value object: the title is a stable localization
        /// id (resolve via <c>LocalizationService.Get</c> at the display site, never stored), the
        /// dominant/secondary trees and up to two identity bonuses (one for pure, two halved for
        /// hybrid). No Unity reference, no saved state.
        /// </summary>
        public readonly struct ClassProfile
        {
            public readonly string TitleId;
            public readonly bool HasDominant;
            public readonly bool IsHybrid;
            public readonly SkillTreeId DominantTree;
            public readonly SkillTreeId SecondaryTree; // valid only when IsHybrid
            public readonly ClassBonus PrimaryBonus;
            public readonly ClassBonus SecondaryBonus;  // empty unless IsHybrid

            public ClassProfile(
                string titleId,
                bool hasDominant,
                bool isHybrid,
                SkillTreeId dominantTree,
                SkillTreeId secondaryTree,
                ClassBonus primaryBonus,
                ClassBonus secondaryBonus)
            {
                TitleId = titleId;
                HasDominant = hasDominant;
                IsHybrid = isHybrid;
                DominantTree = dominantTree;
                SecondaryTree = secondaryTree;
                PrimaryBonus = primaryBonus;
                SecondaryBonus = secondaryBonus;
            }

            /// <summary>True for the no-class "Colono" profile (no bonus, identical to a fresh char).</summary>
            public bool IsColono => !HasDominant;

            /// <summary>Total magnitude on a given axis (sums primary + secondary if both match).</summary>
            public float BonusFor(BonusAxis axis)
            {
                if (axis == BonusAxis.None)
                {
                    return 0f;
                }

                float v = 0f;
                if (PrimaryBonus.Axis == axis)
                {
                    v += PrimaryBonus.Value;
                }

                if (SecondaryBonus.Axis == axis)
                {
                    v += SecondaryBonus.Value;
                }

                return v;
            }
        }

        /// <summary>The "Colono" profile: no dominant tree, no bonus.</summary>
        public static ClassProfile Colono => new ClassProfile(
            ColonoTitleId, false, false,
            default, default,
            default, default);

        private static readonly SkillTreeId[] TreeOrder =
        {
            // Deterministic tie-break order = enum order (Melee, Ranged, Magic, Survival, Crafting).
            SkillTreeId.Melee, SkillTreeId.Ranged, SkillTreeId.Magic, SkillTreeId.Survival, SkillTreeId.Crafting
        };

        /// <summary>
        /// Pure, deterministic inference from points spent per tree. A missing tree key counts as 0.
        /// Returns <see cref="Colono"/> whenever there is no qualifying dominant tree (incl. ties or
        /// a dominant below the 6-point / 40%-share threshold).
        /// </summary>
        public static ClassProfile GetProfile(IReadOnlyDictionary<SkillTreeId, int> pointsPerTree)
        {
            if (pointsPerTree == null)
            {
                return Colono;
            }

            int total = 0;
            int firstPts = -1, secondPts = -1;
            // Use sentinel that is invalid; resolved before use only when firstPts > 0.
            SkillTreeId firstTree = SkillTreeId.Melee;
            SkillTreeId secondTree = SkillTreeId.Melee;
            bool firstSet = false, secondSet = false;

            // Single pass over the canonical tree order (deterministic tie-break: earlier enum wins).
            foreach (var tree in TreeOrder)
            {
                int pts = pointsPerTree.TryGetValue(tree, out var p) ? (p < 0 ? 0 : p) : 0;
                total += pts;

                if (!firstSet || pts > firstPts)
                {
                    // Demote current first to second.
                    secondPts = firstPts;
                    secondTree = firstTree;
                    secondSet = firstSet;

                    firstPts = pts;
                    firstTree = tree;
                    firstSet = true;
                }
                else if (!secondSet || pts > secondPts)
                {
                    secondPts = pts;
                    secondTree = tree;
                    secondSet = true;
                }
            }

            if (total <= 0 || firstPts <= 0)
            {
                return Colono;
            }

            // Dominant gate: most points AND >= 6 AND >= 40% of total.
            bool qualifies = firstPts >= MinDominantPoints
                             && firstPts >= MinDominantShare * total;
            if (!qualifies)
            {
                return Colono;
            }

            // Tie at the top (two trees share the max) => no dominant => Colono.
            if (secondSet && secondPts == firstPts)
            {
                return Colono;
            }

            // Hybrid gate: 2nd >= 70% of 1st (and 2nd actually has points).
            bool hybrid = secondSet && secondPts > 0
                          && secondPts >= HybridSecondaryRatio * firstPts;

            if (!hybrid)
            {
                var bonus = PureBonus(firstTree);
                return new ClassProfile(
                    PureTitleId(firstTree), true, false,
                    firstTree, default,
                    bonus, default);
            }

            // Hybrid: composite title + HALF of each micro-bonus.
            var primary = PureBonus(firstTree);
            var secondary = PureBonus(secondTree);
            return new ClassProfile(
                HybridTitleId(firstTree, secondTree), true, true,
                firstTree, secondTree,
                new ClassBonus(primary.Axis, primary.Value * 0.5f),
                new ClassBonus(secondary.Axis, secondary.Value * 0.5f));
        }

        /// <summary>The pure-tree title localization id (5 trees).</summary>
        public static string PureTitleId(SkillTreeId tree)
        {
            switch (tree)
            {
                case SkillTreeId.Melee: return "ui.class.warrior";    // "Lamina de Cindar"
                case SkillTreeId.Ranged: return "ui.class.hunter";    // "Olho da Mata"
                case SkillTreeId.Magic: return "ui.class.mystic";     // "Tecelao"
                case SkillTreeId.Survival: return "ui.class.farmer";  // "Mao da Terra"
                case SkillTreeId.Crafting: return "ui.class.bond";    // "Voz do Vale"
                default: return ColonoTitleId;
            }
        }

        /// <summary>
        /// Composite hybrid title id (5x4 table = dominant_secondary). Order matters: the dominant
        /// tree leads the composite. e.g. melee+magic => "ui.class.warrior_mystic" ("Lamina Tecela").
        /// </summary>
        public static string HybridTitleId(SkillTreeId dominant, SkillTreeId secondary)
        {
            return PureTitleId(dominant) + "_" + ShortName(secondary);
        }

        private static string ShortName(SkillTreeId tree)
        {
            switch (tree)
            {
                case SkillTreeId.Melee: return "warrior";
                case SkillTreeId.Ranged: return "hunter";
                case SkillTreeId.Magic: return "mystic";
                case SkillTreeId.Survival: return "farmer";
                case SkillTreeId.Crafting: return "bond";
                default: return "colono";
            }
        }

        /// <summary>The full identity micro-bonus for a pure tree (already &lt;= 5%).</summary>
        public static ClassBonus PureBonus(SkillTreeId tree)
        {
            switch (tree)
            {
                case SkillTreeId.Melee: return new ClassBonus(BonusAxis.PostureDealt, WarriorPostureBonus);
                case SkillTreeId.Ranged: return new ClassBonus(BonusAxis.CritChance, HunterCritBonus);
                case SkillTreeId.Magic: return new ClassBonus(BonusAxis.ManaCostReduction, MysticManaCostReduction);
                case SkillTreeId.Survival: return new ClassBonus(BonusAxis.StaminaOutOfCave, FarmerStaminaOutOfCaveBonus);
                case SkillTreeId.Crafting: return new ClassBonus(BonusAxis.FriendshipGained, BondFriendshipBonus);
                default: return new ClassBonus(BonusAxis.None, 0f);
            }
        }
    }
}
