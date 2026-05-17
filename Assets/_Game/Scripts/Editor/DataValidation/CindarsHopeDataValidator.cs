using System;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Farm.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
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
            "item_tool_fishing_rod_basic"
        };

        private static readonly string[] RequiredSeedIds =
        {
            "seed_wheat",
            "seed_carrot"
        };

        [MenuItem("CindarsHope/Validate/Validate MVP Data")]
        public static void ValidateMvpData()
        {
            var errors = new List<string>();

            var itemDatabase = FindRequiredAsset<ItemDatabaseSO>(errors, nameof(ItemDatabaseSO));
            var seedDatabase = FindRequiredAsset<SeedDatabaseSO>(errors, nameof(SeedDatabaseSO));
            var playerData = FindRequiredAsset<PlayerDataSO>(errors, nameof(PlayerDataSO));

            if (itemDatabase != null)
            {
                var items = LoadRegistryItems<ItemDatabaseSO, ItemDataSO>(itemDatabase, errors, nameof(ItemDatabaseSO));
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
