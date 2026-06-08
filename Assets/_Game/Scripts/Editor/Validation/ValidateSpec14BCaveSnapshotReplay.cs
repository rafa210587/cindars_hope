using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Cave.Runtime;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14BCaveSnapshotReplay
    {
        [MenuItem("CindarsHope/Archive/Validation/Validate SPEC 14B - Cave Snapshot Replay")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            RequireType(typeof(CaveLevelSnapshot), "CaveLevelSnapshot", errors, passed);
            RequireType(typeof(VisitedLevelSnapshot), "VisitedLevelSnapshot", errors, passed);
            RequireType(typeof(CaveSnapshotService), "CaveSnapshotService", errors, passed);
            RequireType(typeof(CaveResourceNodeSnapshotEntry), "CaveResourceNodeSnapshotEntry", errors, passed);
            RequireType(typeof(CaveFishingSpotSnapshotEntry), "CaveFishingSpotSnapshotEntry", errors, passed);

            ValidateSnapshotFields(errors, passed);
            ValidateHashDeterminism(errors, passed);
            ValidateControllerIntegration(errors, passed);
            ValidateSaveIntegration(errors, passed);
            ValidateRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 14B Validation] PASSED: {passed.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 14B] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 14B] Validation FAILED with {errors.Count} error(s).");
        }

        private static void RequireType(System.Type type, string label, List<string> errors, List<string> passed)
        {
            if (type != null) passed.Add($"{label} exists.");
            else errors.Add($"{label} missing.");
        }

        private static void ValidateSnapshotFields(List<string> errors, List<string> passed)
        {
            RequireField(typeof(VisitedLevelSnapshot), "CaveWorldSeed", typeof(string), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "CaveRunSeed", typeof(string), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "WalkableTilesList", typeof(List<Vector2Int>), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "WallTilesList", typeof(List<Vector2Int>), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "ResourceNodeStates", typeof(List<CaveResourceNodeSnapshotEntry>), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "FishingSpotState", typeof(CaveFishingSpotSnapshotEntry), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "EnemySpawnPlan", typeof(CaveEnemySpawnPlan), errors, passed);
            RequireField(typeof(VisitedLevelSnapshot), "Warnings", typeof(List<string>), errors, passed);

            ValidateSimpleFields(typeof(CaveResourceNodeSnapshotEntry), errors, passed);
            ValidateSimpleFields(typeof(CaveFishingSpotSnapshotEntry), errors, passed);
        }

        private static void ValidateHashDeterminism(List<string> errors, List<string> passed)
        {
            var service = new CaveSnapshotService();
            var snapshot = new CaveLevelSnapshot(1, "biome_cave_earth", string.Empty, "world", "run");
            snapshot.SetLayoutDimensions(8, 8);
            snapshot.SetEntranceAndExit(new Vector2(1, 1), new Vector2(6, 6));
            snapshot.AddWalkableTile(new Vector2Int(2, 2));
            snapshot.AddWalkableTile(new Vector2Int(3, 2));
            snapshot.AddWallTile(new Vector2Int(0, 0));
            snapshot.ResourceNodeStates.Add(new CaveResourceNodeSnapshotEntry
            {
                NodeInstanceId = "node_1",
                ResourceNodeId = "resource_stone",
                GridPosition = new Vector2Int(4, 4),
                IsDepleted = false
            });
            snapshot.SetFishingSpot(new CaveFishingSpotSnapshotEntry
            {
                HasFishingSpot = true,
                FishingSpotId = "fish_1",
                GridPosition = new Vector2Int(5, 5),
                FishingProfileId = "fishing_cave"
            });
            snapshot.SetEnemySpawnPlan(new CaveEnemySpawnPlan
            {
                CaveLevel = 1,
                BiomeId = "biome_cave_earth",
                CaveWorldSeed = "world",
                CaveRunSeed = "run",
                LevelSeed = "seed",
                Entries = new List<CaveEnemySpawnPlanEntry>
                {
                    new CaveEnemySpawnPlanEntry
                    {
                        EnemyInstanceId = "enemy_1",
                        EnemyId = "enemy_cave_mite",
                        SpawnProfileId = "spawn_enemy_cave_mite",
                        GridPosition = new Vector2Int(2, 2),
                        SpawnIndex = 0,
                        SizeClass = "Tiny"
                    }
                }
            });

            var first = service.CalculateLayoutHash(snapshot);
            var second = service.CalculateLayoutHash(snapshot);
            if (!string.IsNullOrWhiteSpace(first) && first == second)
                passed.Add("CaveSnapshotService LayoutHash is deterministic.");
            else
                errors.Add("CaveSnapshotService LayoutHash is not deterministic.");
        }

        private static void ValidateControllerIntegration(List<string> errors, List<string> passed)
        {
            var controller = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Cave/CaveLevelRuntimeController.cs");
            if (controller == null)
            {
                errors.Add("CaveLevelRuntimeController.cs missing.");
                return;
            }

            if (controller.text.Contains("CaveSnapshotService") && controller.text.Contains("TryGetSnapshot"))
                passed.Add("CaveLevelRuntimeController uses CaveSnapshotService for visited snapshots.");
            else
                errors.Add("CaveLevelRuntimeController does not use CaveSnapshotService snapshot lookup.");

            if (controller.text.Contains("MaterializeFromSnapshot"))
                passed.Add("CaveLevelRuntimeController materializes from snapshot.");
            else
                errors.Add("CaveLevelRuntimeController does not call MaterializeFromSnapshot.");
        }

        private static void ValidateSaveIntegration(List<string> errors, List<string> passed)
        {
            var save = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Save/CaveSaveData.cs");
            if (save == null)
            {
                errors.Add("CaveSaveData.cs missing.");
                return;
            }

            if (save.text.Contains("ResourceNodeStates")
                && save.text.Contains("FishingSpotState")
                && save.text.Contains("CaveEnemySpawnPlan"))
                passed.Add("CaveSaveData preserves resources, fishing hook and CaveEnemySpawnPlan.");
            else
                errors.Add("CaveSaveData does not preserve required SPEC 14B snapshot fields.");
        }

        private static void ValidateRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] files =
            {
                "Assets/_Game/Scripts/Cave/Runtime/CaveSnapshotService.cs",
                "Assets/_Game/Scripts/Cave/Runtime/VisitedLevelSnapshot.cs",
                "Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs"
            };

            foreach (var file in files)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                if (asset == null)
                {
                    errors.Add($"Runtime file missing: {file}");
                    continue;
                }

                if (asset.text.Contains("GameObject.Find") || asset.text.Contains("FindObjectOfType") || asset.text.Contains("FindObjectsByType"))
                    errors.Add($"{file} contains prohibited runtime scene search.");
            }

            passed.Add("SPEC 14B runtime files do not use prohibited global scene search.");
        }

        private static void RequireField(System.Type type, string fieldName, System.Type expectedType, List<string> errors, List<string> passed)
        {
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                errors.Add($"{type.Name}.{fieldName} missing.");
                return;
            }

            if (field.FieldType != expectedType)
            {
                errors.Add($"{type.Name}.{fieldName} expected {expectedType.Name}, found {field.FieldType.Name}.");
                return;
            }

            passed.Add($"{type.Name}.{fieldName} exists.");
        }

        private static void ValidateSimpleFields(System.Type type, List<string> errors, List<string> passed)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsAllowedField(field.FieldType))
                    errors.Add($"{type.Name}.{field.Name} uses non-save-friendly field type {field.FieldType.Name}.");
            }

            passed.Add($"{type.Name} uses ids/simple fields.");
        }

        private static bool IsAllowedField(System.Type type)
        {
            if (type == typeof(string) || type == typeof(int) || type == typeof(bool))
                return true;
            if (type == typeof(Vector2) || type == typeof(Vector2Int) || type == typeof(Vector3))
                return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsAllowedField(type.GetGenericArguments()[0]);
            return false;
        }
    }
}
