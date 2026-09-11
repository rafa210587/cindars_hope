#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills.Runtime;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Skills
{
    /// <summary>
    /// Materializes Living Forge variants for every distinct canonical recipe output. Existing
    /// assets are synchronized from their base item; no recipe or inventory schema is changed.
    /// </summary>
    public static class GenerateLivingForgeOutputVariants
    {
        private const string OutputDirectory = "Assets/_Game/Data/Items/LivingForge";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";
        private const string LogTag = "[skills-20F]";

        public sealed class GenerationCounters
        {
            public int Created;
            public int Updated;
            public int Unchanged;
            public int MissingBaseItems;
            public int RegistryEntriesAdded;
            public int ObsoleteVariantsRemoved;
            public bool NoChanges => Created == 0 && Updated == 0
                                     && RegistryEntriesAdded == 0 && ObsoleteVariantsRemoved == 0;
        }

        [MenuItem("CindarsHope/Generate/Skills/Living Forge Output Variants")]
        public static void Run()
        {
            var counters = Generate();
            Debug.Log($"{LogTag} created={counters.Created}, updated={counters.Updated}, " +
                      $"unchanged={counters.Unchanged}, missingBase={counters.MissingBaseItems}, " +
                      $"removed={counters.ObsoleteVariantsRemoved}, " +
                      $"registryAdded={counters.RegistryEntriesAdded}, noChanges={counters.NoChanges}.");
        }

        public static void RunBatch()
        {
            var counters = Generate();
            if (counters.MissingBaseItems > 0)
                throw new InvalidOperationException(
                    $"Living Forge generation found {counters.MissingBaseItems} recipe outputs without base ItemDataSO assets.");

            Debug.Log($"{LogTag} batch complete: created={counters.Created}, updated={counters.Updated}, " +
                      $"unchanged={counters.Unchanged}, registryAdded={counters.RegistryEntriesAdded}, " +
                      $"removed={counters.ObsoleteVariantsRemoved}, " +
                      $"noChanges={counters.NoChanges}.");
        }

        public static GenerationCounters Generate()
        {
            EnsureFolder(OutputDirectory);
            var counters = new GenerationCounters();
            var allItems = LoadItemsById();
            var recipeDatabase = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath);
            if (recipeDatabase == null)
                throw new InvalidOperationException($"RecipeDatabaseSO was not found at '{RecipeDatabasePath}'.");

            var desiredVariantIds = new HashSet<string>(StringComparer.Ordinal);
            var registeredRecipes = recipeDatabase.All
                .Where(recipe => recipe != null &&
                                 !string.IsNullOrWhiteSpace(recipe.OutputItemId) &&
                                 !LivingForgeOutputVariantCatalog.TryDescribe(
                                     recipe.OutputItemId, out _))
                .ToList();
            var registeredOutputIds = CollectRegisteredRecipeOutputIds(recipeDatabase.All);
            foreach (var baseItemId in registeredOutputIds)
            {
                if (allItems.ContainsKey(baseItemId)) continue;
                counters.MissingBaseItems++;
                Debug.LogError($"{LogTag} Registered recipe output '{baseItemId}' has no ItemDataSO base asset.");
            }

            // Fail closed before asset mutations. RunBatch turns this into a failing invocation;
            // the menu run keeps the report visible without deleting any previous valid output.
            if (counters.MissingBaseItems > 0)
                return counters;

            foreach (var baseItemId in registeredOutputIds)
            {
                var baseItem = allItems[baseItemId];
                var outputAmounts = registeredRecipes
                    .Where(recipe => string.Equals(recipe.OutputItemId, baseItemId,
                        StringComparison.Ordinal))
                    .Select(recipe => recipe.OutputAmount)
                    .ToList();
                bool equipmentRecipe = LivingForgeOutputVariantCatalog.IsPotentialEquipment(
                    baseItem) && outputAmounts.Contains(1);

                if (equipmentRecipe || outputAmounts.Any(amount =>
                        LivingForgeOutputVariantCatalog.IsEligibleConsumable(baseItem, amount,
                            LivingForgeOutputVariantCatalog.Quality1PayloadMultiplier)))
                {
                    desiredVariantIds.Add(LivingForgeOutputVariantCatalog.Quality1Id(baseItemId));
                    Upsert(baseItem, LivingForgeOutputStage.Quality1, allItems, counters);
                }
                if (equipmentRecipe || outputAmounts.Any(amount =>
                        LivingForgeOutputVariantCatalog.IsEligibleConsumable(baseItem, amount,
                            LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier)))
                {
                    desiredVariantIds.Add(LivingForgeOutputVariantCatalog.Quality2Id(baseItemId));
                    Upsert(baseItem, LivingForgeOutputStage.Quality2, allItems, counters);
                }
                if (outputAmounts.Any(amount =>
                        LivingForgeOutputVariantCatalog.IsEligibleConsumable(baseItem, amount,
                            LivingForgeOutputVariantCatalog.PotencyMultiplier)))
                {
                    desiredVariantIds.Add(LivingForgeOutputVariantCatalog.PotencyId(baseItemId));
                    Upsert(baseItem, LivingForgeOutputStage.Potency, allItems, counters);
                }
                if (outputAmounts.Any(amount =>
                        LivingForgeOutputVariantCatalog.IsEligibleConsumable(baseItem, amount,
                            LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier *
                            LivingForgeOutputVariantCatalog.PotencyMultiplier)))
                {
                    desiredVariantIds.Add(
                        LivingForgeOutputVariantCatalog.Quality2PotencyId(baseItemId));
                    Upsert(baseItem, LivingForgeOutputStage.Quality2Potency, allItems, counters);
                }
            }

            counters.ObsoleteVariantsRemoved = RemoveObsoleteVariants(desiredVariantIds);
            counters.RegistryEntriesAdded = SyncRegistry();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return counters;
        }

        /// <summary>Pure field policy used by the generator and EditMode contract tests.</summary>
        public static void ApplyVariant(ItemDataSO target, ItemDataSO baseItem, LivingForgeOutputStage stage)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (baseItem == null) throw new ArgumentNullException(nameof(baseItem));
            float payloadMultiplier = stage switch
            {
                LivingForgeOutputStage.Quality1 =>
                    LivingForgeOutputVariantCatalog.Quality1PayloadMultiplier,
                LivingForgeOutputStage.Quality2 =>
                    LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier,
                LivingForgeOutputStage.Potency =>
                    LivingForgeOutputVariantCatalog.PotencyMultiplier,
                LivingForgeOutputStage.Quality2Potency =>
                    LivingForgeOutputVariantCatalog.Quality2PayloadMultiplier *
                    LivingForgeOutputVariantCatalog.PotencyMultiplier,
                _ => 1f
            };
            bool qualityStage = stage == LivingForgeOutputStage.Quality1 ||
                                stage == LivingForgeOutputStage.Quality2;
            bool consumableEligible = LivingForgeOutputVariantCatalog.IsEligibleConsumable(
                baseItem, 1, payloadMultiplier);
            if ((!qualityStage && !consumableEligible) ||
                (qualityStage && !consumableEligible &&
                 !LivingForgeOutputVariantCatalog.IsPotentialEquipment(baseItem)))
            {
                throw new ArgumentException(
                    "Living Forge variants require eligible equipment or a numeric consumable payload that increases after rounding.",
                    nameof(baseItem));
            }

            var variant = LivingForgeOutputVariantCatalog.Describe(baseItem.Id, stage);
            EditorUtility.CopySerialized(baseItem, target);
            target.Id = variant.ItemId;
            target.DisplayName = (baseItem.DisplayName ?? baseItem.Id) + DisplaySuffix(stage);

            target.BaseValue = baseItem.BaseValue;

            if (consumableEligible)
            {
                target.HungerRestore = LivingForgeOutputVariantCatalog.ScaleInteger(
                    baseItem.HungerRestore, variant.ConsumablePayloadMultiplier);
                target.StaminaRestore = LivingForgeOutputVariantCatalog.ScaleInteger(
                    baseItem.StaminaRestore, variant.ConsumablePayloadMultiplier);
                target.DurabilityRestoreAmount = LivingForgeOutputVariantCatalog.ScaleInteger(
                    baseItem.DurabilityRestoreAmount, variant.ConsumablePayloadMultiplier);
            }
        }

        private static void Upsert(
            ItemDataSO baseItem,
            LivingForgeOutputStage stage,
            IDictionary<string, ItemDataSO> allItems,
            GenerationCounters counters)
        {
            var descriptor = LivingForgeOutputVariantCatalog.Describe(baseItem.Id, stage);
            if (!allItems.TryGetValue(descriptor.ItemId, out var target) || target == null)
            {
                target = ScriptableObject.CreateInstance<ItemDataSO>();
                ApplyVariant(target, baseItem, stage);
                AssetDatabase.CreateAsset(target, $"{OutputDirectory}/{descriptor.ItemId}.asset");
                allItems[descriptor.ItemId] = target;
                counters.Created++;
                return;
            }

            var before = EditorJsonUtility.ToJson(target);
            ApplyVariant(target, baseItem, stage);
            if (!string.Equals(before, EditorJsonUtility.ToJson(target), StringComparison.Ordinal))
            {
                EditorUtility.SetDirty(target);
                counters.Updated++;
            }
            else
            {
                counters.Unchanged++;
            }
        }

        /// <summary>
        /// Deterministic projection of the runtime registry. Loose RecipeDataSO assets are never
        /// inputs because CraftingRuntime cannot execute them.
        /// </summary>
        public static IReadOnlyList<string> CollectRegisteredRecipeOutputIds(
            IEnumerable<RecipeDataSO> registeredRecipes)
        {
            var ids = new SortedSet<string>(StringComparer.Ordinal);
            if (registeredRecipes == null)
                return ids.ToList();

            foreach (var recipe in registeredRecipes)
            {
                if (recipe == null || string.IsNullOrWhiteSpace(recipe.OutputItemId)
                    || LivingForgeOutputVariantCatalog.TryDescribe(recipe.OutputItemId, out _))
                    continue;
                ids.Add(recipe.OutputItemId);
            }
            return ids.ToList();
        }

        private static Dictionary<string, ItemDataSO> LoadItemsById()
        {
            var result = new Dictionary<string, ItemDataSO>(StringComparer.Ordinal);
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && !string.IsNullOrWhiteSpace(item.Id) && !result.ContainsKey(item.Id))
                    result.Add(item.Id, item);
            }
            return result;
        }

        private static int SyncRegistry()
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database == null) return 0;

            var serialized = new SerializedObject(database);
            var items = serialized.FindProperty("_items");
            if (items == null || !items.isArray) return 0;

            var present = new HashSet<ItemDataSO>();
            for (var i = 0; i < items.arraySize; i++)
            {
                if (items.GetArrayElementAtIndex(i).objectReferenceValue is ItemDataSO item)
                    present.Add(item);
            }

            var generated = AssetDatabase.FindAssets("t:ItemDataSO", new[] { OutputDirectory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<ItemDataSO>(path))
                .Where(item => item != null)
                .OrderBy(item => item.Id, StringComparer.Ordinal);

            var added = 0;
            foreach (var item in generated)
            {
                if (!present.Add(item)) continue;
                items.arraySize++;
                items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = item;
                added++;
            }

            if (added > 0)
            {
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
            }
            return added;
        }

        private static int RemoveObsoleteVariants(ISet<string> desiredVariantIds)
        {
            var obsolete = AssetDatabase.FindAssets("t:ItemDataSO", new[] { OutputDirectory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new
                {
                    Path = path,
                    Item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path)
                })
                .Where(entry => entry.Item != null
                                && !desiredVariantIds.Contains(entry.Item.Id))
                .OrderBy(entry => entry.Path, StringComparer.Ordinal)
                .ToList();

            if (obsolete.Count == 0)
                return 0;

            var obsoleteItems = new HashSet<ItemDataSO>(obsolete.Select(entry => entry.Item));
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database != null)
            {
                var serialized = new SerializedObject(database);
                var items = serialized.FindProperty("_items");
                if (items != null && items.isArray)
                {
                    for (var i = items.arraySize - 1; i >= 0; i--)
                    {
                        if (items.GetArrayElementAtIndex(i).objectReferenceValue is not ItemDataSO item
                            || !obsoleteItems.Contains(item))
                            continue;

                        items.GetArrayElementAtIndex(i).objectReferenceValue = null;
                        items.DeleteArrayElementAtIndex(i);
                    }
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(database);
                }
            }

            var removed = 0;
            foreach (var entry in obsolete)
            {
                if (AssetDatabase.DeleteAsset(entry.Path))
                    removed++;
                else
                    Debug.LogError($"{LogTag} Failed to remove obsolete owned variant '{entry.Path}'.");
            }
            return removed;
        }

        private static string DisplaySuffix(LivingForgeOutputStage stage) => stage switch
        {
            LivingForgeOutputStage.Quality1 => " [Q1]",
            LivingForgeOutputStage.Quality2 => " [Q2]",
            LivingForgeOutputStage.Potency => " [+8% potência]",
            LivingForgeOutputStage.Quality2Potency => " [Q2 +8% potência]",
            _ => string.Empty
        };

        private static void EnsureFolder(string directory)
        {
            if (AssetDatabase.IsValidFolder(directory)) return;
            var parts = directory.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
