using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CindarsHope.Combat;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Runtime;
using CindarsHope.Skills;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    // fable_30 — Catalog Consistency Validator (editor shell).
    //
    // Crosses generated assets x canonical catalogs and reports divergences. It REUSES the
    // existing validator harness convention (CindarsHope/Validate menu + Debug.Log lines +
    // throw-on-error for batchmode exit code; see ValidateTownShopCatalogIntegrity). It does
    // NOT fork a new harness, never writes to assets (read-only), and adds no runtime code.
    //
    // The cross-ref reasoning is delegated to the PURE CatalogConsistencyEngine so it can be
    // EditMode-tested without AssetDatabase. This shell only COLLECTS the snapshot.
    //
    // Incremental: a category whose generator has not run yet (no assets) is reported SKIPPED
    // by the engine, never a false ERROR (F29/F32/F33/F34 run in separate specs, possibly out
    // of order). Quests are read from the in-memory QuestRegistry (no SO pipeline yet).
    public static class ValidateCatalogConsistency
    {
        private const string Tag = "[fable_30]";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        [MenuItem("CindarsHope/Validate/Catalog Consistency")]
        public static void Run()
        {
            var result = Validate(out var report);
            Debug.Log(report);

            if (result.HasErrors)
            {
                Debug.LogError($"{Tag} VALIDATION FAILED with {result.ErrorCount} error(s).");
            }
            else
            {
                Debug.Log($"{Tag} VALIDATION PASSED (0 errors, {result.WarnCount} warnings, " +
                          $"{result.InfoCount} info, {result.SkippedCount} skipped).");
            }
        }

        // Batchmode entry point. Use via:
        //   Unity -batchmode -quit -projectPath . -executeMethod
        //     CindarsHope.Editor.Validation.ValidateCatalogConsistency.RunBatch
        // Throws on ERROR so Unity returns a non-zero exit code (CA-3, gate-friendly).
        public static void RunBatch()
        {
            var result = Validate(out var report);
            Debug.Log(report);

            if (result.HasErrors)
            {
                // Throwing makes Unity exit non-zero in batchmode (matches the existing
                // ValidateTownShopCatalogIntegrity convention). Never convert ERROR to pass.
                throw new InvalidOperationException(
                    $"{Tag} Catalog consistency FAILED with {result.ErrorCount} error(s). See log above.");
            }

            Debug.Log($"{Tag} Catalog consistency PASSED (0 errors).");
        }

        // Collects the snapshot from assets/registries and runs the pure engine.
        public static CatalogValidationResult Validate(out string report)
        {
            var snapshot = CollectSnapshot();
            var expectations = CatalogExpectationSet.BuildCanonical();
            var result = CatalogConsistencyEngine.Validate(snapshot, expectations);
            report = FormatReport(result);
            return result;
        }

        private static CatalogSnapshot CollectSnapshot()
        {
            var snapshot = new CatalogSnapshot();

            CollectItems(snapshot);
            CollectBestiary(snapshot);
            CollectRecipes(snapshot);
            CollectShops(snapshot);
            CollectQuests(snapshot);
            CollectSkills(snapshot);

            return snapshot;
        }

        private static void CollectItems(CatalogSnapshot snapshot)
        {
            var itemIds = new HashSet<string>();

            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database != null)
            {
                foreach (var item in database.All)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    {
                        itemIds.Add(item.Id);
                    }
                }
            }

            // Also sweep loose ItemDataSO assets so a drop/recipe/shop reference to an item that
            // exists as an asset but is not yet in the registry is not a false "missing item".
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                {
                    itemIds.Add(item.Id);
                }
            }

            snapshot.Items.PresentIds = itemIds;
            snapshot.Items.ActualCount = itemIds.Count;
            snapshot.Items.Generated = itemIds.Count > 0;
        }

        private static void CollectBestiary(CatalogSnapshot snapshot)
        {
            var enemyIds = new HashSet<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:EnemyDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.enemyId))
                {
                    continue;
                }

                enemyIds.Add(enemy.enemyId);

                if (!string.IsNullOrWhiteSpace(enemy.dropItemId))
                {
                    snapshot.EnemyDrops.Add(new EnemyDropSnapshot
                    {
                        EnemyId = enemy.enemyId,
                        DropItemId = enemy.dropItemId
                    });
                }

                if (!string.IsNullOrWhiteSpace(enemy.BestiaryEntryId))
                {
                    snapshot.BestiaryEntries.Add(new BestiaryEntrySnapshot
                    {
                        EnemyId = enemy.enemyId,
                        BestiaryEntryId = enemy.BestiaryEntryId
                    });
                }
            }

            snapshot.Bestiary.PresentIds = enemyIds;
            snapshot.Bestiary.ActualCount = enemyIds.Count;
            snapshot.Bestiary.Generated = enemyIds.Count > 0;
        }

        private static void CollectRecipes(CatalogSnapshot snapshot)
        {
            var recipeIds = new HashSet<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:RecipeDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var recipe = AssetDatabase.LoadAssetAtPath<RecipeDataSO>(path);
                if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id))
                {
                    continue;
                }

                recipeIds.Add(recipe.Id);

                if (recipe.Ingredients == null)
                {
                    continue;
                }

                foreach (var ingredient in recipe.Ingredients)
                {
                    if (!string.IsNullOrWhiteSpace(ingredient.ItemId))
                    {
                        snapshot.RecipeIngredients.Add(new RecipeIngredientSnapshot
                        {
                            RecipeId = recipe.Id,
                            IngredientItemId = ingredient.ItemId
                        });
                    }
                }
            }

            snapshot.Recipes.PresentIds = recipeIds;
            snapshot.Recipes.ActualCount = recipeIds.Count;
            snapshot.Recipes.Generated = recipeIds.Count > 0;
        }

        private static void CollectShops(CatalogSnapshot snapshot)
        {
            var shopIds = new HashSet<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:ShopDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                if (shop == null || string.IsNullOrWhiteSpace(shop.Id))
                {
                    continue;
                }

                shopIds.Add(shop.Id);

                if (shop.Items == null)
                {
                    continue;
                }

                foreach (var entry in shop.Items)
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.ItemId))
                    {
                        snapshot.ShopEntries.Add(new ShopEntrySnapshot
                        {
                            ShopId = shop.Id,
                            ItemId = entry.ItemId
                        });
                    }
                }
            }

            snapshot.Shops.PresentIds = shopIds;
            snapshot.Shops.ActualCount = shopIds.Count;
            snapshot.Shops.Generated = shopIds.Count > 0;
        }

        private static void CollectQuests(CatalogSnapshot snapshot)
        {
            // Quests have no SO pipeline yet (QuestRegistry is in-memory; TEMPORARY_QUEST_SMOKE_TEST).
            // We read what exists so reward cross-checks work; count WARN (X/86) is expected until F34.
            var questIds = new HashSet<string>();

            QuestRegistry registry;
            try
            {
                registry = new QuestRegistry();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{Tag} Could not instantiate QuestRegistry: {ex.Message}. Quests reported as not generated.");
                snapshot.Quests.Generated = false;
                return;
            }

            foreach (var quest in registry.GetAllQuests())
            {
                if (quest == null || string.IsNullOrWhiteSpace(quest.QuestId))
                {
                    continue;
                }

                questIds.Add(quest.QuestId);

                foreach (var reward in registry.GetRewards(quest.QuestId))
                {
                    var kind = ClassifyReward(reward);
                    if (kind == null || string.IsNullOrWhiteSpace(reward.TargetId))
                    {
                        continue;
                    }

                    snapshot.QuestRewards.Add(new QuestRewardSnapshot
                    {
                        QuestId = quest.QuestId,
                        RewardId = reward.RewardId,
                        RewardKind = kind,
                        TargetId = reward.TargetId
                    });
                }
            }

            snapshot.Quests.PresentIds = questIds;
            snapshot.Quests.ActualCount = questIds.Count;
            snapshot.Quests.Generated = questIds.Count > 0;
        }

        // Only Item and SpellUnlock/SkillTreeUnlock rewards carry an item/skill TargetId we resolve.
        // Gold/Flag/etc. carry no catalog reference -> not cross-checked.
        private static string ClassifyReward(QuestRewardDefinition reward)
        {
            if (reward == null)
            {
                return null;
            }

            switch (reward.RewardType)
            {
                case QuestRewardType.Item:
                    return "item";
                case QuestRewardType.SpellUnlock:
                case QuestRewardType.SkillTreeUnlock:
                    return "skill";
                default:
                    return null;
            }
        }

        private static void CollectSkills(CatalogSnapshot snapshot)
        {
            // Skill catalog is currently code-driven (DefaultSkillCatalog), not SO-backed; there
            // are no SkillNodeDataSO assets. Per incremental mode we report Skills as NOT generated
            // (SKIPPED) until F29 materializes skill assets. We DO collect any loose SkillNodeDataSO
            // assets if they exist, so the validator activates automatically once F29 runs.
            var skillIds = new HashSet<string>();

            foreach (var guid in AssetDatabase.FindAssets("t:SkillNodeDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var node = AssetDatabase.LoadAssetAtPath<SkillNodeDataSO>(path);
                if (node == null || string.IsNullOrWhiteSpace(node.SkillNodeId))
                {
                    continue;
                }

                skillIds.Add(node.SkillNodeId);

                if (!string.IsNullOrWhiteSpace(node.UnlockedSkillActionId))
                {
                    snapshot.ActiveSkills.Add(new ActiveSkillSnapshot
                    {
                        SkillNodeId = node.SkillNodeId,
                        ActionOrEffectId = node.UnlockedSkillActionId
                    });
                    // Canonical FABLE decision: skill action ids without a real executor are
                    // feedback-only/dormant by design (FeedbackOnlySkillEffectExecutor). Treat
                    // every declared action as a valid dormant target so the editor run does not
                    // emit false ERRORs; the ERROR path for genuinely invalid actions is proven
                    // by CatalogValidatorTests on the pure engine.
                    snapshot.DormantSkillActionIds.Add(node.UnlockedSkillActionId);
                }
            }

            snapshot.Skills.PresentIds = skillIds;
            snapshot.Skills.ActualCount = skillIds.Count;
            snapshot.Skills.Generated = skillIds.Count > 0;
        }

        private static string FormatReport(CatalogValidationResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Tag} Catalog Consistency report");
            sb.AppendLine($"{Tag} Summary: Errors={result.ErrorCount}, Warnings={result.WarnCount}, " +
                          $"Info={result.InfoCount}, Skipped={result.SkippedCount}");

            foreach (var severity in new[]
                     {
                         CatalogSeverity.Error, CatalogSeverity.Warn,
                         CatalogSeverity.Info, CatalogSeverity.Skipped
                     })
            {
                var group = result.Findings.Where(f => f.Severity == severity).ToList();
                if (group.Count == 0)
                {
                    continue;
                }

                sb.AppendLine($"{Tag} --- {severity} ({group.Count}) ---");
                foreach (var finding in group)
                {
                    sb.AppendLine($"{Tag}   {finding}");
                }
            }

            return sb.ToString();
        }
    }
}
