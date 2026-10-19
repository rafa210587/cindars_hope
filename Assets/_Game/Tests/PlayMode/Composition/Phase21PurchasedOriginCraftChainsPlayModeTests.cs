using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Craft;
using CindarsHope.Craft.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory;
using CindarsHope.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class Phase21PurchasedOriginCraftChainsPlayModeTests
    {
        [UnityTest, Explicit("Dedicated FarmScene process: real recipe registry, stations, inventory and SellPoint.")]
        public IEnumerator RegisteredWorkbenchWoodAndTimedForge_PreservePurchasedBudget()
        {
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            var inventory = bootstrap.InventoryManager as InventoryManager;
            var player = bootstrap.PlayerManager as PlayerManager;
            Assert.That(inventory, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            var runtime = CraftingRuntime.ActiveInstances.FirstOrDefault(candidate =>
                candidate != null && candidate.IsInitialized && candidate.InventoryManager == inventory);
            Assert.That(runtime, Is.Not.Null, "FarmScene must materialize CraftingRuntime with its real registry.");

            var points = new List<CraftingPoint>();
            var sellPoints = new List<SellPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                points.AddRange(root.GetComponentsInChildren<CraftingPoint>(true));
                sellPoints.AddRange(root.GetComponentsInChildren<SellPoint>(true));
            }
            var workbenchPoint = points.Single(point => point.StationInstanceId == "farm_workbench_01");
            var forgePoint = points.Single(point => point.StationInstanceId == "farm_forge_01");
            Assert.That(workbenchPoint.StationType, Is.EqualTo(WorkshopType.Workbench));
            Assert.That(forgePoint.StationType, Is.EqualTo(WorkshopType.Forge));
            Assert.That(sellPoints, Has.Count.EqualTo(1));
            var sellPoint = sellPoints[0];
            var woodRecipe = runtime.GetRecipesForStation(WorkshopType.Workbench)
                .Single(recipe => recipe.Id == "recipe_workbench_processed_wood");
            var ironRecipe = runtime.GetRecipesForStation(WorkshopType.Forge)
                .Single(recipe => recipe.Id == "recipe_forge_iron_sword");
            Assert.That(woodRecipe.IsInstantaneous, Is.True);
            Assert.That(woodRecipe.Ingredients.Single().ItemId, Is.EqualTo("item_material_wood"));
            Assert.That(woodRecipe.Ingredients.Single().Amount, Is.EqualTo(2));
            Assert.That(woodRecipe.OutputItemId, Is.EqualTo("item_material_processed_wood"));
            Assert.That(woodRecipe.OutputAmount, Is.EqualTo(2));
            Assert.That(ironRecipe.CraftTimeSeconds, Is.GreaterThan(0));
            Assert.That(ironRecipe.Ingredients.Single().ItemId, Is.EqualTo("item_material_iron_ore"));
            Assert.That(ironRecipe.Ingredients.Single().Amount, Is.EqualTo(2));

            var originalInventory = inventory.CaptureSaveData();
            var originalCraft = runtime.CaptureSaveData();
            var originalGold = player.CurrentGold;
            var sellEventCount = 0;
            var sellEventGold = 0;
            Action<EconomyTransactionCompletedEvent> onSell = evt =>
            {
                if (!evt.WasSuccessful || evt.TransactionType != "SellPoint") return;
                sellEventCount++;
                sellEventGold = evt.GoldDelta;
            };
            var workbench = runtime.GetOrCreateStation(workbenchPoint.StationInstanceId, workbenchPoint.StationType);
            var forge = runtime.GetOrCreateStation(forgePoint.StationInstanceId, forgePoint.StationType);
            Assert.That(workbench.IsBusy, Is.False);
            Assert.That(forge.IsBusy, Is.False);
            try
            {
                GameEventBus.Subscribe(onSell);
                inventory.Clear();
                player.SetGold(0);
                sellPoint.RebindRuntimeManagers(inventory, player);

                const int woodPaidPerUnit = 5;
                Assert.That(inventory.TryAddPurchasedItem("item_material_wood", 2,
                    2 * woodPaidPerUnit).Success,
                    Is.True);
                Assert.That(runtime.TryStartCraft(workbench, woodRecipe, out var reason), Is.True, reason);
                Assert.That(workbench.IsBusy, Is.False, "The registered wood recipe is instantaneous.");
                Assert.That(inventory.GetAmount("item_material_wood"), Is.Zero);
                var woodLots = SelectedLots(inventory, woodRecipe.OutputItemId, woodRecipe.OutputAmount);
                AssertRestricted(woodLots, 2 * (woodPaidPerUnit - 1));
                var woodCap = TotalCap(woodLots);
                sellPoint.Interact(player.gameObject);
                Assert.That(sellEventCount, Is.EqualTo(1));
                var woodPayout = sellEventGold;
                Assert.That(woodPayout, Is.InRange(0, woodCap));
                Assert.That(woodPayout, Is.LessThan(2 * woodPaidPerUnit));
                Assert.That(inventory.GetAmount(woodRecipe.OutputItemId), Is.Zero);

                // Two real ore units, one purchased and one earned, enter a timed authored job.
                // The owned liquidation enters the budget once at craft start.
                const int orePaid = 9;
                Assert.That(inventory.TryAddPurchasedItem("item_material_iron_ore", 1, orePaid).Success,
                    Is.True);
                Assert.That(inventory.TryAddItem("item_material_iron_ore", 1).Success, Is.True);
                Assert.That(inventory.TryGetItemData("item_material_iron_ore", out var ore), Is.True);
                var balance = Resources.Load<EconomyBalanceConfigSO>("EconomyBalanceConfig");
                var ownedLiquidation = ItemPriceResolver.ResolveSellingPrice(
                    ore.BaseValue, SellContext.Shipping, balance);
                var expectedBudget = orePaid - 1 + ownedLiquidation;
                Assert.That(runtime.TryStartCraft(forge, ironRecipe, out reason), Is.True, reason);
                Assert.That(forge.HasActiveJob, Is.True);
                Assert.That(inventory.GetAmount("item_material_iron_ore"), Is.Zero);
                AssertRestricted(forge.Job.OutputOriginLots, expectedBudget);
                var savedJob = forge.CaptureSaveData().Job;
                Assert.That(savedJob, Is.Not.Null);
                Assert.That(savedJob.OriginSchemaVersion, Is.EqualTo(1));
                Assert.That(savedJob.OutputOriginLots.Sum(lot => (long)lot.Quantity * lot.UnitResaleCapGold),
                    Is.EqualTo(expectedBudget));

                var deadline = Time.realtimeSinceStartup + 30f;
                while (!forge.HasCompletedOutput && Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.That(forge.HasCompletedOutput, Is.True, "Authored four-second forge job did not finish.");
                Assert.That(runtime.TryCollect(forge, out reason), Is.True, reason);
                var swordLots = SelectedLots(inventory, ironRecipe.OutputItemId, 1);
                AssertRestricted(swordLots, expectedBudget);
                TestContext.WriteLine($"21F craft scene: wood 10g basis -> {woodPayout}g SellPoint; " +
                    $"mixed iron job budget {expectedBudget}g (8g restricted + {ownedLiquidation}g earned). " +
                    "All recipes/stations/assets were registered in FarmScene.");
            }
            finally
            {
                GameEventBus.Unsubscribe(onSell);
                if (forge.HasActiveJob) runtime.TryCancel(forge, out _);
                runtime.LoadFromSaveData(originalCraft);
                originalInventory.OriginSchemaVersion = 1;
                inventory.RestoreFromSaveData(originalInventory);
                player.SetGold(originalGold);
            }
        }

        private static List<ItemOriginLot> SelectedLots(InventoryManager inventory, string itemId, int amount)
        {
            Assert.That(inventory.TryPrepareRemoval(itemId, amount, out var selection), Is.True);
            return selection.CopyLots();
        }

        private static void AssertRestricted(IReadOnlyList<ItemOriginLot> lots, int expectedCap)
        {
            Assert.That(lots, Is.Not.Empty);
            Assert.That(lots.All(lot => lot.Kind == ItemOriginKind.RestrictedDerived), Is.True);
            Assert.That(TotalCap(lots), Is.EqualTo(expectedCap));
        }

        private static long TotalCap(IEnumerable<ItemOriginLot> lots)
            => lots.Sum(lot => (long)lot.Quantity * lot.UnitResaleCapGold);
    }
}
