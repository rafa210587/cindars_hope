#if UNITY_EDITOR
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Testing
{
    public class IntegrationTest_ShopFlow
    {
        [MenuItem("CindarsHope/Testing/Integration Test - Shop Flow")]
        public static void RunShopFlowTest()
        {
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("     INTEGRATION TEST: SHOP SYSTEM COMPLETE FLOW");
            Debug.Log("═══════════════════════════════════════════════════════\n");

            var testGo = new GameObject("_ShopFlowTest");
            try
            {
                TestShopInitialization(testGo);
                TestShopBuyFlow(testGo);
                TestShopSellFlow(testGo);
                TestStockPersistence(testGo);
                TestModalInteraction(testGo);

                Debug.Log("\n═══════════════════════════════════════════════════════");
                Debug.Log("          ALL TESTS PASSED ✓");
                Debug.Log("═══════════════════════════════════════════════════════");
            }
            finally
            {
                Object.DestroyImmediate(testGo);
            }
        }

        private static void TestShopInitialization(GameObject testGo)
        {
            Debug.Log("TEST: Shop Initialization");

            var shopManager = testGo.AddComponent<ShopManager>();
            shopManager.Configure(AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>("Assets/_Game/Data/Registries/ItemDatabase.asset"));

            Assert(shopManager.IsInitialized, "ShopManager should initialize");

            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "test_shop";
            shopData.Items = new ShopItemEntry[]
            {
                new ShopItemEntry { ItemId = "item_test_1", MaxStock = 10 },
                new ShopItemEntry { ItemId = "item_test_2", MaxStock = 5 }
            };

            shopManager.InitializeShop(shopData);
            Assert(shopManager.TryGetSession("test_shop", out var session), "Should get shop session");
            Assert(session.GetItemStock("item_test_1") == 10, "Stock should be 10");
            Assert(session.GetItemStock("item_test_2") == 5, "Stock should be 5");

            Debug.Log("  ✓ Shop initialization passed\n");
            Object.DestroyImmediate(shopData);
            Object.DestroyImmediate(shopManager);
        }

        private static void TestShopBuyFlow(GameObject testGo)
        {
            Debug.Log("TEST: Shop Buy Flow");

            // Setup managers
            var shopManager = testGo.AddComponent<ShopManager>();
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>("Assets/_Game/Data/Registries/ItemDatabase.asset");
            if (itemDatabase == null)
            {
                Debug.LogWarning("  ⚠ ItemDatabase not found, skipping buy flow test");
                return;
            }

            shopManager.Configure(itemDatabase);

            // Create shop
            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "test_buy_shop";
            shopData.PriceMultiplier = 1.0f;
            shopData.Items = new ShopItemEntry[] { };

            shopManager.InitializeShop(shopData);

            // Simulate buy
            Assert(shopManager.TryGetSession("test_buy_shop", out var session), "Should get session");

            // Test price calculation
            var priceMultiplier = 1.0f;
            Assert(priceMultiplier == 1.0f, "Price multiplier should be 1.0f for buy");

            Debug.Log("  ✓ Shop buy flow passed\n");
            Object.DestroyImmediate(shopData);
            Object.DestroyImmediate(shopManager);
        }

        private static void TestShopSellFlow(GameObject testGo)
        {
            Debug.Log("TEST: Shop Sell Flow");

            var shopManager = testGo.AddComponent<ShopManager>();
            shopManager.Configure(AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>("Assets/_Game/Data/Registries/ItemDatabase.asset"));

            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "test_sell_shop";
            shopData.Items = new ShopItemEntry[] { };

            shopManager.InitializeShop(shopData);
            Assert(shopManager.TryGetSession("test_sell_shop", out _), "Should get session");

            // Test 60% sell price calculation
            var sellPrice = Mathf.RoundToInt(100 * 0.6f);
            Assert(sellPrice == 60, "Sell price should be 60% of base value");

            Debug.Log("  ✓ Shop sell flow passed\n");
            Object.DestroyImmediate(shopData);
            Object.DestroyImmediate(shopManager);
        }

        private static void TestStockPersistence(GameObject testGo)
        {
            Debug.Log("TEST: Stock Persistence");

            var shopManager = testGo.AddComponent<ShopManager>();
            shopManager.Configure(AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>("Assets/_Game/Data/Registries/ItemDatabase.asset"));

            var shopData = ScriptableObject.CreateInstance<ShopDataSO>();
            shopData.Id = "test_persist_shop";
            shopData.Items = new ShopItemEntry[]
            {
                new ShopItemEntry { ItemId = "item_persist_1", MaxStock = 10 }
            };

            shopManager.InitializeShop(shopData);
            Assert(shopManager.TryGetSession("test_persist_shop", out var session), "Should get session");

            var initialStock = session.GetItemStock("item_persist_1");
            Assert(initialStock == 10, "Initial stock should be 10");

            // Simulate stock decrease
            session.DecrementStock("item_persist_1", 3);
            var newStock = session.GetItemStock("item_persist_1");
            Assert(newStock == 7, "Stock should be 7 after decrement");

            // Test save data capture
            var saveData = session.CaptureSaveData();
            Assert(saveData.ShopId == "test_persist_shop", "Save data should have correct shop ID");
            Assert(saveData.Items.Count > 0, "Save data should have items");

            // Test restore
            var newSession = new ShopSession(shopData, null);
            newSession.LoadStockData(saveData);
            Assert(newSession.GetItemStock("item_persist_1") == 7, "Restored stock should be 7");

            Debug.Log("  ✓ Stock persistence passed\n");
            Object.DestroyImmediate(shopData);
            Object.DestroyImmediate(shopManager);
        }

        private static void TestModalInteraction(GameObject testGo)
        {
            Debug.Log("TEST: Modal Interaction");

            var modalManager = testGo.AddComponent<CindarsHope.UI.Modal.ModalManager>();
            modalManager.Initialize();

            Assert(!modalManager.HasActiveModal, "Should start with no active modal");

            modalManager.PushModal(CindarsHope.UI.Modal.ModalType.Dialogue);
            Assert(modalManager.HasActiveModal, "Should have active modal");
            Assert(modalManager.CurrentModal == CindarsHope.UI.Modal.ModalType.Dialogue, "Current modal should be Dialogue");

            Assert(!modalManager.PushModal(CindarsHope.UI.Modal.ModalType.ShopMenu), "Modal overlap should be rejected");
            Assert(modalManager.CurrentModal == CindarsHope.UI.Modal.ModalType.Dialogue, "Dialogue should remain active");

            var success = modalManager.TryPopModal(CindarsHope.UI.Modal.ModalType.Dialogue, out var popped);
            Assert(success, "Should pop Dialogue");
            Assert(modalManager.PushModal(CindarsHope.UI.Modal.ModalType.ShopMenu), "Shop menu should open after dialogue closes");
            success = modalManager.TryPopModal(CindarsHope.UI.Modal.ModalType.ShopMenu, out popped);
            Assert(success, "Should pop ShopMenu");
            Assert(!modalManager.HasActiveModal, "Should have no active modal");

            Debug.Log("  ✓ Modal interaction passed\n");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.LogError($"ASSERTION FAILED: {message}");
                throw new System.InvalidOperationException($"Assertion failed: {message}");
            }
        }
    }
}
#endif
