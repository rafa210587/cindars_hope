#if UNITY_EDITOR

using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Repair
{
    /// <summary>
    /// Repairs projectile prefab references in WeaponDataSO and SpellDataSO assets.
    /// This is a residual fix for validator failures where ProjectilePrefab field is null.
    /// </summary>
    public static class RepairProjectilePrefabReferences
    {
        private const string WeaponBowBasicPath = "Assets/_Game/Data/Combat/Weapons/weapon_bow_basic.asset";
        private const string ProjectileArrowPath = "Assets/_Game/Data/Combat/Prefabs/Projectile_Arrow.prefab";

        private const string SpellFireballPath = "Assets/_Game/Data/Combat/Spells/spell_fireball.asset";
        private const string ProjectileFireballPath = "Assets/_Game/Data/Combat/Prefabs/Projectile_Fireball.prefab";

        [MenuItem("CindarsHope/Repair/Combat/Repair Projectile Prefab References")]
        public static void Repair()
        {
            Debug.Log("RepairProjectilePrefabReferences: Starting repair...");

            bool allPassed = true;

            // Repair Bow
            allPassed &= RepairWeaponBowProjectilePrefab();

            // Repair Fireball
            allPassed &= RepairSpellFireballProjectilePrefab();

            if (allPassed)
            {
                Debug.Log("RepairProjectilePrefabReferences: All repairs completed successfully. PASS");
            }
            else
            {
                Debug.LogError("RepairProjectilePrefabReferences: Some repairs failed. See errors above.");
            }
        }

        private static bool RepairWeaponBowProjectilePrefab()
        {
            // Load weapon_bow_basic.asset
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponBowBasicPath);
            if (weapon == null)
            {
                Debug.LogError($"RepairProjectilePrefabReferences: Cannot load WeaponDataSO at {WeaponBowBasicPath}");
                return false;
            }

            // Load Projectile_Arrow.prefab
            var arrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectileArrowPath);
            if (arrowPrefab == null)
            {
                Debug.LogError($"RepairProjectilePrefabReferences: Cannot load GameObject at {ProjectileArrowPath}");
                return false;
            }

            // Check if already assigned (avoid unnecessary save)
            if (weapon.ProjectilePrefab == arrowPrefab)
            {
                Debug.Log($"RepairProjectilePrefabReferences: weapon_bow_basic.ProjectilePrefab already correct.");
                return true;
            }

            // Assign prefab
            weapon.ProjectilePrefab = arrowPrefab;
            EditorUtility.SetDirty(weapon);

            Debug.Log($"RepairProjectilePrefabReferences: Assigned Projectile_Arrow.prefab to weapon_bow_basic.ProjectilePrefab");

            return true;
        }

        private static bool RepairSpellFireballProjectilePrefab()
        {
            // Load spell_fireball.asset
            var spell = AssetDatabase.LoadAssetAtPath<SpellDataSO>(SpellFireballPath);
            if (spell == null)
            {
                Debug.LogError($"RepairProjectilePrefabReferences: Cannot load SpellDataSO at {SpellFireballPath}");
                return false;
            }

            // Load Projectile_Fireball.prefab
            var fireballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectileFireballPath);
            if (fireballPrefab == null)
            {
                Debug.LogError($"RepairProjectilePrefabReferences: Cannot load GameObject at {ProjectileFireballPath}");
                return false;
            }

            // Check if already assigned (avoid unnecessary save)
            if (spell.ProjectilePrefab == fireballPrefab)
            {
                Debug.Log($"RepairProjectilePrefabReferences: spell_fireball.ProjectilePrefab already correct.");
                return true;
            }

            // Assign prefab
            spell.ProjectilePrefab = fireballPrefab;
            EditorUtility.SetDirty(spell);

            Debug.Log($"RepairProjectilePrefabReferences: Assigned Projectile_Fireball.prefab to spell_fireball.ProjectilePrefab");

            return true;
        }

        /// <summary>
        /// Repair method callable via -executeMethod batchmode.
        /// Usage: Unity -projectPath . -executeMethod CindarsHope.EditorTools.Repair.RepairProjectilePrefabReferences.RepairViaCommandLine -quit
        /// </summary>
        public static void RepairViaCommandLine()
        {
            Repair();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("RepairProjectilePrefabReferences: Command line repair complete. Assets saved and refreshed.");
        }
    }
}

#endif
