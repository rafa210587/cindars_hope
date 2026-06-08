namespace CindarsHope.Farm.Processing
{
    public enum QualityTransferPolicy
    {
        PreserveMinimum = 0,
        AverageInputs = 1,
        HighestInputWithChance = 2,
        FixedOutput = 3,
        RecipeDefined = 4,
        NoQuality = 5
    }

    public class ProcessableItem
    {
        public string InputItemId { get; set; }
        public int InputQuantity { get; set; } = 1;
        public string OutputItemId { get; set; }
        public int OutputQuantity { get; set; } = 1;
        public string StationId { get; set; }
        public int ProcessingTimeDays { get; set; } = 1;
        public string RequiredRecipeId { get; set; }
        public string RequiredBuildingId { get; set; }
        public QualityTransferPolicy QualityPolicy { get; set; } = QualityTransferPolicy.PreserveMinimum;
        public bool AllowsInfiniteReprocessing { get; set; } = false;
    }
}
