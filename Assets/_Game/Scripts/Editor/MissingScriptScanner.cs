#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.EditorTools
{
    public static class MissingScriptScanner
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/_Game/Scenes/FarmScene.unity",
            "Assets/_Game/Scenes/TownScene.unity",
            "Assets/_Game/Scenes/CaveScene.unity"
        };

        public static void ScanOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            var found = ScanScene(scene);
            CompleteOrFail($"Scene '{scene.path}'", found);
        }

        public static void ScanGameplayScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[MissingScriptScanner] Scan cancelled because open scene changes were not saved.");
                return;
            }

            var found = 0;
            foreach (var scenePath in GameplayScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                found += ScanScene(scene);
            }

            CompleteOrFail("Gameplay scenes", found);
        }

        public static void ScanPrefabs()
        {
            var found = 0;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                var results = new List<string>();
                ScanGameObjectInPrefab(prefab, prefab.name, path, results);
                foreach (var message in results)
                {
                    Debug.LogError(message, prefab);
                    found++;
                }
            }

            CompleteOrFail("Prefabs in Assets/_Game", found);
        }

        private static int ScanScene(Scene scene)
        {
            var found = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                found += ScanGameObject(root, root.name, scene.path);
            }

            return found;
        }

        private static int ScanGameObject(GameObject gameObject, string objectPath, string scenePath)
        {
            var count = 0;
            var components = gameObject.GetComponents<Component>();
            for (var index = 0; index < components.Length; index++)
            {
                if (components[index] != null)
                {
                    continue;
                }

                Debug.LogError(
                    $"[MissingScriptScanner] Missing Script | Scene: '{scenePath}' | GameObject: '{objectPath}' | Component index: {index}",
                    gameObject);
                count++;
            }

            foreach (Transform child in gameObject.transform)
            {
                count += ScanGameObject(child.gameObject, $"{objectPath}/{child.name}", scenePath);
            }

            return count;
        }

        private static void ScanGameObjectInPrefab(GameObject gameObject, string objectPath, string prefabPath, List<string> results)
        {
            var components = gameObject.GetComponents<Component>();
            for (var index = 0; index < components.Length; index++)
            {
                if (components[index] == null)
                {
                    results.Add(
                        $"[MissingScriptScanner] Missing Script | Prefab: '{prefabPath}' | GameObject: '{objectPath}' | Component index: {index}");
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                ScanGameObjectInPrefab(child.gameObject, $"{objectPath}/{child.name}", prefabPath, results);
            }
        }

        private static void CompleteOrFail(string scope, int found)
        {
            if (found == 0)
            {
                Debug.Log($"[MissingScriptScanner] {scope}: no missing scripts found.");
                return;
            }

            throw new InvalidOperationException($"[MissingScriptScanner] {scope}: {found} missing script(s) found.");
        }
    }
}
#endif
