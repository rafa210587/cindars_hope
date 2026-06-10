#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// WAVE_INTEGRATION_14 — Crafting Station + Processing Jobs
    /// Validates that all required code contracts and documentation are in place.
    ///
    /// Menu: CindarsHope/Validate/Validate WAVE14 Crafting Processing Runtime
    /// </summary>
    public static class ValidateCraftingProcessingRuntimeBinding
    {
        private const string ScriptRoot = "Assets/_Game/Scripts";
        private const string DocsRoot = "docs/validation";

        [MenuItem("CindarsHope/Validate/Validate WAVE14 Crafting Processing Runtime")]
        public static void ValidateAll()
        {
            Debug.Log("[ValidateCraftingProcessingRuntimeBinding] Starting WAVE14 validation...");

            var pass = true;

            // --- Code contracts ---
            pass &= RequireFile($"{ScriptRoot}/Craft/CraftingPoint.cs",
                "CraftingPoint (IInteractable station entry point)");
            pass &= RequireFile($"{ScriptRoot}/Craft/CraftingRuntime.cs",
                "CraftingRuntime (MonoBehaviour crafting coordinator)");
            pass &= RequireFile($"{ScriptRoot}/Craft/CraftingStation.cs",
                "CraftingStation (validate/consume/craft/collect/cancel)");
            pass &= RequireFile($"{ScriptRoot}/Craft/CraftingJob.cs",
                "CraftingJob (processing job with save/load)");
            pass &= RequireFile($"{ScriptRoot}/Craft/CraftingStationRuntimeBootstrap.cs",
                "CraftingStationRuntimeBootstrap (WAVE14 runtime wiring)");
            pass &= RequireFile($"{ScriptRoot}/Craft/Data/RecipeDataSO.cs",
                "RecipeDataSO (recipe ScriptableObject)");
            pass &= RequireFile($"{ScriptRoot}/Craft/Data/RecipeDatabaseSO.cs",
                "RecipeDatabaseSO (recipe registry SO)");
            pass &= RequireFile($"{ScriptRoot}/Craft/Data/WorkshopType.cs",
                "WorkshopType enum");
            pass &= RequireFile($"{ScriptRoot}/UI/Crafting/CraftingModal.cs",
                "CraftingModal (UI layer)");
            pass &= RequireFile($"{ScriptRoot}/UI/Crafting/CraftingMenuViewModel.cs",
                "CraftingMenuViewModel (UI view model)");

            // --- ProcessingJob Collected/idempotency guard ---
            pass &= RequireContentInFile($"{ScriptRoot}/Craft/CraftingJob.cs",
                "Collected",
                "CraftingJob must have Collected state (ProcessingJobState.Collected)");

            // --- No scene search at runtime in bootstrap ---
            pass &= RequireNoForbiddenPatternInFile($"{ScriptRoot}/Craft/CraftingStationRuntimeBootstrap.cs",
                "FindObjectOfType",
                "CraftingStationRuntimeBootstrap must not use FindObjectOfType at runtime");

            // --- Documentation ---
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_CRAFTING_PROCESSING_DECISION.md",
                "WAVE14 decision document");
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_CRAFTING_PROCESSING_REPORT.md",
                "WAVE14 execution report");
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_RECIPE_STATION_AUTHORING_MODEL.md",
                "WAVE14 authoring model");
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_RECIPE_CATALOG_SMOKE_TEST.md",
                "WAVE14 recipe smoke test catalog");
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_PROCESSING_JOB_LIFECYCLE.md",
                "WAVE14 processing job lifecycle doc");
            pass &= RequireFile($"{DocsRoot}/WAVE_INTEGRATION_14_HUMAN_PLAYMODE_CHECKLIST.md",
                "WAVE14 human Play Mode checklist");

            Debug.Log(pass
                ? "[ValidateCraftingProcessingRuntimeBinding] WAVE14 validation: ALL PASS"
                : "[ValidateCraftingProcessingRuntimeBinding] WAVE14 validation: FAILED — see errors above");
        }

        private static bool RequireFile(string relativePath, string label)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath).Replace('/', Path.DirectorySeparatorChar);
            if (File.Exists(fullPath))
            {
                Debug.Log($"  PASS: {label} — {relativePath}");
                return true;
            }

            Debug.LogError($"  FAIL: {label} — file not found: {relativePath}");
            return false;
        }

        private static bool RequireContentInFile(string relativePath, string searchTerm, string label)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath).Replace('/', Path.DirectorySeparatorChar);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"  FAIL: {label} — file not found: {relativePath}");
                return false;
            }

            var content = File.ReadAllText(fullPath);
            if (content.Contains(searchTerm))
            {
                Debug.Log($"  PASS: {label}");
                return true;
            }

            Debug.LogError($"  FAIL: {label} — '{searchTerm}' not found in {relativePath}");
            return false;
        }

        private static bool RequireNoForbiddenPatternInFile(string relativePath, string forbiddenTerm, string label)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath).Replace('/', Path.DirectorySeparatorChar);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"  SKIP: {label} — file not found: {relativePath}");
                return true; // File missing is reported by RequireFile; don't double-fail
            }

            var lines = File.ReadAllLines(fullPath);
            var violations = lines
                .Where(l => l.Contains(forbiddenTerm) && !l.TrimStart().StartsWith("//") && !l.TrimStart().StartsWith("*"))
                .ToArray();

            if (violations.Length == 0)
            {
                Debug.Log($"  PASS: {label}");
                return true;
            }

            Debug.LogError($"  FAIL: {label} — found '{forbiddenTerm}' on {violations.Length} line(s) in {relativePath}");
            return false;
        }
    }
}
#endif
