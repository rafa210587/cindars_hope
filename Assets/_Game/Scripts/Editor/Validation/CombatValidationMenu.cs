#if UNITY_EDITOR

using UnityEditor;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entries for combat system validators.
    /// </summary>
    public static class CombatValidationMenu
    {
        [MenuItem("CindarsHope/Validate/Validate Projectile Prefabs", priority = 46)]
        public static void ValidateProjectilePrefabs()
        {
            var validator = new ProjectilePrefabValidator();
            ProjectValidationRunner.RunValidators(validator);
        }
    }
}

#endif
