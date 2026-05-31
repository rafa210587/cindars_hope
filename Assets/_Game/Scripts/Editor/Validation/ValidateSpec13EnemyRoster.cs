using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Editor.EnemyTaxonomy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec13EnemyRoster
    {
        [MenuItem("CindarsHope/Advanced/Legacy/Validation/Validate SPEC 13B - Enemy Roster")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            var requiredEnemyIds = CreateEnemySpawnEcologyData.BuildProfileDefinitions()
                .Select(p => p.EnemyId)
                .ToHashSet();

            var allEnemies = AssetDatabase.FindAssets("t:EnemyDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var foundIds = allEnemies.Select(e => e.enemyId).ToHashSet();
            passed.Add($"Total EnemyDataSO assets found: {allEnemies.Count}.");

            foreach (var id in requiredEnemyIds)
            {
                if (foundIds.Contains(id))
                    passed.Add($"EnemyDataSO '{id}' found.");
                else
                    errors.Add($"Missing canonical EnemyDataSO '{id}'. Run CindarsHope > SPEC 13 > Create Roster 40 Enemy Data.");
            }

            var rosterCount = allEnemies.Count(e => requiredEnemyIds.Contains(e.enemyId));
            if (rosterCount >= 40)
                passed.Add($"Canonical roster count: {rosterCount}/40.");
            else
                errors.Add($"Canonical roster count: {rosterCount}/40.");

            foreach (var enemy in allEnemies.Where(e => requiredEnemyIds.Contains(e.enemyId)))
            {
                if (string.IsNullOrWhiteSpace(enemy.enemyId))
                    errors.Add($"{enemy.name}: enemyId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.DisplayName))
                    errors.Add($"{enemy.enemyId}: DisplayName is empty.");
                if (string.IsNullOrWhiteSpace(enemy.FactionId))
                    errors.Add($"{enemy.enemyId}: FactionId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.SizeProfileId))
                    errors.Add($"{enemy.enemyId}: SizeProfileId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.MovementProfileId))
                    errors.Add($"{enemy.enemyId}: MovementProfileId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.VulnerabilityProfileId))
                    errors.Add($"{enemy.enemyId}: VulnerabilityProfileId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.ActionSetId))
                    errors.Add($"{enemy.enemyId}: ActionSetId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.BestiaryEntryId))
                    errors.Add($"{enemy.enemyId}: BestiaryEntryId is empty.");
                if (string.IsNullOrWhiteSpace(enemy.PrimaryDamageTypeId))
                    warnings.Add($"{enemy.enemyId}: PrimaryDamageTypeId is empty.");
                if (enemy.maxHp <= 0)
                    errors.Add($"{enemy.enemyId}: maxHp must be > 0.");
                if (enemy.CaveBand < 1 || enemy.CaveBand > 6)
                    errors.Add($"{enemy.enemyId}: CaveBand={enemy.CaveBand} is out of range [1-6].");
            }

            var coveredFactions = allEnemies
                .Where(e => requiredEnemyIds.Contains(e.enemyId))
                .Select(e => e.FactionId)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .ToList();
            passed.Add($"Canonical factions covered: {coveredFactions.Count} ({string.Join(", ", coveredFactions)}).");

            Debug.Log($"[SPEC 13B Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13B] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13B] Validation FAILED with {errors.Count} error(s).");
        }
    }
}
