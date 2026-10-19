using System.Collections.Generic;
using System.Linq;

namespace CindarsHope.Editor.Validation
{
    // fable_30 — Catalog Consistency Validator (PURE cross-ref engine).
    //
    // This file contains ZERO Unity / AssetDatabase dependencies on purpose:
    // the editor shell (ValidateCatalogConsistency) collects assets and builds the
    // snapshot below; this engine only reasons over in-memory data, so it can be
    // covered by EditMode tests with synthetic fixtures (CA-4 / spec risk: "validador
    // acoplar-se a AssetDatabase e ficar intestavel -> motor puro").
    //
    // Severities (spec escopo): ERROR = broken reference, WARN = count divergence,
    // INFO = declared dormant, SKIPPED = generator not run yet (incremental mode).

    public enum CatalogSeverity
    {
        // Order matters for sorting/grouping in the editor log.
        Error = 0,
        Warn = 1,
        Info = 2,
        Skipped = 3
    }

    public enum CatalogCategory
    {
        Items,
        Bestiary,
        Quests,
        Skills,
        Recipes,
        Shops
    }

    public sealed class CatalogFinding
    {
        public CatalogSeverity Severity { get; }
        public CatalogCategory Category { get; }
        public string CheckId { get; }
        public string Message { get; }
        public IReadOnlyList<string> InvolvedIds { get; }

        public CatalogFinding(
            CatalogSeverity severity,
            CatalogCategory category,
            string checkId,
            string message,
            params string[] involvedIds)
        {
            Severity = severity;
            Category = category;
            CheckId = checkId;
            Message = message;
            InvolvedIds = involvedIds ?? new string[0];
        }

        public override string ToString()
        {
            var ids = InvolvedIds.Count > 0 ? $" [{string.Join(", ", InvolvedIds)}]" : string.Empty;
            return $"({Severity}) {Category}/{CheckId}: {Message}{ids}";
        }
    }

    // ─── Snapshot DTOs (pure data; built by the editor shell from assets) ────────

    public sealed class EnemyDropSnapshot
    {
        public string EnemyId;
        public string DropItemId;
    }

    public sealed class QuestRewardSnapshot
    {
        public string QuestId;
        public string RewardId;
        // "item" or "skill" (the only reference kinds this engine resolves).
        public string RewardKind;
        public string TargetId;
    }

    public sealed class RecipeIngredientSnapshot
    {
        public string RecipeId;
        public string IngredientItemId;
    }

    public sealed class ShopEntrySnapshot
    {
        public string ShopId;
        public string ItemId;
    }

    public sealed class ActiveSkillSnapshot
    {
        public string SkillNodeId;
        // The action/effect id the active skill unlocks (UnlockedSkillActionId).
        public string ActionOrEffectId;
    }

    public sealed class BestiaryEntrySnapshot
    {
        public string EnemyId;
        public string BestiaryEntryId;
    }

    // A category snapshot. When Generated == false the engine reports SKIPPED for
    // every check that targets this category, instead of false ERRORs (incremental
    // mode: F32/F33/F34/F29 generators run in separate specs and possibly out of order).
    public sealed class CategorySnapshot
    {
        public CatalogCategory Category;
        public bool Generated;
        // Stable IDs actually present as assets (e.g. all ItemDataSO ids).
        public HashSet<string> PresentIds = new HashSet<string>();
        // Count actually found (for X/Y partial reporting).
        public int ActualCount;
    }

    // Full input to the engine.
    public sealed class CatalogSnapshot
    {
        public CategorySnapshot Items = new CategorySnapshot { Category = CatalogCategory.Items };
        public CategorySnapshot Bestiary = new CategorySnapshot { Category = CatalogCategory.Bestiary };
        public CategorySnapshot Quests = new CategorySnapshot { Category = CatalogCategory.Quests };
        public CategorySnapshot Skills = new CategorySnapshot { Category = CatalogCategory.Skills };
        public CategorySnapshot Recipes = new CategorySnapshot { Category = CatalogCategory.Recipes };
        public CategorySnapshot Shops = new CategorySnapshot { Category = CatalogCategory.Shops };

