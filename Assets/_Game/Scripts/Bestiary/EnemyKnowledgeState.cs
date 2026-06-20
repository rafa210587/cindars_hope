using System.Collections.Generic;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_21 — revealable information categories from BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION §6.
    /// Stable string names are used in save/events (see <see cref="BestiaryKnowledgeCategories"/>);
    /// this enum is the in-memory handle. The bestiary REVEALS these — it never authors the
    /// underlying vulnerability/drop data (that is F06's domain).
    /// </summary>
    public enum BestiaryKnowledgeCategory
    {
        Identity,
        BehaviorSummary,
        ElementVulnerability,
        DropsCommon,
        DropsRare,
        ResistanceTags
    }

    /// <summary>
    /// fable_21 — the K0..K4 progression stages (direction §4). Derived from the discovery counters,
    /// never stored directly; <see cref="EnemyKnowledgeState.GetLevel"/> computes it. "Studied" (K4)
    /// is what the skill-point milestone (EMENDA-B) counts.
    /// </summary>
    public enum EnemyKnowledgeLevel
    {
        Unknown = 0,            // K0 — never encountered
        Seen = 1,               // K1 — identity revealed (>=1 sighting)
        Fought = 2,             // K2 — at least one kill OR behavior revealed
        Defeated = 3,           // K3 — drops-common revealed (>=5 kills) or repeated defeats
        Studied = 4             // K4 — fully documented (all relevant categories unlocked)
    }

    /// <summary>
    /// fable_21 — stable string names for knowledge categories. Used in the save DTO and in
    /// <see cref="Core.Events.BestiaryKnowledgeUnlockedEvent"/> so persistence/telemetry never
    /// depend on enum ordinals. Round-trips with <see cref="Parse"/> / <see cref="ToName"/>.
    /// </summary>
    public static class BestiaryKnowledgeCategories
    {
        public const string Identity = "Identity";
        public const string BehaviorSummary = "BehaviorSummary";
        public const string ElementVulnerability = "ElementVulnerability";
        public const string DropsCommon = "DropsCommon";
        public const string DropsRare = "DropsRare";
        public const string ResistanceTags = "ResistanceTags";

        public static string ToName(BestiaryKnowledgeCategory category)
        {
            switch (category)
            {
                case BestiaryKnowledgeCategory.Identity: return Identity;
                case BestiaryKnowledgeCategory.BehaviorSummary: return BehaviorSummary;
                case BestiaryKnowledgeCategory.ElementVulnerability: return ElementVulnerability;
                case BestiaryKnowledgeCategory.DropsCommon: return DropsCommon;
                case BestiaryKnowledgeCategory.DropsRare: return DropsRare;
                case BestiaryKnowledgeCategory.ResistanceTags: return ResistanceTags;
                default: return category.ToString();
            }
        }

        public static bool TryParse(string name, out BestiaryKnowledgeCategory category)
        {
            switch (name)
            {
                case Identity: category = BestiaryKnowledgeCategory.Identity; return true;
                case BehaviorSummary: category = BestiaryKnowledgeCategory.BehaviorSummary; return true;
                case ElementVulnerability: category = BestiaryKnowledgeCategory.ElementVulnerability; return true;
                case DropsCommon: category = BestiaryKnowledgeCategory.DropsCommon; return true;
                case DropsRare: category = BestiaryKnowledgeCategory.DropsRare; return true;
                case ResistanceTags: category = BestiaryKnowledgeCategory.ResistanceTags; return true;
                default: category = default; return false;
            }
        }
    }

    /// <summary>
    /// fable_21 — per-creature knowledge state. Pure C# (no Unity refs) so it round-trips through the
    /// save DTO and is fully EditMode-testable. Holds the discovery COUNTERS (sightings, kills,
    /// effective hits per element, resisted-hit count, actions seen with a per-action observation
    /// count) plus the SET of unlocked categories. Thresholds live in <see cref="EnemyKnowledgeService"/>.
    /// </summary>
    public class EnemyKnowledgeState
    {
        public string EnemyId;

        // ── Discovery counters ──────────────────────────────────────────────────────────────────
        public int Sightings;
        public int Kills;
        public float SecondsFought;
        public int ResistedHits;

        // Times each enemy ACTION id has been observed (behavior threshold: same action 3x).
        public Dictionary<string, int> ActionsSeen = new Dictionary<string, int>();

        // Times an EFFECTIVE (super-effective / vulnerable) hit landed per damage-type axis
        // (vulnerability threshold: 3 effective hits on an axis).
        public Dictionary<string, int> EffectiveHits = new Dictionary<string, int>();

        // Whether the player has SUFFERED any action from this creature (behavior threshold:
        // suffered 1x reveals behavior immediately).
        public bool SufferedAction;

        // ── Unlocked categories (stable string names) ───────────────────────────────────────────
        public HashSet<string> UnlockedCategories = new HashSet<string>();

        public bool IsUnlocked(BestiaryKnowledgeCategory category)
        {
            return UnlockedCategories.Contains(BestiaryKnowledgeCategories.ToName(category));
        }

        /// <summary>
        /// Computes the K0..K4 stage from the unlocked categories (direction §4). "Studied" (K4)
        /// requires the four core categories that every common creature can reach by play
        /// (Identity, BehaviorSummary, ElementVulnerability, DropsCommon). DropsRare/ResistanceTags
        /// are external/rare and not required for K4 so a creature can be Studied without grinding
        /// rare drops.
        /// </summary>
        public EnemyKnowledgeLevel GetLevel()
        {
            bool identity = IsUnlocked(BestiaryKnowledgeCategory.Identity);
            bool behavior = IsUnlocked(BestiaryKnowledgeCategory.BehaviorSummary);
            bool vuln = IsUnlocked(BestiaryKnowledgeCategory.ElementVulnerability);
            bool dropsCommon = IsUnlocked(BestiaryKnowledgeCategory.DropsCommon);

            if (identity && behavior && vuln && dropsCommon)
            {
                return EnemyKnowledgeLevel.Studied;
            }

            if (dropsCommon || Kills > 0)
            {
                return EnemyKnowledgeLevel.Defeated;
            }

            if (behavior)
            {
                return EnemyKnowledgeLevel.Fought;
            }

            if (identity || Sightings > 0)
            {
                return EnemyKnowledgeLevel.Seen;
            }

            return EnemyKnowledgeLevel.Unknown;
        }
    }
}
