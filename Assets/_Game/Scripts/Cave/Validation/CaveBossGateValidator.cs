using System.Collections.Generic;
using CindarsHope.Cave.Data;

namespace CindarsHope.Cave.Validation
{
    public sealed class CaveBossGateValidator
    {
        public static ValidationResult Validate(CaveBossGateRegistrySO bossGateRegistry)
        {
            var result = new ValidationResult();

            if (bossGateRegistry == null)
            {
                result.AddError("CaveBossGateRegistry not found");
                return result;
            }

            if (bossGateRegistry.Gates == null || bossGateRegistry.Gates.Count == 0)
            {
                result.AddWarning("CaveBossGateRegistry is empty");
                return result;
            }

            var usedIds = new HashSet<string>();
            var usedLevels = new HashSet<int>();

            foreach (var gate in bossGateRegistry.Gates)
            {
                if (string.IsNullOrWhiteSpace(gate.Id))
                {
                    result.AddError("Boss gate has null or empty Id");
                }
                else if (!usedIds.Add(gate.Id))
                {
                    result.AddError($"Duplicate boss gate ID: {gate.Id}");
                }

                if (gate.CaveLevel <= 0)
                {
                    result.AddError($"Boss gate '{gate.Id}' has invalid CaveLevel: {gate.CaveLevel}");
                }
                else if (!usedLevels.Add(gate.CaveLevel))
                {
                    result.AddError($"Duplicate boss gate level: {gate.CaveLevel}");
                }

                if (string.IsNullOrWhiteSpace(gate.BiomeId))
                {
                    result.AddWarning($"Boss gate '{gate.Id}' has empty BiomeId");
                }

                if (gate.CheckpointUnlockedOnDefeat <= 0)
                {
                    result.AddError(
                        $"Boss gate '{gate.Id}' has invalid CheckpointUnlockedOnDefeat: {gate.CheckpointUnlockedOnDefeat}");
                }

                if (gate.CheckpointUnlockedOnDefeat != gate.CaveLevel)
                {
                    result.AddWarning(
                        $"Boss gate '{gate.Id}' unlocks checkpoint {gate.CheckpointUnlockedOnDefeat}, expected {gate.CaveLevel}");
                }

                if (string.IsNullOrWhiteSpace(gate.BossEnemyId))
                {
                    result.AddError($"Boss gate '{gate.Id}' has null or empty BossEnemyId");
                }
                else
                {
                    result.AddPass($"Boss gate '{gate.Id}' references BossEnemyId: {gate.BossEnemyId}");
                }
            }

            if (result.ErrorCount == 0 && result.WarningCount == 0)
            {
                result.AddPass($"CaveBossGateRegistry validated: {bossGateRegistry.Gates.Count} gates");
            }

            return result;
        }
    }
}