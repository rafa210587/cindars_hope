namespace CindarsHope.Farm.Resources
{
    public class ResourceNodeInstanceState
    {
        public string NodeInstanceId { get; set; }
        public string NodeId { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string ZoneId { get; set; }
        public ResourceNodeCurrentState CurrentState { get; set; } = ResourceNodeCurrentState.Available;
        public int LastHarvestedDay { get; set; } = -1;
        public int NextEligibleRefreshDay { get; set; } = -1;
        public int RemainingHits { get; set; } = 1;
        public int RandomSeed { get; set; } = 0;
        public bool IsAvailable => CurrentState == ResourceNodeCurrentState.Available;
        public bool IsDepleted => CurrentState == ResourceNodeCurrentState.Depleted || CurrentState == ResourceNodeCurrentState.Harvested;
    }
}
