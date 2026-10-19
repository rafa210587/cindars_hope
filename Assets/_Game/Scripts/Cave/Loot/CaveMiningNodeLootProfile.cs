using System.Collections.Generic;

namespace CindarsHope.Cave.Loot
{
    public class CaveMiningNodeLootProfile
    {
        public string ProfileId { get; set; }
        public string ResourceNodeId { get; set; }
        public int AllowedFloorMin { get; set; } = 1;
        public int AllowedFloorMax { get; set; } = 100;
        public List<string> AllowedBiomes { get; set; } = new List<string>();
        public List<string> RequiredCaveProgress { get; set; } = new List<string>();
        public string BaseLootTableId { get; set; }
        public string RareLootTableId { get; set; }
        public string QualityRollProfileId { get; set; }
        public string QuantityRollProfileId { get; set; }
        public DepletedStatePolicy DepletedStatePolicy { get; set; } = DepletedStatePolicy.DepletedUntilNewRun;
        public bool SnapshotSeeded { get; set; } = true;

        // Level 101: no common mining allowed (lore/endgame only)
        public bool IsLevel101RestrictedNode { get; set; } = false;

        public bool AllowsFloor(int floor)
        {
            if (IsLevel101RestrictedNode && floor == 101) return false;
            return floor >= AllowedFloorMin && floor <= AllowedFloorMax;
        }
    }
}
