using System.Collections.Generic;

namespace CindarsHope.Farm.Resources
{
    public class ResourceNodeDefinition
    {
        public string NodeId { get; set; }
        public ResourceNodeType NodeType { get; set; }
        public string DisplayName { get; set; }
        public List<string> AllowedZones { get; set; } = new List<string>();
        public int RequiredFarmLevel { get; set; } = 0;
        public string RequiredToolTag { get; set; }
        public string RequiredToolTier { get; set; }
        public string RequiredSeason { get; set; }
        public string RequiredWeather { get; set; }
        public ResourceNodeRefreshPolicy RefreshPolicy { get; set; } = ResourceNodeRefreshPolicy.FixedDays;
        public int RefreshAfterDays { get; set; } = 3;
        public float NextDayRefreshChance { get; set; } = 0f;
        public string DropTableId { get; set; }
        public int StaminaCost { get; set; } = 0;
        public int FatigueCost { get; set; } = 0;
        public bool BlocksPath { get; set; } = false;
        public bool CanBeRemoved { get; set; } = true;
        public bool CanRegrow { get; set; } = true;
        public bool IsLoreProtected { get; set; } = false;
        public bool IsEndgameReserved => NodeType == ResourceNodeType.EndgameNode || NodeType == ResourceNodeType.SpecialLoreNode;
        public bool MaxHitsEnabled { get; set; } = false;
        public int MaxHits { get; set; } = 1;

        public bool IsCollectable =>
            NodeType != ResourceNodeType.SpecialLoreNode &&
            NodeType != ResourceNodeType.EndgameNode &&
            !IsLoreProtected &&
            !IsEndgameReserved;
    }
}
