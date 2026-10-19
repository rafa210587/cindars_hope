using System.Collections.Generic;

namespace CindarsHope.Farm.Tools
{
    public class ToolInstanceState
    {
        public string ToolInstanceId { get; set; }
        public string ToolId { get; set; }
        public FarmToolTier CurrentTier { get; set; }
        public int CurrentDurability { get; set; }
        public int MaxDurability { get; set; } = 100;
        public bool IsUpgradeApplied { get; set; } = false; // idempotency guard
        public string LastAppliedUpgradeId { get; set; }
    }

    public class PlayerMaterials
    {
        public Dictionary<string, int> Items { get; set; } = new Dictionary<string, int>();
        public int Gold { get; set; }

        public bool HasEnough(ToolUpgradeDefinition upgrade)
        {
            if (Gold < upgrade.RequiredGold) return false;
            foreach (var req in upgrade.RequiredMaterials)
            {
                if (!Items.TryGetValue(req.ItemId, out var qty) || qty < req.Quantity)
                    return false;
            }
            return true;
        }

        public void Deduct(ToolUpgradeDefinition upgrade)
        {
            Gold -= upgrade.RequiredGold;
            foreach (var req in upgrade.RequiredMaterials)
            {
                Items[req.ItemId] = (Items.TryGetValue(req.ItemId, out var qty) ? qty : 0) - req.Quantity;
            }
        }
    }

    public class ToolUpgradeResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string NewToolId { get; set; }
        public FarmToolTier NewTier { get; set; }

        public static ToolUpgradeResult Fail(string reason) =>
            new ToolUpgradeResult { Success = false, FailureReason = reason };
    }

    public class FarmToolUpgradeService
    {
        private readonly Dictionary<string, FarmToolDefinition> _toolDefs;
        private readonly Dictionary<string, ToolUpgradeDefinition> _upgradeDefs;

        public FarmToolUpgradeService(
            Dictionary<string, FarmToolDefinition> toolDefs,
            Dictionary<string, ToolUpgradeDefinition> upgradeDefs)
        {
            _toolDefs = toolDefs ?? new Dictionary<string, FarmToolDefinition>();
            _upgradeDefs = upgradeDefs ?? new Dictionary<string, ToolUpgradeDefinition>();
        }

        public ToolUpgradeResult Upgrade(ToolInstanceState instance, string upgradeId,
            PlayerMaterials materials, int farmLevel)
        {
            if (instance == null)
                return ToolUpgradeResult.Fail("InstanceNotFound");

            if (!_upgradeDefs.TryGetValue(upgradeId, out var upgrade))
                return ToolUpgradeResult.Fail("UpgradeDefinitionNotFound");

            // Idempotency guard
            if (instance.LastAppliedUpgradeId == upgradeId)
                return ToolUpgradeResult.Fail("UpgradeAlreadyApplied");

            if (instance.ToolId != upgrade.FromToolId)
                return ToolUpgradeResult.Fail("ToolIdMismatch");

            if (farmLevel < upgrade.RequiredFarmLevel)
                return ToolUpgradeResult.Fail("FarmLevelInsufficient");

            if (upgrade.RequiredGold < 0)
                return ToolUpgradeResult.Fail("InvalidUpgradeCost");

            if (materials == null || !materials.HasEnough(upgrade))
                return ToolUpgradeResult.Fail("InsufficientMaterials");

            if (!_toolDefs.TryGetValue(upgrade.ToToolId, out var newDef))
                return ToolUpgradeResult.Fail("NewToolDefinitionNotFound");

            materials.Deduct(upgrade);
            instance.ToolId = upgrade.ToToolId;
            instance.CurrentTier = newDef.Tier;
            instance.MaxDurability = newDef.DurabilityMax;
            instance.CurrentDurability = newDef.DurabilityMax;
            instance.LastAppliedUpgradeId = upgradeId;
            instance.IsUpgradeApplied = true;

            return new ToolUpgradeResult
            {
                Success = true,
                NewToolId = upgrade.ToToolId,
                NewTier = newDef.Tier
            };
        }

        public ToolUpgradeResult Repair(ToolInstanceState instance, int goldCost, PlayerMaterials materials)
        {
            if (instance == null)
                return ToolUpgradeResult.Fail("InstanceNotFound");

            if (!_toolDefs.TryGetValue(instance.ToolId, out var def))
                return ToolUpgradeResult.Fail("ToolDefinitionNotFound");

            if (!def.CanRepair)
                return ToolUpgradeResult.Fail("ToolNotRepairable");

            if (instance.CurrentDurability >= instance.MaxDurability)
                return ToolUpgradeResult.Fail("ToolAlreadyFullDurability");

            if (materials == null || materials.Gold < goldCost)
                return ToolUpgradeResult.Fail("InsufficientGold");

            materials.Gold -= goldCost;
            instance.CurrentDurability = instance.MaxDurability;
            // Note: Repair does NOT apply LastAppliedUpgradeId — it's a different operation

            return new ToolUpgradeResult { Success = true, NewToolId = instance.ToolId, NewTier = instance.CurrentTier };
        }
    }
}
