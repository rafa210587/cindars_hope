using System.Collections.Generic;

namespace CindarsHope.Crafting
{
    public class CraftingRequest
    {
        public string RecipeId { get; set; }
        public string ActorId { get; set; }
        public string StationId { get; set; }
        public string InputSourcePolicy { get; set; } = "player_inventory";
        public string OutputDestinationPolicy { get; set; } = "player_inventory";
        public List<string> OptionalIngredientChoices { get; set; } = new List<string>();
        public int RequestedQuantity { get; set; } = 1;
        public int RequestedDayTime { get; set; } = 0;
        public PlayerKnownRecipes KnownRecipes { get; set; }
        public int PlayerReputation { get; set; } = 0;
        public int PlayerFarmLevel { get; set; } = 0;
        public int PlayerCaveProgress { get; set; } = 0;
        public string ActiveQuestFlag { get; set; }
        public int PlayerGold { get; set; } = 0;
    }

    public class CraftingResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<string> InputsConsumed { get; set; } = new List<string>();
        public List<string> OutputsCreated { get; set; } = new List<string>();
        public int Quality { get; set; } = 0;
        public string ProcessingJobId { get; set; }
        public List<string> Events { get; set; } = new List<string>();

        public static CraftingResult Fail(string reason) =>
            new CraftingResult { Success = false, FailureReason = reason };
    }

    public class ProcessingJob
    {
        public string ProcessingJobId { get; set; }
        public string RecipeId { get; set; }
        public string StationId { get; set; }
        public List<string> InputStackIds { get; set; } = new List<string>();
        public string OutputItemId { get; set; }
        public int OutputQuantity { get; set; } = 1;
        public int StartDayTime { get; set; }
        public int FinishDayTime { get; set; }
        public ProcessingJobState State { get; set; } = ProcessingJobState.Queued;
        public bool IsReadyToCollect => State == ProcessingJobState.ReadyToCollect;

        public void TryAdvance(int currentDayTime)
        {
            if (State != ProcessingJobState.Processing) return;
            if (currentDayTime >= FinishDayTime)
                State = ProcessingJobState.ReadyToCollect;
        }

        public CraftingResult Collect()
        {
            if (State != ProcessingJobState.ReadyToCollect)
                return CraftingResult.Fail("Job not ready to collect");
            State = ProcessingJobState.Collected;
            return new CraftingResult
            {
                Success = true,
                ProcessingJobId = ProcessingJobId,
                OutputsCreated = new List<string> { OutputItemId }
            };
        }
    }
}
