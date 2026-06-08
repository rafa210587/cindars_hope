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

        [MenuItem("CindarsHope/Advanced/Fix Tree Colliders", priority = 110)]
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
            EditorUtility.DisplayDialog("Fix Colliders",
                $"Done. {total} collider(s) updated (trees + player).\n\nSee Console for details.",
                "OK");
        }

        private static int FixCollidersInActiveScene(string sceneName)
        {
            int count = 0;

            // TreeNode objects (FarmScene)
            foreach (var tree in Object.FindObjectsByType<TreeNode>())
            {
                if (FitColliderToOpaqueSprite(tree.GetComponent<BoxCollider2D>(), tree.GetComponent<SpriteRenderer>(), tree.name))
                    count++;
            }

            // TownTree objects (no TreeNode component)
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (!sr.gameObject.name.StartsWith("TownTree_")) continue;
                    if (FitColliderToOpaqueSprite(sr.GetComponent<BoxCollider2D>(), sr, sr.gameObject.name))
                        count++;
                }
            }

            // Player physical collider
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name != "Player") continue;
                if (FitColliderToOpaqueSprite(root.GetComponent<BoxCollider2D>(), root.GetComponent<SpriteRenderer>(), "Player"))
                    count++;
            }

            Debug.Log($"FixColliders [{sceneName}]: {count} collider(s) updated.");
            return count;
        }

        private static bool FitColliderToOpaqueSprite(BoxCollider2D collider, SpriteRenderer sr, string label)
        {
            if (collider == null || sr == null)
            {
                Debug.LogWarning($"FixColliders: '{label}' missing BoxCollider2D or SpriteRenderer — skipped.");
                return false;
            }

            if (sr.sprite == null)
            {
                Debug.LogWarning($"FixColliders: '{label}' has no sprite — skipped.");
                return false;
            }

            if (!SpriteOpaqueBoundsUtility.TryGetOpaqueLocalBounds(sr.sprite, 0.05f, out var localBounds))
            {
                Debug.LogWarning($"FixColliders: '{label}' could not determine opaque bounds — skipped.");
                return false;
            }

            var newSize   = new Vector2(localBounds.size.x, localBounds.size.y);
            var newOffset = new Vector2(localBounds.center.x, localBounds.center.y);

            if (collider.size == newSize && collider.offset == newOffset)
                return false;

            Undo.RecordObject(collider, "Fix Collider to Sprite");
            collider.size   = newSize;
            collider.offset = newOffset;
            EditorUtility.SetDirty(collider);
            Debug.Log($"  '{label}': size={newSize} offset={newOffset}");
            return true;
        }
    }
}
