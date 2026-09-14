using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.UI.Shop;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21SellPanelPreviewPlayModeTests
    {
        [UnityTest]
        public IEnumerator TownSellPanel_ShowsEffectiveMixedLotQuoteAndCommitsSameGold()
        {
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error, new Regex(
                    "^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            yield return SceneManager.LoadSceneAsync("TownScene", LoadSceneMode.Single);
            yield return null;
            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            var database = bootstrap.ItemDatabase as ItemDatabaseSO;
            var shop = ShopManager.Instance;
            var panel = FindInActiveScene<SellPanel>();
            var shopData = AssetDatabase.LoadAssetAtPath<ShopDataSO>(
                "Assets/_Game/Data/Economy/Shop_Renko.asset");
            var bread = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                "Assets/_Game/Data/Items/item_consumable_food_bread.asset");
            Assert.That(database, Is.Not.Null);
            Assert.That(shop, Is.Not.Null);
            Assert.That(panel, Is.Not.Null, "TownScene must contain the wired sell panel.");
            Assert.That(shopData, Is.Not.Null);
            Assert.That(bread, Is.Not.Null);
            Assert.That(shop.InitializeShop(shopData), Is.True);

            var actor = new GameObject("phase21-sell-preview-actor");
            var oldPlayer = ReadField<PlayerManager>(panel, "_playerManager");
            var oldInventory = ReadField<InventoryManager>(panel, "_inventoryManager");
            var oldModal = ReadField<CindarsHope.Foundation.IModalRuntime>(panel, "_modalManager");
            try
            {
                var inventory = actor.AddComponent<InventoryManager>();
                inventory.Initialize(database);
                var player = actor.AddComponent<PlayerManager>();
                player.Initialize();
                player.SetGold(100);
                Assert.That(inventory.TryAddPurchasedItem(bread.Id, 1, 1).Success, Is.True);
                Assert.That(inventory.TryAddItem(bread.Id, 1).Success, Is.True);
                Assert.That(inventory.TryAddItemWithLots(bread.Id, 1,
                    new[] { new ItemOriginLot(ItemOriginKind.Invalid, 1) }).Success, Is.True);
                panel.Initialize(shop, player, inventory, database, null);
                panel.Show(shopData.Id);
                yield return null;

                var row = FindVisibleRow(panel);
                Assert.That(row, Is.Not.Null);
                var amountText = ReadField<Text>(row, "_amountText");
                var priceText = ReadField<Text>(row, "_priceText");
                var amountInput = ReadField<InputField>(row, "_amountInput");
                var sellButton = ReadField<Button>(row, "_sellButton");
                Assert.That(amountText.text, Does.Contain("Vendavel: 2"));
                Assert.That(amountText.text, Does.Contain("Retido: 1"));
                amountInput.text = "2";
                Assert.That(shop.TryQuoteSellItem(shopData.Id, bread.Id, 2,
                    inventory, out var quote), Is.True);
                Assert.That(quote.LimitedUnits, Is.EqualTo(1));
                Assert.That(priceText.text, Does.Contain($"{quote.FinalGold}g"));
                Assert.That(sellButton.interactable, Is.True);

                var goldBefore = player.CurrentGold;
                sellButton.onClick.Invoke();
                Assert.That(player.CurrentGold - goldBefore, Is.EqualTo(quote.FinalGold));
                Assert.That(inventory.GetAmount(bread.Id), Is.EqualTo(1));
                Assert.That(inventory.GetEligibleAmount(bread.Id), Is.Zero);
                yield return null;
                var remaining = FindVisibleRow(panel);
                Assert.That(remaining, Is.Not.Null);
                Assert.That(ReadField<Text>(remaining, "_amountText").text,
                    Does.Contain("Retido: 1"));
                Assert.That(ReadField<Button>(remaining, "_sellButton").interactable,
                    Is.False);
            }
            finally
            {
                panel.HideVisualOnly();
                panel.Initialize(shop, oldPlayer, oldInventory, database, oldModal);
                Object.Destroy(actor);
            }
        }

        private static T FindInActiveScene<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static SellPanelItem FindVisibleRow(SellPanel panel)
        {
            foreach (var item in panel.GetComponentsInChildren<SellPanelItem>(true))
                if (item.gameObject.activeInHierarchy) return item;
            return null;
        }

        private static T ReadField<T>(object target, string name)
            => (T)target.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    }
}
