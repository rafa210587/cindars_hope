using System;
using System.Collections.Generic;
using CindarsHope.Craft.Data;
using CindarsHope.Core.Data;
using CindarsHope.Farm.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using CindarsHope.World.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.DataValidation
{
    public static class CindarsHopeDataValidator
    {
        private const string SuccessMessage = "Cindar's Hope MVP data validation passed.";

        private static readonly string[] RequiredItemIds =
        {
            "seed_wheat",
            "seed_carrot",
            "item_crop_wheat",
            "item_crop_carrot",
            "item_fish_common",
            "item_wood",
            "item_tool_fishing_rod_basic",
            "item_material_processed_wood"
        };

        private static readonly string[] RequiredSeedIds =
        {
            "seed_wheat",
            "seed_carrot"
        };

        private static readonly string[] RequiredTreeIds =
        {
            "tree_basic"
        };

        private static readonly string[] RequiredRecipeIds =
        {
            "recipe_processed_wood"
        };

        private static readonly string[] RequiredWorkshopIds =
        {
            "workshop_carpentry_basic"
        };

        [MenuItem("CindarsHope/Validate/Validate MVP Data")]
        public static void ValidateMvpData()
        {
            var errors = new List<string>();

            var itemDatabase = FindRequiredAsset<ItemDatabaseSO>(errors, nameof(ItemDatabaseSO));
            var seedDatabase = FindRequiredAsset<SeedDatabaseSO>(errors, nameof(SeedDatabaseSO));
            var playerData = FindRequiredAsset<PlayerDataSO>(errors, nameof(PlayerDataSO));
            var treeDatabase = FindRequiredAsset<TreeDatabaseSO>(errors, nameof(TreeDatabaseSO));
            var recipeDatabase = FindRequiredAsset<RecipeDatabaseSO>(errors, nameof(RecipeDatabaseSO));
            var workshopDatabase = FindRequiredAsset<WorkshopDatabaseSO>(errors, nameof(WorkshopDatabaseSO));
            var items = new List<ItemDataSO>();

            if (itemDatabase != null)
            {
                items = LoadRegistryItems<ItemDatabaseSO, ItemDataSO>(itemDatabase, errors, nameof(ItemDatabaseSO));
                ValidateIdentifiedData(items, RequiredItemIds, errors, "ItemDatabase");
            }

            if (seedDatabase != null)
            {
                var seeds = LoadRegistryItems<SeedDatabaseSO, SeedDataSO>(seedDatabase, errors, nameof(SeedDatabaseSO));
                ValidateIdentifiedData(seeds, RequiredSeedIds, errors, "SeedDatabase");
                ValidateSeeds(seeds, errors);
            }

            if (playerData != null)
            {
                ValidatePlayerData(playerData, errors);
            }

            if (treeDatabase != null)
            {
                var trees = LoadRegistryItems<TreeDatabaseSO, TreeDataSO>(treeDatabase, errors, nameof(TreeDatabaseSO));
                ValidateIdentifiedData(trees, RequiredTreeIds, errors, "TreeDatabase");
                ValidateTrees(trees, items, errors);
            }

            if (recipeDatabase != null)
            {
                var recipes = LoadRegistryItems<RecipeDatabaseSO, RecipeDataSO>(recipeDatabase, errors, nameof(RecipeDatabaseSO));
                ValidateIdentifiedData(recipes, RequiredRecipeIds, errors, "RecipeDatabase");
                ValidateRecipes(recipes, items, errors);
            }

            if (workshopDatabase != null)
            {
                var workshops = LoadRegistryItems<WorkshopDatabaseSO, WorkshopDataSO>(workshopDatabase, errors, nameof(WorkshopDatabaseSO));
                ValidateIdentifiedData(workshops, RequiredWorkshopIds, errors, "WorkshopDatabase");
                ValidateWorkshops(workshops, errors);
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError(error);
                }

                throw new InvalidOperationException($"Cindar's Hope MVP data validation failed with {errors.Count} error(s).");
            }

            Debug.Log(SuccessMessage);
        }

        private static T FindRequiredAsset<T>(ICollection<string> errors, string label)
            where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0)
            {
                errors.Add($"{label} asset not found.");
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                errors.Add($"{label} asset could not be loaded at path: {path}");
            }

            return asset;
        }

        private static List<TItem> LoadRegistryItems<TRegistry, TItem>(
            TRegistry registry,
            ICollection<string> errors,
            string label)
            where TRegistry : ScriptableObject
            where TItem : ScriptableObject, IIdentifiedData
        {
            var items = new List<TItem>();
            var serializedObject = new SerializedObject(registry);
            var itemsProperty = serializedObject.FindProperty("_items");

            if (itemsProperty == null || !itemsProperty.isArray)
            {
                errors.Add($"{label} does not expose serialized _items array.");
                return items;
            }

            for (var index = 0; index < itemsProperty.arraySize; index++)
            {
                var itemProperty = itemsProperty.GetArrayElementAtIndex(index);
                var item = itemProperty.objectReferenceValue as TItem;
                if (item == null)
                {
                    errors.Add($"{label} has null item at index {index}.");
                    continue;
                }

                items.Add(item);
            }

            return items;
        }

        private static void ValidateIdentifiedData<T>(
            IReadOnlyCollection<T> dataItems,
            IEnumerable<string> requiredIds,
            ICollection<string> errors,
            string label)
            where T : ScriptableObject, IIdentifiedData
        {
            var ids = new HashSet<string>();
            var duplicateIds = new HashSet<string>();

            foreach (var data in dataItems)
            {
                if (data == null)
                {
                    errors.Add($"{label} contains null data.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(data.Id))
                {
                    errors.Add($"{label} contains data with empty Id: {data.name}.");
                    continue;
                }

                if (!ids.Add(data.Id))
                {
                    duplicateIds.Add(data.Id);
                }
            }

            foreach (var duplicateId in duplicateIds)
            {
                errors.Add($"{label} contains duplicate Id: {duplicateId}.");
            }

            foreach (var requiredId in requiredIds)
            {
                if (!ids.Contains(requiredId))
                {
                    errors.Add($"{label} is missing required Id: {requiredId}.");
                }
            }
        }

        private static void ValidateSeeds(IEnumerable<SeedDataSO> seeds, ICollection<string> errors)
        {
            foreach (var seed in seeds)
            {
                if (seed == null)
                {
                    continue;
                }

                if (seed.SeedItem == null)
                {
                    errors.Add($"{seed.name} has null SeedItem.");
                }

                if (seed.HarvestItems == null || seed.HarvestItems.Length == 0)
                {
                    errors.Add($"{seed.name} must have at least one HarvestItem.");
                }

                if (seed.HarvestAmounts == null)
                {
                    errors.Add($"{seed.name} has null HarvestAmounts.");
                }

                if (seed.HarvestItems != null && seed.HarvestAmounts != null &&
                    seed.HarvestAmounts.Length != seed.HarvestItems.Length)
                {
                    errors.Add($"{seed.name} HarvestAmounts length must match HarvestItems length.");
                }

                if (seed.GrowthDays < 1)
                {
                    errors.Add($"{seed.name} GrowthDays must be >= 1.");
                }

                if (seed.MinYield < 1)
                {
                    errors.Add($"{seed.name} MinYield must be >= 1.");
                }

                if (seed.MaxYield < seed.MinYield)
                {
                    errors.Add($"{seed.name} MaxYield must be >= MinYield.");
                }
            }
        }

        private static void ValidatePlayerData(PlayerDataSO playerData, ICollection<string> errors)
        {
            if (playerData.StartingGold < 0)
            {
                errors.Add($"{playerData.name} StartingGold must be >= 0.");
            }

            if (playerData.StartingItems == null)
            {
                errors.Add($"{playerData.name} StartingItems must not be null.");
                return;
            }

            var startingAmountsById = new Dictionary<string, int>();
            for (var index = 0; index < playerData.StartingItems.Length; index++)
            {
                var startingItem = playerData.StartingItems[index];
                if (startingItem.Item == null)
                {
                    errors.Add($"{playerData.name} StartingItems has null Item at index {index}.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(startingItem.Item.Id))
                {
                    errors.Add($"{playerData.name} StartingItems has item with empty Id at index {index}: {startingItem.Item.name}.");
                    continue;
                }

                startingAmountsById[startingItem.Item.Id] = startingItem.Amount;
            }

            ValidateStartingItemAmount(startingAmountsById, "seed_wheat", 5, errors);
            ValidateStartingItemAmount(startingAmountsById, "seed_carrot", 3, errors);
            ValidateStartingItemAmount(startingAmountsById, "item_tool_fishing_rod_basic", 1, errors);
        }

        private static void ValidateTrees(
            IEnumerable<TreeDataSO> trees,
            IEnumerable<ItemDataSO> items,
            ICollection<string> errors)
        {
            var itemIds = new HashSet<string>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                itemIds.Add(item.Id);
            }

            foreach (var tree in trees)
            {
                if (tree == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tree.Id))
                {
                    errors.Add($"{tree.name} Id must not be empty.");
                }

                if (string.IsNullOrWhiteSpace(tree.WoodItemId))
                {
                    errors.Add($"{tree.name} WoodItemId must not be empty.");
                }
                else if (!itemIds.Contains(tree.WoodItemId))
                {
                    errors.Add($"{tree.name} WoodItemId '{tree.WoodItemId}' is not known by ItemDatabase.");
                }

                if (tree.RequiredHits < 1)
                {
                    errors.Add($"{tree.name} RequiredHits must be >= 1.");
                }

                if (tree.WoodAmount < 1)
                {
                    errors.Add($"{tree.name} WoodAmount must be >= 1.");
                }
            }
        }

        private static void ValidateRecipes(
            IEnumerable<RecipeDataSO> recipes,
            IEnumerable<ItemDataSO> items,
            ICollection<string> errors)
        {
            var itemIds = BuildItemIdSet(items);

            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    continue;
                }

                if (recipe.RequiredWorkshopLevel < 1)
                {
                    errors.Add($"{recipe.name} RequiredWorkshopLevel must be >= 1.");
                }

                if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
                {
                    errors.Add($"{recipe.name} must have at least one ingredient.");
                }
                else
                {
                    for (var index = 0; index < recipe.Ingredients.Length; index++)
                    {
                        var ingredient = recipe.Ingredients[index];
                        if (string.IsNullOrWhiteSpace(ingredient.ItemId))
                        {
                            errors.Add($"{recipe.name} ingredient at index {index} has empty ItemId.");
                            continue;
                        }

                        if (!itemIds.Contains(ingredient.ItemId))
                        {
                            errors.Add($"{recipe.name} ingredient ItemId '{ingredient.ItemId}' is not known by ItemDatabase.");
                        }

                        if (ingredient.Amount < 1)
                        {
                            errors.Add($"{recipe.name} ingredient '{ingredient.ItemId}' Amount must be >= 1.");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(recipe.OutputItemId))
                {
                    errors.Add($"{recipe.name} OutputItemId must not be empty.");
                }
                else if (!itemIds.Contains(recipe.OutputItemId))
                {
                    errors.Add($"{recipe.name} OutputItemId '{recipe.OutputItemId}' is not known by ItemDatabase.");
                }

                if (recipe.OutputAmount < 1)
                {
                    errors.Add($"{recipe.name} OutputAmount must be >= 1.");
                }
            }
        }

        private static void ValidateWorkshops(IEnumerable<WorkshopDataSO> workshops, ICollection<string> errors)
        {
            foreach (var workshop in workshops)
            {
                if (workshop == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(workshop.Id))
                {
                    errors.Add($"{workshop.name} Id must not be empty.");
                }

                if (workshop.Level < 1)
                {
                    errors.Add($"{workshop.name} Level must be >= 1.");
                }
            }
        }

        private static HashSet<string> BuildItemIdSet(IEnumerable<ItemDataSO> items)
        {
            var itemIds = new HashSet<string>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                itemIds.Add(item.Id);
            }

            return itemIds;
        }

        private static void ValidateStartingItemAmount(
            IReadOnlyDictionary<string, int> startingAmountsById,
            string itemId,
            int expectedAmount,
            ICollection<string> errors)
        {
            if (!startingAmountsById.TryGetValue(itemId, out var amount))
            {
                errors.Add($"PlayerData StartingItems is missing {itemId} x{expectedAmount}.");
                return;
            }

            if (amount != expectedAmount)
            {
                errors.Add($"PlayerData StartingItems expected {itemId} x{expectedAmount}, found x{amount}.");
            }
        }
    }
}
