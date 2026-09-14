using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Farm.Crops;
using CindarsHope.Farm.Data;
using CindarsHope.Farm.Runtime;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
using CindarsHope.Tools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    [TestFixture]
    public sealed class Phase21SteadyIncomeWindowPlayModeTests
    {
        [Serializable]
        private sealed class DayObservation
        {
            public int offsetDay;
            public int gameDay;
            public string plotId;
            public string source;
            public string qualityTier;
            public string qualitySource;
            public string completedDailyGoalIds;
            public int seedBought;
            public int seedConsumed;
            public int harvestedQuantity;
            public int harvestedByEvent;
            public int wateredPlotCount;
            public int staminaAtStart;
            public int staminaSpent;
            public int staminaAtEnd;
            public int walletAtStart;
            public int walletAfterSale;
            public int walletAtEnd;
            public int seedPurchaseGoldDelta;
            public int saleGoldDelta;
            public int transactionNetGoldDelta;
            public int purchaseEventGoldDelta;
            public int saleEventGoldDelta;
            public int purchaseEventCount;
            public int saleEventCount;
            public int ambientGlobalWalletBefore;
            public int ambientGlobalWalletAfter;
            public int ambientGlobalWalletDelta;
            public int authoredGoalRewardGold;
        }

        [Serializable]
        private sealed class WindowObservation
        {
            public string scenarioId;
            public string sampleType;
            public string scene;
            public string isolationScope;
            public string purchaseRoute;
            public string saleRoute;
            public string seedId;
            public string seedItemId;
            public string cropItemId;
            public string shopId;
            public string toolSource;
            public string qualityLimit;
            public string acceptanceLimit;
            public int stageLevel;
            public int plotCount;
            public int firstGameDay;
            public int stableWindowStartOffset;
            public int stableWindowEndOffset;
            public int fixtureOpeningCapital;
            public int seedUnitBuyPriceAtStart;
            public int medianDailySaleGold;
            public int medianDailyTransactionNetGold;
            public int medianDailyAuthoredGoalRewardGold;
            public int medianDailyComposedNetGold;
            public List<DayObservation> days = new List<DayObservation>();
        }

        [UnityTest]
        [Explicit("Dedicated batch process only: advances global day/quest/weather; controlled FarmPlot bridge window, not natural Town/UI play.")]
        public IEnumerator L20_ThreeStaggeredWheatPlots_RecordsD3ToD5DailyMedianWithoutSkills()
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
            var skills = SkillTreeManager.Instance;
            var equipment = EquipmentManager.Instance;
            var shop = ShopManager.Instance;
            var dailyGoals = FarmDailyGoalService.Instance;
            var time = bootstrap.TimeManager;
            Assert.That(database, Is.Not.Null);
            Assert.That(originalInventory, Is.Not.Null);
            Assert.That(originalPlayer, Is.Not.Null);
            Assert.That(stamina, Is.Not.Null);
            Assert.That(progression, Is.Not.Null);
            Assert.That(skills, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(shop, Is.Not.Null);
            Assert.That(dailyGoals, Is.Not.Null);
            Assert.That(time, Is.Not.Null);
            Assert.That(shop.RegisteredShopIds, Is.Empty,
                "This dedicated process must start without a pre-existing shop session.");
            var skillSnapshot = skills.CaptureSaveData();
            Assert.That(skillSnapshot.PurchasedNodeIds, Is.Empty, "L20 income baseline requires no purchased skills.");
            Assert.That(skillSnapshot.NodeRanks, Is.Empty, "L20 income baseline requires no ranked skills.");
            Assert.That(SkillModifierHooks.HarvestYieldBonus, Is.Zero);

            var seed = AssetDatabase.LoadAssetAtPath<SeedDataSO>("Assets/_Game/Data/Seeds/Seed_Trigo.asset");
            var shopData = AssetDatabase.LoadAssetAtPath<ShopDataSO>("Assets/_Game/Data/Economy/Shop_Renko.asset");
            var hoe = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Game/Data/Items/Item_Shop_Hoe_Basic.asset");
            var wateringCan = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Game/Data/Items/Item_Shop_Watering_Can_Basic.asset");
            Assert.That(seed, Is.Not.Null);
            Assert.That(shopData, Is.Not.Null);
            Assert.That(hoe, Is.Not.Null);
            Assert.That(wateringCan, Is.Not.Null);
            Assert.That(seed.Id, Is.EqualTo("seed_wheat"));
            Assert.That(seed.GrowthDays, Is.EqualTo(3), "D0-D2 plantings must mature on D3-D5.");
            Assert.That(seed.RegrowDays, Is.Zero, "The observed cycle must require repurchasing and replanting.");
            Assert.That(seed.SeedItem, Is.Not.Null);
            Assert.That(seed.SeedItem.Id, Is.EqualTo("item_seed_wheat"));
            Assert.That(seed.HarvestItems, Has.Length.EqualTo(1));
            Assert.That(seed.HarvestAmounts, Has.Length.EqualTo(1));
            Assert.That(seed.HarvestAmounts[0], Is.GreaterThan(0));
            var crop = seed.HarvestItems[0];
            Assert.That(crop, Is.Not.Null);
            Assert.That(crop.Id, Is.EqualTo("item_crop_wheat"));
            Assert.That(database.TryGetById(seed.SeedItem.Id, out _), Is.True);
            Assert.That(database.TryGetById(crop.Id, out _), Is.True);
            Assert.That(database.TryGetById(hoe.Id, out _), Is.True);
            Assert.That(database.TryGetById(wateringCan.Id, out _), Is.True);
            Assert.That(shopData.GetEntry(seed.SeedItem.Id), Is.Not.Null);

            var plots = new List<FarmPlot>();
            var sellPoints = new List<SellPoint>();
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                plots.AddRange(root.GetComponentsInChildren<FarmPlot>(true));
                sellPoints.AddRange(root.GetComponentsInChildren<SellPoint>(true));
            }
            plots = plots.Where(plot => plot.isActiveAndEnabled && plot.State == FarmPlotState.Raw)
                .OrderBy(plot => plot.PlotId, StringComparer.Ordinal).Take(3).ToList();
            Assert.That(plots, Has.Count.EqualTo(3), "FarmScene must materialize three active raw plots.");
            Assert.That(plots.Select(plot => plot.PlotId).Distinct().Count(), Is.EqualTo(3));
            Assert.That(plots.All(plot => string.IsNullOrEmpty(plot.FertilizerId)), Is.True,
                "The baseline quality observation must not include prior fertilizer.");
            Assert.That(sellPoints, Has.Count.EqualTo(1));
            var sellPoint = sellPoints[0];

            var originalPlots = plots.Select(plot => plot.CaptureSaveData()).ToArray();
            var originalProgression = progression.CaptureSaveData();
            var originalDailyGoals = dailyGoals.CaptureSaveData();
            var originalEquipment = equipment.CaptureSaveData();
            int originalGold = originalPlayer.CurrentGold;
            int originalHp = originalPlayer.CurrentHP;
            int originalMaxHp = originalPlayer.MaxHP;
            int originalDay = time.CurrentDay;
            int originalStamina = stamina.CurrentStamina;
            int originalMaxStamina = stamina.MaxStamina;
            var actor = new GameObject("phase21-steady-income-isolated-actor");
            DayObservation activeDay = null;
            Action<DailyGoalCompletedEvent> onGoal = evt =>
            {
                if (activeDay != null && !string.IsNullOrEmpty(evt.GoalId))
                    activeDay.completedDailyGoalIds = AppendId(activeDay.completedDailyGoalIds, evt.GoalId);
            };
            Action<CropHarvestedEvent> onHarvest = evt =>
            {
                if (activeDay != null && evt.SeedId == seed.Id && evt.ItemId == crop.Id)
                    activeDay.harvestedByEvent += evt.Amount;
            };
            Action<EconomyTransactionCompletedEvent> onTransaction = evt =>
            {
                if (activeDay == null || !evt.WasSuccessful) return;
                if (evt.TransactionType == "ShopBuy" && evt.ItemId == seed.SeedItem.Id)
                {
                    activeDay.purchaseEventCount++;
                    activeDay.purchaseEventGoldDelta += evt.GoldDelta;
                }
                else if (evt.TransactionType == "SellPoint")
                {
                    activeDay.saleEventCount++;
                    activeDay.saleEventGoldDelta += evt.GoldDelta;
                }
            };
            GameEventBus.Subscribe(onGoal);
            GameEventBus.Subscribe(onHarvest);
            GameEventBus.Subscribe(onTransaction);
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
                foreach (var plot in plots) plot.RebindInventoryManager(inventory);
                sellPoint.RebindRuntimeManagers(inventory, wallet);
                equipment.EquipItem(EquipmentSlot.LeftHand, hoe.Id);
                equipment.EquipItem(EquipmentSlot.RightHand, wateringCan.Id);
                Assert.That(equipment.HasTool(ToolType.Hoe, ToolTier.Basic), Is.True);
                Assert.That(equipment.HasTool(ToolType.WateringCan, ToolTier.Basic), Is.True);

                shop.Configure(database);
                Assert.That(shop.InitializeShop(shopData, time.CurrentDay), Is.True);
                temporaryShopInitialized = true;
                int seedUnitPrice = ShopManager.CalculateBuyPrice(
                    seed.SeedItem, shopData.GetEntry(seed.SeedItem.Id), shopData);
                Assert.That(seedUnitPrice, Is.GreaterThan(0));
                int openingCapital = checked(seedUnitPrice * 6);
                wallet.SetGold(openingCapital);
                var observation = new WindowObservation
                {
                    scenarioId = Phase21BuildManifest.Scenarios[0].ScenarioId,
                    sampleType = "controlled_three_plot_staggered_daily_window_without_skills",
                    scene = SceneManager.GetActiveScene().name,
                    isolationScope = "dedicated_batch_process_global_day_quest_weather_not_fully_reversible",
                    purchaseRoute = "ShopManager.TryBuyItem_Shop_Renko_session_initialized_in_Farm",
                    saleRoute = "materialized_FarmScene_SellPoint.Interact",
                    seedId = seed.Id,
                    seedItemId = seed.SeedItem.Id,
                    cropItemId = crop.Id,
                    shopId = shopData.Id,
                    toolSource = "authored_basic_hoe_and_watering_can_equipped_by_fixture",
                    qualityLimit = "derived_from_public_plot_state_before_harvest_not_inventory_quality_or_sale_price",
                    acceptanceLimit = "not_natural_Town_UI_journey_not_L50_L54_not_less_than_20_percent_acceptance",
                    stageLevel = progression.Level,
                    plotCount = plots.Count,
                    firstGameDay = originalDay,
                    stableWindowStartOffset = 3,
                    stableWindowEndOffset = 5,
                    fixtureOpeningCapital = openingCapital,
                    seedUnitBuyPriceAtStart = seedUnitPrice
                };

                for (int offset = 0; offset <= 5; offset++)
                {
                    Assert.That(time.CurrentDay, Is.EqualTo(originalDay + offset));
                    activeDay = new DayObservation
                    {
                        offsetDay = offset,
                        gameDay = time.CurrentDay,
                        plotId = plots[offset % 3].PlotId,
                        source = offset < 3 ? "purchase_till_plant_water" : "harvest_sell_purchase_replant_water",
                        qualityTier = offset < 3 ? "not_harvested" : string.Empty,
                        qualitySource = offset < 3 ? "not_applicable" : "CropQualityResolver_from_FarmPlot_public_state",
                        completedDailyGoalIds = string.Empty,
                        staminaAtStart = stamina.CurrentStamina,
                        walletAtStart = wallet.CurrentGold,
                        ambientGlobalWalletBefore = originalPlayer.CurrentGold
                    };
                    var row = activeDay;
                    var plot = plots[offset % 3];
                    int spent = 0;

                    if (offset >= 3)
                    {
                        Assert.That(plot.State, Is.EqualTo(FarmPlotState.ReadyToHarvest));
                        Assert.That(plot.DaysGrown, Is.EqualTo(seed.GrowthDays));
                        row.qualityTier = CropQualityResolver.Resolve(
                            FarmPlotLogic.BuildQualityInput(plot.WateredDaysCount, plot.DaysGrown,
                                !string.IsNullOrEmpty(plot.FertilizerId))).ToString();
                        int before = stamina.CurrentStamina;
                        InvokePlotBridge(plot, "TryHarvestInternal");
                        spent += AssertSpent(stamina, before, "harvest");
                        row.harvestedQuantity = inventory.GetAmount(crop.Id);
                        Assert.That(row.harvestedQuantity, Is.GreaterThan(0));
                        Assert.That(row.harvestedByEvent, Is.EqualTo(row.harvestedQuantity));
                        Assert.That(inventory.TryPrepareRemoval(crop.Id, row.harvestedQuantity,
                            out var harvestSelection), Is.True);
                        Assert.That(harvestSelection.CopyLots().All(lot => lot.Kind == ItemOriginKind.Earned),
                            Is.True, "Grown wheat must be Earned even when its seed was purchased.");
                        Assert.That(inventory.Items, Has.Count.EqualTo(1),
                            "Only the crop lot may enter the isolated SellPoint.");
                        int walletBeforeSale = wallet.CurrentGold;
                        Assert.That(sellPoint.CanInteract(actor), Is.True);
                        sellPoint.Interact(actor);
                        row.saleGoldDelta = wallet.CurrentGold - walletBeforeSale;
                        row.walletAfterSale = wallet.CurrentGold;
                        Assert.That(row.saleGoldDelta, Is.GreaterThan(0));
                        Assert.That(inventory.GetAmount(crop.Id), Is.Zero);
                        Assert.That(row.saleEventCount, Is.EqualTo(1));
                        Assert.That(row.saleEventGoldDelta, Is.EqualTo(row.saleGoldDelta));
                    }
                    else
                    {
                        row.walletAfterSale = wallet.CurrentGold;
                        Assert.That(row.saleEventCount, Is.Zero);
                    }

                    int walletBeforePurchase = wallet.CurrentGold;
                    var purchase = shop.TryBuyItem(shopData.Id, seed.SeedItem.Id, 1, wallet, inventory);
                    Assert.That(purchase.Success, Is.True, purchase.Message);
                    row.seedBought = 1;
                    row.seedPurchaseGoldDelta = wallet.CurrentGold - walletBeforePurchase;
                    Assert.That(row.seedPurchaseGoldDelta, Is.LessThan(0));
                    Assert.That(row.purchaseEventCount, Is.EqualTo(1));
                    Assert.That(row.purchaseEventGoldDelta, Is.EqualTo(row.seedPurchaseGoldDelta));
                    Assert.That(inventory.GetAmount(seed.SeedItem.Id), Is.EqualTo(1));
                    Assert.That(inventory.TryPrepareRemoval(seed.SeedItem.Id, 1,
                        out var seedSelection), Is.True);
                    Assert.That(seedSelection.CopyLots().Single().Kind,
                        Is.EqualTo(ItemOriginKind.Purchased));
                    Assert.That(seedSelection.CopyLots().Single().UnitPaidBasisGold,
                        Is.EqualTo(-row.seedPurchaseGoldDelta));

                    if (offset < 3)
                    {
                        int before = stamina.CurrentStamina;
                        InvokePlotBridge(plot, "TryTillInternal");
                        spent += AssertSpent(stamina, before, "till");
                        Assert.That(plot.State, Is.EqualTo(FarmPlotState.TilledDry));
                    }
                    else
                    {
                        Assert.That(plot.State, Is.EqualTo(FarmPlotState.TilledDry),
                            "Harvest must leave the same plot ready for a paid replant.");
                    }
                    int plantBefore = stamina.CurrentStamina;
                    InvokePlotBridge(plot, "TryPlantSeedInternal", seed.Id, seed.SeedItem.Id);
                    spent += AssertSpent(stamina, plantBefore, "plant");
                    Assert.That(plot.PlantedSeedId, Is.EqualTo(seed.Id));
                    Assert.That(plot.State, Is.EqualTo(FarmPlotState.PlantedDry));
                    Assert.That(inventory.GetAmount(seed.SeedItem.Id), Is.Zero);
                    row.seedConsumed = 1;

                    foreach (var growingPlot in plots)
                    {
                        if (growingPlot.State != FarmPlotState.PlantedDry) continue;
                        int waterBefore = stamina.CurrentStamina;
                        InvokePlotBridge(growingPlot, "TryWaterInternal");
                        spent += AssertSpent(stamina, waterBefore, "water");
                        Assert.That(growingPlot.State, Is.EqualTo(FarmPlotState.PlantedWet));
                        row.wateredPlotCount++;
                    }
                    Assert.That(row.wateredPlotCount, Is.GreaterThan(0));
                    row.staminaSpent = spent;
                    row.staminaAtEnd = stamina.CurrentStamina;
                    Assert.That(row.staminaAtStart - row.staminaAtEnd, Is.EqualTo(spent));
                    row.walletAtEnd = wallet.CurrentGold;
                    row.transactionNetGoldDelta = row.walletAtEnd - row.walletAtStart;
                    Assert.That(row.transactionNetGoldDelta,
                        Is.EqualTo(row.saleGoldDelta + row.seedPurchaseGoldDelta));
                    row.ambientGlobalWalletAfter = originalPlayer.CurrentGold;
                    row.ambientGlobalWalletDelta = row.ambientGlobalWalletAfter - row.ambientGlobalWalletBefore;
                    foreach (var goalId in row.completedDailyGoalIds.Split(
                                 new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Assert.That(FarmDailyGoalRewardTable.TryGet(goalId, out var reward), Is.True,
                            $"Completed goal {goalId} has no authored reward.");
                        row.authoredGoalRewardGold += reward.Gold;
                    }
                    Assert.That(row.ambientGlobalWalletDelta, Is.EqualTo(row.authoredGoalRewardGold),
                        $"D{offset} ambient global wallet has an unclassified gold source or reward mismatch.");
                    observation.days.Add(row);
                    activeDay = null;
                    if (offset < 5) time.AdvanceDay();
                }

                Assert.That(observation.days, Has.Count.EqualTo(6));
                Assert.That(observation.days.Skip(3).All(day => day.harvestedQuantity > 0), Is.True);
                Assert.That(observation.days.Skip(3).All(day => day.seedConsumed == 1), Is.True);
                Assert.That(progression.Level, Is.EqualTo(Phase21BuildManifest.Level20));
                Assert.That(skills.CaptureSaveData().NodeRanks, Is.Empty);
                observation.medianDailySaleGold = Median(observation.days.Skip(3).Select(day => day.saleGoldDelta));
                observation.medianDailyTransactionNetGold = Median(
                    observation.days.Skip(3).Select(day => day.transactionNetGoldDelta));
                observation.medianDailyAuthoredGoalRewardGold = Median(
                    observation.days.Skip(3).Select(day => day.authoredGoalRewardGold));
                observation.medianDailyComposedNetGold = Median(observation.days.Skip(3).Select(
                    day => day.transactionNetGoldDelta + day.authoredGoalRewardGold));
                WriteObservation(observation);
                TestContext.WriteLine(
                    $"Phase21 controlled L20 income window: D3-D5 transaction net median=" +
                    $"{observation.medianDailyTransactionNetGold}; composed net with authored goal rewards median=" +
                    $"{observation.medianDailyComposedNetGold}; source=FarmPlot bridge/Shop_Renko/SellPoint; " +
                    "natural Town/UI journey=NOT RUN; L50/L54 and <20% acceptance=NOT RUN.");
            }
            finally
            {
                activeDay = null;
                GameEventBus.Unsubscribe(onGoal);
                GameEventBus.Unsubscribe(onHarvest);
                GameEventBus.Unsubscribe(onTransaction);
                for (int i = 0; i < plots.Count; i++)
                {
                    plots[i].RestoreFromSaveData(originalPlots[i]);
                    plots[i].RebindInventoryManager(originalInventory);
                }
                sellPoint.RebindRuntimeManagers(originalInventory, originalPlayer);
                equipment.RestoreFromSaveData(originalEquipment);
                dailyGoals.RestoreFromSaveData(originalDailyGoals);
                progression.RestoreFromSaveData(originalProgression);
                originalPlayer.RestoreState(originalMaxHp, originalHp, originalGold);
                time.SetCurrentDay(originalDay);
                stamina.Initialize(originalMaxStamina, originalStamina);
                if (temporaryShopInitialized)
                {
                    shop.Shutdown();
                    shop.Initialize();
                }
                UnityEngine.Object.Destroy(actor);
            }
            yield return null;
        }

        private static int AssertSpent(StaminaManager stamina, int before, string action)
        {
            int spent = before - stamina.CurrentStamina;
            Assert.That(spent, Is.GreaterThan(0), $"{action} must spend real FarmPlot stamina.");
            return spent;
        }

        private static string AppendId(string existing, string id)
            => string.IsNullOrEmpty(existing) ? id : existing + "|" + id;

        private static int Median(IEnumerable<int> values)
        {
            var sorted = values.OrderBy(value => value).ToArray();
            Assert.That(sorted, Has.Length.EqualTo(3));
            return sorted[1];
        }

        private static void InvokePlotBridge(FarmPlot plot, string methodName, params object[] arguments)
        {
            var method = typeof(FarmPlot).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"FarmPlot menu bridge {methodName} is unavailable.");
            Assert.That(method.IsAssembly, Is.True, $"{methodName} must remain an internal menu bridge.");
            method.Invoke(plot, arguments);
        }

        private static void WriteObservation(WindowObservation observation)
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string output = Path.Combine(project, "Logs");
            Directory.CreateDirectory(output);
            const string stem = "skills-phase21-steady-income-l20-three-plots";
            File.WriteAllText(Path.Combine(output, stem + ".json"),
                JsonUtility.ToJson(observation, true));
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("recordType,scenarioId,stageLevel,offsetDay,gameDay,plotId,source,qualityTier,qualitySource,seedBought,seedConsumed,harvestedQuantity,harvestedByEvent,wateredPlotCount,staminaAtStart,staminaSpent,staminaAtEnd,walletAtStart,walletAfterSale,walletAtEnd,seedPurchaseGoldDelta,saleGoldDelta,transactionNetGoldDelta,purchaseEventGoldDelta,saleEventGoldDelta,ambientGlobalWalletBefore,ambientGlobalWalletAfter,ambientGlobalWalletDelta,authoredGoalRewardGold,completedDailyGoalIds,medianDailySaleGold,medianDailyTransactionNetGold,medianDailyAuthoredGoalRewardGold,medianDailyComposedNetGold");
            foreach (var day in observation.days)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    "day", observation.scenarioId, observation.stageLevel.ToString(), day.offsetDay.ToString(),
                    day.gameDay.ToString(), day.plotId, day.source, day.qualityTier, day.qualitySource,
                    day.seedBought.ToString(), day.seedConsumed.ToString(), day.harvestedQuantity.ToString(),
                    day.harvestedByEvent.ToString(), day.wateredPlotCount.ToString(),
                    day.staminaAtStart.ToString(), day.staminaSpent.ToString(), day.staminaAtEnd.ToString(),
                    day.walletAtStart.ToString(), day.walletAfterSale.ToString(), day.walletAtEnd.ToString(),
                    day.seedPurchaseGoldDelta.ToString(), day.saleGoldDelta.ToString(),
                    day.transactionNetGoldDelta.ToString(), day.purchaseEventGoldDelta.ToString(),
                    day.saleEventGoldDelta.ToString(), day.ambientGlobalWalletBefore.ToString(),
                    day.ambientGlobalWalletAfter.ToString(), day.ambientGlobalWalletDelta.ToString(),
                    day.authoredGoalRewardGold.ToString(), day.completedDailyGoalIds, "", "", "", ""
                }));
            }
            csv.AppendLine(string.Join(",", new[]
            {
                "median_D3_D5", observation.scenarioId, observation.stageLevel.ToString(),
                "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                observation.medianDailySaleGold.ToString(),
                observation.medianDailyTransactionNetGold.ToString(),
                observation.medianDailyAuthoredGoalRewardGold.ToString(),
                observation.medianDailyComposedNetGold.ToString()
            }));
            File.WriteAllText(Path.Combine(output, stem + ".csv"), csv.ToString());
        }
    }
}
