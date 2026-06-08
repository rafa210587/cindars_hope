#if UNITY_EDITOR
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CindarsHope.Editor.Validation
{
    public class ValidateShopSystem
    {
        private static List<string> _validationResults = new List<string>();
        private static int _passCount = 0;
        private static int _failCount = 0;

        [MenuItem("CindarsHope/Archive/Validation/Validate Shop System")]
        public static void ValidateShops()
        {
            _validationResults.Clear();
            _passCount = 0;
            _failCount = 0;

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("          VALIDATING SPEC 06 SHOP SYSTEM");
            Debug.Log("═══════════════════════════════════════════════════════");

            // 1. Validate shop data assets exist
            ValidateShopDataAssets();

            // 2. Validate NPC dialogue data exists
            ValidateNpcDialogueAssets();

            // 3. Validate shop manager can be instantiated
            ValidateShopManager();

            // 4. Validate modal manager
            ValidateModalManager();

            // 5. Validate dialogue modal
            ValidateDialogueModal();

            // Print results
            PrintResults();
        }

        private static void ValidateShopDataAssets()
        {
            var weaponsShop = AssetDatabase.LoadAssetAtPath<ShopDataSO>("Assets/_Game/Data/Economy/Shop_Weapons_Armor.asset");
            var seedsShop = AssetDatabase.LoadAssetAtPath<ShopDataSO>("Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset");

            CheckAsset("Weapons/Armor Shop Asset", weaponsShop);
            CheckAsset("Seeds/Tools Shop Asset", seedsShop);

            if (weaponsShop != null)
            {
                Check($"Weapons Shop ID = '{weaponsShop.Id}'", weaponsShop.Id == "shop_weapons_armor");
                Check($"Weapons Shop has items", weaponsShop.Items != null && weaponsShop.Items.Length > 0);
                if (weaponsShop.Items != null)
                {
                    Check($"Weapons Shop item count", weaponsShop.Items.Length >= 2);
                }
            }

            if (seedsShop != null)
            {
                Check($"Seeds Shop ID = '{seedsShop.Id}'", seedsShop.Id == "shop_seeds_tools");
                Check($"Seeds Shop has items", seedsShop.Items != null && seedsShop.Items.Length > 0);
                if (seedsShop.Items != null)
                {
                    Check($"Seeds Shop item count", seedsShop.Items.Length >= 3);
                }
            }
        }

        private static void ValidateNpcDialogueAssets()
        {
            var pipDialogue = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Pip_Miudinho.asset");
            var weaponsDialogue = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Weapons_Armor.asset");
            var seedsDialogue = AssetDatabase.LoadAssetAtPath<NpcDataSO>("Assets/_Game/Data/NPCs/Npc_Shop_Seeds_Tools.asset");

            CheckAsset("Pip Dialogue Data", pipDialogue);
            CheckAsset("Weapons Shop Dialogue Data", weaponsDialogue);
            CheckAsset("Seeds Shop Dialogue Data", seedsDialogue);

            if (pipDialogue != null)
            {
                Check($"Pip has opening line", !string.IsNullOrEmpty(pipDialogue.OpeningLine));
                Check($"Pip has closing line", !string.IsNullOrEmpty(pipDialogue.ClosingLine));
            }

            if (weaponsDialogue != null)
            {
                Check($"Weapons Shopkeeper has opening line", !string.IsNullOrEmpty(weaponsDialogue.OpeningLine));
            }

            if (seedsDialogue != null)
            {
                Check($"Seeds Shopkeeper has opening line", !string.IsNullOrEmpty(seedsDialogue.OpeningLine));
            }
        }

        private static void ValidateShopManager()
        {
            var go = new GameObject("_TestShopManager");
            var shopManager = go.AddComponent<ShopManager>();
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>("Assets/_Game/Data/Registries/ItemDatabase.asset");
            shopManager.Configure(itemDatabase);

            Check("ShopManager initializes", shopManager.IsInitialized);

            var testShop = ScriptableObject.CreateInstance<ShopDataSO>();
            testShop.Id = "test_shop";
            testShop.Items = new ShopItemEntry[0];

            shopManager.InitializeShop(testShop);
            var sessionExists = shopManager.TryGetSession("test_shop", out _);
            Check("ShopManager can initialize shop", sessionExists);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(testShop);
        }

        private static void ValidateModalManager()
        {
            var go = new GameObject("_TestModalManager");
            var modalManager = go.AddComponent<CindarsHope.UI.Modal.ModalManager>();
            modalManager.Initialize();

            Check("ModalManager initializes", modalManager.IsInitialized);
            Check("ModalManager starts with no active modal", !modalManager.HasActiveModal);

            modalManager.PushModal(CindarsHope.UI.Modal.ModalType.Dialogue);
            Check("ModalManager can push modal", modalManager.HasActiveModal);

            var success = modalManager.TryPopModal(CindarsHope.UI.Modal.ModalType.Dialogue, out _);
            Check("ModalManager can pop modal", success);
            Check("ModalManager has no active modal after pop", !modalManager.HasActiveModal);

            Object.DestroyImmediate(go);
        }

        private static void ValidateDialogueModal()
        {
            var go = new GameObject("_TestDialogueModal");
            var canvasGroup = go.AddComponent<CanvasGroup>();
            var dialogueText = new GameObject("Text").AddComponent<Text>();
            dialogueText.transform.SetParent(go.transform);
            var button = new GameObject("Button").AddComponent<Button>();
            button.transform.SetParent(go.transform);

            var dialogueModal = go.AddComponent<CindarsHope.UI.Dialogue.DialogueModal>();

            Check("DialogueModal component exists", dialogueModal != null);

            var modalManager = go.AddComponent<CindarsHope.UI.Modal.ModalManager>();
            modalManager.Initialize();
            dialogueModal.Initialize(modalManager);

            Check("DialogueModal initializes", dialogueModal != null);

            Object.DestroyImmediate(go);
        }

        private static void CheckAsset(string name, Object asset)
        {
            if (asset != null)
            {
                Check(name, true);
            }
            else
            {
                Check(name, false);
            }
        }

        private static void Check(string testName, bool result)
        {
            if (result)
            {
                _validationResults.Add($"  ✓ {testName}");
                _passCount++;
            }
            else
            {
                _validationResults.Add($"  ✗ {testName}");
                _failCount++;
            }
        }

        private static void PrintResults()
        {
            foreach (var result in _validationResults)
            {
                Debug.Log(result);
            }

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log($"  Results: {_passCount} passed, {_failCount} failed");
            Debug.Log("═══════════════════════════════════════════════════════");
        }
    }
}
#endif
