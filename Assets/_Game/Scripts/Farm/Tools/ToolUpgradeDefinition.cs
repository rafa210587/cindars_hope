using System.Collections.Generic;

namespace CindarsHope.Farm.Tools
{
    public enum ToolUpgradeResultPolicy { ReplaceTool = 0, ModifyInstance = 1, AddCapability = 2 }

    public class ToolMaterialRequirement
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class ToolUpgradeDefinition
    {
        public string UpgradeId { get; set; }
        public string FromToolId { get; set; }
        public string ToToolId { get; set; }
        public int RequiredGold { get; set; } = 0;
        public List<ToolMaterialRequirement> RequiredMaterials { get; set; } = new List<ToolMaterialRequirement>();
        public string RequiredServiceId { get; set; }
        public string RequiredRecipeId { get; set; }
        public int RequiredFarmLevel { get; set; } = 0;
        public int RequiredCaveDepth { get; set; } = 0;
        public int BuildTimeDays { get; set; } = 0;
        public ToolUpgradeResultPolicy ResultPolicy { get; set; } = ToolUpgradeResultPolicy.ReplaceTool;
        public bool IsCaveRequired => RequiredCaveDepth > 0;
    }
}
