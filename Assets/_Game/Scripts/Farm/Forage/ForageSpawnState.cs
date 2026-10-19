namespace CindarsHope.Farm.Forage
{
    public enum ForageNodeState { Available = 0, Collected = 1, Hidden = 2, Reserved = 3 }

    public class ForageSpawnState
    {
        public string ForageInstanceId { get; set; }
        public string ForageId { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public string ZoneId { get; set; }
        public ForageNodeState CurrentState { get; set; } = ForageNodeState.Available;
        public int SpawnedDay { get; set; } = -1;
        public int CollectedDay { get; set; } = -1;
        public int NextEligibleSpawnDay { get; set; } = -1;
        public int RandomSeed { get; set; } = 0;
        public bool IsAvailable => CurrentState == ForageNodeState.Available;
    }
}
