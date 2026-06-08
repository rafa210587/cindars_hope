#if UNITY_EDITOR

using UnityEditor;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entry for running architecture validators.
    /// </summary>
    public static class ArchitectureValidationMenu
    {
        [MenuItem("CindarsHope/Validate/Run Architecture Validators", priority = 44)]
        public static void RunArchitectureValidation()
        {
            // For now, run with no validators to establish the menu and infrastructure.
            // Validators will be registered in future specs (SPEC_02, SPEC_03, etc).
            ProjectValidationRunner.RunValidators();
        }
    }
}

#endif
