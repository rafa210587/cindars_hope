using System.Collections.Generic;

namespace CindarsHope.Companions
{
    public class JobBoardState
    {
        public List<CompanionFarmJobDefinition> AvailableJobs { get; set; } = new List<CompanionFarmJobDefinition>();
        public Dictionary<string, List<CompanionFarmJobAssignment>> CompanionAssignments { get; set; } = new Dictionary<string, List<CompanionFarmJobAssignment>>();
        public int CurrentDay { get; set; } = 0;

        public void AddJob(CompanionFarmJobDefinition job)
        {
            if (job != null)
            {
                AvailableJobs.Add(job);
            }
        }

        public void AssignJobToCompanion(string companionId, CompanionFarmJobAssignment assignment)
        {
            if (!CompanionAssignments.ContainsKey(companionId))
            {
                CompanionAssignments[companionId] = new List<CompanionFarmJobAssignment>();
            }

            CompanionAssignments[companionId].Add(assignment);
        }

        public List<CompanionFarmJobAssignment> GetCompanionAssignments(string companionId)
        {
            if (CompanionAssignments.TryGetValue(companionId, out var assignments))
            {
                return assignments;
            }
            return new List<CompanionFarmJobAssignment>();
        }

        public CompanionFarmJobDefinition GetJobDefinition(string jobId)
        {
            return AvailableJobs.Find(j => j.JobId == jobId);
        }

        public void ResetDailyState()
        {
            foreach (var kvp in CompanionAssignments)
            {
                foreach (var assignment in kvp.Value)
                {
                    assignment.ResetDaily();
                    assignment.CurrentDay = CurrentDay;
                }
            }
        }
    }
}
