namespace CindarsHope.Companions
{
    public enum JobAssignmentStatus
    {
        Assigned = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        Paused = 4,
        Cancelled = 5
    }

    public class CompanionFarmJobAssignment
    {
        public string AssignmentId { get; set; }
        public string CompanionId { get; set; }
        public string JobId { get; set; }
        public bool IsActive { get; set; }
        public int ExecutionCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public JobAssignmentStatus Status { get; set; } = JobAssignmentStatus.Assigned;
        public int StaminaUsedToday { get; set; } = 0;
        public int CurrentDay { get; set; } = 0;

        public CompanionFarmJobAssignment()
        {
        }

        public CompanionFarmJobAssignment(string assignmentId, string companionId, string jobId)
        {
            AssignmentId = assignmentId;
            CompanionId = companionId;
            JobId = jobId;
            IsActive = true;
            Status = JobAssignmentStatus.Assigned;
        }

        public void ResetDaily()
        {
            ExecutionCount = 0;
            FailureCount = 0;
            StaminaUsedToday = 0;
        }

        public bool CanExecuteMoreToday(int dailyLimit)
        {
            return ExecutionCount < dailyLimit;
        }

        public bool HasStaminaForJob(int staminaBudget, int companionStamina)
        {
            return (StaminaUsedToday + staminaBudget) <= companionStamina;
        }
    }
}
