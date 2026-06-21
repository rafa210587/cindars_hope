#if UNITY_EDITOR
using System.IO;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Testing
{
    public static class CreateShopTestAssets
    {
        private const string EconomyPath = "Assets/_Game/Data/Economy";
        private const string ItemPath = "Assets/_Game/Data/Items";
        private const string NpcPath = "Assets/_Game/Data/NPCs";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        public static void CreateCompleteShopAssets()
        {
            EnsureDirectory(EconomyPath);
            EnsureDirectory(ItemPath);
            EnsureDirectory(NpcPath);

            var sword = CreateOrUpdateItem("Item_Shop_Sword_Iron", "item_shop_weapon_sword_iron", "Espada de Ferro", ItemCategory.Weapon, 1, 50, true);
            var armor = CreateOrUpdateItem("Item_Shop_Armor_Leather", "item_shop_armor_leather", "Armadura de Couro", ItemCategory.Misc, 1, 35, true);
            var tool = CreateOrUpdateItem("Item_Shop_Hoe_Basic", "item_shop_tool_hoe_basic", "Enxada Basica", ItemCategory.Tool, 1, 20, true);
            sword.Description = "Espada simples para treino e combate.";
            armor.Description = "Armadura leve para proteção inicial.";
            tool.Description = "Ferramenta básica para cultivo.";
            var bread = LoadItem("item_consumable_food_bread");
            var potion = LoadItem("item_consumable_potion_hp_small");
            var repairBasic = LoadItem("item_consumable_repair_kit_basic");
            var repairStandard = LoadItem("item_consumable_repair_kit_standard");
            AddToItemDatabase(sword, armor, tool, bread, potion, repairBasic, repairStandard);

            CreateOrUpdateShop(
                "Shop_Weapons_Armor",
                "shop_weapons_armor",
                "Armas e Armaduras",
                "npc_shop_weapons_armor",
                new[]
                {
                    new ShopItemEntry { ItemId = sword.Id, BaseDailyStock = 2 },
                    new ShopItemEntry { ItemId = armor.Id, BaseDailyStock = 3 },
                    new ShopItemEntry { ItemId = repairBasic.Id, BaseDailyStock = 5 },
                    new ShopItemEntry { ItemId = repairStandard.Id, BaseDailyStock = 3 }
                });
            CreateOrUpdateShop(
                "Shop_Seeds_Tools",
                "shop_seeds_tools",
                "Sementes e Utensilios",
                "npc_shop_seeds_tools",
                new[]
                {
                    new ShopItemEntry { ItemId = "item_seed_wheat", BaseDailyStock = 10 },
                    new ShopItemEntry { ItemId = "item_seed_carrot", BaseDailyStock = 8 },
                    new ShopItemEntry { ItemId = tool.Id, BaseDailyStock = 2 },
                    new ShopItemEntry { ItemId = bread.Id, BaseDailyStock = 8 },
                    new ShopItemEntry { ItemId = potion.Id, BaseDailyStock = 5 }
                });
            CreateOrUpdateShop(
                "Shop_General_Store",
                "shop_general_store",
                "General Store",
                string.Empty,
                new[]
                {
                    new ShopItemEntry { ItemId = "item_seed_wheat", BaseDailyStock = 10 },
                    new ShopItemEntry { ItemId = "item_seed_carrot", BaseDailyStock = 8 },
                    new ShopItemEntry { ItemId = bread.Id, BaseDailyStock = 8 },
                    new ShopItemEntry { ItemId = potion.Id, BaseDailyStock = 5 },
                    new ShopItemEntry { ItemId = repairBasic.Id, BaseDailyStock = 4 }
                });
            CreateOrUpdateShop(
                "Shop_Blacksmith",
                "shop_blacksmith",
                "Blacksmith",
                string.Empty,
                new[]
                {
                    new ShopItemEntry { ItemId = sword.Id, BaseDailyStock = 2 },
                    new ShopItemEntry { ItemId = armor.Id, BaseDailyStock = 3 },
                    new ShopItemEntry { ItemId = repairBasic.Id, BaseDailyStock = 5 },
                    new ShopItemEntry { ItemId = repairStandard.Id, BaseDailyStock = 3 }
                });
            CreateOrUpdateShop(
                "Shop_Cave_Supplies",
                "shop_cave_supplies",
                "Cave Supplies",
                string.Empty,
                new[]
                {
                    new ShopItemEntry { ItemId = potion.Id, BaseDailyStock = 6 },
                    new ShopItemEntry { ItemId = bread.Id, BaseDailyStock = 8 },
                    new ShopItemEntry { ItemId = repairBasic.Id, BaseDailyStock = 4 }
                });

            CreateOrUpdateNpc("Npc_Pip_Miudinho", "npc_pip_miudinho", "Pip Miudinho", "Bem-vindo a Cindar's Hope!", "Ate logo!", string.Empty);
            CreateOrUpdateNpc("Npc_Shop_Weapons_Armor", "npc_shop_weapons_armor", "Lojista de Armas", "Ola! Procura armas ou armaduras?", "Volte sempre!", "shop_weapons_armor");
            CreateOrUpdateNpc("Npc_Shop_Seeds_Tools", "npc_shop_seeds_tools", "Vendedor de Sementes", "Bem-vindo! Temos sementes e ferramentas.", "Bom cultivo!", "shop_seeds_tools");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SPEC 06 shop content assets updated.");
        }

        private static ItemDataSO CreateOrUpdateItem(string assetName, string id, string displayName, ItemCategory category, int maxStack, int baseValue, bool isEquippable)
        {
            var path = $"{ItemPath}/{assetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ItemDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.Id = id;
            data.DisplayName = displayName;
            data.Category = category;
            data.MaxStack = maxStack;
            data.BaseValue = baseValue;
            data.IsEquippable = isEquippable;
            EditorUtility.SetDirty(data);
            return data;
        }

        private static ItemDataSO LoadItem(string id)
        {
            var guids = AssetDatabase.FindAssets($"{id} t:ItemDataSO", new[] { ItemPath });
            foreach (var guid in guids)
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            throw new FileNotFoundException($"Required MVP shop item '{id}' was not found.");
        }

        private static void AddToItemDatabase(params ItemDataSO[] items)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database == null)
            {
                Debug.LogWarning($"ItemDatabaseSO not found at '{ItemDatabasePath}'. Shop items were created but not registered.");
                return;
            }

            var serialized = new SerializedObject(database);
            var entries = serialized.FindProperty("_items");
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var alreadyRegistered = false;
                for (var index = 0; index < entries.arraySize; index++)
                {
                    var registered = entries.GetArrayElementAtIndex(index).objectReferenceValue as ItemDataSO;
                    if (registered == item || (registered != null && registered.Id == item.Id))
                    {
                        alreadyRegistered = true;
                        break;
                    }
                }

                if (!alreadyRegistered)
                {
                    entries.InsertArrayElementAtIndex(entries.arraySize);
                    entries.GetArrayElementAtIndex(entries.arraySize - 1).objectReferenceValue = item;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void CreateOrUpdateShop(string assetName, string id, string displayName, string npcId, ShopItemEntry[] items)
        {
            var path = $"{EconomyPath}/{assetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ShopDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.Id = id;
            data.DisplayName = displayName;
            data.NpcId = npcId;
            data.BuyPriceMultiplier = 1f;
            data.SellPriceMultiplier = 0.6f;
            data.DailyRestock = true;
            data.Items = items;
            EditorUtility.SetDirty(data);
        }

        private static void CreateOrUpdateNpc(string assetName, string id, string displayName, string openingLine, string closingLine, string shopId)
        {
            var path = $"{NpcPath}/{assetName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<NpcDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<NpcDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.NpcId = id;
            data.DisplayName = displayName;
            data.OpeningLine = openingLine;
            data.ClosingLine = closingLine;
            data.ShopId = shopId;
            EditorUtility.SetDirty(data);
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
