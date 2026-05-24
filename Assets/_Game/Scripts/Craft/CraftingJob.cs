using System;
using CindarsHope.Craft.Data;
using UnityEngine;

namespace CindarsHope.Craft
{
    public enum CraftingJobStatus
    {
        InProgress,
        Completed,
        Cancelled
    }

    public class CraftingJob
    {
        public string JobId { get; private set; }
        public string StationInstanceId { get; private set; }
        public RecipeDataSO Recipe { get; private set; }
        public CraftingJobStatus Status { get; set; }
        public float RemainingSeconds { get; set; }

        public CraftingJob(string stationInstanceId, RecipeDataSO recipe)
        {
            JobId = Guid.NewGuid().ToString();
            StationInstanceId = stationInstanceId;
            Recipe = recipe;
            Status = CraftingJobStatus.InProgress;
            RemainingSeconds = recipe.CraftTimeSeconds;
        }

        public bool IsComplete => RemainingSeconds <= 0f && Status == CraftingJobStatus.InProgress;

        public void Update(float deltaTime)
        {
            if (Status != CraftingJobStatus.InProgress)
                return;

            RemainingSeconds -= deltaTime;
            if (RemainingSeconds <= 0f)
            {
                RemainingSeconds = 0f;
            }
        }

        public void Complete()
        {
            Status = CraftingJobStatus.Completed;
            RemainingSeconds = 0f;
        }

        public void Cancel()
        {
            Status = CraftingJobStatus.Cancelled;
        }
    }

    [System.Serializable]
    public class CraftingJobSaveData
    {
        public string JobId;
        public string StationInstanceId;
        public string RecipeId;
        public int Status;
        public float RemainingSeconds;
    }
}
