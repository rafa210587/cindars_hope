#if UNITY_EDITOR

using System.Collections.Generic;
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Validation
{
    /// <summary>
    /// Validates projectile prefabs for component presence and configuration.
    /// </summary>
    public class ProjectilePrefabValidator : IProjectValidator
    {
        private const string ProjectilePrefabFolderPath = "Assets/_Game/Data/Combat/Prefabs";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";

        public string ValidatorId => "projectile_prefab_validator";
        public string DisplayName => "Projectile Prefab Validator";

        public ValidationReport Run()
        {
            var report = new ValidationReport();

            // Load databases
            var weaponDb = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            var spellDb = AssetDatabase.LoadAssetAtPath<SpellDatabaseSO>(SpellDatabasePath);

            // Validate prefabs in folder
            ValidateProjectilePrefabsInFolder(report);

            // Validate WeaponDataSO references (Bow weapons)
            if (weaponDb != null)
                ValidateWeaponPrefabReferences(weaponDb, report);

            // Validate SpellDataSO references (Fireball spells)
            if (spellDb != null)
                ValidateSpellPrefabReferences(spellDb, report);

            return report;
        }

        private void ValidateProjectilePrefabsInFolder(ValidationReport report)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { ProjectilePrefabFolderPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                ValidatePrefabComponents(prefab, path, report);
            }
        }

        private void ValidatePrefabComponents(GameObject prefab, string path, ValidationReport report)
        {
            // Check ProjectileBehaviour
            var projectileBehaviour = prefab.GetComponent<ProjectileBehaviour>();
            if (projectileBehaviour == null)
            {
                report.AddIssue("Projectile", "MISSING_PROJECTILE_BEHAVIOUR", ValidationSeverity.Error,
                    $"Projectile prefab '{prefab.name}' missing ProjectileBehaviour component.",
                    path, prefab.name, "Add ProjectileBehaviour component to prefab.");
                return; // If no behaviour, cannot validate further
            }

            // Check Rigidbody2D
            var rigidbody = prefab.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                report.AddIssue("Projectile", "MISSING_RIGIDBODY2D", ValidationSeverity.Error,
                    $"Projectile prefab '{prefab.name}' missing Rigidbody2D component.",
                    path, prefab.name, "Add Rigidbody2D component to prefab.");
            }

            // Check Collider2D
            var collider = prefab.GetComponent<Collider2D>();
            if (collider == null)
            {
                report.AddIssue("Projectile", "MISSING_COLLIDER2D", ValidationSeverity.Error,
                    $"Projectile prefab '{prefab.name}' missing Collider2D component.",
                    path, prefab.name, "Add Collider2D (CircleCollider2D or BoxCollider2D) component to prefab.");
            }
            else if (!collider.isTrigger)
            {
                report.AddIssue("Projectile", "COLLIDER_NOT_TRIGGER", ValidationSeverity.Error,
                    $"Projectile prefab '{prefab.name}' has Collider2D but isTrigger is false.",
                    path, prefab.name, "Set Collider2D.isTrigger = true.");
            }
        }

        private void ValidateWeaponPrefabReferences(WeaponDatabaseSO weaponDb, ValidationReport report)
        {
            var weapons = weaponDb.All;
            if (weapons == null || weapons.Count == 0)
                return;

            foreach (var weapon in weapons)
            {
                if (weapon == null || weapon.Type != WeaponType.Bow)
                    continue;

                if (weapon.ProjectilePrefab == null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(weapon);
                    report.AddIssue("Projectile", "BOW_MISSING_PROJECTILE", ValidationSeverity.Error,
                        $"Bow weapon '{weapon.DisplayName}' (ID: {weapon.Id}) has no ProjectilePrefab assigned.",
                        assetPath, weapon.DisplayName, "Assign ProjectilePrefab in WeaponDataSO inspector.");
                }
            }
        }

        private void ValidateSpellPrefabReferences(SpellDatabaseSO spellDb, ValidationReport report)
        {
            var spells = spellDb.All;
            if (spells == null || spells.Count == 0)
                return;

            foreach (var spell in spells)
            {
                if (spell == null || spell.Type != SpellType.Fireball)
                    continue;

                if (spell.ProjectilePrefab == null)
                {
                    var assetPath = AssetDatabase.GetAssetPath(spell);
                    report.AddIssue("Projectile", "FIREBALL_MISSING_PROJECTILE", ValidationSeverity.Error,
                        $"Fireball spell '{spell.SpellName}' (ID: {spell.Id}) has no ProjectilePrefab assigned.",
                        assetPath, spell.SpellName, "Assign ProjectilePrefab in SpellDataSO inspector.");
                }
            }
        }
    }
}

#endif
