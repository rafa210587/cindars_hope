using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Farm.Shipping;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21PurchasedOriginEconomyPlayModeTests
    {
        [UnityTest]
        [Explicit("Dedicated FarmScene integration batch; saves to a GUID temporary slot.")]
        public IEnumerator FarmRenkoSession_SellPointAndShippingAfterReload_KeepPurchasedCap()
        {
            // The isolated FarmScene fixture may boot without the death-only recovery service;
            // this same baseline is expected by the existing Phase21 economy PlayMode probe.
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error, new Regex(
                    "^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            var tempDirectory = Path.Combine(Application.temporaryCachePath,
                "town-interior-save-tests", Guid.NewGuid().ToString("N"));
            SaveManager save = null;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onSceneLoaded = (scene, mode) =>
            {
                if (scene.name != "FarmScene") return;
                save = GameBootstrap.Instance?.SaveManager as SaveManager;
                save?.ConfigureTemporarySaveDirectoryForValidation(tempDirectory);
            };
            SceneManager.sceneLoaded += onSceneLoaded;
            try { yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single); }
            finally { SceneManager.sceneLoaded -= onSceneLoaded; }
            yield return null;

            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            save = save ?? bootstrap.SaveManager as SaveManager;
            Assert.That(save, Is.Not.Null);
            save.ConfigureTemporarySaveDirectoryForValidation(tempDirectory);
            Assert.That(Path.GetFullPath(save.SaveFilePath),
                Is.EqualTo(Path.Combine(Path.GetFullPath(tempDirectory), "slot_1.json")));
            var inventory = bootstrap.InventoryManager as InventoryManager;
            var player = bootstrap.PlayerManager as PlayerManager;
            var database = bootstrap.ItemDatabase as ItemDatabaseSO;
            var shopManager = ShopManager.Instance;
            var shipping = ShippingBinRuntimeService.Instance;
            Assert.That(inventory, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            Assert.That(database, Is.Not.Null);
            Assert.That(shopManager, Is.Not.Null);
            Assert.That(shipping, Is.Not.Null);

            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(
                "Assets/_Game/Data/Economy/Shop_Renko.asset");
            var bread = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                "Assets/_Game/Data/Items/item_consumable_food_bread.asset");
            Assert.That(shop, Is.Not.Null);
            Assert.That(bread, Is.Not.Null);
            Assert.That(shop.GetEntry(bread.Id), Is.Not.Null);
            var sellPoint = SingleSceneComponent<SellPoint>();
            var shippingBin = SingleSceneComponent<ShippingBinInteractable>();
            sellPoint.RebindRuntimeManagers(inventory, player);
            var originalMarketSource = MarketSkillModifierProvider.MultiplierSource;
            try
            {
                inventory.Clear();
                shipping.RestoreFromSaveData(null);
                shopManager.Shutdown();
                shopManager.Initialize();
                shopManager.Configure(database);
                Assert.That(shopManager.InitializeShop(shop), Is.True);

                var state = MarketStateAtRankThree();
                MarketSkillModifierProvider.MultiplierSource = direction =>
                    MarketSkillModifierProvider.Resolve(state.GetRank(MarketSkillModifierProvider.NodeId),
                        state.GetChosenVariant(MarketSkillModifierProvider.NodeId), direction);
                var paid = ShopManager.CalculateBuyPrice(bread, shop.GetEntry(bread.Id), shop);
                Assert.That(paid, Is.GreaterThan(1));
                player.SetGold(paid);
                var purchase = shopManager.TryBuyItem(shop.Id, bread.Id, 1, player, inventory);
                Assert.That(purchase.Success, Is.True, purchase.Message);
                Assert.That(player.CurrentGold, Is.Zero);
                AssertPurchasedBread(inventory, bread.Id, paid);

                state.FullRespec(restoredPoints: 3);
                Assert.That(state.GetRank(MarketSkillModifierProvider.NodeId), Is.Zero);
                Assert.That(sellPoint.CanInteract(player.gameObject), Is.True);
                var directPayout = CaptureTransactionPayout("SellPoint",
                    () => sellPoint.Interact(player.gameObject));
                Assert.That(inventory.GetAmount(bread.Id), Is.Zero);
                Assert.That(directPayout, Is.InRange(0, paid - 1));

                // Earned bread keeps the normal post-respec price in the same physical channel.
                inventory.AddItem(bread.Id, 1);
                player.SetGold(0);
                var earnedPayout = CaptureTransactionPayout("SellPoint",
                    () => sellPoint.Interact(player.gameObject));
                Assert.That(earnedPayout, Is.GreaterThanOrEqualTo(directPayout));
                Assert.That(inventory.GetAmount(bread.Id), Is.Zero);

                var nextState = MarketStateAtRankThree();
                MarketSkillModifierProvider.MultiplierSource = direction =>
                    MarketSkillModifierProvider.Resolve(nextState.GetRank(MarketSkillModifierProvider.NodeId),
                        nextState.GetChosenVariant(MarketSkillModifierProvider.NodeId), direction);
                var secondPaid = ShopManager.CalculateBuyPrice(bread, shop.GetEntry(bread.Id), shop);
                player.SetGold(secondPaid);
                purchase = shopManager.TryBuyItem(shop.Id, bread.Id, 1, player, inventory);
                Assert.That(purchase.Success, Is.True, purchase.Message);
                AssertPurchasedBread(inventory, bread.Id, secondPaid);
                Assert.That(player.CurrentGold, Is.Zero);
                Assert.That(shippingBin.CanInteract(player.gameObject), Is.True);
                shippingBin.Interact(player.gameObject);
                Assert.That(inventory.GetAmount(bread.Id), Is.Zero);
                Assert.That(shipping.PendingEntries.Count, Is.EqualTo(1));
                Assert.That(shipping.PendingEntries[0].OriginLots[0].UnitPaidBasisGold,
                    Is.EqualTo(secondPaid));
                var preSaveGold = player.CurrentGold;

                Assert.That(save.SaveGame(), Is.True);
                Assert.That(File.Exists(save.SaveFilePath), Is.True);
                var savedJson = File.ReadAllText(save.SaveFilePath);
                Assert.That(savedJson, Does.Contain("OriginSchemaVersion"));
                shipping.RestoreFromSaveData(null);
                player.SetGold(123);
                Assert.That(save.LoadGame(), Is.True);
                Assert.That(player.CurrentGold, Is.EqualTo(preSaveGold));
                Assert.That(shipping.PendingEntries.Count, Is.EqualTo(1));
                Assert.That(shipping.PendingEntries[0].OriginLots[0].UnitPaidBasisGold,
                    Is.EqualTo(secondPaid));

                var nextDay = shipping.PendingEntries[0].ProcessOnDay;
                var shippedPayout = CaptureTransactionPayout("shipping",
                    () => shipping.ProcessOvernight(nextDay, player));
                Assert.That(shippedPayout, Is.InRange(0, secondPaid - 1));
                Assert.That(player.CurrentGold, Is.GreaterThanOrEqualTo(preSaveGold + shippedPayout));
                var postShipmentGold = player.CurrentGold;
                shipping.ProcessOvernight(nextDay, player);
                Assert.That(player.CurrentGold, Is.EqualTo(postShipmentGold));
                Assert.That(save.SaveGame(), Is.True);
                Assert.That(save.LoadGame(), Is.True);
                Assert.That(player.CurrentGold, Is.EqualTo(postShipmentGold));
                shipping.ProcessOvernight(nextDay, player);
                Assert.That(player.CurrentGold, Is.EqualTo(postShipmentGold));
                TestContext.WriteLine($"AC07 controlled FarmScene: Renko bread buy={paid}g, " +
                    $"SellPoint={directPayout}g, earned={earnedPayout}g; second buy={secondPaid}g, " +
                    $"Shipping after save/reload={shippedPayout}g, same-day retry=0g.");
            }
            finally
            {
                MarketSkillModifierProvider.MultiplierSource = originalMarketSource;
                shopManager.Shutdown();
                shopManager.Initialize();
                var fullDirectory = Path.GetFullPath(tempDirectory);
                var allowedParent = Path.GetFullPath(Path.Combine(Application.temporaryCachePath,
                    "town-interior-save-tests"));
                if (Directory.Exists(fullDirectory)
                    && string.Equals(Path.GetDirectoryName(fullDirectory), allowedParent,
                        StringComparison.OrdinalIgnoreCase)
                    && Guid.TryParseExact(Path.GetFileName(fullDirectory), "N", out _))
                    Directory.Delete(fullDirectory, true);
            }
            yield return null;
        }

        private static SkillTreeState MarketStateAtRankThree()
        {
            var state = new SkillTreeState(3);
            state.Purchase(MarketSkillModifierProvider.NodeId, 1, "crafting");
            state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
            state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
            state.SetChosenVariant(MarketSkillModifierProvider.NodeId,
                MarketSkillModifierProvider.BuyVariantId);
            return state;
        }

        private static int CaptureTransactionPayout(string transactionType, Action transaction)
        {
            var count = 0;
            var payout = 0;
            Action<EconomyTransactionCompletedEvent> onTransaction = evt =>
            {
                if (!evt.WasSuccessful || evt.TransactionType != transactionType) return;
                count++;
                payout = evt.GoldDelta;
            };
            GameEventBus.Subscribe(onTransaction);
            try { transaction(); }
            finally { GameEventBus.Unsubscribe(onTransaction); }
            Assert.That(count, Is.EqualTo(1), transactionType + " must complete exactly one transaction.");
            return payout;
        }

        private static void AssertPurchasedBread(InventoryManager inventory, string itemId, int paid)
        {
            Assert.That(inventory.TryPrepareRemoval(itemId, 1, out var selection), Is.True);
            var lots = selection.CopyLots();
            Assert.That(lots.Count, Is.EqualTo(1));
            Assert.That(lots[0].Kind, Is.EqualTo(ItemOriginKind.Purchased));
            Assert.That(lots[0].UnitPaidBasisGold, Is.EqualTo(paid));
            Assert.That(lots[0].UnitResaleCapGold, Is.EqualTo(paid - 1));
        }

        private static T SingleSceneComponent<T>() where T : Component
        {
            var results = new List<T>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                results.AddRange(root.GetComponentsInChildren<T>(true));
            Assert.That(results.Count, Is.EqualTo(1), typeof(T).Name + " must be materialized once in FarmScene.");
            return results[0];
        }
    }
}
