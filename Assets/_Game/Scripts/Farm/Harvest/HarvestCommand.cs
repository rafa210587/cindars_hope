namespace CindarsHope.Farm.Harvest
{
    public class HarvestCommand
    {
        public string PlotId { get; set; }
        public string ActorId { get; set; }
        public int RequestedDay { get; set; }
        public string TargetInventoryId { get; set; }
        public bool AllowInventoryOverflowHandling { get; set; } = true;
    }
}
