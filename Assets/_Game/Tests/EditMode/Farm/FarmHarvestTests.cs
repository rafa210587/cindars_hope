using NUnit.Framework;
using CindarsHope.Farm.Crops;
using CindarsHope.Farm.Harvest;
using CindarsHope.Farm.Processing;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmHarvestTests
    {
        [Test]
        public void HarvestResult_Ok_ContainsItem()
        {
            var result = HarvestResult.Ok("item_turnip", 2, CropQualityTier.Good, false);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.HarvestedItems.Count);
            Assert.AreEqual("item_turnip", result.HarvestedItems[0].ItemId);
            Assert.AreEqual(2, result.YieldAmount);
        }

        [Test]
        public void HarvestResult_Fail_NotSuccess()
        {
            var result = HarvestResult.Fail(HarvestFailureReason.CropNotReady);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HarvestFailureReason.CropNotReady, result.FailureReason);
        }

        [Test]
        public void HarvestResult_DeadCrop_FailsWithDeadReason()
        {
            var result = HarvestResult.Fail(HarvestFailureReason.CropDead);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(HarvestFailureReason.CropDead, result.FailureReason);
        }

        [Test]
        public void HarvestResult_RegrowStarted_SetCorrectly()
        {
            var result = HarvestResult.Ok("item_herbs", 1, CropQualityTier.Normal, regrowStarted: true);
            Assert.IsTrue(result.RegrowStarted);
        }

        [Test]
        public void YieldResolver_BaseYield_InRange()
        {
            var def = new CropDefinitionData("crop_test", "seed_test", "item_test", 5)
            {
                ExpectedYieldMin = 1,
                ExpectedYieldMax = 3
            };
            var input = new CropYieldInput { Definition = def, Quality = CropQualityTier.Normal, Seed = 42 };
            int yield = CropYieldResolver.Resolve(input);
            Assert.GreaterOrEqual(yield, 1);
            Assert.LessOrEqual(yield, 3);
        }

        [Test]
        public void YieldResolver_ExcellentQuality_MayAddBonus()
        {
            var def = new CropDefinitionData("crop_test", "seed_test", "item_test", 5)
            {
                ExpectedYieldMin = 2,
                ExpectedYieldMax = 2
            };
            var input = new CropYieldInput { Definition = def, Quality = CropQualityTier.Excellent, Seed = 0 };
            int yield = CropYieldResolver.Resolve(input);
            Assert.GreaterOrEqual(yield, 2);
        }

        [Test]
        public void YieldResolver_NullDefinition_ReturnsZero()
        {
            var input = new CropYieldInput { Definition = null, Seed = 0 };
            Assert.AreEqual(0, CropYieldResolver.Resolve(input));
        }

        [Test]
        public void ProcessingJob_ReadyOnCorrectDay()
        {
            var job = new FarmProcessingJob
            {
                JobId = "job_01",
                StartDay = 1,
                FinishDay = 3,
                State = ProcessingJobState.Processing
            };
            Assert.IsFalse(job.IsReadyOnDay(2));
            Assert.IsTrue(job.IsReadyOnDay(3));
            Assert.IsTrue(job.IsReadyOnDay(4));
        }

        [Test]
        public void ProcessingJob_Collect_Idempotent()
        {
            var job = new FarmProcessingJob { State = ProcessingJobState.ReadyToCollect };
            Assert.IsTrue(job.Collect());
            Assert.IsFalse(job.Collect(), "Second collection must fail (idempotency)");
            Assert.AreEqual(ProcessingJobState.Collected, job.State);
        }

        [Test]
        public void ProcessingJob_CannotCollect_WhenNotReady()
        {
            var job = new FarmProcessingJob { State = ProcessingJobState.Processing };
            Assert.IsFalse(job.Collect());
        }

        [Test]
        public void ProcessableItem_NoInfiniteReprocessingByDefault()
        {
            var item = new ProcessableItem { InputItemId = "item_milk", OutputItemId = "item_cheese" };
            Assert.IsFalse(item.AllowsInfiniteReprocessing,
                "Infinite reprocessing must be opt-in, not default");
        }
    }
}
