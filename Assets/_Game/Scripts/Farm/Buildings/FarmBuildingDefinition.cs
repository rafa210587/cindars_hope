namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Farm-specific building definition that extends base building with construction costs and requirements.
    /// </summary>
    public class FarmBuildingDefinition : BuildingDefinition
    {
        // Construction costs
        public int CostGold { get; set; } = 0;
        public int CostWood { get; set; } = 0;
        public int CostStone { get; set; } = 0;
        public int CostIron { get; set; } = 0;

        // Build time in days
        public int BuildTimeDays { get; set; } = 1;

        // Can the building be moved after construction?
        public bool CanMoveAfterBuild { get; set; } = true;

        // Can the building be demolished and refunded?
        public bool CanDemolish { get; set; } = true;

        // Refund percentage when demolished (0-100)
        public int DemolishRefundPercent { get; set; } = 50;

        // Is this a workshop? (can process materials)
        public bool IsWorkshop { get; set; } = false;

        // Is this a storage building? (can hold items)
        public bool IsStorage { get; set; } = false;

        // Minimum farm level to build
        public new int RequiredFarmLevel { get; set; } = 1;

        public FarmBuildingDefinition(string buildingId, string displayName, int widthTiles, int heightTiles)
            : base(buildingId, displayName, widthTiles, heightTiles)
        {
        }

        /// <summary>
        /// Check if building has sufficient cost data (at least one cost > 0 or cost is free).
        /// </summary>
        public bool HasValidCost()
        {
            return CostGold >= 0 && CostWood >= 0 && CostStone >= 0 && CostIron >= 0;
        }

        /// <summary>
        /// Check if total cost is 0 (free building).
        /// </summary>
        public bool IsFree()
        {
            return CostGold == 0 && CostWood == 0 && CostStone == 0 && CostIron == 0;
        }
    }
}
