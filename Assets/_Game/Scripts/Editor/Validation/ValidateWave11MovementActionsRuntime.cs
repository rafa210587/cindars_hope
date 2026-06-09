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
                foreach (var error in errors)
                {
                    Debug.LogError(error);
                }

                throw new InvalidOperationException($"ValidateWave11MovementActionsRuntime failed with {errors.Count} issue(s).");
            }

            Debug.Log("ValidateWave11MovementActionsRuntime passed.");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            foreach (var file in RuntimeFiles)
            {
                if (!File.Exists(file))
                {
                    errors.Add($"Missing runtime file: {file}");
                }
            }

            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerDashController.cs", errors,
                ("Space", "Dash input must be Space + direction."),
                ("ReadDirectionalInput", "Dash must read a direction."),
                ("3.5f", "Dash distance must include base 3.5f."),
                ("TrySpendStamina", "Dash must spend stamina."),
                ("PlayerActionFeedbackEvent", "Dash must publish feedback."));

            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerDodgeController.cs", errors,
                ("double tap directional", "Dodge must document double tap directional input."),
                ("1.5f", "Dodge distance must include base 1.5f."),
                ("TrySpendStamina", "Dodge must spend stamina."),
                ("PlayerActionFeedbackEvent", "Dodge must publish feedback."));

            ValidateFile("Assets/_Game/Scripts/Player/Movement/DirectionalDoubleTapDetector.cs", errors,
                ("DoubleTapWindow = 0.25f", "Double tap detector must expose the base double tap window."),
                ("KeyCode.W", "Double tap detector must support WASD."),
                ("KeyCode.UpArrow", "Double tap detector must support arrows."));

            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs", errors,
                ("LeftShift", "Block input must be Left Shift."),
                ("_blockSlowMultiplier = 0.5f", "Block slow multiplier must be less than 1."),
                ("TrySpendStamina", "Block must drain or check stamina."),
                ("PlayerActionFeedbackEvent", "Block must publish feedback."));

            ValidateFile("Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs", errors,
                ("Rigidbody2D", "Movement resolver must support Rigidbody2D."),
                ("MovePosition", "Movement resolver must move the player."),
                ("Collider2D", "Movement resolver must support collision."),
                ("COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER", "Movement resolver must document no-collider debt."));

            ValidateNoActiveSlotReferences(errors);
            return errors;
        }

        private static void ValidateFile(string path, List<string> errors, params (string Text, string Message)[] requirements)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var text = File.ReadAllText(path);
            foreach (var requirement in requirements)
            {
                if (!text.Contains(requirement.Text))
                {
                    errors.Add($"{path}: {requirement.Message}");
                }
            }
        }

        private static void ValidateNoActiveSlotReferences(List<string> errors)
        {
            foreach (var file in RuntimeFiles)
            {
                if (!File.Exists(file))
                {
                    continue;
                }

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
