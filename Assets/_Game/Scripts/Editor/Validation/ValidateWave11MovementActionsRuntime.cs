using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateWave11MovementActionsRuntime
    {
        private static readonly string[] RuntimeFiles =
        {
            "Assets/_Game/Scripts/Player/Movement/PlayerMovementActionRuntimeBootstrap.cs",
            "Assets/_Game/Scripts/Player/Movement/PlayerMovementActionInput.cs",
            "Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs",
            "Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs",
            "Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs",
            "Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs",
            "Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs",
        };

        [MenuItem("CindarsHope/Validate/Validate WAVE11 Movement Actions Runtime")]
        public static void Run()
        {
            var errors = Validate();
            if (errors.Count > 0)
            {
                foreach (var e in errors) Debug.LogError(e);
                throw new InvalidOperationException($"ValidateWave11MovementActionsRuntime failed with {errors.Count} issue(s).");
            }

            Debug.Log("ValidateWave11MovementActionsRuntime PASS.");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            foreach (var file in RuntimeFiles)
            {
                if (!File.Exists(file))
                    errors.Add($"Missing runtime file: {file}");
            }

            // Bootstrap must resolve PlayerController, not PlayerManager directly
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerMovementActionRuntimeBootstrap.cs", errors,
                ("ResolvePlayerController", "Bootstrap must use ResolvePlayerController(), not attach to PlayerManager.gameObject directly."),
                ("GetComponent<PlayerController>", "Bootstrap must check for PlayerController on the resolved object."),
                ("GetComponentInChildren<PlayerController>", "Bootstrap must search children for PlayerController."),
                ("Rigidbody2D", "Bootstrap must validate Rigidbody2D before attaching controllers."),
                ("PlayerMovementActionBootstrap] Resolved PlayerController", "Bootstrap must log successful attach to the real PlayerController."));

            // Dash: canonical values and input helper
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs", errors,
                ("WasDashPressed", "Dash must use PlayerMovementActionInput.WasDashPressed()."),
                ("GetMoveDirectionHeld", "Dash must use PlayerMovementActionInput.GetMoveDirectionHeld()."),
                ("4.0f", "Dash distance must be 4.0f (Combat Core base top)."),
                ("TrySpendStamina", "Dash must spend stamina."),
                ("PlayerActionFeedbackEvent", "Dash must publish feedback."),
                ("Dash requested direction=", "Dash must log direction, distance, and target object."));

            // Dodge: canonical values
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs", errors,
                ("1.8f", "Dodge distance must be 1.8f (Combat Core base top)."),
                ("0.32f", "Dodge duration must be 0.32f."),
                ("TrySpendStamina", "Dodge must spend stamina."),
                ("PlayerActionFeedbackEvent", "Dodge must publish feedback."),
                ("Dodge requested direction=", "Dodge must log direction, distance, and target object."));

            // DoubleTap: correct arrow mapping (DownArrow→down, LeftArrow→left)
            ValidateFile("Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs", errors,
                ("DoubleTapWindow = 0.25f", "Double tap window must be 0.25f."),
                ("Vector2.down, Vector2.left, Vector2.right", "Arrow mapping must be: Up=up, Down=down, Left=left, Right=right (index 4-7)."));

            // Block: canonical values + input helper
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs", errors,
                ("IsBlockHeld", "Block must use PlayerMovementActionInput.IsBlockHeld()."),
                ("0.45f", "Block slow multiplier must be 0.45f (35%-55% range mid-value)."),
                ("18f", "Block stamina drain must be 18f/s (Combat Core canonical)."),
                ("TrySpendStamina", "Block must drain or check stamina."),
                ("PlayerActionFeedbackEvent", "Block must publish feedback."),
                ("Block started speedMultiplier=", "Block must log start with speed value."),
                ("Block stopped speedMultiplier restored=", "Block must log stop with restored value."));

            // Resolver: WaitForFixedUpdate + self-collision ignore
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs", errors,
                ("WaitForFixedUpdate", "Resolver must use WaitForFixedUpdate to sync with physics."),
                ("IsBeingDisplaced = true", "Resolver must set IsBeingDisplaced on PlayerController."),
                ("ShouldIgnoreHit", "Resolver must use ShouldIgnoreHit() to filter self-collision."),
                ("attachedRigidbody == _rigidbody", "Resolver must ignore hits sharing the same Rigidbody2D."),
                ("MovePosition", "Resolver must call Rigidbody2D.MovePosition."),
                ("COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER", "Resolver must document no-collider debt."));

            // Input helper: both paths present
            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerMovementActionInput.cs", errors,
                ("WasDashPressed", "Input helper must expose WasDashPressed()."),
                ("IsBlockHeld", "Input helper must expose IsBlockHeld()."),
                ("GetMoveDirectionHeld", "Input helper must expose GetMoveDirectionHeld()."),
                ("ENABLE_INPUT_SYSTEM", "Input helper must have #if ENABLE_INPUT_SYSTEM guard."));

            ValidateNoActiveSlotReferences(errors);
            return errors;
        }

        private static void ValidateFile(string path, List<string> errors, params (string Text, string Message)[] requirements)
        {
            if (!File.Exists(path)) return;
            var text = File.ReadAllText(path);
            foreach (var (txt, msg) in requirements)
            {
                if (!text.Contains(txt))
                    errors.Add($"{path}: {msg}");
            }
        }

        private static void ValidateNoActiveSlotReferences(List<string> errors)
        {
            foreach (var file in RuntimeFiles)
            {
                if (!File.Exists(file)) continue;
                var text = File.ReadAllText(file);
                if (text.Contains("ActiveSkill", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("ActiveSlot", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("SkillTreeState", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"{file}: movement actions must not use active slots.");
                }
            }
        }
    }
}
