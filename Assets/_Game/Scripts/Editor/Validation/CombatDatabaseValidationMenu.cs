#if UNITY_EDITOR

using UnityEditor;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Menu entry for combat database validators.
    /// </summary>
    public static class CombatDatabaseValidationMenu
    {
        [MenuItem("CindarsHope/Validate/Combat/Validate Combat Databases")]
        public static void ValidateCombatDatabases()
        {
            var validator = new CombatDatabaseValidator();
            ProjectValidationRunner.RunValidators(validator);
        }
    }
}

#endif
