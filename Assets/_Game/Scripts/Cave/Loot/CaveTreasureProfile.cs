using System.Collections.Generic;

namespace CindarsHope.Cave.Loot
{
    public class CaveTreasureProfile
    {
        public string TreasureProfileId { get; set; }
        public CaveLootSourceType TreasureType { get; set; } = CaveLootSourceType.TreasureChest;
        public int AllowedFloorMin { get; set; } = 1;
        public int AllowedFloorMax { get; set; } = 100;
        public List<string> AllowedBiomes { get; set; } = new List<string>();
        public string RequiredKeyOrFlag { get; set; }
        public bool GuardedByPack { get; set; } = false;
        public string LootTableId { get; set; }
        public bool FirstTimeOnly { get; set; } = false;
        public bool Repeatable { get; set; } = true;
        public bool SnapshotSeeded { get; set; } = true;
        public OpenedStatePolicy OpenedStatePolicy { get; set; } = OpenedStatePolicy.OpenedForever;
        public List<string> GrantedStoryFlags { get; set; } = new List<string>();
        public List<string> RequiredStoryFlags { get; set; } = new List<string>();
    }
}
