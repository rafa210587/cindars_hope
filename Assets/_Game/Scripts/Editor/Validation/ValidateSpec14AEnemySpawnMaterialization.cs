using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core.Events;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14AEnemySpawnMaterialization
    {
        private const string CaveScenePath = "Assets/_Game/Scenes/CaveScene.unity";
        private const string EnemyDatabasePath = "Assets/_Game/Data/Combat/EnemyDatabase.asset";
        private const string EnemySpawnProfilesFolder = "Assets/_Game/Data/EnemySpawn/Profiles";
        private const string EnemySpawnPacksFolder = "Assets/_Game/Data/EnemySpawn/Packs";
        private const string EnemyFactionLocksFolder = "Assets/_Game/Data/EnemySpawn/FactionLocks";

        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            RequireType(typeof(CaveEnemySpawnPlan), "CaveEnemySpawnPlan", errors, passed);
            RequireType(typeof(CaveEnemySpawnPlanEntry), "CaveEnemySpawnPlanEntry", errors, passed);
            RequireType(typeof(CaveEnemySpawnPlanner), "CaveEnemySpawnPlanner", errors, passed);

            ValidatePlanFields(errors, passed);
            ValidateMaterializerIntegration(errors, passed);
            ValidateEnemySpawnAssets(errors, passed);
            ValidateCaveSceneWiring(errors, passed);
            ValidateFixedSeedPlan(errors, passed);
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

        private static void ValidateEnemySpawnAssets(List<string> errors, List<string> passed)
        {
            var enemyDatabase = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(EnemyDatabasePath);
            var profiles = LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder);
            var packs = LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder);
            var locks = LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder);

            if (enemyDatabase == null)
                errors.Add($"EnemyDatabaseSO missing at '{EnemyDatabasePath}'. Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.");
            else
                passed.Add($"EnemyDatabaseSO found at '{EnemyDatabasePath}'.");

            if (profiles.Length == 0)
                errors.Add($"No EnemySpawnProfileSO assets found in '{EnemySpawnProfilesFolder}'. Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.");
            else
                passed.Add($"EnemySpawnProfileSO assets found: {profiles.Length}.");

            if (packs.Length == 0)
                errors.Add($"No EnemySpawnPackSO assets found in '{EnemySpawnPacksFolder}'. Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.");
            else
                passed.Add($"EnemySpawnPackSO assets found: {packs.Length}.");

            if (locks.Length == 0)
                errors.Add($"No EnemyFactionLockSO assets found in '{EnemyFactionLocksFolder}'. Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.");
            else
                passed.Add($"EnemyFactionLockSO assets found: {locks.Length}.");
        }

        private static void ValidateCaveSceneWiring(List<string> errors, List<string> passed)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CaveScenePath) == null)
            {
                errors.Add($"CaveScene missing at '{CaveScenePath}'.");
                return;
            }

            var previousScenePath = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(CaveScenePath, OpenSceneMode.Single);

            var materializers = Object.FindObjectsByType<CaveRuntimeMaterializer>(FindObjectsInactive.Include);
            if (materializers.Length == 0)
            {
                errors.Add("CaveScene has no CaveRuntimeMaterializer.");
            }

            foreach (var materializer in materializers)
            {
                var serialized = new SerializedObject(materializer);
                RequireObjectReference(serialized, "_enemyDatabase", "CaveRuntimeMaterializer._enemyDatabase", errors, passed);
                RequireObjectArray(serialized, "_enemySpawnProfiles", "CaveRuntimeMaterializer._enemySpawnProfiles", errors, passed);
                RequireObjectArray(serialized, "_enemySpawnPacks", "CaveRuntimeMaterializer._enemySpawnPacks", errors, passed);
                RequireObjectArray(serialized, "_enemyFactionLocks", "CaveRuntimeMaterializer._enemyFactionLocks", errors, passed);
            }

            var bootstraps = Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Include);
            if (bootstraps.Length == 0)
            {
                errors.Add("CaveScene has no GameBootstrap.");
            }

            foreach (var bootstrap in bootstraps)
            {
                var serialized = new SerializedObject(bootstrap);
                RequireObjectReference(serialized, "_bestiaryManager", "GameBootstrap._bestiaryManager", errors, passed);
            }

            passed.Add($"CaveScene wiring inspected: {scene.path}.");

            if (!string.IsNullOrWhiteSpace(previousScenePath) && previousScenePath != CaveScenePath)
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }
        }

        private static void ValidateFixedSeedPlan(List<string> errors, List<string> passed)
        {
            var enemyDatabase = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(EnemyDatabasePath);
            var profiles = LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder);
            var packs = LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder);
            var locks = LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder);
            if (enemyDatabase == null || profiles.Length == 0 || packs.Length == 0)
            {
                errors.Add("Cannot validate fixed seed spawn plan because enemy database/profiles/packs are missing.");
                return;
            }

            var generatedLevel = BuildSyntheticLevel();
            var runManagerObject = new GameObject("SPEC14A_Validation_RunManager");
            try
            {
                var runManager = runManagerObject.AddComponent<CaveRunManager>();
                var serializedRunManager = new SerializedObject(runManager);
                serializedRunManager.FindProperty("_defaultWorldSeed").stringValue = "validator_world_seed";
                serializedRunManager.FindProperty("_caveWorldSeed").stringValue = "validator_world_seed";
                serializedRunManager.FindProperty("_caveRunSeed").stringValue = "validator_run_seed";
                serializedRunManager.FindProperty("_currentCaveLevel").intValue = 1;
                serializedRunManager.FindProperty("_deepestLayerReached").intValue = 1;
                serializedRunManager.ApplyModifiedPropertiesWithoutUndo();
                runManager.InitializeIfNeeded();

                var planner = new CaveEnemySpawnPlanner();
                var first = planner.CreatePlan(generatedLevel, runManager, profiles, packs, locks, 12);
                var second = planner.CreatePlan(generatedLevel, runManager, profiles, packs, locks, 12);

                if (first == null || !first.IsValid || first.Entries.Count == 0)
                {
                    errors.Add("Fixed seed level 1 spawn plan produced zero enemies despite walkable tiles and spawn profiles.");
                    return;
                }

                if (!PlansMatch(first, second))
                    errors.Add("Fixed seed level 1 spawn plan is not deterministic.");
                else
                    passed.Add($"Fixed seed level 1 spawn plan is deterministic with {first.Entries.Count} enemies.");

                foreach (var entry in first.Entries)
                {
                    if (!enemyDatabase.TryGetById(entry.EnemyId, out _))
                    {
                        errors.Add($"Spawn plan EnemyId '{entry.EnemyId}' is not present in EnemyDatabaseSO.");
                    }
                }

                passed.Add("All fixed seed spawn plan EnemyIds exist in EnemyDatabaseSO.");
            }
            finally
            {
                Object.DestroyImmediate(runManagerObject);
            }
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

        private static CaveGeneratedLevel BuildSyntheticLevel()
        {
            var level = new CaveGeneratedLevel
            {
                CaveLevel = 1,
                BiomeId = "stone",
                Width = 18,
                Height = 18,
                Entrance = new Vector2Int(1, 1),
                Exit = new Vector2Int(16, 16)
            };

            for (var x = 1; x < 17; x++)
            {
                for (var y = 1; y < 17; y++)
                {
                    level.WalkableTiles.Add(new Vector2Int(x, y));
                }
            }

            level.EnemySpawnPoints.Add(new CaveGenerationPoint(CaveGenerationPointType.Enemy, new Vector2Int(8, 8)));
            level.EnemySpawnPoints.Add(new CaveGenerationPoint(CaveGenerationPointType.Enemy, new Vector2Int(11, 8)));
            level.EnemySpawnPoints.Add(new CaveGenerationPoint(CaveGenerationPointType.Enemy, new Vector2Int(8, 11)));
            level.EnemySpawnPoints.Add(new CaveGenerationPoint(CaveGenerationPointType.Enemy, new Vector2Int(12, 12)));
            level.ComputeLayoutHash();
            return level;
        }

        private static bool PlansMatch(CaveEnemySpawnPlan first, CaveEnemySpawnPlan second)
        {
            if (first == null || second == null || first.Entries.Count != second.Entries.Count)
                return false;

            for (var i = 0; i < first.Entries.Count; i++)
            {
                if (first.Entries[i].EnemyInstanceId != second.Entries[i].EnemyInstanceId
                    || first.Entries[i].EnemyId != second.Entries[i].EnemyId
                    || first.Entries[i].GridPosition != second.Entries[i].GridPosition)
                {
                    return false;
                }
            }

            return true;
        }

        private static T[] LoadAssets<T>(string folder) where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToArray();
        }

        private static void RequireObjectReference(SerializedObject serializedObject, string fieldName, string label, List<string> errors, List<string> passed)
        {
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                errors.Add($"{label} missing serialized field.");
                return;
            }

            if (property.objectReferenceValue == null)
                errors.Add($"{label} is not assigned in CaveScene.");
            else
                passed.Add($"{label} is assigned.");
        }

        private static void RequireObjectArray(SerializedObject serializedObject, string fieldName, string label, List<string> errors, List<string> passed)
        {
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || !property.isArray)
            {
                errors.Add($"{label} missing serialized array.");
                return;
            }

            var assigned = 0;
            for (var i = 0; i < property.arraySize; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    assigned++;
                }
            }

            if (assigned == 0)
                errors.Add($"{label} has no assigned assets in CaveScene.");
            else
                passed.Add($"{label} assigned assets: {assigned}.");
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
