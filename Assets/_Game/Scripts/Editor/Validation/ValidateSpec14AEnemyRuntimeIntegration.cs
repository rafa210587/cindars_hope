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

        [MenuItem("CindarsHope/Validate/Enemy Runtime Integration")]
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

            // SPEC 14A-FIX6: additional strict checks
            ValidateSpawnProfilesCount(errors, passed);
            ValidateNoStaleBossGateProgress(errors, passed);
            ValidateSizeProfileScaleVariation(errors, passed);
            ValidateDatabasesPopulated(errors, passed);

            Debug.Log($"SPEC 14A Runtime Integration Validator: {passed.Count} passed, {errors.Count} errors.");
            foreach (var p in passed) Debug.Log($"  PASS: {p}");
            foreach (var e in errors) Debug.LogError($"  FAIL: {e}");

            if (errors.Count == 0)
                EditorUtility.DisplayDialog("Enemy Runtime Integration Validation", $"All {passed.Count} checks passed.", "OK");
            else
                EditorUtility.DisplayDialog("Enemy Runtime Integration Validation",
                    $"{errors.Count} error(s). Check Console for details.", "OK");
        }

        private static void ValidateSpawnProfilesCount(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemySpawnProfileSO", new[] { "Assets/_Game/Data/EnemySpawn/Profiles" });
            if (guids.Length < 60)
                errors.Add($"EnemySpawnProfileSO count = {guids.Length}. Expected >= 60 (40 bands 1-5 + 10 deep + 10 void). Run Regenerate All Enemy Data.");
            else
                passed.Add($"EnemySpawnProfileSO count = {guids.Length} (>= 60).");
        }

        private static void ValidateNoStaleBossGateProgress(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemySpawnProfileSO", new[] { "Assets/_Game/Data/EnemySpawn/Profiles" });
            int stale = 0;
            var samples = new List<string>();
            foreach (var guid in guids)
            {
                var so = AssetDatabase.LoadAssetAtPath<EnemySpawnProfileSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (so != null && !string.IsNullOrWhiteSpace(so.RequiredBossGateProgress))
                {
                    stale++;
                    if (samples.Count < 5) samples.Add($"{so.EnemyId}->{so.RequiredBossGateProgress}");
                }
            }
            if (stale > 0)
                errors.Add($"{stale} EnemySpawnProfileSO still have RequiredBossGateProgress (SPEC 14A-FIX5 removed this). Samples: {string.Join(", ", samples)}. Re-run Regenerate All Enemy Data.");
            else
                passed.Add("No EnemySpawnProfileSO carries stale RequiredBossGateProgress.");
        }

        private static void ValidateSizeProfileScaleVariation(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:EnemySizeProfileSO", new[] { "Assets/_Game/Data/Enemies/SizeProfiles" });
            if (guids.Length == 0)
            {
                errors.Add("No EnemySizeProfileSO assets found. SizeProfile cannot be applied at runtime.");
                return;
            }
            int allOne = 0;
            int tinyAtOne = 0;
            int largeOrAboveAtOne = 0;
            foreach (var guid in guids)
            {
                var so = AssetDatabase.LoadAssetAtPath<EnemySizeProfileSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (so == null) continue;
                if (Mathf.Approximately(so.SpriteScale, 1.0f)) allOne++;
                if (so.SizeClass == EnemySizeClass.Tiny && Mathf.Approximately(so.SpriteScale, 1.0f)) tinyAtOne++;
                if (so.SizeClass >= EnemySizeClass.Large && Mathf.Approximately(so.SpriteScale, 1.0f)) largeOrAboveAtOne++;
            }
            if (tinyAtOne > 0) errors.Add($"{tinyAtOne} SizeProfile(s) with SizeClass=Tiny have SpriteScale=1.00 (should be smaller, e.g. 0.65).");
            if (largeOrAboveAtOne > 0) errors.Add($"{largeOrAboveAtOne} SizeProfile(s) with SizeClass>=Large have SpriteScale=1.00 (should be larger).");
            if (allOne == guids.Length) errors.Add($"All {allOne} SizeProfile(s) have SpriteScale=1.00. Run Regenerate All Enemy Data to refresh scales.");
            if (tinyAtOne == 0 && largeOrAboveAtOne == 0)
                passed.Add($"SizeProfile scales vary correctly across {guids.Length} profiles.");
        }

        private static void ValidateDatabasesPopulated(List<string> errors, List<string> passed)
        {
            CheckDb<EnemyActionSetDatabaseSO>("Assets/_Game/Data/Combat/EnemyActionSetDatabase.asset", 50, errors, passed);
            CheckDb<EnemyActionDatabaseSO>("Assets/_Game/Data/Combat/EnemyActionDatabase.asset", 60, errors, passed);
            CheckDb<EnemyMovementProfileDatabaseSO>("Assets/_Game/Data/Combat/EnemyMovementProfileDatabase.asset", 8, errors, passed);
            CheckDb<EnemyVulnerabilityProfileDatabaseSO>("Assets/_Game/Data/Combat/EnemyVulnerabilityProfileDatabase.asset", 8, errors, passed);
            CheckDb<EnemySizeProfileDatabaseSO>("Assets/_Game/Data/Combat/EnemySizeProfileDatabase.asset", 6, errors, passed);
            CheckDb<EnemyTelegraphProfileDatabaseSO>("Assets/_Game/Data/Combat/EnemyTelegraphProfileDatabase.asset", 6, errors, passed);
        }

        private static void CheckDb<T>(string assetPath, int minCount, List<string> errors, List<string> passed) where T : ScriptableObject
        {
            var db = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (db == null) { errors.Add($"{typeof(T).Name} asset missing at {assetPath}. Run Regenerate All Enemy Data."); return; }
            var so = new SerializedObject(db);
            var prop = so.FindProperty("_items");
            int count = prop != null && prop.isArray ? prop.arraySize : 0;
            if (count < minCount) errors.Add($"{typeof(T).Name} populated with {count} items (expected >= {minCount}). Run Regenerate All Enemy Data.");
            else passed.Add($"{typeof(T).Name} populated with {count} items.");
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
