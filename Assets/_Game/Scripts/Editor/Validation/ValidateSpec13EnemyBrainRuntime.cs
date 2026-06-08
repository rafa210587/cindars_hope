using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Enemy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 13D validator — EnemyBrain runtime MVP.
    /// Run via: CindarsHope > Validation > Validate SPEC 13D - EnemyBrain Runtime
    /// </summary>
    public static class ValidateSpec13EnemyBrainRuntime
    {
        private static readonly string[] EightTestableEnemyIds =
        {
            "enemy_stone_rat",
            "enemy_blackroot_sprout",
            "enemy_kobold_scout",
            "enemy_spore_imp",
            "enemy_mycobulwark",
            "enemy_duergar_frostdelver",
            "enemy_orc_kaand_berserker",
            "enemy_gnome_gem_madcap",
        };

        private static readonly string[] RequiredActionSetIds =
        {
            "actionset_enemy_stone_rat",
            "actionset_enemy_blackroot_sprout",
            "actionset_enemy_kobold_scout",
            "actionset_enemy_spore_imp",
            "actionset_enemy_mycobulwark",
            "actionset_enemy_duergar_frostdelver",
            "actionset_enemy_orc_kaand_berserker",
            "actionset_enemy_gnome_gem_madcap",
        };

        [MenuItem("CindarsHope/Archive/Validation/Validate SPEC 13D - EnemyBrain Runtime")]
        public static void RunValidation()
        {
            var errors   = new List<string>();
            var warnings = new List<string>();
            var passed   = new List<string>();

            // ── 1. EnemyBrain script exists ───────────────────────────────────────
            var brainType = typeof(EnemyBrain);
            if (brainType != null)
                passed.Add("EnemyBrain MonoBehaviour found.");
            else
                errors.Add("EnemyBrain class not found — check compilation.");

            // ── 2. Helper types exist ─────────────────────────────────────────────
            if (typeof(EnemyActionRuntime) != null)
                passed.Add("EnemyActionRuntime class found.");
            else
                errors.Add("EnemyActionRuntime class not found.");

            if (typeof(EnemyVulnerabilityState) != null)
                passed.Add("EnemyVulnerabilityState MonoBehaviour found.");
            else
                errors.Add("EnemyVulnerabilityState class not found.");

            // ── 3. Database SO types exist ────────────────────────────────────────
            if (typeof(EnemyActionDatabaseSO) != null)
                passed.Add("EnemyActionDatabaseSO found.");
            else
                errors.Add("EnemyActionDatabaseSO not found.");

            if (typeof(EnemyActionSetDatabaseSO) != null)
                passed.Add("EnemyActionSetDatabaseSO found.");
            else
                errors.Add("EnemyActionSetDatabaseSO not found.");

            if (typeof(EnemyTelegraphProfileDatabaseSO) != null)
                passed.Add("EnemyTelegraphProfileDatabaseSO found.");
            else
                errors.Add("EnemyTelegraphProfileDatabaseSO not found.");

            // ── 4. EnemyBrain has the required SerializeField databases ───────────
            var brainFields = brainType?.GetFields(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool hasActionSetDb = brainFields?.Any(f => f.FieldType == typeof(EnemyActionSetDatabaseSO)) == true;
            bool hasActionDb    = brainFields?.Any(f => f.FieldType == typeof(EnemyActionDatabaseSO)) == true;
            bool hasTelegraphDb = brainFields?.Any(f => f.FieldType == typeof(EnemyTelegraphProfileDatabaseSO)) == true;
            bool hasMovementProfile = brainFields?.Any(f => f.FieldType == typeof(EnemyMovementProfileSO)) == true;

            if (hasActionSetDb) passed.Add("EnemyBrain has EnemyActionSetDatabaseSO field.");
            else errors.Add("EnemyBrain missing EnemyActionSetDatabaseSO field.");

            if (hasActionDb) passed.Add("EnemyBrain has EnemyActionDatabaseSO field.");
            else errors.Add("EnemyBrain missing EnemyActionDatabaseSO field.");

            if (hasTelegraphDb) passed.Add("EnemyBrain has EnemyTelegraphProfileDatabaseSO field.");
            else errors.Add("EnemyBrain missing EnemyTelegraphProfileDatabaseSO field.");

            if (hasMovementProfile) passed.Add("EnemyBrain has EnemyMovementProfileSO field.");
            else errors.Add("EnemyBrain missing EnemyMovementProfileSO field.");

            // ── 5. EnemyBrainState enum has all required states ───────────────────
            var stateValues = System.Enum.GetNames(typeof(EnemyBrainState));
            string[] requiredStates = { "Idle", "Patrol", "Alert", "Chase", "AttackWindup", "AttackRecover", "Stunned", "Dead", "Kite", "GuardHold" };
            foreach (var s in requiredStates)
            {
                if (stateValues.Contains(s))
                    passed.Add($"EnemyBrainState.{s} exists.");
                else
                    errors.Add($"EnemyBrainState.{s} missing.");
            }

            // ── 6. Vulnerability events exist ─────────────────────────────────────
            if (typeof(CindarsHope.Core.Events.EnemyVulnerabilityStartedEvent) != null)
                passed.Add("EnemyVulnerabilityStartedEvent found.");
            else
                errors.Add("EnemyVulnerabilityStartedEvent missing.");

            if (typeof(CindarsHope.Core.Events.EnemyVulnerabilityEndedEvent) != null)
                passed.Add("EnemyVulnerabilityEndedEvent found.");
            else
                errors.Add("EnemyVulnerabilityEndedEvent missing.");

            // ── 7. Action sets for 8 testable enemies exist ───────────────────────
            var setGuids = AssetDatabase.FindAssets("t:EnemyActionSetSO");
            var allSets = setGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null)
                .ToDictionary(s => s.ActionSetId, s => s);

            foreach (var id in RequiredActionSetIds)
            {
                if (allSets.ContainsKey(id))
                    passed.Add($"ActionSet '{id}' present.");
                else
                    warnings.Add($"ActionSet '{id}' not found — run CindarsHope > SPEC 13 > Create Enemy Actions and Sets.");
            }

            // ── 8. Action assets exist for 8 testable enemies ─────────────────────
            var actionGuids = AssetDatabase.FindAssets("t:EnemyActionSO");
            int actionCount = actionGuids.Length;
            if (actionCount >= 70)
                passed.Add($"EnemyActionSO count: {actionCount} (≥70 OK).");
            else
                warnings.Add($"EnemyActionSO count: {actionCount} — expected ≥70 (run Create Enemy Actions and Sets).");

            // ── 9. EnemyDataSO canonical roster check ─────────────────────────────
            var enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var allEnemies = enemyGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null)
                .ToDictionary(e => e.enemyId, e => e);

            int testableFound = 0;
            foreach (var id in EightTestableEnemyIds)
            {
                if (allEnemies.ContainsKey(id))
                {
                    testableFound++;
                    passed.Add($"EnemyDataSO '{id}' found.");
                }
                else
                    warnings.Add($"EnemyDataSO '{id}' not found — reconcile 13B roster or create canonical assets.");
            }

            if (testableFound >= 8)
                passed.Add($"All 8 testable EnemyDataSO found.");
            else
                warnings.Add($"Only {testableFound}/8 testable EnemyDataSO found. SPEC 13D wiring requires canonical roster.");

            // ── 10. EnemyVulnerabilityState has OpenWindow method ─────────────────
            var openMethod = typeof(EnemyVulnerabilityState).GetMethod("OpenWindow");
            if (openMethod != null)
                passed.Add("EnemyVulnerabilityState.OpenWindow() exists.");
            else
                errors.Add("EnemyVulnerabilityState.OpenWindow() missing.");

            // ── 11. EnemyBrain.SetState is public ─────────────────────────────────
            var setStateMethod = brainType?.GetMethod("SetState",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (setStateMethod != null)
                passed.Add("EnemyBrain.SetState(EnemyBrainState) is public.");
            else
                errors.Add("EnemyBrain.SetState(EnemyBrainState) not found or not public.");

            // ── Print results ──────────────────────────────────────────────────────
            Debug.Log($"[SPEC 13D Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed)   Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors)   Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13D] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13D] Validation FAILED with {errors.Count} error(s).");
        }
    }
}
