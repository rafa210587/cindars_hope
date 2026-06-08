using System.Collections.Generic;

namespace CindarsHope.Cave.Loot
{
    /// <summary>
    /// Persists loot state for a specific node/chest within a (CaveRunSeed, CaveLevel) pair.
    /// Revisiting the same CaveLevel with the same CaveRunSeed must return the same loot state.
    /// </summary>
    public class CaveLootSnapshotEntry
    {
        public string SnapshotId { get; set; }
        public int CaveRunSeed { get; set; }
        public int CaveLevel { get; set; }
        // Unique instance identifier within the level (position-based or deterministic ID)
        public string SourceInstanceId { get; set; }
        public CaveLootSourceType SourceType { get; set; }
        // Grid position for deterministic placement
        public int GridX { get; set; }
        public int GridY { get; set; }
        // Seed used for loot rolls — derived from CaveRunSeed + CaveLevel + SourceInstanceId
        public int LootRollSeed { get; set; }
        // State — once opened, cannot reopen; once depleted, stays depleted per policy
        public bool IsOpened { get; set; } = false;
        public bool IsDepleted { get; set; } = false;
        public int OpenedDay { get; set; } = -1;
        public int DepletedDay { get; set; } = -1;
        // Tracks which rewards were already consumed (idempotency for first-time rewards)
        public List<string> RewardConsumedFlags { get; set; } = new List<string>();
    }
}
