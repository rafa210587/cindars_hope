using System.Collections.Generic;
using CindarsHope.World.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// Editor-only validator for WAVE_INTEGRATION_13 scene transition contracts.
    /// Validates that SceneTransitionGate and SceneSpawnAnchor GameObjects
    /// have non-empty IDs and match the canonical transition map.
    ///
    /// Run via: CindarsHope/Validation/Validate Scene Transitions
    /// </summary>
    public static class ValidateSceneTransitions
    {
        private static readonly string[] TargetScenes = new[]
        {
            "Assets/_Game/Scenes/FarmScene.unity",
            "Assets/_Game/Scenes/TownScene.unity",
            "Assets/_Game/Scenes/CaveScene.unity",
        };

        private static readonly string[] ExpectedGates = new[]
        {
            SceneId.GateFarmTownExit,
            SceneId.GateFarmCaveEntrance,
            SceneId.GateTownFarmExit,
            SceneId.GateCaveFarmExit,
        };

        private static readonly string[] ExpectedAnchors = new[]
        {
            SceneId.SpawnFarmFromTown,
            SceneId.SpawnFarmFromCave,
            SceneId.SpawnTownFromFarm,
            SceneId.SpawnCaveFromFarm,
        };

        [MenuItem("CindarsHope/Validation/Validate Scene Transitions")]
        public static void ValidateAll()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== ValidateSceneTransitions — WAVE_INTEGRATION_13 ===");

            var foundGates = new HashSet<string>();
            var foundAnchors = new HashSet<string>();
            var errors = new List<string>();

            foreach (var scenePath in TargetScenes)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    report.AppendLine($"SCENE_NOT_FOUND: {scenePath}");
                    continue;
                }

                report.AppendLine($"\n--- Scene: {scenePath} ---");

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                var gates = Object.FindObjectsByType<SceneTransitionGate>(FindObjectsInactive.Exclude);
                var anchors = Object.FindObjectsByType<SceneSpawnAnchor>(FindObjectsInactive.Exclude);

                foreach (var gate in gates)
                {
                    if (string.IsNullOrWhiteSpace(gate.TransitionId))
                    {
                        errors.Add($"{scenePath}: SceneTransitionGate on '{gate.name}' has empty TransitionId.");
                    }
                    if (string.IsNullOrWhiteSpace(gate.ToSceneId))
                    {
                        errors.Add($"{scenePath}: SceneTransitionGate on '{gate.name}' has empty ToSceneId.");
                    }
                    if (string.IsNullOrWhiteSpace(gate.TargetSpawnAnchorId))
                    {
                        errors.Add($"{scenePath}: SceneTransitionGate on '{gate.name}' has empty TargetSpawnAnchorId.");
                    }
                    foundGates.Add(gate.TransitionId);
                    report.AppendLine($"  GATE: {gate.TransitionId} → {gate.ToSceneId} @{gate.TargetSpawnAnchorId}");
                }

                foreach (var anchor in anchors)
                {
                    if (string.IsNullOrWhiteSpace(anchor.SpawnAnchorId))
                    {
                        errors.Add($"{scenePath}: SceneSpawnAnchor on '{anchor.name}' has empty SpawnAnchorId.");
                    }
                    foundAnchors.Add(anchor.SpawnAnchorId);
                    report.AppendLine($"  ANCHOR: {anchor.SpawnAnchorId} at {anchor.Position}");
                }

                EditorSceneManager.CloseScene(scene, true);
            }

            // Check all expected gates are present
            foreach (var expectedGate in ExpectedGates)
            {
                if (!foundGates.Contains(expectedGate))
                {
                    report.AppendLine($"MISSING_GATE: {expectedGate} not found in any scene.");
                }
                else
                {
                    report.AppendLine($"GATE_OK: {expectedGate}");
                }
            }

            // Check all expected anchors are present
            foreach (var expectedAnchor in ExpectedAnchors)
            {
                if (!foundAnchors.Contains(expectedAnchor))
                {
                    report.AppendLine($"MISSING_ANCHOR: {expectedAnchor} not found in any scene.");
                }
                else
                {
                    report.AppendLine($"ANCHOR_OK: {expectedAnchor}");
                }
            }

            if (errors.Count > 0)
            {
                report.AppendLine("\n=== ERRORS ===");
                foreach (var err in errors)
                {
                    report.AppendLine($"ERROR: {err}");
                    Debug.LogError($"[ValidateSceneTransitions] {err}");
                }
                report.AppendLine($"\nRESULT: FAIL — {errors.Count} error(s) found.");
            }
            else
            {
                report.AppendLine("\nRESULT: PASS — No config errors found.");
            }

            Debug.Log(report.ToString());
        }
    }
}
