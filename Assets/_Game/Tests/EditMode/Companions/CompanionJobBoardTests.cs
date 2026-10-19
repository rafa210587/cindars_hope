using NUnit.Framework;
using CindarsHope.Companions;

namespace CindarsHope.Tests.EditMode.Companions
{
    [TestFixture]
    public class CompanionJobBoardTests
    {
        private JobBoardState boardState;
        private CompanionJobBoardService jobBoardService;

        [SetUp]
        public void Setup()
        {
            boardState = new JobBoardState();
            jobBoardService = new CompanionJobBoardService(boardState);

            RegisterTestJobs();
            jobBoardService.RegisterCompanionStamina("companion_01", 100);
        }

        private void RegisterTestJobs()
        {
            var planterJob = new CompanionFarmJobDefinition
            {
                JobId = "job_planter_01",
                JobType = CompanionFarmJobType.Planter,
                RequiredToolOrStation = "plow",
                StartTimeHour = 6,
                EndTimeHour = 12,
                StaminaBudgetPerDay = 20,
                RelationshipGainPerJob = 1,
                FatigueGainPerJob = 2,
                DailyLimit = 3,
                Priority = 1,
                Area = new AllowedArea { AreaId = "farm_north", GridX = 0, GridY = 0, SizeX = 10, SizeY = 10 },
                Resources = new AllowedResources { MaxDailyConsumption = 5 },
                OutputRules = new OutputRules { DestinationStorageId = "storage_seeds" }
            };

            boardState.AddJob(planterJob);

            var watererJob = new CompanionFarmJobDefinition
            {
                JobId = "job_waterer_01",
                JobType = CompanionFarmJobType.Waterer,
                RequiredToolOrStation = "watering_can",
                StartTimeHour = 8,
                EndTimeHour = 16,
                StaminaBudgetPerDay = 15,
                RelationshipGainPerJob = 1,
                FatigueGainPerJob = 1,
                DailyLimit = 5,
                Priority = 1,
                Area = new AllowedArea { AreaId = "farm_south", GridX = 10, GridY = 0, SizeX = 10, SizeY = 10 },
                Resources = new AllowedResources { MaxDailyConsumption = 50 },
                OutputRules = new OutputRules { AutoSell = false }
            };

            boardState.AddJob(watererJob);
        }

        [Test]
        public void TestJobDefinitionCreation()
        {
            var job = boardState.GetJobDefinition("job_planter_01");
            Assert.IsNotNull(job);
            Assert.AreEqual(CompanionFarmJobType.Planter, job.JobType);
            Assert.AreEqual(6, job.StartTimeHour);
            Assert.AreEqual(12, job.EndTimeHour);
        }

        [Test]
        public void TestJobCanExecuteAtTime()
        {
            var job = boardState.GetJobDefinition("job_planter_01");

            Assert.IsTrue(job.CanExecuteAtTime(6));
            Assert.IsTrue(job.CanExecuteAtTime(9));
            Assert.IsFalse(job.CanExecuteAtTime(5));
            Assert.IsFalse(job.CanExecuteAtTime(12));
        }

        [Test]
        public void TestCompanionJobAssignment()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            var assignments = boardState.GetCompanionAssignments("companion_01");
            Assert.AreEqual(1, assignments.Count);
            Assert.AreEqual("job_planter_01", assignments[0].JobId);
        }

        [Test]
        public void TestJobExecutionValidation_Success()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            var result = jobBoardService.ValidateJobExecution("companion_01", "job_planter_01", 9, true);
            Assert.AreEqual(JobExecutionResult.Success, result);
        }

        [Test]
        public void TestJobExecutionValidation_TimeWindowClosed()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            var result = jobBoardService.ValidateJobExecution("companion_01", "job_planter_01", 5, true);
            Assert.AreEqual(JobExecutionResult.TimeWindowClosed, result);
        }

        [Test]
        public void TestJobExecutionValidation_InsufficientStamina()
        {
            jobBoardService.RegisterCompanionStamina("companion_02", 10);
            var assignment = new CompanionFarmJobAssignment("assign_02", "companion_02", "job_planter_01");
            boardState.AssignJobToCompanion("companion_02", assignment);

            var result = jobBoardService.ValidateJobExecution("companion_02", "job_planter_01", 9, true);
            Assert.AreEqual(JobExecutionResult.InsufficientStamina, result);
        }

        [Test]
        public void TestJobExecutionValidation_DailyLimitExceeded()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            assignment.ExecutionCount = 3;

            var result = jobBoardService.ValidateJobExecution("companion_01", "job_planter_01", 9, true);
            Assert.AreEqual(JobExecutionResult.DailyLimitExceeded, result);
        }

        [Test]
        public void TestRecordJobExecution_Success()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            jobBoardService.RecordJobExecution("companion_01", "job_planter_01", true);

            Assert.AreEqual(1, assignment.ExecutionCount);
            Assert.AreEqual(0, assignment.FailureCount);
            Assert.AreEqual(20, assignment.StaminaUsedToday);
            Assert.AreEqual(JobAssignmentStatus.Completed, assignment.Status);
        }

        [Test]
        public void TestRecordJobExecution_Failure()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            jobBoardService.RecordJobExecution("companion_01", "job_planter_01", false);

            Assert.AreEqual(0, assignment.ExecutionCount);
            Assert.AreEqual(1, assignment.FailureCount);
            Assert.AreEqual(JobAssignmentStatus.Failed, assignment.Status);
        }

        [Test]
        public void TestMultipleJobAssignments()
        {
            var assignment1 = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            var assignment2 = new CompanionFarmJobAssignment("assign_02", "companion_01", "job_waterer_01");

            boardState.AssignJobToCompanion("companion_01", assignment1);
            boardState.AssignJobToCompanion("companion_01", assignment2);

            var assignments = boardState.GetCompanionAssignments("companion_01");
            Assert.AreEqual(2, assignments.Count);
        }

        [Test]
        public void TestDeactivateJobAssignment()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            jobBoardService.DeactivateJobAssignment("companion_01", "job_planter_01");

            Assert.IsFalse(assignment.IsActive);
            Assert.AreEqual(JobAssignmentStatus.Cancelled, assignment.Status);
        }

        [Test]
        public void TestResetDailyState()
        {
            var assignment = new CompanionFarmJobAssignment("assign_01", "companion_01", "job_planter_01");
            boardState.AssignJobToCompanion("companion_01", assignment);

            assignment.ExecutionCount = 3;
            assignment.StaminaUsedToday = 60;
            boardState.CurrentDay = 5;

            boardState.ResetDailyState();

            Assert.AreEqual(0, assignment.ExecutionCount);
            Assert.AreEqual(0, assignment.StaminaUsedToday);
            Assert.AreEqual(5, assignment.CurrentDay);
        }

        [Test]
        public void TestAllowedResources_IsItemAllowed()
        {
            var job = boardState.GetJobDefinition("job_planter_01");
            job.Resources.AllowedItemIds.Add("seed_wheat");
            job.Resources.AllowedItemIds.Add("seed_corn");

            Assert.IsTrue(job.IsItemAllowed("seed_wheat"));
            Assert.IsTrue(job.IsItemAllowed("seed_corn"));
            Assert.IsFalse(job.IsItemAllowed("seed_rice"));
        }

        [Test]
        public void TestAllowedResources_ForbiddenItemsOverride()
        {
            var job = boardState.GetJobDefinition("job_waterer_01");
            job.Resources.ForbiddenItemIds.Add("mana_fruit");

            Assert.IsFalse(job.IsItemAllowed("mana_fruit"));
        }
    }
}
