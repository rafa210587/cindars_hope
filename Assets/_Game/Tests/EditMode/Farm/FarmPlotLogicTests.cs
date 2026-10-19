using NUnit.Framework;
using CindarsHope.Farm;
using CindarsHope.Farm.Crops;

namespace CindarsHope.Farm.Tests
{
    [TestFixture]
    public class FarmPlotLogicTests
    {
        private static SeedParams MakeSeed(int growthDays = 3, int regrowDays = 0)
        {
            return new SeedParams
            {
                SeedId = "seed_carrot",
                GrowthDays = growthDays,
                RegrowDays = regrowDays,
                HarvestPairs = new[] { ("item_carrot", 2) },
                IsValid = true
            };
        }

        [Test]
        public void TryTill_FromRaw_WithTool_Succeeds()
        {
            var logic = new FarmPlotLogic();
            Assert.AreEqual(FarmPlotState.Raw, logic.State);
            var result = logic.TryTill(hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(result);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void TryTill_FromRaw_WithoutTool_AndNoSliceMode_Fails()
        {
            var logic = new FarmPlotLogic();
            var result = logic.TryTill(hasTool: false, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(result);
            Assert.AreEqual(FarmPlotState.Raw, logic.State);
        }

        [Test]
        public void TryWaterPlot_FromTilledDry_Succeeds()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            var result = logic.TryWaterPlot(hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(result);
            Assert.AreEqual(FarmPlotState.TilledWet, logic.State);
        }

        [Test]
        public void TryPlantSeed_SeasonDenied_ReturnsFalse()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            var result = logic.TryPlantSeed("seed_carrot", seasonAllowed: false, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            Assert.IsFalse(result);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void TryPlantSeed_ValidState_SetsPlantedSeedId()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            var result = logic.TryPlantSeed("seed_carrot", seasonAllowed: true, hasInInventory: true, temporaryBypass: false, staminaOk: true);
            Assert.IsTrue(result);
            Assert.AreEqual("seed_carrot", logic.PlantedSeedId);
            Assert.AreEqual(FarmPlotState.PlantedDry, logic.State);
        }

        [Test]
        public void ProcessDay_WetPlant_AdvancesDaysGrown()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            var seed = MakeSeed(growthDays: 3);
            var result = logic.ProcessDay(1, seed);
            Assert.IsTrue(result.CropAdvanced || result.BecameReady);
            Assert.AreEqual(1, logic.DaysGrown);
        }

        [Test]
        public void ProcessDay_SameDay_IsIdempotent()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            var seed = MakeSeed(growthDays: 3);
            logic.ProcessDay(1, seed);
            var daysAfterFirst = logic.DaysGrown;
            logic.SetStateInternal(FarmPlotState.PlantedWet); // restaurar estado para wet novamente (workaround do dry)
            logic.ProcessDay(1, seed); // mesmo dia
            Assert.AreEqual(daysAfterFirst, logic.DaysGrown, "ProcessDay idempotente para o mesmo dia");
        }

        [Test]
        public void ProcessDay_DryPlant_DiesAfterThreshold()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            // Deixar PlantedDry e processar dias sem rega
            var seed = MakeSeed(growthDays: 5);
            DayResult last = default;
            for (var d = 1; d <= FarmPlotLogic.DefaultDeathThresholdDays; d++)
            {
                last = logic.ProcessDay(d, seed);
            }
            Assert.IsTrue(last.CropDied);
            Assert.AreEqual(FarmPlotState.Dead, logic.State);
        }

