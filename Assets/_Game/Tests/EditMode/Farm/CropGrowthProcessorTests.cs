using NUnit.Framework;
using CindarsHope.Farm.Crops;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class CropGrowthProcessorTests
    {
        private CropGrowthProcessor _processor;
        private CropDefinitionData _turnipDef;

        [SetUp]
        public void Setup()
        {
            _processor = new CropGrowthProcessor();
            _turnipDef = new CropDefinitionData("crop_turnip", "seed_turnip", "item_turnip", 5)
            {
                RequiresWater = true,
                DiesAfterDaysWithoutWater = 3
            };
        }

        private CropGrowthState StartState() => new CropGrowthState
        {
            DaysGrown = 0,
            DaysWithoutWater = 0,
            IsDead = false,
            IsReadyToHarvest = false,
            LastProcessedDay = 0
        };

        private CropGrowthInput WateredInput(int day) => new CropGrowthInput
        {
            IsWateredToday = true,
            IsValidSeason = true,
            CurrentDay = day,
            Definition = _turnipDef
        };

        private CropGrowthInput DryInput(int day) => new CropGrowthInput
        {
            IsWateredToday = false,
            IsValidSeason = true,
            CurrentDay = day,
            Definition = _turnipDef
        };

        [Test]
        public void WateredCrop_GrowsOneDay()
        {
            var state = StartState();
            var result = _processor.Process(state, WateredInput(1));
            Assert.AreEqual(1, result.DaysGrown);
            Assert.IsFalse(result.IsReadyToHarvest);
        }

        [Test]
        public void UnwateredCrop_DoesNotGrow()
        {
            var state = StartState();
            var result = _processor.Process(state, DryInput(1));
            Assert.AreEqual(0, result.DaysGrown);
        }

        [Test]
        public void UnwateredCrop_IncrementsDaysWithoutWater()
        {
            var state = StartState();
            var result = _processor.Process(state, DryInput(1));
            Assert.AreEqual(1, result.DaysWithoutWater);
        }

        [Test]
        public void CropDies_AtDeathThreshold()
        {
            var state = StartState();
            state = _processor.Process(state, DryInput(1));
            state = _processor.Process(state, DryInput(2));
            state = _processor.Process(state, DryInput(3));
            Assert.IsTrue(state.IsDead);
        }

        [Test]
        public void CropDoesNotDie_BeforeThreshold()
        {
            var state = StartState();
            state = _processor.Process(state, DryInput(1));
            state = _processor.Process(state, DryInput(2));
            Assert.IsFalse(state.IsDead);
        }

        [Test]
        public void DeadCrop_IsNotProcessed()
        {
            var state = StartState();
            state.IsDead = true;
            state.DaysGrown = 0;
            var result = _processor.Process(state, WateredInput(1));
            Assert.AreEqual(0, result.DaysGrown);
            Assert.IsTrue(result.IsDead);
        }

        [Test]
        public void ReadyCrop_IsNotProcessedFurther()
        {
            var state = StartState();
            state.IsReadyToHarvest = true;
            state.DaysGrown = 5;
            var result = _processor.Process(state, WateredInput(10));
            Assert.AreEqual(5, result.DaysGrown);
        }

        [Test]
        public void CropBecomesReady_WhenGrowthDaysReached()
        {
            var state = StartState();
            for (int day = 1; day <= 5; day++)
                state = _processor.Process(state, WateredInput(day));
            Assert.IsTrue(state.IsReadyToHarvest);
        }

        [Test]
        public void SameDay_NotProcessedTwice()
        {
            var state = StartState();
            state = _processor.Process(state, WateredInput(5));
            var secondProcess = _processor.Process(state, WateredInput(5));
            Assert.AreEqual(state.DaysGrown, secondProcess.DaysGrown);
        }

        [Test]
        public void WateringResets_DaysWithoutWater()
        {
            var state = StartState();
            state = _processor.Process(state, DryInput(1));
            state = _processor.Process(state, DryInput(2));
            Assert.AreEqual(2, state.DaysWithoutWater);
            state = _processor.Process(state, WateredInput(3));
            Assert.AreEqual(0, state.DaysWithoutWater);
        }

        [Test]
        public void InvalidSeason_NoCropDeathOrGrowth()
        {
            var state = StartState();
            var offSeasonInput = new CropGrowthInput
            {
                IsWateredToday = false,
                IsValidSeason = false,
                CurrentDay = 1,
                Definition = _turnipDef
            };
            state = _processor.Process(state, offSeasonInput);
            state = _processor.Process(state, new CropGrowthInput { IsWateredToday = false, IsValidSeason = false, CurrentDay = 2, Definition = _turnipDef });
            state = _processor.Process(state, new CropGrowthInput { IsWateredToday = false, IsValidSeason = false, CurrentDay = 3, Definition = _turnipDef });
            Assert.IsFalse(state.IsDead, "Crop should not die during invalid season (dormant)");
            Assert.AreEqual(0, state.DaysGrown);
        }

        [Test]
        public void QualityResolver_ExcellentWhenHighScore()
        {
            var input = new CropQualityInput { WateringConsistencyScore = 80, SeasonMatch = false, FertilizerApplied = false, IsQualityEnabled = true };
            Assert.AreEqual(CropQualityTier.Excellent, CropQualityResolver.Resolve(input));
        }

        [Test]
        public void QualityResolver_GoodWhenMediumScore()
        {
            var input = new CropQualityInput { WateringConsistencyScore = 55, SeasonMatch = false, FertilizerApplied = false, IsQualityEnabled = true };
            Assert.AreEqual(CropQualityTier.Good, CropQualityResolver.Resolve(input));
        }

        [Test]
        public void QualityResolver_NormalWhenDisabled()
        {
            var input = new CropQualityInput { WateringConsistencyScore = 100, SeasonMatch = true, IsQualityEnabled = false };
            Assert.AreEqual(CropQualityTier.Normal, CropQualityResolver.Resolve(input));
        }
    }
}
