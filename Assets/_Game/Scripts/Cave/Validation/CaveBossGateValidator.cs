using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Validation
{
    public sealed class CaveBossGateValidator
    {
        public static ValidationResult Validate(
            CaveBossGateRegistrySO bossGateRegistry,
            DataRegistrySO<EnemyDataSO> enemyDatabase)
        {
            var result = new ValidationResult();

            if (bossGateRegistry == null)
            {
                result.AddError("CaveBossGateRegistry not found");
                return result;
            }

            if (bossGateRegistry.Gates.Count == 0)
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
                else if (usedIds.Contains(gate.Id))
                {
                    result.AddError($"Duplicate boss gate ID: {gate.Id}");
                }
                else
                {
                    usedIds.Add(gate.Id);
                }

                if (gate.CaveLevel <= 0)
                {
                    result.AddError($"Boss gate '{gate.Id}' has invalid CaveLevel: {gate.CaveLevel}");
                }
                else if (usedLevels.Contains(gate.CaveLevel))
                {
                    result.AddError($"Duplicate boss gate level: {gate.CaveLevel}");
                }
                else
                {
                    usedLevels.Add(gate.CaveLevel);
                }

                if (string.IsNullOrWhiteSpace(gate.BiomeId))
                {
                    result.AddWarning($"Boss gate '{gate.Id}' has empty BiomeId");
                }

                if (gate.CheckpointUnlockedOnDefeat <= 0)
                {
                    result.AddError($"Boss gate '{gate.Id}' has invalid CheckpointUnlockedOnDefeat: {gate.CheckpointUnlockedOnDefeat}");
                }

                if (string.IsNullOrWhiteSpace(gate.BossEnemyId))
                {
                    result.AddError($"Boss gate '{gate.Id}' has null or empty BossEnemyId");
                }
                else if (enemyDatabase != null)
                {
                    var bossData = enemyDatabase.GetById(gate.BossEnemyId);
                    if (bossData == null)
                    {
                        result.AddError($"Boss gate '{gate.Id}' references non-existent BossEnemyId: {gate.BossEnemyId}");
                    }
                }
            }

            if (result.ErrorCount == 0 && result.WarningCount == 0)
            {
                result.AddInfo($"CaveBossGateRegistry validated: {bossGateRegistry.Gates.Count} gates");
            }

            return result;
        }
    }

    public sealed class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();
        private readonly List<string> _infos = new List<string>();

        public int ErrorCount => _errors.Count;
        public int WarningCount => _warnings.Count;
        public int InfoCount => _infos.Count;

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
        public IReadOnlyList<string> Infos => _infos.AsReadOnly();

        public void AddError(string message)
        {
            _errors.Add(message);
            Debug.LogError(message);
        }

        public void AddWarning(string message)
        {
            _warnings.Add(message);
            Debug.LogWarning(message);
        }

        public void AddInfo(string message)
        {
            _infos.Add(message);
            Debug.Log(message);
        }

        public bool IsValid => ErrorCount == 0;

        public override string ToString()
        {
            var lines = new List<string>();
            if (_infos.Count > 0)
            {
                lines.Add($"Infos ({_infos.Count}):");
                foreach (var info in _infos)
                {
                    lines.Add($"  [INFO] {info}");
                }
            }
            if (_warnings.Count > 0)
            {
                lines.Add($"Warnings ({_warnings.Count}):");
                foreach (var warning in _warnings)
                {
                    lines.Add($"  [WARN] {warning}");
                }
            }
            if (_errors.Count > 0)
            {
                lines.Add($"Errors ({_errors.Count}):");
                foreach (var error in _errors)
                {
                    lines.Add($"  [ERR] {error}");
                }
            }
            return string.Join("\n", lines);
        }
    }
}
