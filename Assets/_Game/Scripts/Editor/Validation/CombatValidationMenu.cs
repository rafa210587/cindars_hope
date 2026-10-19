#if UNITY_EDITOR

using UnityEditor;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entries for combat system validators.
    /// </summary>
    public static class CombatValidationMenu
    {
        public static void ValidateProjectilePrefabs()
        {
            var validator = new ProjectilePrefabValidator();
            ProjectValidationRunner.RunValidators(validator);
        }
    }
}

#endif
