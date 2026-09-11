using System;
using System.Collections.Generic;
using CindarsHope.EditorTools.Validation;
using CindarsHope.Foundation;
using CindarsHope.World.Scenes;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Inspects transition components only in the requested scenes. Acquisition owns
    /// temporary scene loads; inspection never saves or closes the caller's scenes.
    /// </summary>
    public static class ValidateSceneTransitions
    {
        private static readonly string[] TargetScenes =
        {
            "Assets/_Game/Scenes/FarmScene.unity",
            "Assets/_Game/Scenes/TownScene.unity",
            "Assets/_Game/Scenes/CaveScene.unity",
        };

        private static readonly string[] ExpectedGates =
        {
            SceneId.GateFarmTownExit, SceneId.GateFarmCaveEntrance,
            SceneId.GateTownFarmExit, SceneId.GateCaveFarmExit,
        };

        private static readonly string[] ExpectedAnchors =
        {
            SceneId.SpawnFarmFromTown, SceneId.SpawnFarmFromCave,
            SceneId.SpawnTownFromFarm, SceneId.SpawnCaveFromFarm,
        };

        public static void ValidateAll()
        {
            var report = ValidateScenePaths(TargetScenes);
            var summary = report.GetSummary(nameof(ValidateSceneTransitions));
            if (report.Passed) Debug.Log(summary);
            else Debug.LogError(summary);
        }

        public static ValidationReport ValidateScenePaths(IReadOnlyList<string> scenePaths)
        {
            var acquisition = new ValidationReport();
            var originalScene = SceneManager.GetActiveScene();
            if (!originalScene.IsValid() || !originalScene.isLoaded)
            {
                acquisition.IsConfigured = false;
                AddError(acquisition, "NO_ACTIVE_SCENE", "An active editor scene is required for additive inspection.");
                return acquisition;
            }

            var selectedScenes = new List<Scene>();
            var openedScenes = new List<Scene>();
            try
            {
                if (scenePaths != null)
                {
                    foreach (string path in scenePaths)
                    {
                        if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                        {
                            AddError(acquisition, "SCENE_NOT_FOUND", "Required scene is missing.", path);
                            continue;
                        }

                        try
                        {
                            var scene = SceneManager.GetSceneByPath(path);
                            if (!scene.isLoaded)
                            {
                                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                                openedScenes.Add(scene);
                            }
                            selectedScenes.Add(scene);
                        }
                        catch (Exception exception)
                        {
                            AddError(acquisition, "SCENE_LOAD_FAILED", exception.Message, path);
                        }
                    }
                }

                var report = ValidateLoadedScenes(selectedScenes);
                report.Issues.AddRange(acquisition.Issues);
                return report;
            }
            finally
            {
                if (originalScene.IsValid() && originalScene.isLoaded)
                {
                    SceneManager.SetActiveScene(originalScene);
                }

                for (int index = openedScenes.Count - 1; index >= 0; index--)
                {
                    var scene = openedScenes[index];
                    if (scene.IsValid() && scene.isLoaded)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }

        public static ValidationReport ValidateLoadedScenes(IReadOnlyList<Scene> scenes)
        {
            var report = new ValidationReport { IsConfigured = scenes != null && scenes.Count > 0 };
            var foundGates = new HashSet<string>();
            var foundAnchors = new HashSet<string>();
            var inspectedScenes = new HashSet<Scene>();

            if (scenes != null)
            {
                foreach (var scene in scenes)
                {
                    if (!scene.IsValid() || !scene.isLoaded)
                    {
                        AddError(report, "SCENE_NOT_LOADED", "A requested scene is not loaded.");
                        continue;
                    }
                    if (!inspectedScenes.Add(scene)) continue;

                    foreach (var root in scene.GetRootGameObjects())
                    {
                        foreach (var gate in root.GetComponentsInChildren<SceneTransitionGate>(true))
                        {
                            if (string.IsNullOrWhiteSpace(gate.TransitionId))
                                AddError(report, "EMPTY_GATE_ID", "TransitionId is empty.", scene.path, gate.name);
                            else if (!foundGates.Add(gate.TransitionId))
                                AddError(report, "DUPLICATE_GATE", gate.TransitionId, scene.path, gate.name);

                            if (string.IsNullOrWhiteSpace(gate.ToSceneId))
                                AddError(report, "EMPTY_DESTINATION", "ToSceneId is empty.", scene.path, gate.name);
                            if (string.IsNullOrWhiteSpace(gate.TargetSpawnAnchorId))
                                AddError(report, "EMPTY_TARGET_ANCHOR", "TargetSpawnAnchorId is empty.", scene.path, gate.name);
                        }

                        foreach (var anchor in root.GetComponentsInChildren<SceneSpawnAnchor>(true))
                        {
                            if (string.IsNullOrWhiteSpace(anchor.SpawnAnchorId))
                                AddError(report, "EMPTY_ANCHOR_ID", "SpawnAnchorId is empty.", scene.path, anchor.name);
                            else if (!foundAnchors.Add(anchor.SpawnAnchorId))
                                AddError(report, "DUPLICATE_ANCHOR", anchor.SpawnAnchorId, scene.path, anchor.name);
                        }
                    }
                }
            }

            foreach (string id in ExpectedGates)
            {
                if (!foundGates.Contains(id)) AddError(report, "MISSING_GATE", id);
            }
            foreach (string id in ExpectedAnchors)
            {
                if (!foundAnchors.Contains(id)) AddError(report, "MISSING_ANCHOR", id);
            }
            return report;
        }

        private static void AddError(ValidationReport report, string code, string message,
            string path = "", string objectName = "")
        {
            report.AddIssue("SceneTransitions", code, ValidationSeverity.Error, message, path, objectName);
        }
    }
}
