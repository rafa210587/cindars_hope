using CindarsHope.Farm.Buildings;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Farm
{
    public class ConstructionJobTests
    {
        private FarmBuildingDefinition _workshop;
        private ConstructionJob _job;

        [SetUp]
        public void SetUp()
        {
            _workshop = new FarmBuildingDefinition("workshop", "Workshop", 6, 5)
            {
                IsWorkshop = true,
                CostGold = 100,
                CostWood = 50,
                BuildTimeDays = 3
            };

            _job = new ConstructionJob("job_1", _workshop, 10, 15);
        }

        [Test]
        public void JobStartsInPendingState()
        {
            Assert.That(_job.State, Is.EqualTo(ConstructionState.Pending));
        }

        [Test]
        public void StartConstructionTransitionsToInProgress()
        {
            _job.StartConstruction(5);
            Assert.That(_job.State, Is.EqualTo(ConstructionState.InProgress));
        }

        [Test]
        public void StartConstructionSetsStartDay()
        {
            _job.StartConstruction(5);
            Assert.That(_job.StartDay, Is.EqualTo(5));
        }

        [Test]
        public void StartConstructionSetsCompletionDay()
        {
            _job.StartConstruction(5);
            Assert.That(_job.CompletionDay, Is.EqualTo(5 + _workshop.BuildTimeDays));
        }

        [Test]
        public void CompleteConstructionTransitionsToCompleted()
        {
            _job.StartConstruction(5);
            _job.CompleteConstruction();
            Assert.That(_job.State, Is.EqualTo(ConstructionState.Completed));
        }

        [Test]
        public void CancelTransitionsToCancelled()
        {
            _job.StartConstruction(5);
            _job.Cancel();
            Assert.That(_job.State, Is.EqualTo(ConstructionState.Cancelled));
        }

        [Test]
        public void RotationIsNormalized()
        {
            var job = new ConstructionJob("job_2", _workshop, 10, 15, 8); // 8 % 4 = 0
            Assert.That(job.Rotation, Is.EqualTo(0));
        }

        [Test]
        public void FarmBuildingWithCostHasValidCost()
        {
            Assert.That(_workshop.HasValidCost(), Is.True);
        }

        [Test]
        public void FarmBuildingWithNoCostIsFree()
        {
            var freBuilding = new FarmBuildingDefinition("free", "Free Building", 2, 2);
            Assert.That(freBuilding.IsFree(), Is.True);
        }

        [Test]
        public void DaysRemainingIsInitiallyEqual ToBuildTimeDays()
        {
            Assert.That(_job.DaysRemaining, Is.EqualTo(_workshop.BuildTimeDays));
        }
    }
}
