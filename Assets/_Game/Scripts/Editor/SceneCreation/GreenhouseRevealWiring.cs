using CindarsHope.Farm.Scene;
using CindarsHope.Farm.Watering;
using CindarsHope.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>Wires the existing reveal owner without changing the greenhouse footprint or art.</summary>
    public static class GreenhouseRevealWiring
    {
        public static RoofRevealController Configure(Transform greenhouse, SpriteRenderer exterior)
        {
            var reveal = GreenhouseRevealRuntime.Configure(greenhouse, exterior);
            var trigger = reveal.GetComponent<BoxCollider2D>();
            EditorUtility.SetDirty(reveal);
            EditorUtility.SetDirty(trigger);
            return reveal;
        }

        public static void RepairActiveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (Application.isPlaying || scene.name != "FarmScene")
                throw new System.InvalidOperationException("Open FarmScene outside Play Mode before repairing reveal.");
            foreach (var root in scene.GetRootGameObjects())
                foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                    if (renderer.name == "GreenhouseFloor" && renderer.transform.parent.name == "Greenhouse")
                    {
                        Configure(renderer.transform.parent, renderer);
                        EditorSceneManager.MarkSceneDirty(scene);
                        Debug.Log("Greenhouse reveal configured; scene not saved automatically.");
                        return;
                    }
            throw new System.InvalidOperationException("Authored greenhouse exterior was not found.");
        }
    }
}
