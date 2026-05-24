#if UNITY_EDITOR
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using CindarsHope.Player;
using CindarsHope.UI.Dialogue;
using CindarsHope.UI.Shop;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Testing
{
    public class CreateNpcPrefabs
    {
        private const string PrefabPath = "Assets/_Game/Prefabs/NPCs";

        [MenuItem("CindarsHope/Testing/Create NPC Prefabs (Complete Spec 06)")]
        public static void CreateNpcPrefabs_Complete()
        {
            EnsureDirectory();

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("        CREATING NPC PREFABS (SPEC 06)");
            Debug.Log("═══════════════════════════════════════════════════════\n");

            CreateWeaponsArmorShopkeeper();
            CreateSeedsToolsShopkeeper();
            CreatePip();

            Debug.Log($"\n═══════════════════════════════════════════════════════");
            Debug.Log($"  ✓ Created 3 NPC prefabs successfully");
            Debug.Log($"═══════════════════════════════════════════════════════");
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists(PrefabPath))
                Directory.CreateDirectory(PrefabPath);
        }

        private static void CreateWeaponsArmorShopkeeper()
        {
            var prefabName = "NPC_WeaponsArmorShop";
            var go = new GameObject(prefabName);

            var shopData = AssetDatabase.LoadAssetAtPath<ShopDataSO>("Assets/_Game/Data/Economy/Shop_Weapons_Armor.asset");
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Weapons_Armor.asset");

            var controller = go.AddComponent<NpcShopController>();
            controller.GetType().GetField("_shopData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, shopData);
            controller.GetType().GetField("_npcData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, npcData);

            var prefabPath = $"{PrefabPath}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"  ✓ Created: {prefabName}");
        }

        private static void CreateSeedsToolsShopkeeper()
        {
            var prefabName = "NPC_SeedsToolsShop";
            var go = new GameObject(prefabName);

            var shopData = AssetDatabase.LoadAssetAtPath<ShopDataSO>("Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset");
            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Seeds_Tools.asset");

            var controller = go.AddComponent<NpcShopController>();
            controller.GetType().GetField("_shopData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, shopData);
            controller.GetType().GetField("_npcData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, npcData);

            var prefabPath = $"{PrefabPath}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"  ✓ Created: {prefabName}");
        }

        private static void CreatePip()
        {
            var prefabName = "NPC_Pip_Receptionist";
            var go = new GameObject(prefabName);

            var npcData = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset");

            var controller = go.AddComponent<NpcShopController>();
            controller.GetType().GetField("_npcData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(controller, npcData);
            // Pip has no shop, so _shopData stays null

            var prefabPath = $"{PrefabPath}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"  ✓ Created: {prefabName}");
        }
    }
}
#endif
