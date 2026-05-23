#if UNITY_EDITOR
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class ItemDataInitializer
    {
        private const string ItemsPath = "Assets/_Game/Data/Items/";
        private const string InitKey = "ItemDataInitialized";

        static ItemDataInitializer()
        {
            if (!SessionState.GetBool(InitKey, false))
            {
                SessionState.SetBool(InitKey, true);
                GenerateDefaultItems();
            }
        }

        private static void GenerateDefaultItems()
        {
            var itemsDir = "Assets/_Game/Data/Items";
            if (!AssetDatabase.IsValidFolder(itemsDir))
            {
                var parentDir = AssetDatabase.IsValidFolder("Assets/_Game/Data") ? "Assets/_Game/Data" : "Assets/_Game";
                AssetDatabase.CreateFolder(parentDir, "Items");
            }

            CreateSeeds();
            CreateCrops();
            CreateConsumables();
            CreateMaterials();

            AssetDatabase.SaveAssets();
        }

        private static void CreateSeeds()
        {
            var seeds = new[]
            {
                ("item_seed_wheat", "Wheat Seed"),
                ("item_seed_carrot", "Carrot Seed"),
                ("item_seed_moonbean", "Moonbean Seed"),
                ("item_seed_sunpepper", "Sunpepper Seed"),
                ("item_seed_crystal_berry", "Crystal Berry Seed"),
                ("item_seed_starroot", "Starroot Seed"),
            };

            foreach (var (id, name) in seeds)
            {
                CreateItemDataSO(id, name, ItemCategory.Seed, ConsumableSubtype.None, 1, 10, 0, true);
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
                CreateItemDataSO(id, name, ItemCategory.Consumable, subtype, maxStack, baseValue, hunger, true);
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

        private static void CreateItemDataSO(
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
                return;

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
        }
    }
}
#endif