        public List<EnemyDropSnapshot> EnemyDrops = new List<EnemyDropSnapshot>();
        public List<QuestRewardSnapshot> QuestRewards = new List<QuestRewardSnapshot>();
        public List<RecipeIngredientSnapshot> RecipeIngredients = new List<RecipeIngredientSnapshot>();
        public List<ShopEntrySnapshot> ShopEntries = new List<ShopEntrySnapshot>();
        public List<ActiveSkillSnapshot> ActiveSkills = new List<ActiveSkillSnapshot>();
        public List<BestiaryEntrySnapshot> BestiaryEntries = new List<BestiaryEntrySnapshot>();
        // EffectIds / dormant flags considered VALID targets for active skills (check g).
        public HashSet<string> ValidSkillActionIds = new HashSet<string>();
        public HashSet<string> DormantSkillActionIds = new HashSet<string>();

        public CategorySnapshot CategoryOf(CatalogCategory category)
        {
            switch (category)
            {
                case CatalogCategory.Items: return Items;
                case CatalogCategory.Bestiary: return Bestiary;
                case CatalogCategory.Quests: return Quests;
                case CatalogCategory.Skills: return Skills;
                case CatalogCategory.Recipes: return Recipes;
                case CatalogCategory.Shops: return Shops;
                default: return null;
            }
        }
    }

    public sealed class CatalogValidationResult
    {
        public List<CatalogFinding> Findings { get; } = new List<CatalogFinding>();

        public int ErrorCount => Findings.Count(f => f.Severity == CatalogSeverity.Error);
        public int WarnCount => Findings.Count(f => f.Severity == CatalogSeverity.Warn);
        public int InfoCount => Findings.Count(f => f.Severity == CatalogSeverity.Info);
        public int SkippedCount => Findings.Count(f => f.Severity == CatalogSeverity.Skipped);

        public bool HasErrors => ErrorCount > 0;
    }

