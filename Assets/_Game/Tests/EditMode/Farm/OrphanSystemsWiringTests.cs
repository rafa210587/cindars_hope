using CindarsHope.Farm;
using CindarsHope.Farm.Crops;
using CindarsHope.Farm.Integration;
using CindarsHope.Farm.Resources;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Farm
{
    /// <summary>
    /// F15 — wiring dos módulos órfãos (clima, chuva, refresh, qualidade, fertilizante).
    /// Os módulos em si já têm testes das WAVES 02/05; aqui validamos o wiring e os helpers novos.
    /// </summary>
    public class OrphanSystemsWiringTests
    {
        [SetUp]
        public void SetUp()
        {
            FarmFertilityRuntime.ResetForTests();
        }

        // ------------------------------------------------------------------ clima

        [Test]
        public void ResolveWeatherForDay_IsDeterministic_AndMatchesGenerator()
        {
            for (var day = 1; day <= 12; day++)
            {
                var first = WorldWeatherService.ResolveWeatherForDay(day);
                var second = WorldWeatherService.ResolveWeatherForDay(day);
                var expected = WeatherGenerator.GenerateWeather(GameDate.FromAbsoluteDay(day));

                Assert.AreEqual(first, second, $"Dia {day} deve ser determinístico.");
                Assert.AreEqual(expected, first, $"Dia {day} deve casar com o WeatherGenerator.");
            }
        }

        [Test]
        public void IsWetWeather_OnlyRainAndStorm()
        {
            Assert.IsTrue(WorldWeatherService.IsWetWeather(WeatherType.Rainy));
            Assert.IsTrue(WorldWeatherService.IsWetWeather(WeatherType.Stormy));
            Assert.IsFalse(WorldWeatherService.IsWetWeather(WeatherType.Clear));
            Assert.IsFalse(WorldWeatherService.IsWetWeather(WeatherType.Cloudy));
        }

        // ------------------------------------------------------------------ chuva rega

        [Test]
        public void ShouldWaterState_ZeroPercent_NeverWaters()
        {
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.PlantedDry, 0));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.TilledDry, 0));
        }

        [Test]
        public void ShouldWaterState_Rain50_WatersOnlyPlanted()
        {
            Assert.IsTrue(RainIrrigationRunner.ShouldWaterState(FarmPlotState.PlantedDry, 50));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.TilledDry, 50));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.PlantedWet, 50));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.ReadyToHarvest, 50));
        }

        [Test]
        public void ShouldWaterState_Storm100_WatersPlantedAndTilled()
        {
            Assert.IsTrue(RainIrrigationRunner.ShouldWaterState(FarmPlotState.PlantedDry, 100));
            Assert.IsTrue(RainIrrigationRunner.ShouldWaterState(FarmPlotState.TilledDry, 100));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.Raw, 100));
            Assert.IsFalse(RainIrrigationRunner.ShouldWaterState(FarmPlotState.Dead, 100));
        }

        // ------------------------------------------------------------------ qualidade

        [Test]
        public void BuildQualityInput_FullWateringPlusFertilizer_ResolvesExcellent()
        {
            var input = FarmPlot.BuildQualityInput(10, 10, fertilizerApplied: true);
            Assert.AreEqual(100, input.WateringConsistencyScore);
            Assert.AreEqual(CropQualityTier.Excellent, CropQualityResolver.Resolve(input));
        }

        [Test]
        public void BuildQualityInput_HalfWatering_ResolvesGood()
        {
            // 50 + 10 (estação) = 60 >= 50 → Good
            var input = FarmPlot.BuildQualityInput(5, 10, fertilizerApplied: false);
            Assert.AreEqual(CropQualityTier.Good, CropQualityResolver.Resolve(input));
        }

        [Test]
        public void BuildQualityInput_PoorWatering_ResolvesNormal()
        {
            // 20 + 10 = 30 < 50 → Normal
            var input = FarmPlot.BuildQualityInput(2, 10, fertilizerApplied: false);
            Assert.AreEqual(CropQualityTier.Normal, CropQualityResolver.Resolve(input));
        }

        [Test]
        public void BuildQualityInput_ClampsConsistency()
        {
            var input = FarmPlot.BuildQualityInput(50, 10, fertilizerApplied: false);
            Assert.AreEqual(100, input.WateringConsistencyScore);
        }

        [Test]
        public void GetQualityBonusUnits_Mapping()
        {
            Assert.AreEqual(0, FarmPlot.GetQualityBonusUnits(CropQualityTier.Normal));
            Assert.AreEqual(1, FarmPlot.GetQualityBonusUnits(CropQualityTier.Good));
            Assert.AreEqual(2, FarmPlot.GetQualityBonusUnits(CropQualityTier.Excellent));
        }

        // ------------------------------------------------------------------ fertilizante

        [Test]
        public void FertilityRuntime_ApplyAndConsume_Lifecycle()
        {
            var result = FarmFertilityRuntime.TryApply("plot_0", "fertilizer_simple", currentDay: 1);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(FarmFertilityRuntime.HasActiveFertilizer("plot_0"));

            FarmFertilityRuntime.ConsumeOnHarvest("plot_0");
            Assert.IsFalse(FarmFertilityRuntime.HasActiveFertilizer("plot_0"));
        }

        [Test]
        public void FertilityRuntime_ImprovedReplacesLowerTier()
        {
            Assert.IsTrue(FarmFertilityRuntime.TryApply("plot_1", "fertilizer_simple", 1).Success);
            var upgrade = FarmFertilityRuntime.TryApply("plot_1", "fertilizer_improved", 1);
            Assert.IsTrue(upgrade.Success, "Improved (ReplaceLowerTier) deve substituir o simples.");
        }

        [Test]
        public void FertilityRuntime_UnknownFertilizer_Fails()
        {
            var result = FarmFertilityRuntime.TryApply("plot_2", "fertilizer_fake", 1);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void FertilityRuntime_RestoreModifier_RebuildsActiveState()
        {
            FarmFertilityRuntime.RestoreModifier("plot_3", "fertilizer_simple", 5);
            Assert.IsTrue(FarmFertilityRuntime.HasActiveFertilizer("plot_3"));
        }

        // ------------------------------------------------------------------ refresh de nós

        [Test]
        public void DefaultDefinitions_CoverFarmInteractableTypes()
        {
            var definitions = FarmResourceRefreshRuntime.BuildDefaultDefinitions();
            Assert.IsTrue(definitions.ContainsKey("farm_node_tree"));
            Assert.IsTrue(definitions.ContainsKey("farm_node_rock"));
            Assert.IsTrue(definitions.ContainsKey("farm_node_forage"));
        }

        [Test]
        public void ResolveNodeId_Mapping()
        {
            Assert.AreEqual("farm_node_tree", FarmResourceRefreshRuntime.ResolveNodeId(FarmResourceInteractableType.Tree));
            Assert.AreEqual("farm_node_rock", FarmResourceRefreshRuntime.ResolveNodeId(FarmResourceInteractableType.Rock));
            Assert.AreEqual("farm_node_forage", FarmResourceRefreshRuntime.ResolveNodeId(FarmResourceInteractableType.Forage));
            Assert.AreEqual(string.Empty, FarmResourceRefreshRuntime.ResolveNodeId(FarmResourceInteractableType.LakeFishing));
        }

        [Test]
        public void RefreshProcessor_FixedDays_RespectsEligibleDay()
        {
            var processor = new FarmResourceRefreshProcessor(FarmResourceRefreshRuntime.BuildDefaultDefinitions());
            var node = new ResourceNodeInstanceState
            {
                NodeInstanceId = "farm_node_tree_0_0",
                NodeId = "farm_node_tree",
                CurrentState = ResourceNodeCurrentState.Harvested,
                LastHarvestedDay = 1,
                NextEligibleRefreshDay = 4,
                RandomSeed = 7
            };

            Assert.IsFalse(processor.TryRefresh(node, BuildContext(3)), "Dia 3 < dia elegível 4: não renova.");
            Assert.IsTrue(processor.TryRefresh(node, BuildContext(4)), "Dia 4: renova.");
            Assert.AreEqual(ResourceNodeCurrentState.Available, node.CurrentState);
        }

        private static ResourceNodeRefreshContext BuildContext(int day)
        {
            return new ResourceNodeRefreshContext
            {
                CurrentDay = day,
                CurrentSeason = Season.Primavera.ToString(),
                CurrentWeather = WeatherType.Clear.ToString(),
                FarmLevel = 0,
                RandomSeed = day
            };
        }
    }
}
