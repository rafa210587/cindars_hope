using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core.Events;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec14AFix2CombatFeedback
    {
        [MenuItem("CindarsHope/Validation/Validate SPEC 14A-FIX2 - Spawn Density and Combat Feedback")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var passed = new List<string>();

            ValidateSpawnDensity(errors, passed);
            ValidatePlayerDamagedEvent(errors, passed);
            ValidateEnemyContactDamageLog(errors, passed);
            ValidateFloatingNumbersSubscription(errors, passed);
            ValidateForbiddenRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 14A-FIX2 Validation] PASSED: {passed.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 14A-FIX2] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 14A-FIX2] Validation FAILED with {errors.Count} error(s).");
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
    }
}
