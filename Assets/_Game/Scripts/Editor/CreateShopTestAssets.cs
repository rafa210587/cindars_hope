#if UNITY_EDITOR
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Testing
{
    public class CreateShopTestAssets
    {
        private const string DataPath = "Assets/_Game/Data/Shops";
        private const string NpcDataPath = "Assets/_Game/Data/NPCs";
        private static int _createdCount = 0;

        [MenuItem("CindarsHope/Testing/Create Shop Assets (Complete Spec 06)")]
        public static void CreateCompleteShopAssets()
        {
            _createdCount = 0;
            EnsureDirectories();

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("        CREATING COMPLETE SPEC 06 SHOP ASSETS");
            Debug.Log("═══════════════════════════════════════════════════════\n");

            CreateWeaponsArmorShop();
            CreateSeedsToolsShop();
            CreateNpcDialogueData();

            AssetDatabase.Refresh();
            Debug.Log($"\n═══════════════════════════════════════════════════════");
            Debug.Log($"  ✓ Created {_createdCount} assets successfully");
            Debug.Log($"═══════════════════════════════════════════════════════");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(NpcDataPath))
                Directory.CreateDirectory(NpcDataPath);
        }

        private static void CreateWeaponsArmorShop()
        {
            var path = $"{DataPath}/ShopData_WeaponsArmor.asset";

            // Delete if exists to avoid duplicates
            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "shop_weapons_armor";
            shopData.ShopKeeperId = "npc_shop_weapons_armor";
            shopData.BaseDailyStock = 5;
            shopData.PriceMultiplier = 1.0f;
            shopData.Items = new ShopItemEntry[]
            {
                new ShopItemEntry { ItemId = "item_weapon_wooden_sword", MaxStock = 3 },
                new ShopItemEntry { ItemId = "item_armor_cloth_shirt", MaxStock = 5 },
                new ShopItemEntry { ItemId = "item_accessory_leather_gloves", MaxStock = 4 }
            };

            AssetDatabase.CreateAsset(shopData, path);
            _createdCount++;
            Debug.Log($"  ✓ Created: {shopData.Id}");
        }

        private static void CreateSeedsToolsShop()
        {
            var path = $"{DataPath}/ShopData_SeedsTools.asset";

            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "shop_seeds_tools";
            shopData.ShopKeeperId = "npc_shop_seeds_tools";
            shopData.BaseDailyStock = 10;
            shopData.PriceMultiplier = 1.0f;
            shopData.Items = new ShopItemEntry[]
            {
                new ShopItemEntry { ItemId = "seed_wheat", MaxStock = 10 },
                new ShopItemEntry { ItemId = "seed_carrot", MaxStock = 8 },
                new ShopItemEntry { ItemId = "item_tool_hoe_basic", MaxStock = 2 },
                new ShopItemEntry { ItemId = "item_tool_watering_can_basic", MaxStock = 2 },
                new ShopItemEntry { ItemId = "item_potion_health_basic", MaxStock = 5 }
            };

            AssetDatabase.CreateAsset(shopData, path);
            _createdCount++;
            Debug.Log($"  ✓ Created: {shopData.Id}");
        }

        private static void CreateNpcDialogueData()
        {
            // Pip receptionist
            CreateDialogueData("npc_pip", "Bem-vindo a Cindar's Hope!", "Até logo!");

            // Weapons/Armor shopkeeper
            CreateDialogueData("npc_shop_weapons_armor", "Olá! Procura armas ou armaduras?", "Volte sempre!");

            // Seeds/Tools shopkeeper
            CreateDialogueData("npc_shop_seeds_tools", "Bem-vindo! Temos sementes e ferramentas.", "Bom cultivo!");
        }

        private static void CreateDialogueData(string npcId, string openingLine, string closingLine)
        {
            var path = $"{NpcDataPath}/NpcDialogue_{npcId}.asset";

            if (File.Exists(path))
                AssetDatabase.DeleteAsset(path);

            var dialogueData = ScriptableObject.CreateInstance<NpcDialogueDataSO>();
            dialogueData.Id = npcId;
            dialogueData.OpeningLine = openingLine;
            dialogueData.ClosingLine = closingLine;

            AssetDatabase.CreateAsset(dialogueData, path);
            _createdCount++;
            Debug.Log($"  ✓ Created: {npcId}");
        }
    }
}
#endif
