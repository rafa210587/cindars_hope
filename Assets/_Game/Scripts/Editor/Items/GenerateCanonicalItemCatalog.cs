#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Items
{
    // fable_32 — Canonical Item Catalog generator.
    //
    // Materializes the PURE CanonicalItemCatalog rows into ItemDataSO + RecipeDataSO assets,
    // IDEMPOTENTLY: load-or-create by id, UPDATE fields by id only when they differ, NEVER
    // delete (saved stacks keep stable ids — anti-regression). A 2nd consecutive run reports
    // 0 created / 0 updated (CA-3). Reuses the established ItemDataInitializer asset pattern
    // (AssetDatabase.CreateAsset under Assets/_Game/Data/Items); does NOT fork a 2nd database.
    //
    // NOTE (asset generation deferred): per the executing context, Unity asset generation is
    // DEFERRED. This script is the generator + data; run it in the Unity Editor (menu or
    // batchmode) at the human checkpoint to actually create/update the .asset files, then run
    // the fable_30 validator (CindarsHope/Validate/Catalog Consistency) and attach the log.
    public static class GenerateCanonicalItemCatalog
    {
        private const string ItemsDir = "Assets/_Game/Data/Items";
        private const string RecipesDir = "Assets/_Game/Data/Craft/Recipes";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string Tag = "[fable_32]";

        public sealed class GenerationCounters
        {
            public int ItemsCreated;
            public int ItemsUpdated;
            public int ItemsUnchanged;
            public int RecipesCreated;
            public int RecipesUpdated;
            public int RecipesUnchanged;
            public int RegistryEntriesAdded;

            public bool NoChanges =>
                ItemsCreated == 0 && ItemsUpdated == 0 &&
                RecipesCreated == 0 && RecipesUpdated == 0 && RegistryEntriesAdded == 0;
        }

        public static void Run()
        {
            var counters = Generate(out var report);
            Debug.Log(report);
            Debug.Log($"{Tag} Item catalog generation complete. " +
                      $"Items: +{counters.ItemsCreated} new / {counters.ItemsUpdated} updated / {counters.ItemsUnchanged} unchanged. " +
                      $"Recipes: +{counters.RecipesCreated} new / {counters.RecipesUpdated} updated / {counters.RecipesUnchanged} unchanged. " +
                      $"Registry entries added: {counters.RegistryEntriesAdded}.");
        }

        // Batchmode entry point:
        //   Unity -batchmode -quit -projectPath . -executeMethod
        //     CindarsHope.Editor.Items.GenerateCanonicalItemCatalog.RunBatch
        public static void RunBatch()
        {
            var counters = Generate(out var report);
            Debug.Log(report);
            Debug.Log($"{Tag} Item catalog generation (batch) complete. NoChanges(2nd-run signal)={counters.NoChanges}.");
        }

        public static GenerationCounters Generate(out string report)
        {
            var counters = new GenerationCounters();

            EnsureFolder(ItemsDir);
            EnsureFolder(RecipesDir);

            var byId = LoadAllItemsById();

            // 1) Items (expanded: base + quality variants).
            foreach (var row in CanonicalItemCatalog.ExpandedRows())
            {
                UpsertItem(row, byId, counters);
            }

            // 2) Recipes.
            foreach (var recipe in CanonicalItemCatalog.RecipeRows())
            {
                UpsertRecipe(recipe, counters);
            }

            // 3) Register every ItemDataSO into the ItemDatabaseSO registry (so F30 reports COMPLETE).
            counters.RegistryEntriesAdded = SyncRegistry();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            report = BuildReport(counters);
            return counters;
        }

        private static void UpsertItem(CatalogItemRow row, Dictionary<string, ItemDataSO> byId, GenerationCounters counters)
        {
            if (!byId.TryGetValue(row.Id, out var asset) || asset == null)
            {
                asset = ScriptableObject.CreateInstance<ItemDataSO>();
                ApplyItemRow(asset, row);
                var path = AssetDatabase.GenerateUniqueAssetPath($"{ItemsDir}/{row.Id}.asset");
                AssetDatabase.CreateAsset(asset, path);
                byId[row.Id] = asset;
                counters.ItemsCreated++;
                return;
            }

            // Existing — update by id ONLY when something differs (idempotency: 2nd run = no change).
            if (ApplyItemRowIfChanged(asset, row))
            {
                EditorUtility.SetDirty(asset);
                counters.ItemsUpdated++;
            }
            else
            {
                counters.ItemsUnchanged++;
            }
        }

        private static void ApplyItemRow(ItemDataSO asset, CatalogItemRow row)
        {
            asset.Id = row.Id;
            asset.DisplayName = row.DisplayName;
            if (string.IsNullOrEmpty(asset.Description))
            {
                asset.Description = $"Item: {row.DisplayName}";
            }
            asset.Category = row.Category;
            asset.ConsumableSubtype = row.ConsumableSubtype;
            asset.MaxStack = Mathf.Max(1, row.MaxStack);
            asset.BaseValue = Mathf.Max(0, row.BaseValue);
            asset.HungerRestore = Mathf.Max(0, row.HungerRestore);
            asset.StaminaRestore = Mathf.Max(0, row.StaminaRestore);
            asset.IsEquippable = row.IsEquippable;
            asset.DurabilityRestoreAmount = Mathf.Max(0, row.DurabilityRestoreAmount);
            if (!string.IsNullOrEmpty(row.AmmoType))
            {
                asset.AmmoType = row.AmmoType;
            }
            ApplyAmmoEquipFields(asset, row);
        }

        // Ammo rows are hand-equippable (arrow stack in one hand, bow in the other). Derive the
        // SPEC_08 equip contract from the category so the .asset gets UseKind=EquipAmmo and
        // AllowedEquipmentSlots=[LeftHand,RightHand] without a manual YAML edit. Returns true if a
        // field was changed (used by the idempotent update path).
        private static bool ApplyAmmoEquipFields(ItemDataSO asset, CatalogItemRow row)
        {
            if (row.Category != ItemCategory.Ammo)
            {
                return false;
            }

            var changed = false;

            if (asset.UseKind != ItemUseKind.EquipAmmo)
            {
                asset.UseKind = ItemUseKind.EquipAmmo;
                changed = true;
            }

            var desiredSlots = new[] { CindarsHope.Equipment.EquipmentSlot.LeftHand, CindarsHope.Equipment.EquipmentSlot.RightHand };
            if (!SlotsEqual(asset.AllowedEquipmentSlots, desiredSlots))
            {
                asset.AllowedEquipmentSlots = desiredSlots;
                changed = true;
            }

            return changed;
        }

        private static bool SlotsEqual(CindarsHope.Equipment.EquipmentSlot[] a, CindarsHope.Equipment.EquipmentSlot[] b)
        {
            if (a == null) return b == null || b.Length == 0;
            if (b == null) return a.Length == 0;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        // Returns true if any catalog-owned field changed (and applies the change). Fields NOT
        // owned by the catalog (Icon, UseKind, SpellSource, etc.) are left untouched so hand/other
        // generator tuning is preserved.
        private static bool ApplyItemRowIfChanged(ItemDataSO asset, CatalogItemRow row)
        {
            var changed = false;

            if (asset.DisplayName != row.DisplayName) { asset.DisplayName = row.DisplayName; changed = true; }
            if (asset.Category != row.Category) { asset.Category = row.Category; changed = true; }
            if (asset.ConsumableSubtype != row.ConsumableSubtype) { asset.ConsumableSubtype = row.ConsumableSubtype; changed = true; }

            var maxStack = Mathf.Max(1, row.MaxStack);
            if (asset.MaxStack != maxStack) { asset.MaxStack = maxStack; changed = true; }

            var bv = Mathf.Max(0, row.BaseValue);
            if (asset.BaseValue != bv) { asset.BaseValue = bv; changed = true; }

            var hunger = Mathf.Max(0, row.HungerRestore);
            if (asset.HungerRestore != hunger) { asset.HungerRestore = hunger; changed = true; }

            var stamina = Mathf.Max(0, row.StaminaRestore);
            if (asset.StaminaRestore != stamina) { asset.StaminaRestore = stamina; changed = true; }

            if (asset.IsEquippable != row.IsEquippable) { asset.IsEquippable = row.IsEquippable; changed = true; }

            var dura = Mathf.Max(0, row.DurabilityRestoreAmount);
            if (asset.DurabilityRestoreAmount != dura) { asset.DurabilityRestoreAmount = dura; changed = true; }

            if (!string.IsNullOrEmpty(row.AmmoType) && asset.AmmoType != row.AmmoType)
            {
                asset.AmmoType = row.AmmoType;
                changed = true;
            }

            // Ammo equip contract (UseKind=EquipAmmo + hand slots) — re-run materializes it onto
            // existing arrow assets that predate this field, then reports 0 changes thereafter.
            if (ApplyAmmoEquipFields(asset, row))
            {
                changed = true;
            }

            return changed;
        }

        private static void UpsertRecipe(CatalogRecipeRow recipe, GenerationCounters counters)
        {
            var existing = FindRecipeById(recipe.Id);
            if (existing == null)
            {
                var asset = ScriptableObject.CreateInstance<RecipeDataSO>();
                ApplyRecipe(asset, recipe);
                var path = AssetDatabase.GenerateUniqueAssetPath($"{RecipesDir}/{recipe.Id}.asset");
                AssetDatabase.CreateAsset(asset, path);
                counters.RecipesCreated++;
                return;
            }

            if (ApplyRecipeIfChanged(existing, recipe))
            {
                EditorUtility.SetDirty(existing);
                counters.RecipesUpdated++;
            }
            else
            {
                counters.RecipesUnchanged++;
            }
        }

        private static void ApplyRecipe(RecipeDataSO asset, CatalogRecipeRow recipe)
        {
            asset.SetId(recipe.Id);
            asset.DisplayName = recipe.DisplayName;
            asset.OutputItemId = recipe.OutputItemId;
            asset.OutputAmount = Mathf.Max(1, recipe.OutputAmount);
            asset.RequiredStationType = recipe.Station;
            asset.Ingredients = recipe.Ingredients
                .Select(kv => new RecipeIngredient(kv.Key, kv.Value))
                .ToArray();
        }

        private static bool ApplyRecipeIfChanged(RecipeDataSO asset, CatalogRecipeRow recipe)
        {
            var changed = false;

            if (asset.DisplayName != recipe.DisplayName) { asset.DisplayName = recipe.DisplayName; changed = true; }
            if (asset.OutputItemId != recipe.OutputItemId) { asset.OutputItemId = recipe.OutputItemId; changed = true; }
            if (asset.RequiredStationType != recipe.Station) { asset.RequiredStationType = recipe.Station; changed = true; }

            var outAmount = Mathf.Max(1, recipe.OutputAmount);
            if (asset.OutputAmount != outAmount) { asset.OutputAmount = outAmount; changed = true; }

            var desired = recipe.Ingredients.Select(kv => new RecipeIngredient(kv.Key, kv.Value)).ToArray();
            if (!IngredientsEqual(asset.Ingredients, desired))
            {
                asset.Ingredients = desired;
                changed = true;
            }

            return changed;
        }

        private static bool IngredientsEqual(RecipeIngredient[] a, RecipeIngredient[] b)
        {
            if (a == null) return b == null || b.Length == 0;
            if (b == null) return a.Length == 0;
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++)
            {
                if (a[i].ItemId != b[i].ItemId || a[i].Amount != b[i].Amount)
                {
                    return false;
                }
            }
            return true;
        }

        // Registers every ItemDataSO asset into the ItemDatabaseSO _items array (append-only).
        private static int SyncRegistry()
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database == null)
            {
                Debug.LogWarning($"{Tag} ItemDatabase not found at {ItemDatabasePath}; items are still discoverable by the validator via the loose t:ItemDataSO sweep. Registry sync skipped.");
                return 0;
            }

            var so = new SerializedObject(database);
            var prop = so.FindProperty("_items");
            if (prop == null || !prop.isArray)
            {
                Debug.LogWarning($"{Tag} ItemDatabase has no '_items' array property; registry sync skipped.");
                return 0;
            }

            var present = new HashSet<ItemDataSO>();
            for (var i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue is ItemDataSO existing && existing != null)
                {
                    present.Add(existing);
                }
            }

            var added = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemsDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item == null || present.Contains(item))
                {
                    continue;
                }

                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = item;
                present.Add(item);
                added++;
            }

            if (added > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
            }

            return added;
        }

        private static Dictionary<string, ItemDataSO> LoadAllItemsById()
        {
            var byId = new Dictionary<string, ItemDataSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null && !string.IsNullOrWhiteSpace(item.Id) && !byId.ContainsKey(item.Id))
                {
                    byId[item.Id] = item;
                }
            }
            return byId;
        }

        private static RecipeDataSO FindRecipeById(string id)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:RecipeDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var recipe = AssetDatabase.LoadAssetAtPath<RecipeDataSO>(path);
                if (recipe != null && recipe.Id == id)
                {
                    return recipe;
                }
            }
            return null;
        }

        private static void EnsureFolder(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir))
            {
                return;
            }

            var parts = dir.Split('/');
            var current = parts[0]; // "Assets"
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static string BuildReport(GenerationCounters counters)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Tag} Canonical Item Catalog generation report");
            sb.AppendLine($"{Tag} Expanded item rows in table: {CanonicalItemCatalog.ExpandedRows().Count} " +
                          $"(base rows: {CanonicalItemCatalog.BaseRows().Count}).");
            sb.AppendLine($"{Tag} Recipe rows in table: {CanonicalItemCatalog.RecipeRows().Count}.");
            sb.AppendLine($"{Tag} Items: created={counters.ItemsCreated}, updated={counters.ItemsUpdated}, unchanged={counters.ItemsUnchanged}.");
            sb.AppendLine($"{Tag} Recipes: created={counters.RecipesCreated}, updated={counters.RecipesUpdated}, unchanged={counters.RecipesUnchanged}.");
            sb.AppendLine($"{Tag} Registry entries added: {counters.RegistryEntriesAdded}.");
            sb.AppendLine($"{Tag} 2nd-run idempotency signal (expected on a re-run): NoChanges={counters.NoChanges}.");
            return sb.ToString();
        }
    }
}
#endif
