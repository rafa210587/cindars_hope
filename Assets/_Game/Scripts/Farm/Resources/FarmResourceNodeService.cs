using System.Collections.Generic;

namespace CindarsHope.Farm.Resources
{
    public class ResourceNodeHarvestResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string[] DroppedItemIds { get; set; } = System.Array.Empty<string>();
        public int[] DroppedQuantities { get; set; } = System.Array.Empty<int>();
        public int StaminaCostApplied { get; set; }
        public int FatigueCostApplied { get; set; }
        public ResourceNodeCurrentState NodeStateAfter { get; set; }

        public static ResourceNodeHarvestResult Fail(string reason) =>
            new ResourceNodeHarvestResult { Success = false, FailureReason = reason };
    }

    public class FarmResourceNodeService
    {
        private readonly Dictionary<string, ResourceNodeDefinition> _definitions;

        public FarmResourceNodeService(Dictionary<string, ResourceNodeDefinition> definitions)
        {
            _definitions = definitions ?? new Dictionary<string, ResourceNodeDefinition>();
        }

        public ResourceNodeHarvestResult Harvest(ResourceNodeInstanceState node, int currentDay, string toolTier = "Basic", string currentSeason = null)
        {
            if (node == null)
                return ResourceNodeHarvestResult.Fail("NodeNotFound");

            if (!_definitions.TryGetValue(node.NodeId, out var def))
                return ResourceNodeHarvestResult.Fail("DefinitionNotFound");

            if (!def.IsCollectable)
                return ResourceNodeHarvestResult.Fail("NodeNotCollectable");

            if (!node.IsAvailable)
                return ResourceNodeHarvestResult.Fail("NodeNotAvailable");

            if (def.RequiredFarmLevel > 0)
                return ResourceNodeHarvestResult.Fail("FarmLevelInsufficient");

            if (!string.IsNullOrEmpty(def.RequiredSeason) && def.RequiredSeason != currentSeason)
                return ResourceNodeHarvestResult.Fail("WrongSeason");

            // Apply hit logic for multi-hit nodes
            if (def.MaxHitsEnabled)
            {
                node.RemainingHits--;
                if (node.RemainingHits > 0)
                {
                    return new ResourceNodeHarvestResult
                    {
                        Success = true,
                        FailureReason = null,
                        DroppedItemIds = System.Array.Empty<string>(),
                        DroppedQuantities = System.Array.Empty<int>(),
                        StaminaCostApplied = def.StaminaCost,
                        FatigueCostApplied = def.FatigueCost,
                        NodeStateAfter = ResourceNodeCurrentState.Available
                    };
                }
            }

            // Deplete node — idempotent: same-day second call returns NotAvailable
            node.CurrentState = ResourceNodeCurrentState.Harvested;
            node.LastHarvestedDay = currentDay;

            if (def.CanRegrow && def.RefreshPolicy == ResourceNodeRefreshPolicy.FixedDays)
                node.NextEligibleRefreshDay = currentDay + def.RefreshAfterDays;

            var drops = ResolveDrops(def.DropTableId);

            return new ResourceNodeHarvestResult
            {
                Success = true,
                DroppedItemIds = drops.Item1,
                DroppedQuantities = drops.Item2,
                StaminaCostApplied = def.StaminaCost,
                FatigueCostApplied = def.FatigueCost,
                NodeStateAfter = node.CurrentState
            };
        }

        private (string[], int[]) ResolveDrops(string dropTableId)
        {
            if (string.IsNullOrEmpty(dropTableId))
                return (new[] { "item_unknown" }, new[] { 1 });
            return (new[] { dropTableId }, new[] { 1 });
        }
    }
}
