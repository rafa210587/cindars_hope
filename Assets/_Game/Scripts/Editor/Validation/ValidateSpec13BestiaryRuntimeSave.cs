using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CindarsHope.Combat;
using CindarsHope.Editor.EnemyTaxonomy;
using CindarsHope.Enemy;
using CindarsHope.Save;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateSpec13BestiaryRuntimeSave
    {
        [MenuItem("CindarsHope/Archive/Validation/Validate SPEC 13E - Bestiary Runtime Save")]
        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            ValidateRuntimeTypes(errors, passed);
            ValidateSaveDtos(errors, passed);
            ValidateEvents(errors, passed);
            ValidateAssets(errors, warnings, passed);
            ValidateForbiddenRuntimeSearch(errors, passed);

            Debug.Log($"[SPEC 13E Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13E] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13E] Validation FAILED with {errors.Count} error(s).");
        }

        private static void ValidateRuntimeTypes(List<string> errors, List<string> passed)
        {
            if (typeof(BestiaryManager) != null) passed.Add("BestiaryManager exists.");
            else errors.Add("BestiaryManager missing.");

            if (typeof(EnemyBestiaryEntrySO) != null) passed.Add("EnemyBestiaryEntrySO exists.");
            else errors.Add("EnemyBestiaryEntrySO missing.");

            var capture = typeof(BestiaryManager).GetMethod("CaptureSaveData", BindingFlags.Public | BindingFlags.Instance);
            var restore = typeof(BestiaryManager).GetMethod("RestoreFromSaveData", BindingFlags.Public | BindingFlags.Instance);
            if (capture != null && capture.ReturnType == typeof(BestiarySaveData)) passed.Add("BestiaryManager.CaptureSaveData returns BestiarySaveData.");
            else errors.Add("BestiaryManager.CaptureSaveData missing or wrong return type.");

            if (restore != null) passed.Add("BestiaryManager.RestoreFromSaveData exists.");
            else errors.Add("BestiaryManager.RestoreFromSaveData missing.");
        }

        private static void ValidateSaveDtos(List<string> errors, List<string> passed)
        {
            var bestiaryField = typeof(GameSaveData).GetField("Bestiary");
            if (bestiaryField != null && bestiaryField.FieldType == typeof(BestiarySaveData))
                passed.Add("GameSaveData has BestiarySaveData field.");
            else
                errors.Add("GameSaveData.Bestiary field missing.");

            ValidateSimpleFields(typeof(BestiarySaveData), errors, passed);
            ValidateSimpleFields(typeof(BestiaryEntrySaveData), errors, passed);
        }

        private static void ValidateEvents(List<string> errors, List<string> passed)
        {
            ValidateSimpleFields(typeof(CindarsHope.Core.Events.BestiaryEntryUpdatedEvent), errors, passed);
        }

        private static void ValidateAssets(List<string> errors, List<string> warnings, List<string> passed)
        {
            var enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var enemies = enemyGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.enemyId))
                .ToDictionary(e => e.enemyId, e => e);

            var entryGuids = AssetDatabase.FindAssets("t:EnemyBestiaryEntrySO");
            var entries = entryGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyBestiaryEntrySO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.BestiaryEntryId))
                .ToDictionary(e => e.BestiaryEntryId, e => e);

            passed.Add($"EnemyDataSO assets found: {enemies.Count}.");
            passed.Add($"EnemyBestiaryEntrySO assets found: {entries.Count}.");

            int requiredEnemyCount = 0;
            int requiredEntryCount = 0;

            foreach (var enemyId in GetRequiredEnemyIds())
            {
                if (!enemies.TryGetValue(enemyId, out var enemy))
                {
                    warnings.Add($"EnemyDataSO '{enemyId}' not found. Run CindarsHope > SPEC 13 > Create Roster 40 Enemy Data.");
                    continue;
                }

                requiredEnemyCount++;

                if (string.IsNullOrWhiteSpace(enemy.BestiaryEntryId))
                {
                    errors.Add($"EnemyDataSO '{enemyId}' has empty BestiaryEntryId.");
                    continue;
                }

                if (!entries.TryGetValue(enemy.BestiaryEntryId, out var entry))
                {
                    errors.Add($"EnemyDataSO '{enemyId}' references BestiaryEntryId '{enemy.BestiaryEntryId}' but no EnemyBestiaryEntrySO asset exists. Run CindarsHope > SPEC 13 > Create Bestiary Entries 40.");
                    continue;
                }

                if (entry.EnemyId != enemyId)
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' points to EnemyId '{entry.EnemyId}', expected '{enemyId}'.");

                if (string.IsNullOrWhiteSpace(entry.ShortDescription))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty ShortDescription.");
                if (string.IsNullOrWhiteSpace(entry.HabitatText))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty HabitatText.");
                if (string.IsNullOrWhiteSpace(entry.BehaviorHint))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty BehaviorHint.");
                if (string.IsNullOrWhiteSpace(entry.VulnerabilityHintLocked))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty VulnerabilityHintLocked.");
                if (string.IsNullOrWhiteSpace(entry.VulnerabilityHintDiscovered))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty VulnerabilityHintDiscovered.");
                if (string.IsNullOrWhiteSpace(entry.KnownDropsHint))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty KnownDropsHint.");
                if (string.IsNullOrWhiteSpace(entry.FactionText))
                    errors.Add($"Bestiary entry '{entry.BestiaryEntryId}' has empty FactionText.");

                requiredEntryCount++;
            }

            if (requiredEnemyCount >= 40) passed.Add($"Required EnemyDataSO present: {requiredEnemyCount}/40+.");
            else warnings.Add($"Required EnemyDataSO present: {requiredEnemyCount}/40. SPEC 13B roster assets are not fully generated.");

            if (requiredEntryCount >= 40) passed.Add($"Required bestiary entries linked: {requiredEntryCount}/40+.");
            else errors.Add($"Required bestiary entries linked: {requiredEntryCount}/40. Run bestiary generator after roster assets exist.");
        }

        private static void ValidateForbiddenRuntimeSearch(List<string> errors, List<string> passed)
        {
            string[] runtimeFiles =
            {
                "Assets/_Game/Scripts/Enemy/BestiaryManager.cs",
                "Assets/_Game/Scripts/Enemy/BestiarySaveData.cs",
                "Assets/_Game/Scripts/Enemy/EnemyBestiaryEntrySO.cs"
            };

            foreach (var file in runtimeFiles)
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(file);
                if (asset == null)
                {
                    errors.Add($"Runtime file missing: {file}");
                    continue;
                }

                if (asset.text.Contains("GameObject.Find") ||
                    asset.text.Contains("FindObjectOfType") ||
                    asset.text.Contains("FindObjectsByType"))
                {
                    errors.Add($"{file} contains prohibited runtime scene search.");
                }
            }

            passed.Add("SPEC 13E runtime files do not use prohibited scene searches.");
        }

        private static void ValidateSimpleFields(System.Type type, List<string> errors, List<string> passed)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsSimpleSaveField(field.FieldType))
                {
                    errors.Add($"{type.Name}.{field.Name} uses non-simple type {field.FieldType.Name}.");
                }
            }

            passed.Add($"{type.Name} uses simple public fields.");
        }

        private static bool IsSimpleSaveField(System.Type type)
        {
            if (type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool))
                return true;

            if (type.IsEnum)
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return IsSimpleSaveField(type.GetGenericArguments()[0]);

            if (type == typeof(BestiaryEntrySaveData))
                return true;

            return false;
        }

        private static readonly string[] RequiredEnemyIds =
        {
            "enemy_verdant_mite", "enemy_spore_crawler", "enemy_goblin_scrounger", "enemy_kobold_sentry",
            "enemy_rot_beetle", "enemy_pale_grub", "enemy_mushroom_puffball", "enemy_goblin_shaman",
            "enemy_kobold_trapmaster", "enemy_orc_grunt", "enemy_cave_leaper", "enemy_duergar_crossbowman",
            "enemy_burrowing_maggot", "enemy_fungal_spreader", "enemy_drow_skirmisher", "enemy_orc_berserker",
            "enemy_duergar_warder", "enemy_undead_shambler", "enemy_cultist_zealot", "enemy_gnome_tinkerer",
            "enemy_phase_stalker", "enemy_earth_elemental_minor", "enemy_cave_burrower_elite", "enemy_drow_witch",
            "enemy_undead_knight", "enemy_construct_sentry", "enemy_abyssal_hound", "enemy_corrupted_vine_horror",
            "enemy_ninrorin_phantom", "enemy_gnome_wargolem", "enemy_draconic_wyrmling", "enemy_abyssal_lurker",
            "enemy_corrupted_orc_champion", "enemy_earth_elemental_greater", "enemy_undead_lich_acolyte",
            "enemy_draconic_guardian", "enemy_goblin_warchief", "enemy_orc_warlord", "enemy_abyssal_gatekeeper",
            "enemy_cave_mite_queen", "enemy_fungal_patriarch", "enemy_duergar_artificer_lord", "enemy_void_herald",
            "enemy_draconic_elder",
        };

        private static string[] GetRequiredEnemyIds()
        {
            return CreateEnemySpawnEcologyData.BuildProfileDefinitions()
                .Select(p => p.EnemyId)
                .Distinct()
                .ToArray();
        }
    }
}
