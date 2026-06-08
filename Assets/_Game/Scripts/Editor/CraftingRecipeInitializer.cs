#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    public static class CraftingRecipeInitializer
    {
        private const string RecipePath = "Assets/_Game/Data/Recipes";
        private const string ItemPath = "Assets/_Game/Data/Items";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string RecipeDatabasePath = "Assets/_Game/Data/Registries/RecipeDatabase.asset";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        [MenuItem("CindarsHope/Archive/Testing/Create Crafting Assets (Spec 07)")]
        public static void CreateSpec07TestContent()
        {
            EnsureDirectory(RecipePath);
            EnsureDirectory(ItemPath);

            var wood = CreateOrUpdateItem("item_material_wood", "item_material_wood", "Wood", ItemCategory.Material, 99, false);
            var ironOre = CreateOrUpdateItem("item_material_iron_ore", "item_material_iron_ore", "Iron Ore", ItemCategory.Ore, 99, false);
            var wheat = CreateOrUpdateItem("item_crop_wheat", "item_crop_wheat", "Wheat", ItemCategory.Crop, 99, false);
            var processedWood = CreateOrUpdateItem("Item_Processed_Wood", "item_material_processed_wood", "Processed Wood", ItemCategory.Material, 99, false);
            var bread = CreateOrUpdateItem("item_consumable_food_bread", "item_consumable_food_bread", "Bread", ItemCategory.Consumable, 99, false);
            var sword = AssetDatabase.LoadAssetAtPath<ItemDataSO>($"{ItemPath}/Item_Shop_Sword_Iron.asset");

            AddToRegistry<ItemDatabaseSO, ItemDataSO>(ItemDatabasePath, wood, ironOre, wheat, processedWood, bread, sword);

            var pocket = CreateOrUpdateRecipe(
                "Recipe_Pocket_ProcessedWood", "recipe_pocket_processed_wood", "Hand-carved Timber",
                WorkshopType.None, 0f, processedWood.Id, 1, new[] { new RecipeIngredient(wood.Id, 1) });
            var workbench = CreateOrUpdateRecipe(
                "Recipe_Workbench_ProcessedWood", "recipe_workbench_processed_wood", "Workbench Timber",
                WorkshopType.Workbench, 0f, processedWood.Id, 2, new[] { new RecipeIngredient(wood.Id, 2) });
            var forge = CreateOrUpdateRecipe(
                "Recipe_Forge_IronSword", "recipe_forge_iron_sword", "Iron Sword",
                WorkshopType.Forge, 4f, sword != null ? sword.Id : "item_shop_weapon_sword_iron", 1,
                new[] { new RecipeIngredient(ironOre.Id, 2) });
            var cooking = CreateOrUpdateRecipe(
                "Recipe_Cooking_Bread", "recipe_cooking_bread", "Fresh Bread",
                WorkshopType.CookingStation, 2f, bread.Id, 1, new[] { new RecipeIngredient(wheat.Id, 2) });

            AddToRegistry<RecipeDatabaseSO, RecipeDataSO>(RecipeDatabasePath, pocket, workbench, forge, cooking);
            RemoveLegacyRecipe("recipe_processed_wood");
            AddStarterItems(wood, ironOre, wheat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SPEC 07 crafting recipes and starter kit updated.");
        }

        private static ItemDataSO CreateOrUpdateItem(string assetName, string id, string displayName, ItemCategory category, int maxStack, bool equippable)
        {
            var path = $"{ItemPath}/{assetName}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (item != null)
            {
                return item;
            }

            item = FindExistingItemById(id);
            if (item != null)
            {
                return item;
            }

            item = ScriptableObject.CreateInstance<ItemDataSO>();
            AssetDatabase.CreateAsset(item, path);
            item.Id = id;
            item.DisplayName = displayName;
            item.Description = $"SPEC 07 starter/test item: {displayName}.";
            item.Category = category;
            item.MaxStack = maxStack;
            item.IsEquippable = equippable;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static ItemDataSO FindExistingItemById(string id)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        private static RecipeDataSO CreateOrUpdateRecipe(
            string assetName,
            string id,
            string displayName,
            WorkshopType stationType,
            float craftTimeSeconds,
            string outputItemId,
            int outputAmount,
            RecipeIngredient[] ingredients)
        {
            var path = $"{RecipePath}/{assetName}.asset";
            var recipe = AssetDatabase.LoadAssetAtPath<RecipeDataSO>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<RecipeDataSO>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            recipe.SetId(id);
            recipe.DisplayName = displayName;
            recipe.Description = $"SPEC 07 MVP/test recipe: {displayName}.";
            recipe.RequiredStationType = stationType;
            recipe.RequiredWorkshopLevel = 1;
            recipe.CraftTimeSeconds = craftTimeSeconds;
            recipe.OutputItemId = outputItemId;
            recipe.OutputAmount = outputAmount;
            recipe.Ingredients = ingredients;
            recipe.IsUnlockedByDefault = true;
            recipe.StaminaCost = 0;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static void AddStarterItems(params ItemDataSO[] items)
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData == null)
            {
                Debug.LogWarning($"PlayerDataSO not found at '{PlayerDataPath}'. Starter kit was not installed.");
                return;
            }

            var startingItems = new List<StartingItem>(playerData.StartingItems ?? new StartingItem[0]);
            AddOrReplaceStartingItem(startingItems, items[0], 8);
            AddOrReplaceStartingItem(startingItems, items[1], 4);
            AddOrReplaceStartingItem(startingItems, items[2], 4);
            playerData.StartingItems = startingItems.ToArray();
            EditorUtility.SetDirty(playerData);
        }

        private static void AddOrReplaceStartingItem(List<StartingItem> startingItems, ItemDataSO item, int amount)
        {
            for (var index = 0; index < startingItems.Count; index++)
            {
                if (startingItems[index].Item == item
                    || (startingItems[index].Item != null && startingItems[index].Item.Id == item.Id))
                {
                    startingItems[index] = new StartingItem { Item = item, Amount = amount };
                    return;
                }
            }

            startingItems.Add(new StartingItem { Item = item, Amount = amount });
        }

        private static void AddToRegistry<TRegistry, TData>(string path, params TData[] items)
            where TRegistry : DataRegistrySO<TData>
            where TData : ScriptableObject, IIdentifiedData
        {
            var registry = AssetDatabase.LoadAssetAtPath<TRegistry>(path);
            if (registry == null)
            {
                Debug.LogWarning($"Registry not found at '{path}'.");
                return;
            }

            var serialized = new SerializedObject(registry);
            var entries = serialized.FindProperty("_items");
            foreach (var item in items)
            {
                if (item == null || Contains(entries, item) || ContainsId<TData>(entries, item.Id))
                {
                    continue;
                }

                entries.InsertArrayElementAtIndex(entries.arraySize);
                entries.GetArrayElementAtIndex(entries.arraySize - 1).objectReferenceValue = item;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }

        private static void RemoveLegacyRecipe(string recipeId)
        {
            var registry = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(RecipeDatabasePath);
            if (registry == null)
            {
                Debug.LogWarning($"Registry not found at '{RecipeDatabasePath}'.");
                return;
            }

            var serialized = new SerializedObject(registry);
            var entries = serialized.FindProperty("_items");
            for (var index = entries.arraySize - 1; index >= 0; index--)
            {
                var recipe = entries.GetArrayElementAtIndex(index).objectReferenceValue as RecipeDataSO;
                if (recipe != null && recipe.Id == recipeId)
                {
                    entries.DeleteArrayElementAtIndex(index);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }

        private static bool Contains(SerializedProperty entries, Object item)
        {
            for (var index = 0; index < entries.arraySize; index++)
            {
                if (entries.GetArrayElementAtIndex(index).objectReferenceValue == item)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsId<TData>(SerializedProperty entries, string id)
            where TData : ScriptableObject, IIdentifiedData
        {
            for (var index = 0; index < entries.arraySize; index++)
            {
                var item = entries.GetArrayElementAtIndex(index).objectReferenceValue as TData;
                if (item != null && item.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
#endif