        [Test]
        public void ProcessDay_TilledWet_DriesOut()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledWet);
            var seed = new SeedParams { IsValid = false };
            var result = logic.ProcessDay(1, seed);
            Assert.IsTrue(result.TilledWetDried);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void TryHarvestPure_ReadyState_ReturnsSuccessWithItems()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            var seed = MakeSeed();
            var outcome = logic.TryHarvestPure(seed, yieldModifier: 0f, fertilizerActive: false);
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(1, outcome.Items.Length);
            Assert.AreEqual("item_carrot", outcome.Items[0].itemId);
            Assert.AreEqual(4, outcome.Items[0].amount,
                "A colheita atual inclui +2 unidades da qualidade Excellent.");
        }

        [Test]
        public void OnHarvestCompleted_WithRegrow_SetsCorrectDaysGrown()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            var seed = MakeSeed(growthDays: 6, regrowDays: 2);
            logic.OnHarvestCompleted(seed);
            // DaysGrown = GrowthDays - RegrowDays = 6 - 2 = 4
            Assert.AreEqual(4, logic.DaysGrown);
            Assert.AreEqual(2, logic.RegrowRemainingDays);
            Assert.AreEqual(FarmPlotState.PlantedDry, logic.State);
        }

        [Test]
        public void OnHarvestCompleted_NoRegrow_ResetsPlot()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            var seed = MakeSeed(growthDays: 3, regrowDays: 0);
            logic.OnHarvestCompleted(seed);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
            Assert.AreEqual(string.Empty, logic.PlantedSeedId);
            Assert.AreEqual(0, logic.DaysGrown);
        }

        [Test]
        public void TryFertilizePure_SetsId()
        {
            var logic = new FarmPlotLogic();
            var result = logic.TryFertilizePure("fert_basic", hasInInventory: true);
            Assert.IsTrue(result);
            Assert.AreEqual("fert_basic", logic.FertilizerId);
        }

        [Test]
        public void CaptureSaveData_RestoreFromSaveData_RoundTrip()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            logic.ProcessDay(5, MakeSeed());

            var save = logic.CaptureSaveData(plotIndex: 0);

            var logic2 = new FarmPlotLogic();
            logic2.RestoreFromSaveData(save, seedIsResolvable: true);

            Assert.AreEqual(logic.DaysGrown, logic2.DaysGrown);
            Assert.AreEqual(logic.PlantedSeedId, logic2.PlantedSeedId);
            Assert.AreEqual(logic.LastProcessedDay, logic2.LastProcessedDay);
        }

        [Test]
        public void BuildQualityInput_FullWatering_ReturnsHighScore()
        {
            // wateredDaysCount == daysGrown => consistência = 100%; com SeasonMatch=true => score >= ExcellentThreshold
            var input = FarmPlotLogic.BuildQualityInput(wateredDaysCount: 5, daysGrown: 5, fertilizerApplied: false);
            Assert.AreEqual(100, input.WateringConsistencyScore);
            Assert.IsTrue(input.SeasonMatch);
            var tier = CropQualityResolver.Resolve(input);
            Assert.GreaterOrEqual((int)tier, (int)CropQualityTier.Excellent);
        }

        [Test]
        public void TryWaterSilent_FromTilledDry_SetsWet()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            var success = logic.TryWaterSilent();
            Assert.IsTrue(success);
            Assert.AreEqual(FarmPlotState.TilledWet, logic.State);
        }

        [Test]
        public void TryWaterSilent_FromPlantedDry_SetsPlantedWet()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            var success = logic.TryWaterSilent();
            Assert.IsTrue(success);
            Assert.AreEqual(FarmPlotState.PlantedWet, logic.State);
        }

        [Test]
        public void TryWaterSilent_FromReadyToHarvest_ReturnsFalse()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            var success = logic.TryWaterSilent();
            Assert.IsFalse(success);
        }

        [Test]
        public void TryAdvanceStagePure_Increments_DaysGrown()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            var seed = MakeSeed(growthDays: 3);
            var advanced = logic.TryAdvanceStagePure(seed);
            Assert.IsTrue(advanced);
            Assert.AreEqual(1, logic.DaysGrown);
        }

        [Test]
        public void TryAdvanceStagePure_ReachesGrowthDays_BecomesReadyToHarvest()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            var seed = MakeSeed(growthDays: 2);
            logic.TryAdvanceStagePure(seed); // day 1
            logic.TryAdvanceStagePure(seed); // day 2 => ready
            Assert.AreEqual(FarmPlotState.ReadyToHarvest, logic.State);
        }

        [Test]
        public void TryClearDead_WithTool_ResetsToTilledDry()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.Dead);
            // Restaurar plantedSeedId que SetStateInternal limpa em Dead (precisa ser Dead)
            // Dead não limpa plantedSeedId no SetStateInternal apenas se já é Dead
            var (success, clearedId) = logic.TryClearDead(hasTool: true, temporarySliceMode: false, staminaOk: true);
            Assert.IsTrue(success);
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
        }

        [Test]
        public void TryClearDead_WithoutTool_NoSliceMode_Fails()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.Dead);
            var (success, _) = logic.TryClearDead(hasTool: false, temporarySliceMode: false, staminaOk: true);
            Assert.IsFalse(success);
            Assert.AreEqual(FarmPlotState.Dead, logic.State);
        }

        [Test]
        public void RestoreFromSaveData_UnresolvableSeed_FallsBackToTilledDry()
        {
            var logic = new FarmPlotLogic();
            var save = new FarmPlotSaveData
            {
                PlotIndex = 0,
                State = FarmPlotState.PlantedWet.ToString(),
                PlantedSeedId = "seed_unknown",
                DaysGrown = 2,
                GrowthProgressDays = 2,
                LastUpdatedDay = 5,
                LastProcessedDay = 4
            };
            var restored = logic.RestoreFromSaveData(save, seedIsResolvable: false);
            Assert.IsTrue(restored); // retorna true mas estado é normalizado
            Assert.AreEqual(FarmPlotState.TilledDry, logic.State);
            Assert.AreEqual(string.Empty, logic.PlantedSeedId);
        }

        [Test]
        public void ProcessDay_WetPlant_SingleDay_DriesAfterProcess()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.PlantedWet);
            var seed = MakeSeed(growthDays: 3);
            logic.ProcessDay(1, seed);
            // Após processar dia com PlantedWet, deve secar para PlantedDry (não ready ainda)
            Assert.AreEqual(FarmPlotState.PlantedDry, logic.State);
        }

        [Test]
        public void TryHarvestPure_WithFertilizer_AppliesYieldModifier()
        {
            var logic = new FarmPlotLogic();
            logic.SetStateInternal(FarmPlotState.TilledDry);
            logic.TryPlantSeed("seed_carrot", true, true, false, true);
            logic.SetStateInternal(FarmPlotState.ReadyToHarvest);
            var seed = MakeSeed();
            var outcome = logic.TryHarvestPure(seed, yieldModifier: 0.5f, fertilizerActive: true);
            Assert.IsTrue(outcome.Success);
            // baseAmount=2, yieldModifier=0.5 => +1 => 3
            Assert.AreEqual(5, outcome.Items[0].amount,
                "Base 2 + bônus de fertilizante 1 + bônus de qualidade Excellent 2.");
            Assert.IsTrue(outcome.FertilizerWasActive);
        }
    }
}
