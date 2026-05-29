using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14AEnemyRuntimeIntegration
    {
        private const string EnemyDataFolder = "Assets/_Game/Data/Enemies";
        private const string MovementProfileFolder = "Assets/_Game/Data/Combat/MovementProfiles";
        private const string VulnerabilityProfileFolder = "Assets/_Game/Data/Combat/VulnerabilityProfiles";
        private const string SizeProfileFolder = "Assets/_Game/Data/Combat/SizeProfiles";
        private const string FactionLocksFolder = "Assets/_Game/Data/EnemySpawn/FactionLocks";

        [MenuItem("CindarsHope/Validation/Validate SPEC 14A - Enemy Runtime Integration")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            ValidateFactionLockProgression(errors, passed);
            ValidateMovementProfileDatabaseExists(errors, passed);
            ValidateVulnerabilityProfileDatabaseExists(errors, passed);
            ValidateSizeProfileDatabaseExists(errors, passed);
            ValidateEnemyDataProfileLinks(errors, passed);
            ValidateNoBannedRuntimeSearch(errors, passed);

            Debug.Log($"SPEC 14A-FIX4 Validator: {passed.Count} passed, {errors.Count} errors.");
            foreach (var p in passed) Debug.Log($"  PASS: {p}");
            foreach (var e in errors) Debug.LogError($"  FAIL: {e}");

            if (errors.Count == 0)
                EditorUtility.DisplayDialog("SPEC 14A-FIX4 Validation", $"All {passed.Count} checks passed.", "OK");
            else
                EditorUtility.DisplayDialog("SPEC 14A-FIX4 Validation",
                    $"{errors.Count} error(s). Check Console for details.", "OK");
        }

        private static void ValidateFactionLockProgression(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyFactionLockSO", new[] { FactionLocksFolder });
            if (guids.Length == 0)
            {
                errors.Add($"No EnemyFactionLockSO assets found in {FactionLocksFolder}.");
                return;
            }

            int levelGated = 0;
            int bossGated = 0;
            int defaultUnlocked = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<EnemyFactionLockSO>(path);
                if (so == null) continue;
                if (so.IsUnlockedByDefault) defaultUnlocked++;
                else if (so.RequiredCaveLevelMin > 0) levelGated++;
                else if (!string.IsNullOrEmpty(so.RequiredBossGateId)) bossGated++;
            }

            passed.Add($"FactionLocks: {guids.Length} total, {defaultUnlocked} default-unlocked, {levelGated} level-gated, {bossGated} boss-gated.");

            if (levelGated == 0 && bossGated == 0)
                errors.Add("No FactionLockSO with RequiredCaveLevelMin or RequiredBossGateId — higher band enemies can never unlock.");
            else
                passed.Add("Faction lock progression has level-gated or boss-gated locks.");
        }

        private static void ValidateMovementProfileDatabaseExists(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyMovementProfileDatabaseSO");
            if (guids.Length == 0)
                errors.Add("No EnemyMovementProfileDatabaseSO asset found. Wire one in the Cave scene.");
            else
                passed.Add($"EnemyMovementProfileDatabaseSO asset found ({guids.Length}).");
        }

        private static void ValidateVulnerabilityProfileDatabaseExists(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyVulnerabilityProfileDatabaseSO");
            if (guids.Length == 0)
                errors.Add("No EnemyVulnerabilityProfileDatabaseSO asset found. Wire one in the Cave scene.");
            else
                passed.Add($"EnemyVulnerabilityProfileDatabaseSO asset found ({guids.Length}).");
        }

        private static void ValidateSizeProfileDatabaseExists(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemySizeProfileDatabaseSO");
            if (guids.Length == 0)
                errors.Add("No EnemySizeProfileDatabaseSO asset found. Wire one in the Cave scene.");
            else
                passed.Add($"EnemySizeProfileDatabaseSO asset found ({guids.Length}).");
        }

        private static void ValidateEnemyDataProfileLinks(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { EnemyDataFolder });
            if (guids.Length == 0)
            {
                errors.Add($"No EnemyDataSO assets found in {EnemyDataFolder}.");
                return;
            }

            int missingMovement = 0;
            int missingVuln = 0;
            int missingSize = 0;
            var examples = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
                if (so == null) continue;
                bool bad = false;
                if (string.IsNullOrEmpty(so.MovementProfileId)) { missingMovement++; bad = true; }
                if (string.IsNullOrEmpty(so.VulnerabilityProfileId)) { missingVuln++; bad = true; }
                if (string.IsNullOrEmpty(so.SizeProfileId)) { missingSize++; bad = true; }
                if (bad && examples.Count < 5) examples.Add(so.enemyId ?? path);
            }

            passed.Add($"EnemyDataSO: {guids.Length} total.");
            if (missingMovement > 0)
                errors.Add($"{missingMovement} EnemyDataSO missing MovementProfileId. Examples: {string.Join(", ", examples)}");
            else
                passed.Add("All EnemyDataSO have MovementProfileId.");

            if (missingVuln > 0)
                errors.Add($"{missingVuln} EnemyDataSO missing VulnerabilityProfileId.");
            else
                passed.Add("All EnemyDataSO have VulnerabilityProfileId.");

            if (missingSize > 0)
                errors.Add($"{missingSize} EnemyDataSO missing SizeProfileId.");
            else
                passed.Add("All EnemyDataSO have SizeProfileId.");
        }

        private static void ValidateNoBannedRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] filesToCheck =
            {
                "Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs",
                "Assets/_Game/Scripts/Enemy/EnemyBrain.cs",
            };

            string[] banned = { "FindObjectOfType", "FindObjectsOfType", "FindObjectsByType", "GameObject.Find(" };
            var violations = new List<string>();

            foreach (var file in filesToCheck)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                if (asset == null) continue;
                foreach (var b in banned)
                {
                    if (asset.text.Contains(b))
                        violations.Add($"{file}: uses banned '{b}'");
                }
            }

            if (violations.Count == 0)
                passed.Add("No banned runtime global search found in key files.");
            else
                foreach (var v in violations)
                    errors.Add(v);
        }
    }
}
