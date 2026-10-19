using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21EconomyAcceptancePlayModeTests
    {
        [UnityTest]
        [Explicit("AC05 conhecido: compra R3 por 17g e SellPoint paga 20g; executar pelo nome completo ate a politica economica aprovada corrigir o ciclo")]
        public IEnumerator SensoMercado_R3CompraRespecVendaEmSellPointMaterializadoNaFarm_NaoPodeGerarLucro()
        {
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error, new Regex(
                    "^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;

            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            var database = bootstrap.ItemDatabase as ItemDatabaseSO;
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(
                "Assets/_Game/Data/Economy/Shop_Renko.asset");
            var bread = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                "Assets/_Game/Data/Items/item_consumable_food_bread.asset");
            Assert.That(database, Is.Not.Null);
            Assert.That(shop, Is.Not.Null);
            Assert.That(bread, Is.Not.Null);
            Assert.That(database.TryGetById(bread.Id, out var registered), Is.True);
            Assert.That(registered, Is.SameAs(bread));
            Assert.That(shop.GetEntry(bread.Id), Is.Not.Null);

            var sellPoints = new List<SellPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                sellPoints.AddRange(root.GetComponentsInChildren<SellPoint>(true));
            Assert.That(sellPoints.Count, Is.EqualTo(1),
                "O teste requer o SellPoint materializado pela FarmScene.");
            var sellPoint = sellPoints[0];
            var originalInventory = bootstrap.InventoryManager as InventoryManager;
            var originalPlayer = bootstrap.PlayerManager;
            Assert.That(originalInventory, Is.Not.Null);
            Assert.That(originalPlayer, Is.Not.Null);

            var shopManager = ShopManager.Instance;
            Assert.That(shopManager, Is.Not.Null);
            Assert.That(shopManager.RegisteredShopIds, Is.Empty,
                "O diagnostico explicito exige sessao isolada; nao consumir estoque de outra run.");
            shopManager.Configure(database);
            Assert.That(shopManager.InitializeShop(shop), Is.True);

            var actor = new GameObject("phase21-economy-cross-channel-actor");
            var previousMarketSource = MarketSkillModifierProvider.MultiplierSource;
            EconomyTransactionCompletedEvent lastSale = default;
            bool sawSale = false;
            System.Action<EconomyTransactionCompletedEvent> onTransaction = evt =>
            {
                if (evt.TransactionType != "SellPoint") return;
                lastSale = evt;
                sawSale = true;
            };
            GameEventBus.Subscribe(onTransaction);
            try
            {
                var player = actor.AddComponent<PlayerManager>();
                var inventory = actor.AddComponent<InventoryManager>();
                player.Initialize();
                inventory.Initialize(database);
                sellPoint.RebindRuntimeManagers(inventory, player);

                var state = new SkillTreeState(3);
                state.Purchase(MarketSkillModifierProvider.NodeId, 1, "crafting");
                state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
                state.RankUp(MarketSkillModifierProvider.NodeId, "crafting");
                state.SetChosenVariant(MarketSkillModifierProvider.NodeId,
                    MarketSkillModifierProvider.BuyVariantId);
                MarketSkillModifierProvider.MultiplierSource = direction =>
                    MarketSkillModifierProvider.Resolve(
                        state.GetRank(MarketSkillModifierProvider.NodeId),
                        state.GetChosenVariant(MarketSkillModifierProvider.NodeId), direction);

                // Controlled rank setup isolates the pricing exploit; purchase itself uses the live shop.
                int buyPrice = ShopManager.CalculateBuyPrice(bread, shop.GetEntry(bread.Id), shop);
                Assert.That(buyPrice, Is.GreaterThan(0));
                player.SetGold(buyPrice);
                var bought = shopManager.TryBuyItem(shop.Id, bread.Id, 1, player, inventory);
                Assert.That(bought.Success, Is.True, bought.Message);
                Assert.That(player.CurrentGold, Is.Zero);
                Assert.That(inventory.GetAmount(bread.Id), Is.EqualTo(1));

                // Reset only this isolated price-state. Publishing a global respec here would
                // clear live player systems even though their SkillTreeManager did not respec.
                state.FullRespec(restoredPoints: 3);
                Assert.That(state.RespecCount, Is.EqualTo(1));
                Assert.That(state.GetRank(MarketSkillModifierProvider.NodeId), Is.Zero);

                Assert.That(sellPoint.CanInteract(actor), Is.True);
                sellPoint.Interact(actor);
                Assert.That(sawSale, Is.True,
                    "A venda deve passar pelo SellPoint materializado na FarmScene com managers isolados.");
                Assert.That(lastSale.WasSuccessful, Is.True, lastSale.Message);
                Assert.That(inventory.GetAmount(bread.Id), Is.Zero);
                Assert.That(lastSale.GoldDelta, Is.EqualTo(player.CurrentGold));
                TestContext.WriteLine($"AC05 cross-channel: scene=FarmScene; item={bread.Id}; " +
                    $"shop={shop.Id}; buy={buyPrice}; respecCount={state.RespecCount}; " +
                    $"sellChannel=SellPoint; sell={lastSale.GoldDelta}; finalGold={player.CurrentGold}");
                Assert.That(player.CurrentGold, Is.LessThanOrEqualTo(buyPrice),
                    $"AC05: pão comprado por {buyPrice}g no {shop.Id} e vendido por " +
                    $"{player.CurrentGold}g no SellPoint da Farm gerou lucro após respec.");
            }
            finally
            {
                GameEventBus.Unsubscribe(onTransaction);
                MarketSkillModifierProvider.MultiplierSource = previousMarketSource;
                sellPoint.RebindRuntimeManagers(originalInventory, originalPlayer);
                // This explicit diagnostic created the only shop session in this process.
                // Restore the bootstrap-owned manager to its initial empty/initialized state.
                shopManager.Shutdown();
                shopManager.Initialize();
                Object.Destroy(actor);
            }
            yield return null;
        }
    }
}
