#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// WAVE_INTEGRATION_16 — Cave Entrance + Cave Runtime Bridge
    /// Validates that all required code contracts and documentation are in place.
    ///
    /// Menu: CindarsHope/Validate/Validate WAVE16 Cave Entrance Runtime Bridge
    /// </summary>
    public static class ValidateWave16CaveEntranceRuntimeBridge
    {
        private const string ScriptRoot = "Assets/_Game/Scripts";
        private const string DocsRoot = "docs/validation";

        [MenuItem("CindarsHope/Validate/Validate WAVE16 Cave Entrance Runtime Bridge")]
        public static void ValidateAll()
        {
            Debug.Log("[ValidateWave16CaveEntranceRuntimeBridge] Starting WAVE16 validation...");

            var pass = true;

            // ----------------------------------------------------------------
            // Code contracts — WAVE16 new files
            // ----------------------------------------------------------------
            pass &= RequireFile(
                $"{ScriptRoot}/Cave/Runtime/CaveEntranceInteractable.cs",
                "CaveEntranceInteractable (IInteractable cave entry for FarmScene/TownScene)");

            pass &= RequireFile(
                $"{ScriptRoot}/Cave/Runtime/CaveRuntimeBridge.cs",
                "CaveRuntimeBridge (RuntimeInitializeOnLoadMethod bridge; WAVE13+cave runtime)");

            pass &= RequireFile(
                $"{ScriptRoot}/Core/Events/CaveRunStartedEvent.cs",
                "CaveRunStartedEvent (published on cave entry)");

            pass &= RequireFile(
                $"{ScriptRoot}/Core/Events/CaveExitedEvent.cs",
                "CaveExitedEvent (published on surface exit from cave)");

            // ----------------------------------------------------------------
            // Existing cave runtime — must NOT be duplicated
            // ----------------------------------------------------------------
            pass &= RequireFile(
                $"{ScriptRoot}/Cave/Runtime/CaveRunManager.cs",
                "CaveRunManager (existing cave run lifecycle manager — must be reused)");

            pass &= RequireFile(
                $"{ScriptRoot}/Cave/Runtime/CaveRuntimeState.cs",
                "CaveRuntimeState (existing run state — must be reused, not duplicated)");

            pass &= RequireFile(
                $"{ScriptRoot}/Cave/CaveExitPortal.cs",
                "CaveExitPortal (existing cave→surface/level exit — must be reused)");

            pass &= RequireFile(
                $"{ScriptRoot}/Cave/CaveLevelRuntimeController.cs",
                "CaveLevelRuntimeController (existing level controller — must be reused)");

            // ----------------------------------------------------------------
            // WAVE13 transition system — must be reused
            // ----------------------------------------------------------------
            pass &= RequireFile(
                $"{ScriptRoot}/World/Scenes/SceneTransitionRouter.cs",
                "SceneTransitionRouter (WAVE13 — must be used by CaveEntranceInteractable)");

            pass &= RequireFile(
                $"{ScriptRoot}/World/Scenes/SceneId.cs",
                "SceneId (WAVE13 stable IDs — used for cave spawn anchor IDs)");

            // ----------------------------------------------------------------
            // Anti-duplication: CaveEntranceInteractable must use SceneTransitionRouter
            // ----------------------------------------------------------------
            pass &= RequireContentInFile(
                $"{ScriptRoot}/Cave/Runtime/CaveEntranceInteractable.cs",
                "SceneTransitionRouter",
                "CaveEntranceInteractable must use SceneTransitionRouter (not SceneManager.LoadScene directly)");

            pass &= RequireContentInFile(
                $"{ScriptRoot}/Cave/Runtime/CaveEntranceInteractable.cs",
                "IInteractable",
                "CaveEntranceInteractable must implement IInteractable");

            // ----------------------------------------------------------------
            // Anti-pattern: CaveRuntimeBridge must NOT create new CaveRunManager
            // ----------------------------------------------------------------
            pass &= RequireNoForbiddenPatternInFile(
                $"{ScriptRoot}/Cave/Runtime/CaveRuntimeBridge.cs",
                "new CaveRunManager",
                "CaveRuntimeBridge must NOT instantiate a new CaveRunManager (reuse existing in CaveScene)");

            // ----------------------------------------------------------------
            // ADR-0005 compliance: CaveRunSeed must not be set on exit
            // ----------------------------------------------------------------
            pass &= RequireNoForbiddenPatternInFile(
                $"{ScriptRoot}/Cave/Runtime/CaveEntranceInteractable.cs",
                "GenerateNewRunSeed",
                "CaveEntranceInteractable must NOT call GenerateNewRunSeed (ADR-0005: seed changes only on death/debug)");

            // ----------------------------------------------------------------
            // Documentation
            // ----------------------------------------------------------------
            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_CAVE_ENTRANCE_REPORT.md",
                "WAVE16 main execution report");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_CAVE_ENTRANCE_DECISION.md",
                "WAVE16 decision document");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_CAVE_SCENE_AUDIT.md",
                "WAVE16 cave scene audit");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_CAVE_RUNTIME_BRIDGE_REPORT.md",
                "WAVE16 cave runtime bridge report");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_CAVE_ENTRANCE_ROUTE_MAP.md",
                "WAVE16 cave entrance route map");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_STATE_PRESERVATION_MATRIX.md",
                "WAVE16 state preservation matrix");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE16 human Play Mode checklist");

            pass &= RequireFile(
                $"{DocsRoot}/WAVE_INTEGRATION_16_HUMAN_UNITY_CAVE_WIRING_INSTRUCTIONS.md",
                "WAVE16 human Unity wiring instructions");

            // ----------------------------------------------------------------
            // Result
            // ----------------------------------------------------------------
            if (pass)
            {
                Debug.Log("[ValidateWave16CaveEntranceRuntimeBridge] ALL CHECKS PASSED. " +
                          "Status: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED");
            }
            else
            {
                Debug.LogError("[ValidateWave16CaveEntranceRuntimeBridge] SOME CHECKS FAILED. " +
                               "See errors above.");
            }
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static bool RequireFile(string path, string description)
        {
            if (File.Exists(path))
            {
                Debug.Log($"  [OK] {description}: {path}");
                return true;
            }

            Debug.LogError($"  [FAIL] {description}: MISSING at {path}");
            return false;
        }

        private static bool RequireContentInFile(string path, string pattern, string description)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"  [FAIL] Cannot check pattern — file missing: {path}");
                return false;
            }

            var content = File.ReadAllText(path);
            if (content.Contains(pattern))
            {
                Debug.Log($"  [OK] {description}");
                return true;
            }

            Debug.LogError($"  [FAIL] {description}: pattern '{pattern}' not found in {path}");
            return false;
        }

        private static bool RequireNoForbiddenPatternInFile(string path, string forbiddenPattern, string description)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"  [FAIL] Cannot check forbidden pattern — file missing: {path}");
                return false;
            }

            var content = File.ReadAllText(path);
            if (!content.Contains(forbiddenPattern))
            {
                Debug.Log($"  [OK] {description}");
                return true;
            }

            Debug.LogError($"  [FAIL] {description}: forbidden pattern '{forbiddenPattern}' found in {path}");
            return false;
        }
    }
}
#endif
