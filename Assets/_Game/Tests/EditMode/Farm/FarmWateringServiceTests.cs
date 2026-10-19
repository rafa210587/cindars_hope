using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Watering;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmWateringServiceTests
    {
        private FarmWateringService _service;

        [SetUp]
        public void Setup() => _service = new FarmWateringService();

        private FarmPlotWaterState ExternalPlot(string id = "plot_01") =>
            new FarmPlotWaterState { PlotId = id, IsExternal = true };

        private FarmPlotWaterState InteriorPlot(string id = "plot_in") =>
            new FarmPlotWaterState { PlotId = id, IsInterior = true, IsExternal = false };

        private FarmPlotWaterState GreenhousePlot(string id = "plot_gh") =>
            new FarmPlotWaterState { PlotId = id, IsGreenhouse = true, IsExternal = false };

        [Test]
        public void ManualWatering_MarksPlotWatered()
        {
            var plot = ExternalPlot();
            _service.ApplyManualWatering(plot, 5);
            Assert.IsTrue(plot.IsWateredToday);
            Assert.AreEqual(5, plot.LastWateredDay);
            Assert.Contains(WaterSource.Manual, plot.WateredBySources);
        }

        [Test]
        public void ManualWatering_DoesNotGrowCropDirectly()
        {
            var plot = ExternalPlot();
            _service.ApplyManualWatering(plot, 1);
            Assert.IsTrue(plot.IsWateredToday);
            // No crop growth property on WaterState — watering is decoupled
        }

        [Test]
        public void Rain_WatersExternalPlots()
        {
            var plots = new List<FarmPlotWaterState> { ExternalPlot() };
            _service.ApplyRainWatering(plots, 3);
            Assert.IsTrue(plots[0].IsWateredToday);
            Assert.Contains(WaterSource.Rain, plots[0].WateredBySources);
        }

        [Test]
        public void Rain_DoesNotWaterInteriorPlots()
        {
            var plots = new List<FarmPlotWaterState> { InteriorPlot() };
            _service.ApplyRainWatering(plots, 3);
            Assert.IsFalse(plots[0].IsWateredToday);
        }

        [Test]
        public void Rain_DoesNotWaterGreenhousePlots()
        {
            var plots = new List<FarmPlotWaterState> { GreenhousePlot() };
            _service.ApplyRainWatering(plots, 3);
            Assert.IsFalse(plots[0].IsWateredToday);
        }

        [Test]
        public void Storm_MarksExternalPlotsWithStormSource()
        {
            var plots = new List<FarmPlotWaterState> { ExternalPlot() };
            _service.ApplyRainWatering(plots, 4, isStorm: true);
            Assert.IsTrue(plots[0].IsWateredToday);
            Assert.Contains(WaterSource.Storm, plots[0].WateredBySources);
        }

        [Test]
        public void IrrigationCoverage_WatersMatchingPlots()
        {
            var plot = ExternalPlot();
            plot.IrrigationCoverageId = "zone_a";
            _service.ApplyIrrigationCoverage(new List<FarmPlotWaterState> { plot }, "zone_a", 2);
            Assert.IsTrue(plot.IsWateredToday);
            Assert.Contains(WaterSource.IrrigationSimple, plot.WateredBySources);
        }

        [Test]
        public void IrrigationCoverage_DoesNotWaterNonMatchingPlots()
        {
            var plot = ExternalPlot();
            plot.IrrigationCoverageId = "zone_b";
            _service.ApplyIrrigationCoverage(new List<FarmPlotWaterState> { plot }, "zone_a", 2);
            Assert.IsFalse(plot.IsWateredToday);
        }

        [Test]
        public void ResetDayWaterStates_ClearsWaterState()
        {
            var plot = ExternalPlot();
            _service.ApplyManualWatering(plot, 1);
            Assert.IsTrue(plot.IsWateredToday);
            _service.ResetDayWaterStates(new List<FarmPlotWaterState> { plot });
            Assert.IsFalse(plot.IsWateredToday);
            Assert.AreEqual(0, plot.WateredBySources.Count);
        }

        [Test]
        public void Greenhouse_ExcludesRain()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_gh");
            Assert.IsTrue(provider.IsRainExcluded("plot_gh"));
            Assert.IsFalse(provider.IsRainExcluded("plot_01"));
        }

        [Test]
        public void Greenhouse_AllowsSeasonOverride_WhenUnlocked()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_gh");
            provider.SetGreenhouseUnlocked(true);
            Assert.IsTrue(provider.CanOverrideSeason("plot_gh"));
        }

        [Test]
        public void Greenhouse_BlocksSeasonOverride_WhenLocked()
        {
            var provider = new GreenhouseContextProvider();
            provider.RegisterGreenhousePlot("plot_gh");
            provider.SetGreenhouseUnlocked(false);
            Assert.IsFalse(provider.CanOverrideSeason("plot_gh"));
        }
    }
}
