using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateAndRepairTreeColliders
    {
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";

        [MenuItem("CindarsHope/Fix Tree Colliders", priority = 1)]
        public static void FixTreeColliders()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            int total = 0;

            EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Single);
            total += FixCollidersInActiveScene("FarmScene");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
            total += FixCollidersInActiveScene("TownScene");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Fix Tree Colliders",
                $"Done. {total} collider(s) updated to sprite bounds.\n\nSee Console for details.",
                "OK");
        }

        private static int FixCollidersInActiveScene(string sceneName)
        {
            int count = 0;

            // TreeNode objects (FarmScene)
            foreach (var tree in Object.FindObjectsByType<TreeNode>())
            {
                if (FitColliderToSprite(tree.GetComponent<BoxCollider2D>(), tree.GetComponent<SpriteRenderer>(), tree.name))
                    count++;
            }

            // TownTree objects (no TreeNode component)
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (!sr.gameObject.name.StartsWith("TownTree_")) continue;
                    if (FitColliderToSprite(sr.GetComponent<BoxCollider2D>(), sr, sr.gameObject.name))
                        count++;
                }
            }

            Debug.Log($"FixTreeColliders [{sceneName}]: {count} collider(s) updated.");
            return count;
        }

        private static bool FitColliderToSprite(BoxCollider2D collider, SpriteRenderer sr, string label)
        {
            if (collider == null || sr == null)
            {
                Debug.LogWarning($"FixTreeColliders: '{label}' missing BoxCollider2D or SpriteRenderer — skipped.");
                return false;
            }

            if (sr.sprite == null)
            {
                Debug.LogWarning($"FixTreeColliders: '{label}' has no sprite — collider not changed.");
                return false;
            }

            var b = sr.sprite.bounds;
            var newSize   = new Vector2(b.size.x, b.size.y);
            var newOffset = new Vector2(b.center.x, b.center.y);

            if (collider.size == newSize && collider.offset == newOffset)
                return false;

            Undo.RecordObject(collider, "Fix Tree Collider");
            collider.size   = newSize;
            collider.offset = newOffset;
            EditorUtility.SetDirty(collider);
            Debug.Log($"  '{label}': size={newSize} offset={newOffset}");
            return true;
        }
    }
}
