namespace CindarsHope.Core.Events
{
    public readonly struct CraftingStationOpenedEvent
    {
        public CraftingStationOpenedEvent(string stationInstanceId) => StationInstanceId = stationInstanceId;
        public string StationInstanceId { get; }
    }

    public readonly struct CraftingStationClosedEvent
    {
        public CraftingStationClosedEvent(string stationInstanceId) => StationInstanceId = stationInstanceId;
        public string StationInstanceId { get; }
    }

    public readonly struct CraftingJobStartedEvent
    {
        public CraftingJobStartedEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingJobCompletedEvent
    {
        public CraftingJobCompletedEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingJobCancelledEvent
    {
        public CraftingJobCancelledEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingOutputCollectedEvent
    {
        public CraftingOutputCollectedEvent(string stationInstanceId, string recipeId, string itemId, int amount)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            ItemId = itemId;
            Amount = amount;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string ItemId { get; }
        public int Amount { get; }
    }

    public readonly struct CraftingFailedEvent
    {
        public CraftingFailedEvent(string stationInstanceId, string recipeId, string message)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            Message = message;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string Message { get; }
    }
}
