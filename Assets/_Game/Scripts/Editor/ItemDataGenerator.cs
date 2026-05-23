#if UNITY_EDITOR
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    public static class ItemDataGenerator
    {
        private const string ItemsPath = "Assets/_Game/Data/Items/";

        [MenuItem("CindarsHope/Generate/Create Item Examples")]
        public static void CreateItemExamples()
        {
            CreateSeeds();
            CreateCrops();
            CreateConsumables();
            CreateMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Item examples created successfully!");
        }

        private static void CreateSeeds()
        {
            var seeds = new[]
            {
                ("item_seed_wheat", "Wheat Seed", 5, 2, 2),
                ("item_seed_carrot", "Carrot Seed", 8, 3, 3),
                ("item_seed_moonbean", "Moonbean Seed", 18, 7, 4),
                ("item_seed_sunpepper", "Sunpepper Seed", 24, 10, 4),
                ("item_seed_crystal_berry", "Crystal Berry Seed", 45, 18, 5),
                ("item_seed_starroot", "Starroot Seed", 75, 30, 6),
            };

            foreach (var (id, name, buyPrice, sellPrice, growthDays) in seeds)
            {
                CreateItemDataSO(id, name, ItemCategory.Seed, ConsumableSubtype.None, 1, buyPrice, 0, true);
            }
        }

        private static void CreateCrops()
        {
            var crops = new[]
            {
                ("item_crop_wheat", "Wheat"),
                ("item_crop_carrot", "Carrot"),
                ("item_crop_moonbean", "Moonbean"),
                ("item_crop_sunpepper", "Sunpepper"),
                ("item_crop_crystal_berry", "Crystal Berry"),
                ("item_crop_starroot", "Starroot"),
            };

            foreach (var (id, name) in crops)
            {
                CreateItemDataSO(id, name, ItemCategory.Crop, ConsumableSubtype.None, 99, 0, 0, false);
            }
        }

        private static void CreateConsumables()
        {
            var consumables = new[]
            {
                ("item_consumable_potion_hp_small", "Small HP Potion", ConsumableSubtype.Potion, 99, 0, 10),
                ("item_consumable_food_bread", "Bread", ConsumableSubtype.Food, 99, 0, 8),
                ("item_consumable_food_carrot_stew", "Carrot Stew", ConsumableSubtype.Food, 99, 0, 6),
                ("item_consumable_food_moonbean_soup", "Moonbean Soup", ConsumableSubtype.BuffFood, 99, 0, 15),
                ("item_consumable_food_spicy_sunpepper", "Spicy Sunpepper", ConsumableSubtype.BuffFood, 99, 0, 20),
                ("item_consumable_food_crystal_jam", "Crystal Jam", ConsumableSubtype.Food, 99, 0, 18),
                ("item_consumable_food_starroot_pie", "Starroot Pie", ConsumableSubtype.BuffFood, 99, 0, 30),
            };

            foreach (var (id, name, subtype, maxStack, baseValue, hunger) in consumables)
            {
                var asset = CreateItemDataSO(id, name, ItemCategory.Consumable, subtype, maxStack, baseValue, hunger, true);
                asset.ConsumableSubtype = subtype;
            }
        }

        private static void CreateMaterials()
        {
            var materials = new[]
            {
                ("item_material_wood", "Wood"),
                ("item_material_stone", "Stone"),
                ("item_material_copper_ore", "Copper Ore"),
                ("item_material_iron_ore", "Iron Ore"),
            };

            foreach (var (id, name) in materials)
            {
                CreateItemDataSO(id, name, ItemCategory.Material, ConsumableSubtype.None, 99, 0, 0, false);
            }
        }

        private static ItemDataSO CreateItemDataSO(
            string id,
            string displayName,
            ItemCategory category,
            ConsumableSubtype subtype,
            int maxStack,
            int baseValue,
            int hungerRestore,
            bool isEquippable)
        {
            var path = $"{ItemsPath}{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (existing != null)
            {
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<ItemDataSO>();
            asset.Id = id;
            asset.DisplayName = displayName;
            asset.Description = $"Item: {displayName}";
            asset.Category = category;
            asset.ConsumableSubtype = subtype;
            asset.MaxStack = maxStack;
            asset.BaseValue = baseValue;
            asset.HungerRestore = hungerRestore;
            asset.IsEquippable = isEquippable;

            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
#endif
