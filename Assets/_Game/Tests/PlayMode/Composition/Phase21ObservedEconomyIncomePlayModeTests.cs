using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Farm.Data;
using CindarsHope.Farm.Runtime;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.Tools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class Phase21ObservedEconomyIncomePlayModeTests
    {
        [Serializable]
        private sealed class ControlledPayoutObservation
        {
            public string scenarioId;
            public string sampleType;
            public string source;
            public string seedId;
            public string itemId;
            public string transactionChannel;
            public string scene;
            public string sellPoint;
            public string isolationScope;
            public int stageLevel;
            public int observedDay;
            public int authoredHarvestQuantity;
            public int openingGold;
            public int closingGold;
            public int walletDelta;
            public int eventGoldDelta;
            public int ambientGlobalWalletBefore;
            public int ambientGlobalWalletAfter;
            public int ambientGlobalWalletDelta;
        }

        [Serializable]
        private sealed class FarmPlotCycleObservation
        {
            public string scenarioId;
            public string sampleType;
            public string scene;
            public string plotId;
            public string seedId;
            public string seedItemId;
            public string cropItemId;
            public string toolSource;
            public string shopId;
            public string purchaseRoute;
            public string saleChannel;
            public string isolationScope;
            public string completedDailyGoalIds;
            public int stageLevel;
            public int startDay;
            public int harvestDay;
            public int daysAdvanced;
            public int purchasedSeedQuantity;
            public int consumedSeedQuantity;
            public int harvestedCropQuantity;
            public int walletBeforePurchase;
            public int walletAfterPurchase;
            public int walletAfterSale;
            public int purchaseGoldDelta;
            public int saleGoldDelta;
            public int isolatedNetGoldDeltaExcludingFixtureToolsAndAmbientRewards;
            public int ambientGlobalWalletBefore;
            public int ambientGlobalWalletAfter;
            public int ambientGlobalWalletDelta;
            public int authoredDailyGoalRewardGoldForCompletedIds;
            public int tillStaminaSpent;
            public int plantStaminaSpent;
            public int waterStaminaSpent;
            public int harvestStaminaSpent;
        }

        [UnityTest]
        [Explicit("Dedicated batch process only: controlled payout; no daily median; global time/quest/weather are not fully reversible.")]
        public IEnumerator L20_ControlledWheatPayout_RecordsObservedSellPointTransaction()
        {
            // This observes one real sale, not crop production or a full day of income.
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null, "FarmScene must materialize GameBootstrap.");
            var database = bootstrap.ItemDatabase as ItemDatabaseSO;
            var originalInventory = bootstrap.InventoryManager as InventoryManager;
            var originalPlayer = bootstrap.PlayerManager as PlayerManager;
            var progression = PlayerProgressionManager.Instance;
            var dailyGoals = FarmDailyGoalService.Instance;
            Assert.That(database, Is.Not.Null);
            Assert.That(originalInventory, Is.Not.Null);
            Assert.That(originalPlayer, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(dailyGoals, Is.Not.Null);
            Assert.That(bootstrap.TimeManager, Is.Not.Null);

            var seed = AssetDatabase.LoadAssetAtPath<SeedDataSO>(
                "Assets/_Game/Data/Seeds/Seed_Trigo.asset");
            Assert.That(seed, Is.Not.Null);
            Assert.That(seed.Id, Is.EqualTo("seed_wheat"));
            Assert.That(seed.HarvestItems, Has.Length.EqualTo(1));
            Assert.That(seed.HarvestAmounts, Has.Length.EqualTo(1));
            var crop = seed.HarvestItems[0];
            int quantity = seed.HarvestAmounts[0];
            Assert.That(crop, Is.Not.Null);
            Assert.That(crop.Id, Is.EqualTo("item_crop_wheat"));
            Assert.That(quantity, Is.GreaterThan(0));
            Assert.That(database.TryGetById(crop.Id, out var registeredCrop), Is.True);
            Assert.That(registeredCrop, Is.SameAs(crop));

            var sellPoints = new List<SellPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                sellPoints.AddRange(root.GetComponentsInChildren<SellPoint>(true));
            Assert.That(sellPoints, Has.Count.EqualTo(1),
                "FarmScene must have one materialized SellPoint for this observation.");
            var sellPoint = sellPoints[0];

            var originalProgression = progression.CaptureSaveData();
            var originalDailyGoals = dailyGoals.CaptureSaveData();
            int originalPlayerGold = originalPlayer.CurrentGold;
            int originalPlayerHp = originalPlayer.CurrentHP;
            int originalPlayerMaxHp = originalPlayer.MaxHP;
            var actor = new GameObject("phase21-observed-income-isolated-actor");
            bool sawSale = false;
            EconomyTransactionCompletedEvent sale = default;
            Action<EconomyTransactionCompletedEvent> onTransaction = evt =>
            {
                if (evt.TransactionType != "SellPoint") return;
                Assert.That(sawSale, Is.False, "The isolated lot should generate exactly one sale event.");
                sawSale = true;
                sale = evt;
            };
            GameEventBus.Subscribe(onTransaction);
            try
            {
                var staged = progression.CaptureSaveData();
                staged.TotalXp = ProgressionCurve.TotalXpForLevel(Phase21BuildManifest.Level20);
                staged.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(
                    Phase21BuildManifest.Level20);
                progression.RestoreFromSaveData(staged);
                Assert.That(progression.Level, Is.EqualTo(Phase21BuildManifest.Level20));

                var wallet = actor.AddComponent<PlayerManager>();
                var inventory = actor.AddComponent<InventoryManager>();
                wallet.Initialize();
                wallet.SetGold(0);
                inventory.Initialize(database);
                Assert.That(inventory.Items, Is.Empty,
                    "The isolated inventory must contain only this crop lot before the sale.");
                Assert.That(inventory.TryAddItem(crop.Id, quantity).Success, Is.True,
                    "Controlled input quantity comes from the authored wheat seed asset.");
                Assert.That(inventory.Items, Has.Count.EqualTo(1));
                Assert.That(inventory.GetAmount(crop.Id), Is.EqualTo(quantity));
                sellPoint.RebindRuntimeManagers(inventory, wallet);

                int day = bootstrap.TimeManager.CurrentDay;
                int openingGold = wallet.CurrentGold;
                int ambientGlobalWalletBefore = originalPlayer.CurrentGold;
                Assert.That(sellPoint.CanInteract(actor), Is.True);
                sellPoint.Interact(actor);
                int closingGold = wallet.CurrentGold;
                int delta = closingGold - openingGold;
                int ambientGlobalWalletAfter = originalPlayer.CurrentGold;

                Assert.That(sawSale, Is.True, "The materialized SellPoint must publish a sale event.");
                Assert.That(sale.WasSuccessful, Is.True, sale.Message);
                Assert.That(sale.GoldDelta, Is.EqualTo(delta));
                Assert.That(delta, Is.GreaterThan(0));
                Assert.That(inventory.GetAmount(crop.Id), Is.Zero);
                Assert.That(bootstrap.TimeManager.CurrentDay, Is.EqualTo(day),
                    "This fixture observes one payout without advancing a gameplay day.");

                var observation = new ControlledPayoutObservation
                {
                    scenarioId = Phase21BuildManifest.Scenarios[0].ScenarioId,
                    sampleType = "controlled_payout_single_transaction_not_daily_median",
                    source = "authored_seed_yield_injected_into_isolated_inventory",
                    seedId = seed.Id,
                    itemId = crop.Id,
                    transactionChannel = sale.TransactionType,
                    scene = SceneManager.GetActiveScene().name,
                    sellPoint = sellPoint.name,
                    isolationScope = "dedicated_batch_process_only_global_day_quest_weather_not_fully_reversible",
                    stageLevel = progression.Level,
                    observedDay = day,
                    authoredHarvestQuantity = quantity,
                    openingGold = openingGold,
                    closingGold = closingGold,
                    walletDelta = delta,
                    eventGoldDelta = sale.GoldDelta,
                    ambientGlobalWalletBefore = ambientGlobalWalletBefore,
                    ambientGlobalWalletAfter = ambientGlobalWalletAfter,
                    ambientGlobalWalletDelta = ambientGlobalWalletAfter - ambientGlobalWalletBefore
                };
                WriteObservation(observation);
                TestContext.WriteLine(
                    $"Phase21 controlled payout: scenario={observation.scenarioId}; stage=L{observation.stageLevel}; " +
                    $"day={day}; source={observation.source}; seed={seed.Id}; item={crop.Id}; qty={quantity}; " +
                    $"scene={observation.scene}; component={observation.sellPoint}; channel={sale.TransactionType}; " +
                    $"openingGold={openingGold}; closingGold={closingGold}; walletDelta={delta}; " +
                    $"ambientGlobalWallet={ambientGlobalWalletBefore}->{ambientGlobalWalletAfter}; " +
                    "dailyMedian=NOT_RUN (no planting/harvest/day-window observation)");
            }
            finally
            {
                GameEventBus.Unsubscribe(onTransaction);
                sellPoint.RebindRuntimeManagers(originalInventory, originalPlayer);
                dailyGoals.RestoreFromSaveData(originalDailyGoals);
                progression.RestoreFromSaveData(originalProgression);
                originalPlayer.RestoreState(originalPlayerMaxHp, originalPlayerHp, originalPlayerGold);
                UnityEngine.Object.Destroy(actor);
            }
            yield return null;
        }

        [UnityTest]
        [Explicit("Dedicated batch process only: FarmPlot bridge cycle, no menu input or daily median; global time/quest/weather are not fully reversible.")]
        public IEnumerator L20_FarmPlotWheatCycle_RecordsPurchaseHarvestAndSale()
        {
            SceneManager.LoadScene("FarmScene", LoadSceneMode.Single);
            yield return null;

            var bootstrap = GameBootstrap.Instance;
            Assert.That(bootstrap, Is.Not.Null);
            var database = bootstrap.ItemDatabase as ItemDatabaseSO;
            var originalInventory = bootstrap.InventoryManager as InventoryManager;
            var originalPlayer = bootstrap.PlayerManager as PlayerManager;
            var stamina = bootstrap.StaminaManager as StaminaManager;
            var progression = PlayerProgressionManager.Instance;
            var equipment = EquipmentManager.Instance;
            var shopManager = ShopManager.Instance;
            var dailyGoals = FarmDailyGoalService.Instance;
            var time = bootstrap.TimeManager;
            Assert.That(database, Is.Not.Null);
            Assert.That(originalInventory, Is.Not.Null);
            Assert.That(originalPlayer, Is.Not.Null);
            Assert.That(stamina, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(shopManager, Is.Not.Null);
            Assert.That(dailyGoals, Is.Not.Null);
            Assert.That(time, Is.Not.Null);
            Assert.That(shopManager.RegisteredShopIds, Is.Empty,
                "This diagnostic owns its temporary shop session and requires an empty baseline.");

            var seed = AssetDatabase.LoadAssetAtPath<SeedDataSO>(
                "Assets/_Game/Data/Seeds/Seed_Trigo.asset");
            var shopData = AssetDatabase.LoadAssetAtPath<ShopDataSO>(
                "Assets/_Game/Data/Economy/Shop_Renko.asset");
            Assert.That(seed, Is.Not.Null);
            Assert.That(shopData, Is.Not.Null);
            Assert.That(seed.Id, Is.EqualTo("seed_wheat"));
            Assert.That(seed.GrowthDays, Is.EqualTo(3));
            Assert.That(seed.SeedItem, Is.Not.Null);
            Assert.That(seed.SeedItem.Id, Is.EqualTo("item_seed_wheat"));
            Assert.That(seed.HarvestItems, Has.Length.EqualTo(1));
            var crop = seed.HarvestItems[0];
            Assert.That(crop, Is.Not.Null);
            Assert.That(crop.Id, Is.EqualTo("item_crop_wheat"));
            Assert.That(database.TryGetById(seed.SeedItem.Id, out _), Is.True);
            Assert.That(database.TryGetById(crop.Id, out _), Is.True);
            Assert.That(shopData.GetEntry(seed.SeedItem.Id), Is.Not.Null);

            var plots = new List<FarmPlot>();
            var sellPoints = new List<SellPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                plots.AddRange(root.GetComponentsInChildren<FarmPlot>(true));
                sellPoints.AddRange(root.GetComponentsInChildren<SellPoint>(true));
            }
            Assert.That(sellPoints, Has.Count.EqualTo(1));
            var plot = plots.Find(candidate => candidate.isActiveAndEnabled && candidate.State == FarmPlotState.Raw);
            Assert.That(plot, Is.Not.Null, "FarmScene must expose an active raw FarmPlot.");
            var sellPoint = sellPoints[0];

            var originalPlot = plot.CaptureSaveData();
            var originalProgression = progression.CaptureSaveData();
            var originalDailyGoals = dailyGoals.CaptureSaveData();
            var originalEquipment = equipment.CaptureSaveData();
            int originalPlayerGold = originalPlayer.CurrentGold;
            int originalPlayerHp = originalPlayer.CurrentHP;
            int originalPlayerMaxHp = originalPlayer.MaxHP;
            int originalDay = time.CurrentDay;
            int originalStamina = stamina.CurrentStamina;
            int originalMaxStamina = stamina.MaxStamina;
            var actor = new GameObject("phase21-farm-plot-income-isolated-actor");
            bool sawSale = false;
            int harvestedByEvent = 0;
            var completedDailyGoalIds = new List<string>();
            EconomyTransactionCompletedEvent sale = default;
            Action<DailyGoalCompletedEvent> onDailyGoalCompleted = evt =>
            {
                if (!string.IsNullOrEmpty(evt.GoalId))
                    completedDailyGoalIds.Add(evt.GoalId);
            };
            Action<CropHarvestedEvent> onHarvest = evt =>
            {
                if (evt.SeedId == seed.Id &&
                    evt.TilePosition == Vector2Int.RoundToInt(plot.transform.position) &&
                    evt.ItemId == crop.Id)
                    harvestedByEvent += evt.Amount;
            };
            Action<EconomyTransactionCompletedEvent> onTransaction = evt =>
            {
                if (evt.TransactionType != "SellPoint") return;
                Assert.That(sawSale, Is.False, "The isolated crop lot must generate one payout event.");
                sawSale = true;
                sale = evt;
            };
            GameEventBus.Subscribe(onHarvest);
            GameEventBus.Subscribe(onTransaction);
            GameEventBus.Subscribe(onDailyGoalCompleted);
            bool temporaryShopInitialized = false;
            try
            {
                var staged = progression.CaptureSaveData();
                staged.TotalXp = ProgressionCurve.TotalXpForLevel(Phase21BuildManifest.Level20);
                staged.UnspentSkillPoints = PlayerProgressionRules.CalculateTotalSkillPointsAtLevel(
                    Phase21BuildManifest.Level20);
                progression.RestoreFromSaveData(staged);
                Assert.That(progression.Level, Is.EqualTo(Phase21BuildManifest.Level20));

                var wallet = actor.AddComponent<PlayerManager>();
                var inventory = actor.AddComponent<InventoryManager>();
                wallet.Initialize();
                inventory.Initialize(database);
                Assert.That(inventory.Items, Is.Empty);
                plot.RebindInventoryManager(inventory);
                sellPoint.RebindRuntimeManagers(inventory, wallet);

                // The catalog sells wheat seed but does not sell a watering can. Equip the
                // authored basic tool IDs as controlled fixture prerequisites; exclude their
                // acquisition cost from the observed net wallet delta.
                var hoe = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                    "Assets/_Game/Data/Items/Item_Shop_Hoe_Basic.asset");
                var wateringCan = AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                    "Assets/_Game/Data/Items/Item_Shop_Watering_Can_Basic.asset");
                Assert.That(hoe, Is.Not.Null);
                Assert.That(wateringCan, Is.Not.Null);
                Assert.That(database.TryGetById(hoe.Id, out _), Is.True);
                Assert.That(database.TryGetById(wateringCan.Id, out _), Is.True);
                equipment.EquipItem(EquipmentSlot.LeftHand, hoe.Id);
                equipment.EquipItem(EquipmentSlot.RightHand, wateringCan.Id);
                Assert.That(equipment.HasTool(ToolType.Hoe, ToolTier.Basic), Is.True);
                Assert.That(equipment.HasTool(ToolType.WateringCan, ToolTier.Basic), Is.True);

                shopManager.Configure(database);
                Assert.That(shopManager.InitializeShop(shopData, time.CurrentDay), Is.True);
                temporaryShopInitialized = true;
                int seedCost = ShopManager.CalculateBuyPrice(
                    seed.SeedItem, shopData.GetEntry(seed.SeedItem.Id), shopData);
                Assert.That(seedCost, Is.GreaterThan(0));
                wallet.SetGold(seedCost);
                int walletBeforePurchase = wallet.CurrentGold;
                var purchase = shopManager.TryBuyItem(shopData.Id, seed.SeedItem.Id, 1, wallet, inventory);
                Assert.That(purchase.Success, Is.True, purchase.Message);
                int walletAfterPurchase = wallet.CurrentGold;
                Assert.That(inventory.GetAmount(seed.SeedItem.Id), Is.EqualTo(1));
                Assert.That(walletBeforePurchase - walletAfterPurchase, Is.EqualTo(seedCost));

                int startDay = time.CurrentDay;
                int ambientGlobalWalletBefore = originalPlayer.CurrentGold;
                int before = stamina.CurrentStamina;
                InvokePlotMenuBridge(plot, "TryTillInternal");
                int tillCost = before - stamina.CurrentStamina;
                Assert.That(plot.State, Is.EqualTo(FarmPlotState.TilledDry));
                Assert.That(tillCost, Is.GreaterThan(0));

                before = stamina.CurrentStamina;
                InvokePlotMenuBridge(plot, "TryPlantSeedInternal", seed.Id, seed.SeedItem.Id);
                int plantCost = before - stamina.CurrentStamina;
                Assert.That(plot.PlantedSeedId, Is.EqualTo(seed.Id));
                Assert.That(plot.State, Is.EqualTo(FarmPlotState.PlantedDry));
                Assert.That(inventory.GetAmount(seed.SeedItem.Id), Is.Zero);
                Assert.That(plantCost, Is.GreaterThan(0));

                int waterCost = 0;
                for (int day = 0; day < seed.GrowthDays; day++)
                {
                    before = stamina.CurrentStamina;
                    InvokePlotMenuBridge(plot, "TryWaterInternal");
                    waterCost += before - stamina.CurrentStamina;
                    Assert.That(plot.State, Is.EqualTo(FarmPlotState.PlantedWet));
                    time.AdvanceDay();
                    Assert.That(time.CurrentDay, Is.EqualTo(startDay + day + 1));
                    Assert.That(plot.DaysGrown, Is.EqualTo(day + 1));
                    if (day < seed.GrowthDays - 1)
                        Assert.That(plot.State, Is.EqualTo(FarmPlotState.PlantedDry));
                }
                Assert.That(waterCost, Is.GreaterThan(0));
                Assert.That(plot.State, Is.EqualTo(FarmPlotState.ReadyToHarvest));

                before = stamina.CurrentStamina;
                InvokePlotMenuBridge(plot, "TryHarvestInternal");
                int harvestCost = before - stamina.CurrentStamina;
                int cropQuantity = inventory.GetAmount(crop.Id);
                Assert.That(cropQuantity, Is.GreaterThan(0));
                Assert.That(harvestedByEvent, Is.EqualTo(cropQuantity));
                Assert.That(harvestCost, Is.GreaterThan(0));
                Assert.That(inventory.Items, Has.Count.EqualTo(1),
                    "The isolated sale lot must contain only the harvested crop.");

                int walletBeforeSale = wallet.CurrentGold;
                sellPoint.Interact(actor);
                int walletAfterSale = wallet.CurrentGold;
                Assert.That(sawSale, Is.True);
                Assert.That(sale.WasSuccessful, Is.True, sale.Message);
                Assert.That(sale.GoldDelta, Is.EqualTo(walletAfterSale - walletBeforeSale));
                Assert.That(sale.GoldDelta, Is.GreaterThan(0));
                Assert.That(inventory.GetAmount(crop.Id), Is.Zero);
                int ambientGlobalWalletAfter = originalPlayer.CurrentGold;
                int ambientGlobalWalletDelta = ambientGlobalWalletAfter - ambientGlobalWalletBefore;
                int authoredDailyGoalRewardGold = 0;
                foreach (var goalId in completedDailyGoalIds)
                {
                    Assert.That(FarmDailyGoalRewardTable.TryGet(goalId, out var reward), Is.True,
                        $"Completed daily goal {goalId} has no authored reward entry.");
                    authoredDailyGoalRewardGold += reward.Gold;
                }
                Assert.That(completedDailyGoalIds, Does.Contain(FarmDailyGoalCatalog.GoalFirstHarvest));
                Assert.That(completedDailyGoalIds, Does.Contain(FarmDailyGoalCatalog.GoalSellFirstCrop));
                Assert.That(ambientGlobalWalletDelta, Is.GreaterThanOrEqualTo(authoredDailyGoalRewardGold),
                    "Ambient bootstrap wallet should include authored daily-goal rewards; any additional DayStarted effects remain unclassified.");

                var observation = new FarmPlotCycleObservation
                {
                    scenarioId = Phase21BuildManifest.Scenarios[0].ScenarioId,
                    sampleType = "controlled_farmplot_cycle_single_harvest_not_daily_median",
                    scene = SceneManager.GetActiveScene().name,
                    plotId = plot.PlotId,
                    seedId = seed.Id,
                    seedItemId = seed.SeedItem.Id,
                    cropItemId = crop.Id,
                    toolSource = "authored_basic_hoe_and_watering_can_equipped_by_fixture",
                    shopId = shopData.Id,
                    purchaseRoute = "ShopManager.TryBuyItem_Renko_Town_asset_session_initialized_in_Farm_no_Town_UI",
                    saleChannel = sale.TransactionType,
                    isolationScope = "dedicated_batch_process_only_global_day_quest_weather_not_fully_reversible",
                    completedDailyGoalIds = string.Join("|", completedDailyGoalIds),
                    stageLevel = progression.Level,
                    startDay = startDay,
                    harvestDay = time.CurrentDay,
                    daysAdvanced = time.CurrentDay - startDay,
                    purchasedSeedQuantity = 1,
                    consumedSeedQuantity = 1,
                    harvestedCropQuantity = cropQuantity,
                    walletBeforePurchase = walletBeforePurchase,
                    walletAfterPurchase = walletAfterPurchase,
                    walletAfterSale = walletAfterSale,
                    purchaseGoldDelta = walletAfterPurchase - walletBeforePurchase,
                    saleGoldDelta = sale.GoldDelta,
                    isolatedNetGoldDeltaExcludingFixtureToolsAndAmbientRewards = walletAfterSale - walletBeforePurchase,
                    ambientGlobalWalletBefore = ambientGlobalWalletBefore,
                    ambientGlobalWalletAfter = ambientGlobalWalletAfter,
                    ambientGlobalWalletDelta = ambientGlobalWalletDelta,
                    authoredDailyGoalRewardGoldForCompletedIds = authoredDailyGoalRewardGold,
                    tillStaminaSpent = tillCost,
                    plantStaminaSpent = plantCost,
                    waterStaminaSpent = waterCost,
                    harvestStaminaSpent = harvestCost
                };
                WriteFarmPlotCycleObservation(observation);
                TestContext.WriteLine(
                    $"Phase21 FarmPlot cycle: scenario={observation.scenarioId}; stage=L{observation.stageLevel}; " +
                    $"scene={observation.scene}; plot={observation.plotId}; days={startDay}->{time.CurrentDay}; " +
                    $"shop={shopData.Id}; purchaseRoute={observation.purchaseRoute}; seed={seed.SeedItem.Id} x1; seedCost={seedCost}; " +
                    $"harvest={crop.Id} x{cropQuantity}; harvestEventAmount={harvestedByEvent}; " +
                    $"channel={sale.TransactionType}; wallet={walletBeforePurchase}->{walletAfterPurchase}->{walletAfterSale}; " +
                    $"saleGoldDelta={sale.GoldDelta}; isolatedNetExcludingFixtureToolsAndAmbientRewards={observation.isolatedNetGoldDeltaExcludingFixtureToolsAndAmbientRewards}; " +
                    $"ambientGlobalWallet={ambientGlobalWalletBefore}->{ambientGlobalWalletAfter}; dailyGoalIds={observation.completedDailyGoalIds}; " +
                    $"stamina=till:{tillCost}/plant:{plantCost}/water:{waterCost}/harvest:{harvestCost}; " +
                    "dailyMedian=NOT_RUN (one controlled plot cycle; no menu/input or natural daily activity sample)");
            }
            finally
            {
                GameEventBus.Unsubscribe(onHarvest);
                GameEventBus.Unsubscribe(onTransaction);
                GameEventBus.Unsubscribe(onDailyGoalCompleted);
                plot.RestoreFromSaveData(originalPlot);
                plot.RebindInventoryManager(originalInventory);
                sellPoint.RebindRuntimeManagers(originalInventory, originalPlayer);
                equipment.RestoreFromSaveData(originalEquipment);
                dailyGoals.RestoreFromSaveData(originalDailyGoals);
                progression.RestoreFromSaveData(originalProgression);
                originalPlayer.RestoreState(originalPlayerMaxHp, originalPlayerHp, originalPlayerGold);
                time.SetCurrentDay(originalDay);
                stamina.Initialize(originalMaxStamina, originalStamina);
                if (temporaryShopInitialized)
                {
                    shopManager.Shutdown();
                    shopManager.Initialize();
                }
                UnityEngine.Object.Destroy(actor);
            }
            yield return null;
        }

        private static void InvokePlotMenuBridge(FarmPlot plot, string methodName, params object[] arguments)
        {
            // Runtime exposes these internal bridges to the menu controller, while the
            // PlayMode test assembly has no InternalsVisibleTo grant. Invoke that exact
            // bridge, then assert only observable plot/inventory/stamina outcomes.
            var method = typeof(FarmPlot).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"FarmPlot menu bridge {methodName} is unavailable.");
            Assert.That(method.IsAssembly, Is.True, $"{methodName} must remain an internal menu bridge.");
            method.Invoke(plot, arguments);
        }

        private static void WriteObservation(ControlledPayoutObservation observation)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.Combine(project, "Logs");
            Directory.CreateDirectory(output);
            string stem = "skills-phase21-observed-income-l20-controlled-payout";
            File.WriteAllText(Path.Combine(output, stem + ".json"),
                JsonUtility.ToJson(observation, true));
            File.WriteAllText(Path.Combine(output, stem + ".csv"),
                "scenarioId,sampleType,isolationScope,stageLevel,observedDay,source,seedId,itemId,authoredHarvestQuantity,scene,sellPoint,transactionChannel,openingGold,closingGold,walletDelta,eventGoldDelta,ambientGlobalWalletBefore,ambientGlobalWalletAfter,ambientGlobalWalletDelta\n" +
                $"{observation.scenarioId},{observation.sampleType},{observation.isolationScope},{observation.stageLevel},{observation.observedDay}," +
                $"{observation.source},{observation.seedId},{observation.itemId},{observation.authoredHarvestQuantity}," +
                $"{observation.scene},{observation.sellPoint},{observation.transactionChannel},{observation.openingGold}," +
                $"{observation.closingGold},{observation.walletDelta},{observation.eventGoldDelta}," +
                $"{observation.ambientGlobalWalletBefore},{observation.ambientGlobalWalletAfter},{observation.ambientGlobalWalletDelta}\n");
        }

        private static void WriteFarmPlotCycleObservation(FarmPlotCycleObservation observation)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.Combine(project, "Logs");
            Directory.CreateDirectory(output);
            string stem = "skills-phase21-observed-income-l20-farmplot-cycle";
            File.WriteAllText(Path.Combine(output, stem + ".json"),
                JsonUtility.ToJson(observation, true));
            File.WriteAllText(Path.Combine(output, stem + ".csv"),
                "scenarioId,sampleType,isolationScope,stageLevel,scene,plotId,startDay,harvestDay,daysAdvanced,shopId,purchaseRoute,seedItemId,purchasedSeedQuantity,consumedSeedQuantity,cropItemId,harvestedCropQuantity,toolSource,saleChannel,walletBeforePurchase,walletAfterPurchase,walletAfterSale,purchaseGoldDelta,saleGoldDelta,isolatedNetGoldDeltaExcludingFixtureToolsAndAmbientRewards,ambientGlobalWalletBefore,ambientGlobalWalletAfter,ambientGlobalWalletDelta,authoredDailyGoalRewardGoldForCompletedIds,completedDailyGoalIds,tillStaminaSpent,plantStaminaSpent,waterStaminaSpent,harvestStaminaSpent\n" +
                $"{observation.scenarioId},{observation.sampleType},{observation.isolationScope},{observation.stageLevel},{observation.scene}," +
                $"{observation.plotId},{observation.startDay},{observation.harvestDay},{observation.daysAdvanced}," +
                $"{observation.shopId},{observation.purchaseRoute},{observation.seedItemId},{observation.purchasedSeedQuantity}," +
                $"{observation.consumedSeedQuantity},{observation.cropItemId},{observation.harvestedCropQuantity}," +
                $"{observation.toolSource},{observation.saleChannel},{observation.walletBeforePurchase}," +
                $"{observation.walletAfterPurchase},{observation.walletAfterSale},{observation.purchaseGoldDelta}," +
                $"{observation.saleGoldDelta},{observation.isolatedNetGoldDeltaExcludingFixtureToolsAndAmbientRewards}," +
                $"{observation.ambientGlobalWalletBefore},{observation.ambientGlobalWalletAfter},{observation.ambientGlobalWalletDelta}," +
                $"{observation.authoredDailyGoalRewardGoldForCompletedIds},{observation.completedDailyGoalIds}," +
                $"{observation.tillStaminaSpent},{observation.plantStaminaSpent},{observation.waterStaminaSpent}," +
                $"{observation.harvestStaminaSpent}\n");
        }
    }
}
