#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.EditorTools
{
    public static class MissingScriptScanner
    {
        [MenuItem("Cindar's Hope/Validation/Scan Open Scene for Missing Scripts")]
        public static void ScanOpenScene()
        {
            int found = 0;
            var scene = SceneManager.GetActiveScene();

            foreach (var root in scene.GetRootGameObjects())
            {
                found += ScanGameObject(root, root.name);
            }

            if (found == 0)
                Debug.Log("[MissingScriptScanner] Cena aberta: nenhum missing script encontrado.");
            else
                Debug.LogWarning($"[MissingScriptScanner] Cena aberta: {found} missing script(s) encontrado(s).");
        }

        [MenuItem("Cindar's Hope/Validation/Scan Prefabs in Assets/_Game for Missing Scripts")]
        public static void ScanPrefabs()
        {
            int found = 0;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game" });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var results = new List<string>();
                ScanGameObjectInPrefab(prefab, prefab.name, path, results);

                foreach (var msg in results)
                {
                    Debug.LogWarning(msg, prefab);
                    found++;
                }
            }

            if (found == 0)
                Debug.Log("[MissingScriptScanner] Prefabs Assets/_Game: nenhum missing script encontrado.");
            else
                Debug.LogWarning($"[MissingScriptScanner] Prefabs Assets/_Game: {found} missing script(s) encontrado(s).");
        }

        private static int ScanGameObject(GameObject go, string path)
        {
            int count = 0;
            var components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning(
                        $"[MissingScriptScanner] Missing Script | GameObject: \"{path}\" | Componente índice: {i}",
                        go);
                    count++;
                }
            }

            foreach (Transform child in go.transform)
            {
                count += ScanGameObject(child.gameObject, $"{path}/{child.name}");
            }

            return count;
        }

        private static void ScanGameObjectInPrefab(GameObject go, string objectPath, string prefabAssetPath, List<string> results)
        {
            var components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    results.Add(
                        $"[MissingScriptScanner] Missing Script | Prefab: \"{prefabAssetPath}\" | GameObject: \"{objectPath}\" | Componente índice: {i}");
                }
            }

            foreach (Transform child in go.transform)
            {
                ScanGameObjectInPrefab(child.gameObject, $"{objectPath}/{child.name}", prefabAssetPath, results);
            }
        }
    }
}
#endif
