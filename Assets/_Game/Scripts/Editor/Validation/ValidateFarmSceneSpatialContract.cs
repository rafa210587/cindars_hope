#if UNITY_EDITOR

using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Validates that the generated FarmScene materializes the canonical spatial contract.</summary>
    public static class ValidateFarmSceneSpatialContract
    {
        private const string FarmScenePath = "Assets/_Game/Scenes/FarmScene.unity";

        [MenuItem("CindarsHope/Validar Layout Espacial FarmScene")]
        public static void Validate()
        {
            var errors = 0;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(FarmScenePath) == null)
            {
                Debug.LogError("ValidateFarmSceneSpatialContract: FarmScene.unity was not found.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Additive);
            try
            {
                var ids = new HashSet<string>();
                foreach (var footprint in FarmSceneSpatialContract.All)
                {
                    if (!ids.Add(footprint.Id) || !IsValidPolygon(footprint) || !IsInBounds(footprint))
                    {
                        errors++;
                        Debug.LogError("ValidateFarmSceneSpatialContract: invalid footprint " + footprint.Id);
                    }
                }

                errors += RequireCollider(scene, "MountainCollider", FarmSceneSpatialContract.Mountain);
                errors += RequireCollider(scene, "RiverSeg_N", FarmSceneSpatialContract.River);
                // Lake collision remains deferred to the following polygonal-materialization spec.
                errors += RequireObject(scene, "Bridge_01", FarmSceneSpatialContract.Bridge);
                errors += RequireObject(scene, "CaveEntrance", FarmSceneSpatialContract.CaveMouth);
                errors += RequireObject(scene, "Portal_Farm_To_Town", FarmSceneSpatialContract.TownExit);

                var spawn = FindByName(scene, "Spawn_farm_default");
                if (spawn != null && IsBlockedPosition(spawn.transform.position))
                {
                    errors++;
                    Debug.LogError("ValidateFarmSceneSpatialContract: spawn is inside a solid or water footprint.");
                }

                var bridge = FindByName(scene, "Bridge_01");
                if (bridge != null && bridge.GetComponent<Collider2D>() != null)
                {
                    errors++;
                    Debug.LogError("ValidateFarmSceneSpatialContract: bridge must remain walkable.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            if (errors == 0)
            {
                Debug.Log("ValidateFarmSceneSpatialContract: 0 error(s)");
            }
            else
            {
                Debug.LogError("ValidateFarmSceneSpatialContract: " + errors + " error(s)");
            }
        }

        private static bool IsValidPolygon(FarmSceneFootprint footprint) => footprint.Polygon != null && footprint.Polygon.Count >= 3;

        private static bool IsInBounds(FarmSceneFootprint footprint)
        {
            foreach (var point in footprint.Polygon)
            {
                if (point.x < FarmLevel1LayoutContract.MinX || point.x > FarmLevel1LayoutContract.MaxX ||
                    point.y < FarmLevel1LayoutContract.MinY || point.y > FarmLevel1LayoutContract.MaxY)
                    return false;
            }
            return true;
        }

        private static int RequireCollider(Scene scene, string objectName, string footprintId)
        {
            var obj = FindByName(scene, objectName);
            if (obj != null && obj.GetComponent<Collider2D>() != null) return 0;
            Debug.LogError("ValidateFarmSceneSpatialContract: missing collider for " + footprintId + " (" + objectName + ").");
            return 1;
        }

        private static int RequireObject(Scene scene, string objectName, string footprintId)
        {
            if (FindByName(scene, objectName) != null) return 0;
            Debug.LogError("ValidateFarmSceneSpatialContract: missing object for " + footprintId + " (" + objectName + ").");
            return 1;
        }

        private static GameObject FindByName(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = FindRecursive(root.transform, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindRecursive(Transform transform, string name)
        {
            if (transform.name == name) return transform.gameObject;
            foreach (Transform child in transform)
            {
                var found = FindRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static bool IsBlockedPosition(Vector3 position)
        {
            foreach (var footprint in FarmSceneSpatialContract.All)
            {
                if (!footprint.BlocksMovement) continue;
                if (footprint.Bounds.Contains(position)) return true;
            }
            return false;
        }
    }
}
#endif
