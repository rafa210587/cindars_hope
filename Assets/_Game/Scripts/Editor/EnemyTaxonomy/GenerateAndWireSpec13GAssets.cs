using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
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

        // Batchmode entry: Unity.exe -executeMethod CindarsHope.Editor.EnemyTaxonomy.GenerateAndWireSpec13GAssets.Execute
        public static void Execute() => GenerateAndWire();

        [MenuItem("CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets")]
        public static void GenerateAndWire()
        {
            CreateDefaultEnemyProfiles.CreateAll();
            CreateRoster40EnemyData.CreateRoster();
            CreateEnemyActionsAndSets.CreateAll();
            CreateBestiaryEntries40.CreateEntries();
            CreateEnemySpawnEcologyData.CreateData();

            var enemyDatabase = EnsureEnemyDatabaseHasRoster();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            WireCaveScene(enemyDatabase);
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

        private static void WireCaveScene(EnemyDatabaseSO enemyDatabase)
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(CaveScenePath))
            {
                Debug.LogWarning($"GenerateAndWireSpec13GAssets: CaveScene not found at '{CaveScenePath}'. Scene wiring skipped.");
                return;
            }

            var previousScene = SceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(CaveScenePath, OpenSceneMode.Single);

            var materializers = Object.FindObjectsByType<CaveRuntimeMaterializer>(FindObjectsInactive.Include);
            if (materializers.Length == 0)
            {
                Debug.LogError($"GenerateAndWireSpec13GAssets: no CaveRuntimeMaterializer found in '{CaveScenePath}'.");
            }
            else
            {
                foreach (var materializer in materializers)
                {
                    WireMaterializer(materializer, enemyDatabase);
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
                $"FactionLocks={LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder).Length}.");

            if (!string.IsNullOrWhiteSpace(previousScene) && previousScene != CaveScenePath)
            {
                EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
            }
        }

        private static void WireMaterializer(CaveRuntimeMaterializer materializer, EnemyDatabaseSO enemyDatabase)
        {
            var serializedMaterializer = new SerializedObject(materializer);
            SetReference(serializedMaterializer, "_enemyDatabase", enemyDatabase);
            SetObjectArray(serializedMaterializer, "_enemySpawnProfiles", LoadAssets<EnemySpawnProfileSO>(EnemySpawnProfilesFolder));
            SetObjectArray(serializedMaterializer, "_enemySpawnPacks", LoadAssets<EnemySpawnPackSO>(EnemySpawnPacksFolder));
            SetObjectArray(serializedMaterializer, "_enemyFactionLocks", LoadAssets<EnemyFactionLockSO>(EnemyFactionLocksFolder));
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
