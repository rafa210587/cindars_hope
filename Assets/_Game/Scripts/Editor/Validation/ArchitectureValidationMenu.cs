#if UNITY_EDITOR

using CindarsHope.Editor.Validation;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entry for running architecture validators.
    /// </summary>
    public static class ArchitectureValidationMenu
    {
        public static void RunArchitectureValidation()
        {
            ProjectValidationRunner.RunValidators(
                new ValidateFarmLevel1LayoutContract(),
                new CombatDatabaseValidator(),
                new ValidateFarmScaleContract(),
                new ProjectilePrefabValidator());
        }
    }
}

#endif
