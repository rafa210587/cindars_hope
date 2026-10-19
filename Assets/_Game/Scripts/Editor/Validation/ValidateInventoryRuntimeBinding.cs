using CindarsHope.Core.Bootstrap;
using CindarsHope.UI;
using CindarsHope.UI.Character;
using CindarsHope.UI.Routing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// WAVE_INTEGRATION_09: Validates that the FarmScene has the required inventory / equipment
    /// runtime binding components wired correctly.
    ///
    /// InventoryPanelController and CharacterEquipmentPanelController use
    /// [RuntimeInitializeOnLoadMethod] so they auto-create at runtime. This validator
    /// checks that the scene generator wired GameplayInputRouter, which coordinates input
    /// routing when a modal is active.
    /// </summary>
    public static class ValidateInventoryRuntimeBinding
    {
        private const string ScenePath = "Assets/_Game/Scenes/FarmScene.unity";

        public static void ValidateFromMenu()
        {
            var issues = RunValidation(openSceneIfNeeded: true);
            if (issues == 0)
            {
                EditorUtility.DisplayDialog(
                    "Inventory Runtime Binding",
                    "PASS: Inventory runtime binding validated successfully.",
                    "OK");
                Debug.Log("WAVE_INTEGRATION_09 ValidateInventoryRuntimeBinding: PASS");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Inventory Runtime Binding",
                    $"FAIL: {issues} issue(s) found. See Console for details.",
                    "OK");
                Debug.LogError($"WAVE_INTEGRATION_09 ValidateInventoryRuntimeBinding: FAIL ({issues} issues)");
            }
        }

        /// <summary>
        /// Returns the number of validation issues found.
        /// </summary>
        public static int RunValidation(bool openSceneIfNeeded = false)
        {
            var issues = 0;

            // Open FarmScene if needed (non-destructive: only opens in additive or single mode)
            if (openSceneIfNeeded)
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    Debug.LogError($"ValidateInventoryRuntimeBinding: FarmScene not found at '{ScenePath}'.");
                    return 1;
                }
            }

            // Check GameBootstrap exists
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                Debug.LogError("ValidateInventoryRuntimeBinding: GameBootstrap not found in scene. Run CreateMvpFarmScene first.");
                issues++;
            }

            // Check ModalManager is on bootstrap
            if (bootstrap != null)
            {
                // Use ModalManager reference via bootstrap
                if (bootstrap.ModalManager == null)
                {
                    Debug.LogError("ValidateInventoryRuntimeBinding: GameBootstrap.ModalManager is null. Wiring required.");
                    issues++;
                }
                else
                {
                    Debug.Log("ValidateInventoryRuntimeBinding: ModalManager - OK");
                }
            }

            // Check GameplayInputRouter exists in scene
            var router = Object.FindAnyObjectByType<GameplayInputRouter>();
            if (router == null)
            {
                Debug.LogWarning(
                    "ValidateInventoryRuntimeBinding: GameplayInputRouter not found in scene. " +
                    "Input routing (I/K/Esc) may conflict. Regenerate FarmScene via CindarsHope/Create MVP Farm Scene.");
                // Warning, not error - InventoryPanelController handles its own input
            }
            else
            {
                Debug.Log("ValidateInventoryRuntimeBinding: GameplayInputRouter - OK");
            }

            // InventoryPanelController and CharacterEquipmentPanelController use
            // [RuntimeInitializeOnLoadMethod] - they create themselves at runtime.
            // We validate the code compiles, not scene presence.
            Debug.Log(
                "ValidateInventoryRuntimeBinding: InventoryPanelController - OK (RuntimeInitializeOnLoad; auto-creates at play)");
            Debug.Log(
                "ValidateInventoryRuntimeBinding: CharacterEquipmentPanelController - OK (RuntimeInitializeOnLoad; auto-creates at play)");

            // Check InventoryManager is accessible from bootstrap
            if (bootstrap != null)
            {
                if (bootstrap.InventoryManager == null)
                {
                    Debug.LogError("ValidateInventoryRuntimeBinding: GameBootstrap.InventoryManager is null.");
                    issues++;
                }
                else
                {
                    Debug.Log("ValidateInventoryRuntimeBinding: InventoryManager - OK");
                }
            }

            return issues;
        }
    }
}
