using System.Collections.Generic;
using System.Linq;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14AFix2CombatFeedback
    {
        private const string EnemySpawnProfilesFolder = "Assets/_Game/Data/EnemySpawn/Profiles";
        private const string EnemySpawnPacksFolder = "Assets/_Game/Data/EnemySpawn/Packs";
        private const string EnemyFactionLocksFolder = "Assets/_Game/Data/EnemySpawn/FactionLocks";

        [MenuItem("CindarsHope/Archive/Validation/Validate SPEC 14A-FIX3 - Spawn Ecology and Combat Feedback")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            ValidateSpawnDensity(errors, passed);
            ValidatePlayerDamagedEvent(errors, passed);
            ValidateEnemyContactDamageLog(errors, passed);
            ValidateFloatingNumbersSubscription(errors, passed);
            ValidateSpawnEcologyData(errors, passed);
            ValidateMenuConsolidation(errors, passed);
            ValidateBiomeFix(errors, passed);
            ValidateForbiddenRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 14A-FIX3 Validation] PASSED: {passed.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 14A-FIX3] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 14A-FIX3] Validation FAILED with {errors.Count} error(s).");
        }

        private static void ValidateSpawnDensity(List<string> errors, List<string> passed)
        {
            var plannerAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs");
            if (plannerAsset == null)
            {
                errors.Add("CaveEnemySpawnPlanner.cs missing.");
                return;
            }

            if (plannerAsset.text.Contains("MinEnemiesPerLevel = 14"))
                passed.Add("CaveEnemySpawnPlanner.MinEnemiesPerLevel is 14.");
            else
                errors.Add("CaveEnemySpawnPlanner.MinEnemiesPerLevel must be 14 (was 12).");

            if (plannerAsset.text.Contains("MaxEnemiesPerLevel = 24"))
                passed.Add("CaveEnemySpawnPlanner.MaxEnemiesPerLevel is 24.");
            else
                errors.Add("CaveEnemySpawnPlanner.MaxEnemiesPerLevel must be 24 (was 20).");

            if (plannerAsset.text.Contains("DefaultMaxEnemies = 24"))
                passed.Add("CaveEnemySpawnPlanner.DefaultMaxEnemies is 24.");
            else
                errors.Add("CaveEnemySpawnPlanner.DefaultMaxEnemies must be 24 (was 20).");

            var materializerAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Game/Scripts/Cave/Runtime/CaveRuntimeMaterializer.cs");
            if (materializerAsset == null)
            {
                errors.Add("CaveRuntimeMaterializer.cs missing.");
                return;
            }

            if (materializerAsset.text.Contains("_maxEnemiesPerLevel = 24"))
                passed.Add("CaveRuntimeMaterializer._maxEnemiesPerLevel default is 24.");
            else
                errors.Add("CaveRuntimeMaterializer._maxEnemiesPerLevel default must be 24 (was 20).");
        }

        private static void ValidatePlayerDamagedEvent(List<string> errors, List<string> passed)
        {
            var eventType = typeof(PlayerDamagedEvent);
            if (eventType == null)
            {
                errors.Add("PlayerDamagedEvent type not found.");
                return;
            }

            passed.Add("PlayerDamagedEvent exists.");

            var damageAmount = eventType.GetProperty("DamageAmount");
            var worldPosition = eventType.GetProperty("WorldPosition");
            var sourceId = eventType.GetProperty("SourceId");
            var sourceName = eventType.GetProperty("SourceName");

            if (damageAmount != null) passed.Add("PlayerDamagedEvent.DamageAmount exists.");
            else errors.Add("PlayerDamagedEvent.DamageAmount missing.");

            if (worldPosition != null) passed.Add("PlayerDamagedEvent.WorldPosition exists.");
            else errors.Add("PlayerDamagedEvent.WorldPosition missing.");

            if (sourceId != null) passed.Add("PlayerDamagedEvent.SourceId exists.");
            else errors.Add("PlayerDamagedEvent.SourceId missing.");

            if (sourceName != null) passed.Add("PlayerDamagedEvent.SourceName exists.");
            else errors.Add("PlayerDamagedEvent.SourceName missing.");
        }

        private static void ValidateEnemyContactDamageLog(List<string> errors, List<string> passed)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Game/Scripts/Combat/EnemyContactDamage.cs");
            if (asset == null)
            {
                errors.Add("EnemyContactDamage.cs missing.");
                return;
            }

            if (asset.text.Contains("CombatLog: EnemyContactDamage") && asset.text.Contains("SourceName=") && asset.text.Contains("SourceEnemyId="))
                passed.Add("EnemyContactDamage log includes SourceName and SourceEnemyId.");
            else
                errors.Add("EnemyContactDamage log must include SourceName and SourceEnemyId.");

            if (asset.text.Contains("PlayerDamagedEvent"))
                passed.Add("EnemyContactDamage publishes PlayerDamagedEvent.");
            else
                errors.Add("EnemyContactDamage must publish PlayerDamagedEvent.");

            if (asset.text.Contains("GameEventBus.Publish"))
                passed.Add("EnemyContactDamage uses GameEventBus.Publish.");
            else
                errors.Add("EnemyContactDamage must use GameEventBus.Publish for PlayerDamagedEvent.");
        }

        private static void ValidateFloatingNumbersSubscription(List<string> errors, List<string> passed)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs");
            if (asset == null)
            {
                errors.Add("FloatingDamageNumberDisplayer.cs missing.");
                return;
            }

            if (asset.text.Contains("Subscribe<DamageAppliedEvent>"))
                passed.Add("FloatingDamageNumberDisplayer subscribes to DamageAppliedEvent.");
            else
                errors.Add("FloatingDamageNumberDisplayer must subscribe to DamageAppliedEvent.");

            if (asset.text.Contains("Subscribe<PlayerDamagedEvent>"))
                passed.Add("FloatingDamageNumberDisplayer subscribes to PlayerDamagedEvent.");
            else
                errors.Add("FloatingDamageNumberDisplayer must subscribe to PlayerDamagedEvent.");

            if (asset.text.Contains("Unsubscribe<PlayerDamagedEvent>"))
                passed.Add("FloatingDamageNumberDisplayer unsubscribes from PlayerDamagedEvent on disable.");
            else
                errors.Add("FloatingDamageNumberDisplayer must unsubscribe from PlayerDamagedEvent in OnDisable.");

            if (asset.text.Contains("Color.red"))
                passed.Add("FloatingDamageNumberDisplayer uses red color for player damage numbers.");
            else
                errors.Add("FloatingDamageNumberDisplayer must use Color.red for player damage numbers.");
        }

        private static void ValidateForbiddenRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] files =
            {
                "Assets/_Game/Scripts/Combat/EnemyContactDamage.cs",
                "Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs",
                "Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs"
            };

            foreach (var file in files)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                if (asset == null)
                {
                    errors.Add($"Runtime file missing: {file}");
                    continue;
                }

                if (asset.text.Contains("GameObject.Find(") || asset.text.Contains("FindObjectOfType(") || asset.text.Contains("FindObjectsByType("))
                    errors.Add($"{file} contains prohibited runtime scene search.");
            }

            passed.Add("SPEC 14A-FIX2 runtime files do not use prohibited scene searches.");
        }

        private static void ValidateSpawnEcologyData(List<string> errors, List<string> passed)
        {
            var packGuids = AssetDatabase.FindAssets("t:EnemySpawnPackSO", new[] { EnemySpawnPacksFolder });
            if (packGuids.Length == 0)
            {
                errors.Add($"No EnemySpawnPackSO assets found in {EnemySpawnPacksFolder}.");
                return;
            }
            passed.Add($"Found {packGuids.Length} EnemySpawnPackSO asset(s).");

            bool hasLowBand = false, hasMidBand = false;
            int enabledPacksBelowMin = 0;
            foreach (var guid in packGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var pack = AssetDatabase.LoadAssetAtPath<EnemySpawnPackSO>(path);
                if (pack == null) continue;
                if (pack.CaveLevelMax <= 10) hasLowBand = true;
                if (pack.CaveLevelMin <= 25 && pack.CaveLevelMax >= 11) hasMidBand = true;
                if (pack.IsEnabled && pack.MaxTotalEnemies < 8) enabledPacksBelowMin++;
            }

            if (hasLowBand) passed.Add("EnemySpawnPackSO: at least one pack covers level band 1-10 (stone/low).");
            else errors.Add("EnemySpawnPackSO: no enabled pack covers levels 1-10. Add a low-band pack.");

            if (hasMidBand) passed.Add("EnemySpawnPackSO: at least one pack covers level band 11-25 (fungal/mid).");
            else errors.Add("EnemySpawnPackSO: no enabled pack covers levels 11-25. Add a mid-band pack.");

            if (enabledPacksBelowMin == 0) passed.Add("All enabled packs have MaxTotalEnemies >= 8.");
            else errors.Add($"{enabledPacksBelowMin} enabled pack(s) have MaxTotalEnemies < 8. Increase to at least 8 to meet density targets.");

            var profileGuids = AssetDatabase.FindAssets("t:EnemySpawnProfileSO", new[] { EnemySpawnProfilesFolder });
            if (profileGuids.Length == 0)
                errors.Add($"No EnemySpawnProfileSO assets found in {EnemySpawnProfilesFolder}.");
            else
                passed.Add($"Found {profileGuids.Length} EnemySpawnProfileSO asset(s).");
        }

        private static void ValidateMenuConsolidation(List<string> errors, List<string> passed)
        {
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/_Game/Scripts/Editor" });
            var violations = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".cs")) continue;
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset == null || !asset.text.Contains("\"Cindar's Hope/")) continue;
                violations.Add(path);
            }

            if (violations.Count == 0)
                passed.Add("No editor file uses deprecated 'Cindar's Hope/' menu root — all consolidated under 'CindarsHope/'.");
            else
                foreach (var v in violations)
                    errors.Add($"Editor file still uses 'Cindar's Hope/' MenuItem: {v}. Rename to 'CindarsHope/'.");
        }

        private static void ValidateBiomeFix(List<string> errors, List<string> passed)
        {
            var plannerAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Game/Scripts/Cave/Runtime/CaveEnemySpawnPlanner.cs");
            if (plannerAsset == null)
            {
                errors.Add("CaveEnemySpawnPlanner.cs missing — cannot validate biome fix.");
                return;
            }

            if (plannerAsset.text.Contains("BuildBiomeTags"))
                passed.Add("CaveEnemySpawnPlanner.BuildBiomeTags method exists.");
            else
            {
                errors.Add("CaveEnemySpawnPlanner.BuildBiomeTags not found — biome multi-tag fix missing.");
                return;
            }

            if (plannerAsset.text.Contains("normalizedTag") && plannerAsset.text.Contains("levelTag"))
                passed.Add("CaveEnemySpawnPlanner.BuildBiomeTags derives both levelTag and normalizedTag for multi-biome coverage (level 15 fix).");
            else
                errors.Add("CaveEnemySpawnPlanner.BuildBiomeTags must produce both levelTag (from level range) and normalizedTag (from BiomeId) to fix level-15 zero-enemy bug.");

            if (plannerAsset.text.Contains("NormalizeBiomeTag"))
                passed.Add("CaveEnemySpawnPlanner.NormalizeBiomeTag used within BuildBiomeTags.");
            else
                errors.Add("CaveEnemySpawnPlanner.NormalizeBiomeTag not referenced — biome normalization path may be missing.");

            if (plannerAsset.text.Contains("pass * 13337") || plannerAsset.text.Contains("pass*13337"))
                passed.Add("CaveEnemySpawnPlanner uses deterministic multi-pass seed offset (pass * 13337).");
            else
                errors.Add("CaveEnemySpawnPlanner multi-pass loop must use deterministic seed offset (levelSeed + pass * 13337) for FASE9F stable-run contract.");
        }
    }
}
