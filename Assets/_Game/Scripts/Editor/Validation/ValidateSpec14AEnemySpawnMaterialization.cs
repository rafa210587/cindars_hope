using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Events;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14AEnemySpawnMaterialization
    {
        [MenuItem("CindarsHope/Validation/Validate SPEC 14A - Enemy Spawn Materialization")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            RequireType(typeof(CaveEnemySpawnPlan), "CaveEnemySpawnPlan", errors, passed);
            RequireType(typeof(CaveEnemySpawnPlanEntry), "CaveEnemySpawnPlanEntry", errors, passed);
            RequireType(typeof(CaveEnemySpawnPlanner), "CaveEnemySpawnPlanner", errors, passed);

            ValidatePlanFields(errors, passed);
            ValidateMaterializerIntegration(errors, passed);
            ValidatePlannerIntegration(errors, passed);
            ValidateDeterministicInstanceIds(errors, passed);
            ValidateEvents(errors, passed);
            ValidateBestiaryManager(errors, passed);
            ValidateForbiddenRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 14A Validation] PASSED: {passed.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 14A] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 14A] Validation FAILED with {errors.Count} error(s).");
        }

        private static void RequireType(System.Type type, string label, List<string> errors, List<string> passed)
        {
            if (type != null) passed.Add($"{label} exists.");
            else errors.Add($"{label} missing.");
        }

        private static void ValidatePlanFields(List<string> errors, List<string> passed)
        {
            ValidateSimpleFields(typeof(CaveEnemySpawnPlan), errors, passed);
            ValidateSimpleFields(typeof(CaveEnemySpawnPlanEntry), errors, passed);

            RequireField(typeof(CaveEnemySpawnPlan), "CaveLevel", typeof(int), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "BiomeId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "CaveWorldSeed", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "CaveRunSeed", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "LevelSeed", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "LayoutHash", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlan), "Warnings", typeof(List<string>), errors, passed);

            RequireField(typeof(CaveEnemySpawnPlanEntry), "EnemyInstanceId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "EnemyId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "SpawnProfileId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "PackId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "GridPosition", typeof(Vector2Int), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "WorldPosition", typeof(Vector3), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "RoomId", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "SpawnIndex", typeof(int), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "IsElite", typeof(bool), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "SizeClass", typeof(string), errors, passed);
            RequireField(typeof(CaveEnemySpawnPlanEntry), "FactionId", typeof(string), errors, passed);
        }

        private static void ValidateMaterializerIntegration(List<string> errors, List<string> passed)
        {
            var materializer = typeof(CaveRuntimeMaterializer);
            if (materializer.GetProperty("LastEnemySpawnPlan") != null)
                passed.Add("CaveRuntimeMaterializer exposes LastEnemySpawnPlan.");
            else
                errors.Add("CaveRuntimeMaterializer.LastEnemySpawnPlan missing.");

            var resultProperty = typeof(CaveRuntimeMaterializationResult).GetProperty("CreatedEnemies");
            if (resultProperty != null)
                passed.Add("CaveRuntimeMaterializationResult counts CreatedEnemies.");
            else
                errors.Add("CaveRuntimeMaterializationResult.CreatedEnemies missing.");

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs");
            if (asset == null)
            {
                errors.Add("CaveRuntimeMaterializer.cs missing.");
                return;
            }

            if (asset.text.Contains("MaterializeEnemies(") && asset.text.Contains("EnemySpawnedEvent"))
                passed.Add("CaveRuntimeMaterializer materializes enemies and publishes spawn events.");
            else
                errors.Add("CaveRuntimeMaterializer does not show enemy materialization/event integration.");
        }

        private static void ValidatePlannerIntegration(List<string> errors, List<string> passed)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs");
            if (asset == null)
            {
                errors.Add("CaveEnemySpawnPlanner.cs missing.");
                return;
            }

            if (asset.text.Contains("new EnemySpawnResolver"))
                passed.Add("CaveEnemySpawnPlanner uses EnemySpawnResolver.");
            else
                errors.Add("CaveEnemySpawnPlanner does not use EnemySpawnResolver.");

            if (asset.text.Contains("CaveWorldSeed") && asset.text.Contains("CaveRunSeed") && asset.text.Contains("generatedLevel.CaveLevel"))
                passed.Add("CaveEnemySpawnPlanner seed path includes world seed, run seed and cave level.");
            else
                errors.Add("CaveEnemySpawnPlanner seed path must include CaveWorldSeed, CaveRunSeed and CaveLevel.");

            if (asset.text.Contains("Guid.NewGuid"))
                errors.Add("CaveEnemySpawnPlanner must not use Guid.NewGuid for enemy instance ids.");
            else
                passed.Add("CaveEnemySpawnPlanner does not use random GUIDs for enemy instance ids.");

            if (asset.text.Contains("MinDistanceBetweenEnemies"))
                passed.Add("CaveEnemySpawnPlanner enforces minimum distance between enemies.");
            else
                errors.Add("CaveEnemySpawnPlanner does not enforce minimum distance between enemies.");
        }

        private static void ValidateDeterministicInstanceIds(List<string> errors, List<string> passed)
        {
            var first = CaveEnemySpawnPlanner.BuildEnemyInstanceId(1, "room_0", 2, "enemy_cave_mite", "world", "run");
            var second = CaveEnemySpawnPlanner.BuildEnemyInstanceId(1, "room_0", 2, "enemy_cave_mite", "world", "run");
            var differentRun = CaveEnemySpawnPlanner.BuildEnemyInstanceId(1, "room_0", 2, "enemy_cave_mite", "world", "run2");

            if (first == second && first != differentRun)
                passed.Add("EnemyInstanceId is deterministic for same seed and changes with run seed.");
            else
                errors.Add("EnemyInstanceId determinism failed.");
        }

        private static void ValidateEvents(List<string> errors, List<string> passed)
        {
            var spawnedFields = typeof(EnemySpawnedEvent).GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (spawnedFields.Any(f => f.Name == "EnemyInstanceId") && spawnedFields.Any(f => f.Name == "CaveLevel"))
                passed.Add("EnemySpawnedEvent includes EnemyInstanceId and CaveLevel.");
            else
                errors.Add("EnemySpawnedEvent missing EnemyInstanceId/CaveLevel.");

            foreach (var field in spawnedFields)
            {
                if (!IsAllowedField(field.FieldType))
                {
                    errors.Add($"EnemySpawnedEvent.{field.Name} uses Unity/runtime reference type {field.FieldType.Name}.");
                }
            }

            passed.Add("EnemySpawnedEvent payload uses ids/simple value types.");
        }

        private static void ValidateBestiaryManager(List<string> errors, List<string> passed)
        {
            var bestiaryAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Game/Scripts/Enemy/BestiaryManager.cs");
            if (bestiaryAsset == null)
            {
                errors.Add("BestiaryManager.cs missing.");
                return;
            }

            if (bestiaryAsset.text.Contains("EnemySpawnedEvent") || bestiaryAsset.text.Contains("EnemySeenEvent"))
                passed.Add("BestiaryManager subscribes to enemy spawn/seen events.");
            else
                errors.Add("BestiaryManager does not consume EnemySpawnedEvent/EnemySeenEvent.");
        }

        private static void ValidateForbiddenRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] files =
            {
                "Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlan.cs",
                "Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs",
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

            passed.Add("SPEC 14A touched runtime files do not use prohibited scene searches.");
        }

        private static void ValidateSimpleFields(System.Type type, List<string> errors, List<string> passed)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsAllowedField(field.FieldType))
                    errors.Add($"{type.Name}.{field.Name} uses non-save-friendly field type {field.FieldType.Name}.");
            }

            passed.Add($"{type.Name} uses IDs/simple fields for plan data.");
        }

        private static void RequireField(
            System.Type type,
            string fieldName,
            System.Type expectedType,
            List<string> errors,
            List<string> passed)
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

        private static bool IsAllowedField(System.Type type)
        {
            if (type == typeof(string) || type == typeof(int) || type == typeof(bool))
                return true;

            if (type == typeof(Vector2Int) || type == typeof(Vector2) || type == typeof(Vector3))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsAllowedField(type.GetGenericArguments()[0]);

            if (type == typeof(CaveEnemySpawnPlanEntry))
                return true;

            return false;
        }
    }
}
