using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using UnityEngine;

namespace CindarsHope.Cave.Validation
{
    public static class CaveReplayValidator
    {
        public static ValidationResult ValidateReplaySystem(CaveRunManager runManager, CaveLevelRuntimeController levelController)
        {
            var result = new ValidationResult();

            if (runManager == null)
            {
                result.AddError("CaveRunManager is null");
                return result;
            }

            if (levelController == null)
            {
                result.AddError("CaveLevelRuntimeController is null");
                return result;
            }

            result.Merge(ValidateSeeds(runManager));
            result.Merge(ValidateCheckpoints(runManager));
            result.Merge(ValidateSnapshots(runManager));
            result.Merge(ValidateLevelGeneration(levelController));

            return result;
        }

        private static ValidationResult ValidateSeeds(CaveRunManager runManager)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(runManager.CaveWorldSeed))
            {
                result.AddError("CaveWorldSeed is empty");
            }
            else
            {
                result.AddPass($"CaveWorldSeed valid: {runManager.CaveWorldSeed.Substring(0, 8)}...");
            }

            if (string.IsNullOrWhiteSpace(runManager.CaveRunSeed))
            {
                result.AddError("CaveRunSeed is empty");
            }
            else
            {
                result.AddPass($"CaveRunSeed valid: {runManager.CaveRunSeed.Substring(0, 8)}...");
            }

            return result;
        }

        private static ValidationResult ValidateCheckpoints(CaveRunManager runManager)
        {
            var result = new ValidationResult();

            var checkpoints = runManager.State.UnlockedCheckpoints;
            if (checkpoints.Count == 0)
            {
                result.AddError("No checkpoints unlocked (should have at least checkpoint 1)");
            }
            else if (!checkpoints.Contains(1))
            {
                result.AddError("Checkpoint 1 not unlocked");
            }
            else
            {
                result.AddPass($"Checkpoints unlocked: {string.Join(",", checkpoints)}");
            }

            return result;
        }

        private static ValidationResult ValidateSnapshots(CaveRunManager runManager)
        {
            var result = new ValidationResult();

            var snapshots = runManager.State.VisitedLevelSnapshots;
            if (snapshots.Count == 0)
            {
                result.AddWarning("No snapshots recorded (normal for first visit)");
            }
            else
            {
                foreach (var kvp in snapshots)
                {
                    var level = kvp.Key;
                    var snapshot = kvp.Value;

                    if (!snapshot.IsValid())
                    {
                        result.AddError($"Snapshot for level {level} is invalid");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(snapshot.LayoutHash))
                        {
                            result.AddWarning($"Snapshot for level {level} has no LayoutHash");
                        }
                        else
                        {
                            result.AddPass($"Snapshot level {level}: valid (hash={snapshot.LayoutHash.Substring(0, 8)})");
                        }

                        if (snapshot.EnemySpawns.Count == 0)
                        {
                            result.AddWarning($"Snapshot level {level} has no enemy spawns");
                        }

                        if (snapshot.ResourceNodes.Count == 0)
                        {
                            result.AddWarning($"Snapshot level {level} has no resource nodes");
                        }
                    }
                }
            }

            return result;
        }

        private static ValidationResult ValidateLevelGeneration(CaveLevelRuntimeController levelController)
        {
            var result = new ValidationResult();

            var generatedLevel = levelController.CurrentGeneratedLevel;
            if (generatedLevel == null)
            {
                result.AddWarning("No level currently generated");
                return result;
            }

            if (generatedLevel.CaveLevel <= 0)
            {
                result.AddError("Current level cave number is invalid");
            }
            else
            {
                result.AddPass($"Current level: {generatedLevel.CaveLevel}");
            }

            if (generatedLevel.Rooms.Count == 0)
            {
                result.AddError("Generated level has no rooms");
            }
            else
            {
                result.AddPass($"Generated rooms: {generatedLevel.Rooms.Count}");
            }

            if (generatedLevel.EnemySpawnPoints.Count == 0)
            {
                result.AddWarning("Generated level has no enemy spawn points");
            }
            else
            {
                result.AddPass($"Enemy spawn points: {generatedLevel.EnemySpawnPoints.Count}");
            }

            if (generatedLevel.ResourceSpawnPoints.Count == 0)
            {
                result.AddWarning("Generated level has no resource spawn points");
            }
            else
            {
                result.AddPass($"Resource spawn points: {generatedLevel.ResourceSpawnPoints.Count}");
            }

            if (string.IsNullOrWhiteSpace(generatedLevel.LayoutHash))
            {
                result.AddWarning("Generated level has no LayoutHash computed");
            }
            else
            {
                result.AddPass($"LayoutHash computed: {generatedLevel.LayoutHash.Substring(0, 8)}");
            }

            return result;
        }
    }

    public sealed class ValidationResult
    {
        private readonly List<(string message, ValidationLevel level)> _entries = new();

        public int PassCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ErrorCount { get; private set; }

        public bool IsValid => ErrorCount == 0;

        public void AddPass(string message)
        {
            _entries.Add((message, ValidationLevel.Pass));
            PassCount++;
        }

        public void AddWarning(string message)
        {
            _entries.Add((message, ValidationLevel.Warning));
            WarningCount++;
        }

        public void AddError(string message)
        {
            _entries.Add((message, ValidationLevel.Error));
            ErrorCount++;
        }

        public void Merge(ValidationResult other)
        {
            if (other == null)
            {
                return;
            }

            foreach (var (message, level) in other._entries)
            {
                _entries.Add((message, level));
            }

            PassCount += other.PassCount;
            WarningCount += other.WarningCount;
            ErrorCount += other.ErrorCount;
        }

        public void LogResults(MonoBehaviour context)
        {
            if (_entries.Count == 0)
            {
                Debug.Log("CaveReplayValidator: No validation results to log.", context);
                return;
            }

            var summary = $"Validation Results: {PassCount} pass, {WarningCount} warning, {ErrorCount} error";
            if (IsValid)
            {
                Debug.Log($"<color=green>{summary}</color>", context);
            }
            else
            {
                Debug.LogError($"<color=red>{summary}</color>", context);
            }

            foreach (var (message, level) in _entries)
            {
                switch (level)
                {
                    case ValidationLevel.Pass:
                        Debug.Log($"  ✓ {message}", context);
                        break;
                    case ValidationLevel.Warning:
                        Debug.LogWarning($"  ⚠ {message}", context);
                        break;
                    case ValidationLevel.Error:
                        Debug.LogError($"  ✗ {message}", context);
                        break;
                }
            }
        }
    }

    public enum ValidationLevel
    {
        Pass,
        Warning,
        Error
    }
}
