using System;
using System.Collections.Generic;

namespace CindarsHope.Companions
{
    public enum JobExecutionResult
    {
        Success = 0,
        InsufficientStamina = 1,
        AreaRestrictionViolation = 2,
        TimeWindowClosed = 3,
        DailyLimitExceeded = 4,
        CompanionUnavailable = 5,
        JobNotFound = 6,
        ResourceRestrictionViolation = 7
    }

    public class CompanionJobBoardService
    {
        private JobBoardState boardState;
        private Dictionary<string, int> companionStaminaCache = new Dictionary<string, int>();

        public CompanionJobBoardService(JobBoardState boardState)
        {
            this.boardState = boardState ?? throw new ArgumentNullException(nameof(boardState));
        }

        public void RegisterCompanionStamina(string companionId, int maxStamina)
        {
            companionStaminaCache[companionId] = maxStamina;
        }

        public JobExecutionResult ValidateJobExecution(string companionId, string jobId, int currentHour, bool companionAvailable)
        {
            if (!companionAvailable)
                return JobExecutionResult.CompanionUnavailable;

            var job = boardState.GetJobDefinition(jobId);
            if (job == null)
                return JobExecutionResult.JobNotFound;

            if (!job.CanExecuteAtTime(currentHour))
                return JobExecutionResult.TimeWindowClosed;

            var assignments = boardState.GetCompanionAssignments(companionId);
            var assignment = assignments.Find(a => a.JobId == jobId);

            if (assignment == null)
                return JobExecutionResult.JobNotFound;

            if (!assignment.CanExecuteMoreToday(job.DailyLimit))
                return JobExecutionResult.DailyLimitExceeded;

            if (!companionStaminaCache.TryGetValue(companionId, out var stamina))
                return JobExecutionResult.CompanionUnavailable;

            if (!assignment.HasStaminaForJob(job.StaminaBudgetPerDay, stamina))
                return JobExecutionResult.InsufficientStamina;

            return JobExecutionResult.Success;
        }

        public void RecordJobExecution(string companionId, string jobId, bool success)
        {
            var assignments = boardState.GetCompanionAssignments(companionId);
            var assignment = assignments.Find(a => a.JobId == jobId);

            if (assignment == null) return;

            var job = boardState.GetJobDefinition(jobId);
            if (job == null) return;

            if (success)
            {
                assignment.ExecutionCount++;
                assignment.Status = JobAssignmentStatus.Completed;
                assignment.StaminaUsedToday += job.StaminaBudgetPerDay;
            }
            else
            {
                assignment.FailureCount++;
                assignment.Status = JobAssignmentStatus.Failed;
            }
        }

        public List<CompanionFarmJobAssignment> GetActiveJobsForCompanion(string companionId)
        {
            var assignments = boardState.GetCompanionAssignments(companionId);
            return assignments.FindAll(a => a.IsActive && a.Status == JobAssignmentStatus.Assigned);
        }

        public void DeactivateJobAssignment(string companionId, string jobId)
        {
            var assignments = boardState.GetCompanionAssignments(companionId);
            var assignment = assignments.Find(a => a.JobId == jobId);
            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.Status = JobAssignmentStatus.Cancelled;
            }
        }
    }
}
