#if UNITY_EDITOR

using UnityEditor;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entries for combat system validators.
    /// </summary>
    public static class CombatValidationMenu
    {
        [MenuItem("CindarsHope/Advanced/Validate Projectile Prefabs", priority = 102)]
        public static void ValidateProjectilePrefabs()
        {
            var validator = new ProjectilePrefabValidator();
            ProjectValidationRunner.RunValidators(validator);
        }
    }
}

#endif
