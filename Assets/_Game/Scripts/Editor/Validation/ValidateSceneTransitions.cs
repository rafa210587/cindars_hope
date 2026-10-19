using System;
using System.Collections.Generic;
using CindarsHope.EditorTools.Validation;
using CindarsHope.Foundation;
using CindarsHope.SceneManagement;
using CindarsHope.Cave.Runtime;
using UnityEditor;
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
            var destinations = new List<(string scene, string anchor, string source)>();
            var sceneAnchors = new HashSet<string>();

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
                        foreach (var point in root.GetComponentsInChildren<SceneSpawnPoint>(true))
                        {
                            if (string.IsNullOrWhiteSpace(point.SpawnId))
                                AddError(report, "EMPTY_ANCHOR_ID", "SpawnId is empty.", scene.path, point.name);
                            else
                            {
                                if (!sceneAnchors.Add(scene.name + ":" + point.SpawnId))
                                    AddError(report, "DUPLICATE_ANCHOR", point.SpawnId, scene.path, point.name);
                                foundAnchors.Add(point.SpawnId);
                                // Legacy aliases retain expected-ID coverage without resolving a portal's literal target.
                                foundAnchors.Add(point.SpawnId.StartsWith("spawn_") ? point.SpawnId : "spawn_" + point.SpawnId);
                            }
                        }
                        foreach (var portal in root.GetComponentsInChildren<ScenePortal>(true))
                        {
                            string destination = string.IsNullOrWhiteSpace(portal.TargetSceneName)
                                ? System.IO.Path.GetFileNameWithoutExtension(portal.TargetScenePath) : portal.TargetSceneName;
                            if (string.IsNullOrWhiteSpace(destination))
                                AddError(report, "EMPTY_DESTINATION", "Portal destination is empty.", scene.path, portal.name);
                            if (string.IsNullOrWhiteSpace(portal.TargetSpawnId))
                                AddError(report, "EMPTY_TARGET_ANCHOR", "Portal target is empty.", scene.path, portal.name);
                            destinations.Add((destination, portal.TargetSpawnId, portal.name));
                            if (scene.name == SceneId.Farm && destination == SceneId.Town) foundGates.Add(SceneId.GateFarmTownExit);
                            if (scene.name == SceneId.Town && destination == SceneId.Farm) foundGates.Add(SceneId.GateTownFarmExit);
                            if (scene.name == SceneId.Cave && destination == SceneId.Farm) foundGates.Add(SceneId.GateCaveFarmExit);
                        }
                        foreach (var entrance in root.GetComponentsInChildren<CaveEntranceInteractable>(true))
                        {
                            var serialized = new SerializedObject(entrance);
                            string gateId = serialized.FindProperty("_transitionGateId").stringValue;
                            string destination = serialized.FindProperty("_targetCaveScene").stringValue;
                            string target = serialized.FindProperty("_targetSpawnId").stringValue;
                            if (string.IsNullOrWhiteSpace(gateId)) AddError(report, "EMPTY_GATE_ID", "Cave gate id is empty.", scene.path, entrance.name);
                            else if (!foundGates.Add(gateId)) AddError(report, "DUPLICATE_GATE", gateId, scene.path, entrance.name);
                            if (string.IsNullOrWhiteSpace(destination)) AddError(report, "EMPTY_DESTINATION", "Cave destination is empty.", scene.path, entrance.name);
                            if (string.IsNullOrWhiteSpace(target)) AddError(report, "EMPTY_TARGET_ANCHOR", "Cave target is empty.", scene.path, entrance.name);
                            destinations.Add((destination, target, entrance.name));
                        }
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

            // Legacy portals resolve the literal SpawnId, not the normalized reporting alias.
            foreach (var destination in destinations)
                if (!sceneAnchors.Contains(destination.scene + ":" + destination.anchor) &&
                    !HasModernAnchor(scenes, destination.scene, destination.anchor))
                    AddError(report, "UNRESOLVED_TARGET_ANCHOR", destination.scene + ":" + destination.anchor, "", destination.source);

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

        private static bool HasModernAnchor(IReadOnlyList<Scene> scenes, string sceneName, string id)
        {
            if (scenes == null) return false;
            foreach (var scene in scenes)
                if (scene.IsValid() && scene.isLoaded && scene.name == sceneName)
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var anchor in root.GetComponentsInChildren<SceneSpawnAnchor>(true))
                            if (anchor.SpawnAnchorId == id) return true;
            return false;
        }

        private static void AddError(ValidationReport report, string code, string message,
            string path = "", string objectName = "")
        {
            report.AddIssue("SceneTransitions", code, ValidationSeverity.Error, message, path, objectName);
        }
    }
}
