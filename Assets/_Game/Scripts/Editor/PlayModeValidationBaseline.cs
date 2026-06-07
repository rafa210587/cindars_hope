#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 01.07: PlayMode Validation Baseline
    /// Defines where PlayMode tests live, when required, and how they route to final human validation by wave.
    /// </summary>
    public static class PlayModeValidationBaseline
    {
        // TEST LOCATIONS
        public const string PlayModeTestFolder = "Assets/_Game/Tests/PlayMode";
        public const string PlayModeTestAsmDef = "Assets/_Game/Tests/PlayMode/Tests.PlayMode.asmdef";
        public const string EditModeTestFolder = "Assets/_Game/Tests/EditMode";
        public const string EditModeTestAsmDef = "Assets/_Game/Tests/EditMode/Tests.EditMode.asmdef";

        // POLICY
        public const string PlayModeRequiredFor = "Runtime/gameplay specs: combat, UI/input, scene/prefab wiring, movement, cave procedural";
        public const string PlayModeNotRequiredFor = "Pure data specs, registries, DTOs, validators, migrations, deterministic logic (use EditMode)";
        public const string FinalHumanValidationRouting = "Deferred to FINAL_HUMAN_VALIDATION_BY_WAVE.md per wave/lote at end of implementation";
        public const string StatusCapBeforeFinalHuman = "UNITY_VALIDATED max; cannot claim PLAYMODE_VALIDATED without human checklist evidence";

        // CONSTRAINTS
        public static readonly string[] PlayModeTestConstraints = new[]
        {
            "Tests must live in Assets/_Game/Tests/PlayMode/",
            "Tests must reference PlayMode.asmdef only",
            "Tests must use NUnit [UnityTest] or Unity Test Runner patterns",
            "Tests must not require human intervention or real-time interaction",
            "Tests must not modify ProjectSettings or Packages",
            "Tests must not create scene changes persisted to disk",
            "Automated PlayMode is optional; manual scenario OK when automation blocks exist"
        };

        /// <summary>
        /// Query whether PlayMode test is required for a spec type.
        /// </summary>
        public static bool IsPlayModeRequired(string specType)
        {
            return specType.Contains("Runtime") || specType.Contains("Gameplay") || specType.Contains("UI") || specType.Contains("Combat");
        }

        /// <summary>
        /// Status cap for specs before final human validation.
        /// </summary>
        public static string MaxStatusBeforeFinalHumanValidation => "UNITY_VALIDATED";
    }
}
#endif