    public static class CatalogConsistencyEngine
    {
        // Runs all eight cross-checks (a)-(h) plus count expectations against the
        // snapshot, honoring incremental SKIPPED for categories whose generator has
        // not run. Pure function: same input -> same findings.
        public static CatalogValidationResult Validate(
            CatalogSnapshot snapshot,
            CatalogExpectationSet expectations)
        {
            var result = new CatalogValidationResult();
            if (snapshot == null)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Error, CatalogCategory.Items, "engine",
                    "Snapshot was null."));
                return result;
            }

            expectations = expectations ?? new CatalogExpectationSet();

            // Count expectations + canonical-id presence per category.
            EvaluateCategory(result, snapshot.Items, expectations, snapshot);
            EvaluateCategory(result, snapshot.Bestiary, expectations, snapshot);
            EvaluateCategory(result, snapshot.Quests, expectations, snapshot);
            EvaluateCategory(result, snapshot.Skills, expectations, snapshot);

            // (c) enemy drop -> item.
            CheckCrossRef(
                result, snapshot.Bestiary, snapshot.Items, "(c)-drop-to-item",
                CatalogCategory.Bestiary,
                snapshot.EnemyDrops
                    .Where(d => !string.IsNullOrWhiteSpace(d?.DropItemId))
                    .Select(d => (Source: d.EnemyId, Target: d.DropItemId)),
                (src, tgt) => $"Enemy '{src}' drops item '{tgt}' which does not exist in the item catalog.");

            // (d) quest reward -> item or skill.
            CheckQuestRewards(result, snapshot);

            // (e) recipe -> ingredient item.
            CheckCrossRef(
                result, snapshot.Recipes, snapshot.Items, "(e)-recipe-to-ingredient",
                CatalogCategory.Recipes,
                snapshot.RecipeIngredients
                    .Where(i => !string.IsNullOrWhiteSpace(i?.IngredientItemId))
                    .Select(i => (Source: i.RecipeId, Target: i.IngredientItemId)),
                (src, tgt) => $"Recipe '{src}' requires ingredient '{tgt}' which does not exist in the item catalog.");

            // (f) shop entry -> item.
            CheckCrossRef(
                result, snapshot.Shops, snapshot.Items, "(f)-shop-to-item",
                CatalogCategory.Shops,
                snapshot.ShopEntries
                    .Where(s => !string.IsNullOrWhiteSpace(s?.ItemId))
                    .Select(s => (Source: s.ShopId, Target: s.ItemId)),
                (src, tgt) => $"Shop '{src}' lists item '{tgt}' which does not exist in the item catalog.");

            // (g) active skill -> valid executor / declared-dormant flag.
            CheckActiveSkills(result, snapshot);

            // (h) duplicate BestiaryEntryId.
            CheckDuplicateBestiaryEntries(result, snapshot);

            return result;
        }

        // (a) canonical id without asset, (b) asset without canonical id, plus count divergence.
        private static void EvaluateCategory(
            CatalogValidationResult result,
            CategorySnapshot category,
            CatalogExpectationSet expectations,
            CatalogSnapshot snapshot)
        {
            if (category == null)
            {
                return;
            }

            var expectation = expectations.For(category.Category);

            if (!category.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, category.Category, "incremental",
                    $"Category '{category.Category}' has no generated assets yet; SKIPPED (run its generator first)."));
                return;
            }

            // (a) canonical id present in expectation but missing as asset.
            if (expectation != null && expectation.CanonicalIds.Count > 0)
            {
                foreach (var canonicalId in expectation.CanonicalIds)
                {
                    if (!category.PresentIds.Contains(canonicalId))
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Error, category.Category, "(a)-missing-canonical",
                            $"Canonical id '{canonicalId}' has no asset in category '{category.Category}'.",
                            canonicalId));
                    }
                }

                // (b) asset whose id is not in the documented canonical list (extra/undocumented).
                foreach (var presentId in category.PresentIds)
                {
                    if (!expectation.CanonicalIds.Contains(presentId))
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Warn, category.Category, "(b)-undocumented-asset",
                            $"Asset id '{presentId}' in category '{category.Category}' is not in the documented canonical id list.",
                            presentId));
                    }
                }
            }

            // Count divergence: WARN with X/Y (never ERROR — spec: count is WARN).
            if (expectation != null && expectation.ExpectedCount > 0
                && category.ActualCount != expectation.ExpectedCount)
            {
                var partial = category.ActualCount < expectation.ExpectedCount;
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Warn, category.Category, "count",
                    $"Category '{category.Category}' count {category.ActualCount}/{expectation.ExpectedCount} " +
                    (partial ? "(partial generation)." : "(more assets than documented).")));
            }
        }

        private static void CheckCrossRef(
            CatalogValidationResult result,
            CategorySnapshot sourceCategory,
            CategorySnapshot targetCategory,
            string checkId,
            CatalogCategory reportCategory,
            IEnumerable<(string Source, string Target)> references,
            System.Func<string, string, string> messageBuilder)
        {
            // Incremental honesty: if either side is not generated, we cannot judge
            // these references yet -> SKIPPED, never a false ERROR.
            if (sourceCategory == null || !sourceCategory.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, reportCategory, checkId,
                    $"Source category for '{checkId}' not generated yet; cross-check SKIPPED."));
                return;
            }

            if (targetCategory == null || !targetCategory.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, reportCategory, checkId,
                    $"Target category for '{checkId}' not generated yet; cross-check SKIPPED."));
                return;
            }

            foreach (var reference in references)
            {
                if (!targetCategory.PresentIds.Contains(reference.Target))
                {
                    result.Findings.Add(new CatalogFinding(
                        CatalogSeverity.Error, reportCategory, checkId,
                        messageBuilder(reference.Source, reference.Target),
                        reference.Source, reference.Target));
                }
            }
        }

        private static void CheckQuestRewards(CatalogValidationResult result, CatalogSnapshot snapshot)
        {
            const string checkId = "(d)-quest-reward";

            if (snapshot.Quests == null || !snapshot.Quests.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, CatalogCategory.Quests, checkId,
                    "Quest category not generated yet; reward cross-check SKIPPED."));
                return;
            }

            foreach (var reward in snapshot.QuestRewards)
            {
                if (reward == null || string.IsNullOrWhiteSpace(reward.TargetId))
                {
                    continue;
                }

                var kind = (reward.RewardKind ?? string.Empty).Trim().ToLowerInvariant();
                if (kind == "item")
                {
                    if (snapshot.Items == null || !snapshot.Items.Generated)
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Skipped, CatalogCategory.Quests, checkId,
                            "Item category not generated yet; quest item reward cross-check SKIPPED."));
                        continue;
                    }

                    if (!snapshot.Items.PresentIds.Contains(reward.TargetId))
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Error, CatalogCategory.Quests, checkId,
                            $"Quest '{reward.QuestId}' reward '{reward.RewardId}' grants item '{reward.TargetId}' which does not exist in the item catalog.",
                            reward.QuestId, reward.TargetId));
                    }
                }
                else if (kind == "skill")
                {
                    if (snapshot.Skills == null || !snapshot.Skills.Generated)
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Skipped, CatalogCategory.Quests, checkId,
                            "Skill category not generated yet; quest skill reward cross-check SKIPPED."));
                        continue;
                    }

                    if (!snapshot.Skills.PresentIds.Contains(reward.TargetId))
                    {
                        result.Findings.Add(new CatalogFinding(
                            CatalogSeverity.Error, CatalogCategory.Quests, checkId,
                            $"Quest '{reward.QuestId}' reward '{reward.RewardId}' grants skill '{reward.TargetId}' which does not exist in the skill catalog.",
                            reward.QuestId, reward.TargetId));
                    }
                }
            }
        }

        private static void CheckActiveSkills(CatalogValidationResult result, CatalogSnapshot snapshot)
        {
            const string checkId = "(g)-active-skill-executor";

            if (snapshot.Skills == null || !snapshot.Skills.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, CatalogCategory.Skills, checkId,
                    "Skill category not generated yet; active-skill executor cross-check SKIPPED."));
                return;
            }

            foreach (var active in snapshot.ActiveSkills)
            {
                if (active == null || string.IsNullOrWhiteSpace(active.ActionOrEffectId))
                {
                    continue;
                }

                if (snapshot.ValidSkillActionIds.Contains(active.ActionOrEffectId))
                {
                    continue;
                }

                if (snapshot.DormantSkillActionIds.Contains(active.ActionOrEffectId))
                {
                    // Declared dormant — valid but worth reporting as INFO.
                    result.Findings.Add(new CatalogFinding(
                        CatalogSeverity.Info, CatalogCategory.Skills, checkId,
                        $"Active skill '{active.SkillNodeId}' uses declared-dormant action '{active.ActionOrEffectId}'.",
                        active.SkillNodeId, active.ActionOrEffectId));
                    continue;
                }

                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Error, CatalogCategory.Skills, checkId,
                    $"Active skill '{active.SkillNodeId}' references action/executor '{active.ActionOrEffectId}' that is neither a valid executor nor a declared-dormant flag.",
                    active.SkillNodeId, active.ActionOrEffectId));
            }
        }

        private static void CheckDuplicateBestiaryEntries(CatalogValidationResult result, CatalogSnapshot snapshot)
        {
            const string checkId = "(h)-duplicate-bestiary-entry";

            if (snapshot.Bestiary == null || !snapshot.Bestiary.Generated)
            {
                result.Findings.Add(new CatalogFinding(
                    CatalogSeverity.Skipped, CatalogCategory.Bestiary, checkId,
                    "Bestiary category not generated yet; duplicate-entry check SKIPPED."));
                return;
            }

            var seen = new Dictionary<string, string>();
            foreach (var entry in snapshot.BestiaryEntries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.BestiaryEntryId))
                {
                    continue;
                }

                if (seen.TryGetValue(entry.BestiaryEntryId, out var firstEnemy))
                {
                    result.Findings.Add(new CatalogFinding(
                        CatalogSeverity.Error, CatalogCategory.Bestiary, checkId,
                        $"Duplicate BestiaryEntryId '{entry.BestiaryEntryId}' used by enemies '{firstEnemy}' and '{entry.EnemyId}'.",
                        entry.BestiaryEntryId, firstEnemy, entry.EnemyId));
                }
                else
                {
                    seen[entry.BestiaryEntryId] = entry.EnemyId;
                }
            }
        }
    }
}
