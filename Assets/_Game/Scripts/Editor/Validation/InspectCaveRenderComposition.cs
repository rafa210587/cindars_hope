using System.Text;
using CindarsHope.Cave.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Read-only evidence for D15; does not regenerate, hide or recolor any object.</summary>
    public static class InspectCaveRenderComposition
    {
        public static string InspectActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            var text = new StringBuilder("Cave render inspection scene=" + scene.name + " playing=" + Application.isPlaying + "\n");
            if (scene.name != "CaveScene") throw new System.InvalidOperationException("Inspect the active CaveScene.");
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var materializer in root.GetComponentsInChildren<CaveRuntimeMaterializer>(true))
                {
                    var serialized = new SerializedObject(materializer);
                    text.AppendLine("floorPrefab=" + serialized.FindProperty("_floorTilePrefab").objectReferenceValue +
                        " wallPrefab=" + serialized.FindProperty("_wallTilePrefab").objectReferenceValue);
                }
                foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                    if (renderer.bounds.size.x < 3f && renderer.bounds.size.y < 3f &&
                        !renderer.name.Contains("FloorTile_") && !renderer.name.Contains("WallTile_")) continue;
                    var sprite = renderer.sprite;
                    var material = renderer.sharedMaterial;
                    text.AppendLine(renderer.name + " bounds=" + renderer.bounds + " color=" + renderer.color +
                        " sort=" + renderer.sortingLayerName + "/" + renderer.sortingOrder +
                        " sprite=" + (sprite != null ? AssetDatabase.GetAssetPath(sprite) : "NULL") +
                        " texture=" + (sprite != null ? sprite.texture.name : "NULL") +
                        " shader=" + (material != null && material.shader != null ? material.shader.name : "NULL") +
                        " materialTexture=" + (material != null && material.mainTexture != null ? material.mainTexture.name : "NULL"));
                }
            }
            string result = text.ToString();
            Debug.Log(result);
            return result;
        }
    }
}
