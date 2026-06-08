using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    public static class GenerateAndWireSpec13GAssets
    {
        private const string CaveScenePath = "Assets/_Game/Scenes/CaveScene.unity";
        private const string EnemyDatabasePath = "Assets/_Game/Data/Combat/EnemyDatabase.asset";
        private const string EnemySpawnProfilesFolder = "Assets/_Game/Data/EnemySpawn/Profiles";
        private const string EnemySpawnPacksFolder = "Assets/_Game/Data/EnemySpawn/Packs";
        private const string EnemyFactionLocksFolder = "Assets/_Game/Data/EnemySpawn/FactionLocks";

        private const string CombatDataFolder = "Assets/_Game/Data/Combat";
        private const string ActionDatabasePath = "Assets/_Game/Data/Combat/EnemyActionDatabase.asset";
        private const string ActionSetDatabasePath = "Assets/_Game/Data/Combat/EnemyActionSetDatabase.asset";
        private const string TelegraphDatabasePath = "Assets/_Game/Data/Combat/EnemyTelegraphProfileDatabase.asset";
        private const string MovementDatabasePath = "Assets/_Game/Data/Combat/EnemyMovementProfileDatabase.asset";
        private const string VulnerabilityDatabasePath = "Assets/_Game/Data/Combat/EnemyVulnerabilityProfileDatabase.asset";
        private const string SizeDatabasePath = "Assets/_Game/Data/Combat/EnemySizeProfileDatabase.asset";

        private const string ActionsFolder    = "Assets/_Game/Data/Enemies/Actions";
        private const string ActionSetsFolder = "Assets/_Game/Data/Enemies/ActionSets";
        private const string TelegraphFolder  = "Assets/_Game/Data/Enemies/TelegraphProfiles";
        private const string MovementFolder   = "Assets/_Game/Data/Enemies/MovementProfiles";
        private const string VulnFolder       = "Assets/_Game/Data/Enemies/VulnerabilityProfiles";
        private const string SizeFolder       = "Assets/_Game/Data/Enemies/SizeProfiles";

        // Batchmode entry: Unity.exe -executeMethod CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.Execute
        public static void Execute() => GenerateAndWire();

        [MenuItem("CindarsHope/Archive/Generate/Enemy Runtime Data/Regenerate All Enemy Data")]
        public static void RegenerateAllEnemyData() => GenerateAndWire();

        [MenuItem("CindarsHope/Archive/SPEC 13/Generate And Wire SPEC 13G Assets")]
        public static void GenerateAndWire()
        {
            CreateDefaultEnemyProfiles.CreateAll();
            CreateRoster40EnemyData.CreateRoster();
            CreateEnemyActionsAndSets.CreateAll();
            CreateBestiaryEntries40.CreateEntries();
            CreateEnemySpawnEcologyData.CreateData();

            var enemyDatabase = EnsureEnemyDatabaseHasRoster();

            EnsureFolder(CombatDataFolder);
            var actionDb      = EnsureDatabase<EnemyActionDatabaseSO, EnemyActionSO>(ActionDatabasePath, ActionsFolder);
            var actionSetDb   = EnsureDatabase<EnemyActionSetDatabaseSO, EnemyActionSetSO>(ActionSetDatabasePath, ActionSetsFolder);
            var telegraphDb   = EnsureDatabase<EnemyTelegraphProfileDatabaseSO, EnemyTelegraphProfileSO>(TelegraphDatabasePath, TelegraphFolder);
            var movementDb    = EnsureDatabase<EnemyMovementProfileDatabaseSO, EnemyMovementProfileSO>(MovementDatabasePath, MovementFolder);
            var vulnerabilityDb = EnsureDatabase<EnemyVulnerabilityProfileDatabaseSO, EnemyVulnerabilityProfileSO>(VulnerabilityDatabasePath, VulnFolder);
            var sizeDb        = EnsureDatabase<EnemySizeProfileDatabaseSO, EnemySizeProfileSO>(SizeDatabasePath, SizeFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireCaveScene(enemyDatabase, actionDb, actionSetDb, telegraphDb, movementDb, vulnerabilityDb, sizeDb);

            // SPEC 14A-FIX6: post-wiring assertions — fail loud if generator left stale state
            AssertPostGenerationInvariants(actionSetDb, movementDb, vulnerabilityDb, sizeDb);
        }

        private static void AssertPostGenerationInvariants(
            EnemyActionSetDatabaseSO actionSetDb,
            EnemyMovementProfileDatabaseSO movementDb,
            EnemyVulnerabilityProfileDatabaseSO vulnerabilityDb,
            EnemySizeProfileDatabaseSO sizeDb)
        {
            int failures = 0;

            // 1. Profile count must exceed legacy 40-only roster
            var profiles = LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder);
            if (profiles.Length <= 40)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: ProfilesTotal={profiles.Length}. Expected > 40 (bands 6-7 should add 20 new profiles). Check CreateEnemySpawnEcologyData.");
                failures++;
            }

            // 2. No profile may still carry RequiredBossGateProgress (FIX5 removed the field from all P() calls)
            int staleGate = profiles.Count(p => p != null && !string.IsNullOrWhiteSpace(p.RequiredBossGateProgress));
            if (staleGate > 0)
            {
                var sample = profiles.Where(p => p != null && !string.IsNullOrWhiteSpace(p.RequiredBossGateProgress)).Take(3).Select(p => $"{p.EnemyId}->{p.RequiredBossGateProgress}").ToArray();
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: {staleGate} profile(s) still have RequiredBossGateProgress set (sample: {string.Join(", ", sample)}). FIX5 should leave this empty.");
                failures++;
            }

            // 3. Pack count must include bands 6-7 (>=25 expected: 19 original + 6 new)
            var packs = LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder);
            if (packs.Length < 25)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: PacksTotal={packs.Length}. Expected >= 25 (19 original + 6 new bands 6-7 packs).");
                failures++;
            }

            // 4. CaveRuntimeMaterializer must end up with the same profile count as on disk
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CaveScenePath))
            {
                var materializers = Object.FindObjectsByType<CaveRuntimeMaterializer>(FindObjectsInactive.Include);
                foreach (var mat in materializers)
                {
                    var serialized = new SerializedObject(mat);
                    var spawnProfilesProp = serialized.FindProperty("_enemySpawnProfiles");
                    if (spawnProfilesProp != null && spawnProfilesProp.isArray && spawnProfilesProp.arraySize != profiles.Length)
                    {
                        Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: CaveScene materializer wired with {spawnProfilesProp.arraySize} profiles but disk has {profiles.Length}. Re-run wiring or check WireMaterializer.");
                        failures++;
                    }
                }
            }

            // 5. Databases must all be populated (not just created)
            if (actionSetDb != null && CountDatabaseItems(actionSetDb) < 50)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: ActionSetDatabase has {CountDatabaseItems(actionSetDb)} items (expected >=50 for 40+20 enemies).");
                failures++;
            }
            if (movementDb != null && CountDatabaseItems(movementDb) < 8)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: MovementProfileDatabase has {CountDatabaseItems(movementDb)} items (expected >=8 movement archetypes).");
                failures++;
            }
            if (vulnerabilityDb != null && CountDatabaseItems(vulnerabilityDb) < 8)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: VulnerabilityProfileDatabase has {CountDatabaseItems(vulnerabilityDb)} items (expected >=8).");
                failures++;
            }
            if (sizeDb != null && CountDatabaseItems(sizeDb) < 6)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets FAIL: SizeProfileDatabase has {CountDatabaseItems(sizeDb)} items (expected >=6: tiny/small/medium/large/huge/boss).");
                failures++;
            }

            if (failures == 0)
                Debug.Log($"GenerateAndWireSpec13GAssets: ALL POST-WIRING ASSERTIONS PASSED. Profiles={profiles.Length}, Packs={packs.Length}.");
            else
                Debug.LogError($"GenerateAndWireSpec13GAssets: {failures} POST-WIRING ASSERTION(S) FAILED. See errors above.");
        }

        private static int CountDatabaseItems(ScriptableObject db)
        {
            var so = new SerializedObject(db);
            var prop = so.FindProperty("_items");
            return prop != null && prop.isArray ? prop.arraySize : 0;
        }

        private static EnemyDatabaseSO EnsureEnemyDatabaseHasRoster()
        {
            EnsureFolder("Assets/_Game/Data/Combat");

            var database = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(EnemyDatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<EnemyDatabaseSO>();
                database.name = "EnemyDatabase";
                AssetDatabase.CreateAsset(database, EnemyDatabasePath);
            }

            var enemies = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { "Assets/_Game/Data/Enemies", "Assets/_Game/Data/Combat" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(enemy => enemy != null && !string.IsNullOrWhiteSpace(enemy.enemyId))
                .GroupBy(enemy => enemy.enemyId)
                .Select(group => group.OrderBy(enemy => AssetDatabase.GetAssetPath(enemy)).First())
                .OrderBy(enemy => enemy.enemyId)
                .ToArray();

            var serializedDatabase = new SerializedObject(database);
            var itemsProperty = serializedDatabase.FindProperty("_items");
            if (itemsProperty == null || !itemsProperty.isArray)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets: EnemyDatabaseSO at '{EnemyDatabasePath}' has no '_items' array.");
                return database;
            }

            itemsProperty.arraySize = enemies.Length;
            for (var i = 0; i < enemies.Length; i++)
            {
                itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
            Debug.Log($"GenerateAndWireSpec13GAssets: EnemyDatabase wired with {enemies.Length} EnemyDataSO assets.");
            return database;
        }

        private static void WireCaveScene(
            EnemyDatabaseSO enemyDatabase,
            EnemyActionDatabaseSO actionDb,
            EnemyActionSetDatabaseSO actionSetDb,
            EnemyTelegraphProfileDatabaseSO telegraphDb,
            EnemyMovementProfileDatabaseSO movementDb,
            EnemyVulnerabilityProfileDatabaseSO vulnerabilityDb,
            EnemySizeProfileDatabaseSO sizeDb)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(CaveScenePath))
            {
                Debug.LogWarning($"GenerateAndWireSpec13GAssets: CaveScene not found at '{CaveScenePath}'. Scene wiring skipped.");
                return;
            }

            var previousScene = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(CaveScenePath, OpenSceneMode.Single);

            // SPEC 14A-FIX6.1: re-load database references AFTER scene change.
            // EditorSceneManager.OpenScene(Single) can invalidate ScriptableObject references held since
            // before the load (Unity "fake null" / MissingReferenceException on subsequent access).
            // Reading them back from disk by path guarantees live wrappers for wiring + logging.
            enemyDatabase   = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(EnemyDatabasePath)              ?? enemyDatabase;
            actionDb        = AssetDatabase.LoadAssetAtPath<EnemyActionDatabaseSO>(ActionDatabasePath)        ?? actionDb;
            actionSetDb     = AssetDatabase.LoadAssetAtPath<EnemyActionSetDatabaseSO>(ActionSetDatabasePath)  ?? actionSetDb;
            telegraphDb     = AssetDatabase.LoadAssetAtPath<EnemyTelegraphProfileDatabaseSO>(TelegraphDatabasePath) ?? telegraphDb;
            movementDb      = AssetDatabase.LoadAssetAtPath<EnemyMovementProfileDatabaseSO>(MovementDatabasePath)   ?? movementDb;
            vulnerabilityDb = AssetDatabase.LoadAssetAtPath<EnemyVulnerabilityProfileDatabaseSO>(VulnerabilityDatabasePath) ?? vulnerabilityDb;
            sizeDb          = AssetDatabase.LoadAssetAtPath<EnemySizeProfileDatabaseSO>(SizeDatabasePath)     ?? sizeDb;

            var materializers = Object.FindObjectsByType<CaveRuntimeMaterializer>(FindObjectsInactive.Include);
            if (materializers.Length == 0)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets: no CaveRuntimeMaterializer found in '{CaveScenePath}'.");
            }
            else
            {
                foreach (var materializer in materializers)
                {
                    WireMaterializer(materializer, enemyDatabase, actionDb, actionSetDb, telegraphDb, movementDb, vulnerabilityDb, sizeDb);
                }
            }

            var bootstraps = Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Include);
            if (bootstraps.Length == 0)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets: no GameBootstrap found in '{CaveScenePath}'.");
            }
            else
            {
                foreach (var bootstrap in bootstraps)
                {
                    WireBestiaryManager(bootstrap);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(
                $"GenerateAndWireSpec13GAssets: CaveScene wired. " +
                $"Materializers={materializers.Length}, Bootstraps={bootstraps.Length}, " +
                $"Profiles={LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder).Length}, " +
                $"Packs={LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder).Length}, " +
                $"FactionLocks={LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder).Length}, " +
                $"Actions={SafeName(actionDb)}, ActionSets={SafeName(actionSetDb)}, " +
                $"Movement={SafeName(movementDb)}, Vulnerability={SafeName(vulnerabilityDb)}, Size={SafeName(sizeDb)}.");

            if (!string.IsNullOrWhiteSpace(previousScene) && previousScene != CaveScenePath)
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        // SPEC 14A-FIX6.1: Unity Object aware name accessor.
        // The C# `?.` operator does NOT handle Unity's "fake null" / destroyed objects — it only checks
        // the C# reference. `obj != null` uses Unity's overloaded `==` and correctly returns false for
        // destroyed UnityEngine.Objects, preventing MissingReferenceException on `obj.name`.
        private static string SafeName(Object obj)
        {
            return obj != null ? obj.name : "null";
        }

        private static void WireMaterializer(
            CaveRuntimeMaterializer materializer,
            EnemyDatabaseSO enemyDatabase,
            EnemyActionDatabaseSO actionDb,
            EnemyActionSetDatabaseSO actionSetDb,
            EnemyTelegraphProfileDatabaseSO telegraphDb,
            EnemyMovementProfileDatabaseSO movementDb,
            EnemyVulnerabilityProfileDatabaseSO vulnerabilityDb,
            EnemySizeProfileDatabaseSO sizeDb)
        {
            var serializedMaterializer = new SerializedObject(materializer);
            SetReference(serializedMaterializer, "_enemyDatabase", enemyDatabase);
            SetObjectArray(serializedMaterializer, "_enemySpawnProfiles", LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder));
            SetObjectArray(serializedMaterializer, "_enemySpawnPacks", LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder));
            SetObjectArray(serializedMaterializer, "_enemyFactionLocks", LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder));
            SetReference(serializedMaterializer, "_actionSetDatabase", actionSetDb);
            SetReference(serializedMaterializer, "_actionDatabase", actionDb);
            SetReference(serializedMaterializer, "_telegraphDatabase", telegraphDb);
            SetReference(serializedMaterializer, "_movementProfileDatabase", movementDb);
            SetReference(serializedMaterializer, "_vulnerabilityProfileDatabase", vulnerabilityDb);
            SetReference(serializedMaterializer, "_sizeProfileDatabase", sizeDb);
            serializedMaterializer.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(materializer);
        }

        private static void WireBestiaryManager(GameBootstrap bootstrap)
        {
            var bestiaryManager = bootstrap.GetComponent<BestiaryManager>();
            if (bestiaryManager == null)
            {
                bestiaryManager = bootstrap.gameObject.AddComponent<BestiaryManager>();
            }

            var serializedBootstrap = new SerializedObject(bootstrap);
            SetReference(serializedBootstrap, "_bestiaryManager", bestiaryManager);
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bootstrap);
            EditorUtility.SetDirty(bestiaryManager);
        }

        private static T[] LoadAssets<T>(string folder) where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .OrderBy(asset => asset.name)
                .ToArray();
        }

        private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"GenerateAndWireSpec13GAssets: serialized field '{propertyName}' missing on '{serializedObject.targetObject.name}'.");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetObjectArray<T>(SerializedObject serializedObject, string propertyName, IReadOnlyList<T> values)
            where T : Object
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogWarning($"GenerateAndWireSpec13GAssets: serialized array '{propertyName}' missing on '{serializedObject.targetObject.name}'.");
                return;
            }

            property.arraySize = values?.Count ?? 0;
            for (var i = 0; i < property.arraySize; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static TDb EnsureDatabase<TDb, TItem>(string assetPath, string itemsFolder)
            where TDb : DataRegistrySO<TItem>
            where TItem : ScriptableObject, CindarsHope.Core.Data.IIdentifiedData
        {
            var db = AssetDatabase.LoadAssetAtPath<TDb>(assetPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<TDb>();
                db.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                AssetDatabase.CreateAsset(db, assetPath);
            }

            var items = AssetDatabase.FindAssets($"t:{typeof(TItem).Name}", new[] { itemsFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<TItem>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(item => item != null)
                .OrderBy(item => item.name)
                .ToArray();

            var so = new SerializedObject(db);
            var prop = so.FindProperty("_items");
            if (prop != null && prop.isArray)
            {
                prop.arraySize = items.Length;
                for (int i = 0; i < items.Length; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(db);
            Debug.Log($"GenerateAndWireSpec13GAssets: {typeof(TDb).Name} populated with {items.Length} items from '{itemsFolder}'.");
            return db;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
