using System.Collections.Generic;

namespace CindarsHope.Farm.Tools
{
    public class FarmToolDefinition
    {
        public string ToolId { get; set; }
        public FarmToolType ToolType { get; set; }
        public FarmToolTier Tier { get; set; } = FarmToolTier.Tier0_Improvised;
        public List<string> MaterialTags { get; set; } = new List<string>();
        public int DurabilityMax { get; set; } = 100;
        public float BaseStaminaCostModifier { get; set; } = 1.0f;
        public float BaseFatigueCostModifier { get; set; } = 1.0f;
        public int AreaOfEffect { get; set; } = 1;
        public float ActionSpeedModifier { get; set; } = 1.0f;
        public int RequiredPlayerLevel { get; set; } = 0;
        public int RequiredFarmLevel { get; set; } = 0;
        public string RequiredQuestFlag { get; set; }
        public bool CanRepair { get; set; } = true;
        public bool CanUpgrade { get; set; } = true;
        public bool CanBeUsedAsEmergencyAttack { get; set; } = false;
        public bool IsEndgameReserved => Tier >= FarmToolTier.Tier6_Meteoric;
    }
}
