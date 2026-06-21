using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 13C validator — Enemy actions and action sets.
    /// Run via: CindarsHope > Validation > Validate SPEC 13C - Enemy Actions
    /// </summary>
    public static class ValidateSpec13EnemyActions
    {
        private static readonly string[] RequiredActionSetIds =
        {
            "actionset_enemy_cave_mite",
            "actionset_enemy_stone_rat",
            "actionset_enemy_cave_bat",
            "actionset_enemy_goblin_grashnaar_scavenger",
            "actionset_enemy_kobold_scout",
            "actionset_enemy_mossling",
            "actionset_enemy_cracked_bone",
            "actionset_enemy_blackroot_sprout",
            "actionset_enemy_spore_imp",
            "actionset_enemy_rootsnare",
            "actionset_enemy_hollow_stagling",
            "actionset_enemy_goblin_urudakh_trapper",
            "actionset_enemy_thorn_archer",
            "actionset_enemy_orc_nyx_stalker",
            "actionset_enemy_mycobulwark",
            "actionset_enemy_nyx_moth",
            "actionset_enemy_frost_gnawer",
            "actionset_enemy_duergar_frostdelver",
            "actionset_enemy_duergar_shieldbreaker",
            "actionset_enemy_icebound_sentinel",
            "actionset_enemy_glassbone",
            "actionset_enemy_cold_cult_acolyte",
            "actionset_enemy_crystal_leaper",
            "actionset_enemy_frost_wailer",
            "actionset_enemy_ember_tick",
            "actionset_enemy_ash_crawler",
            "actionset_enemy_orc_kaand_berserker",
            "actionset_enemy_orc_kaand_ashcaller",
            "actionset_enemy_lava_bulwark",
            "actionset_enemy_cinder_spitter",
            "actionset_enemy_scorched_cultist",
            "actionset_enemy_furnace_warden",
            "actionset_enemy_rune_shard",
            "actionset_enemy_clockwork_guard",
            "actionset_enemy_gnome_gem_madcap",
            "actionset_enemy_gnomorin_rune_tinker",
            "actionset_enemy_sealed_knight",
            "actionset_enemy_mirror_adept",
            "actionset_enemy_puzzle_golem",
            "actionset_enemy_oathless_shade",
        };

        private static readonly string[] RequiredTelegraphIds =
        {
            "telegraph_fast_melee",
            "telegraph_heavy_melee",
            "telegraph_ranged_projectile",
            "telegraph_caster_spell",
            "telegraph_area_pulse",
            "telegraph_burrow_emerge",
            "telegraph_leap",
            "telegraph_phase",
        };

        private static readonly HashSet<string> ValidDamageTypes = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "physical", "fire", "ice", "toxic", "lightning", "arcane", "true"
        };

        public static void RunValidation()
        {
            var errors   = new List<string>();
            var warnings = new List<string>();
            var passed   = new List<string>();

            // ── 1. Telegraph profiles ─────────────────────────────────────────────
            var tpGuids = AssetDatabase.FindAssets("t:EnemyTelegraphProfileSO");
            var tpIds = tpGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyTelegraphProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => a.TelegraphProfileId)
                .ToHashSet();

            foreach (var id in RequiredTelegraphIds)
            {
                if (tpIds.Contains(id))
                    passed.Add($"TelegraphProfile '{id}' found.");
                else
                    errors.Add($"TelegraphProfile '{id}' not found — run CindarsHope > SPEC 13 > Create Enemy Actions and Sets.");
            }

            // ── 2. Load all EnemyActionSO ─────────────────────────────────────────
            var actionGuids = AssetDatabase.FindAssets("t:EnemyActionSO");
            var allActions = actionGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyActionSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var actionIdMap = allActions.ToDictionary(a => a.ActionId, a => a);
            passed.Add($"EnemyActionSO total: {allActions.Count}.");

            // ── 3. Load all EnemyActionSetSO ──────────────────────────────────────
            var setGuids = AssetDatabase.FindAssets("t:EnemyActionSetSO");
            var allSets = setGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var setIdMap = allSets.ToDictionary(s => s.ActionSetId, s => s);
            passed.Add($"EnemyActionSetSO total: {allSets.Count}.");

            // ── 4. Required action sets exist ─────────────────────────────────────
            foreach (var id in RequiredActionSetIds)
            {
                if (setIdMap.ContainsKey(id))
                    passed.Add($"ActionSet '{id}' found.");
                else
                    errors.Add($"ActionSet '{id}' not found — run CindarsHope > SPEC 13 > Create Enemy Actions and Sets.");
            }

            if (allSets.Count >= 40)
                passed.Add($"Action set count: {allSets.Count} (≥40 OK).");
            else
                errors.Add($"Action set count: {allSets.Count} — expected at least 40.");

            // ── 5. Each action set has ≥1 action ──────────────────────────────────
            foreach (var s in allSets.Where(s => RequiredActionSetIds.Contains(s.ActionSetId)))
            {
                if (s.ActionIds == null || s.ActionIds.Length == 0)
                {
                    errors.Add($"ActionSet '{s.ActionSetId}' has no ActionIds.");
                    continue;
                }

                passed.Add($"ActionSet '{s.ActionSetId}' has {s.ActionIds.Length} action(s).");

                foreach (var aid in s.ActionIds)
                {
                    if (!actionIdMap.ContainsKey(aid))
                        errors.Add($"ActionSet '{s.ActionSetId}' references ActionId '{aid}' which does not exist as an asset.");
                }
            }

            // ── 6. Per-action validation ───────────────────────────────────────────
            foreach (var a in allActions)
            {
                if (string.IsNullOrEmpty(a.ActionId))
                {
                    warnings.Add($"Action asset '{a.name}' has empty ActionId.");
                    continue;
                }

                bool isOffensive = a.ActionType != EnemyActionType.SelfBuff;

                if (isOffensive)
                {
                    if (!ValidDamageTypes.Contains(a.DamageType))
                        errors.Add($"Action '{a.ActionId}': DamageType '{a.DamageType}' is not a recognised type (physical/fire/ice/toxic/lightning/arcane/true).");

                    if (string.IsNullOrEmpty(a.TelegraphProfileId))
                        errors.Add($"Action '{a.ActionId}': offensive action must have TelegraphProfileId.");

                    if (a.CooldownSeconds <= 0f)
                        errors.Add($"Action '{a.ActionId}': CooldownSeconds must be > 0.");

                    if (a.WindupSeconds < 0f)
                        errors.Add($"Action '{a.ActionId}': WindupSeconds must be ≥ 0.");

                    if (a.RecoverSeconds < 0f)
                        errors.Add($"Action '{a.ActionId}': RecoverSeconds must be ≥ 0.");
                }

                // DamageType must not contain a status ID
                if (a.DamageType != null && a.DamageType.StartsWith("status_"))
                    errors.Add($"Action '{a.ActionId}': DamageType contains a status ID — must be separate fields.");

                // StatusApplicationIds must not contain DamageType values
                if (a.StatusApplicationIds != null)
                {
                    foreach (var sid in a.StatusApplicationIds)
                    {
                        if (ValidDamageTypes.Contains(sid))
                            errors.Add($"Action '{a.ActionId}': StatusApplicationId '{sid}' looks like a DamageType — must be separate.");
                    }
                }
            }

            // ── 7. EnemyDataSO cross-check (informational) ────────────────────────
            var enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var allEnemies = enemyGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            int wiredCount = 0;
            foreach (var e in allEnemies)
            {
                if (string.IsNullOrEmpty(e.ActionSetId))
                    continue;

                if (!setIdMap.ContainsKey(e.ActionSetId))
                    warnings.Add($"EnemyDataSO '{e.enemyId}' references ActionSetId '{e.ActionSetId}' which does not exist.");
                else
                    wiredCount++;
            }

            var canonicalIds = new HashSet<string>(RequiredActionSetIds.Select(id => id.Replace("actionset_", "")));
            int canonicalFound = allEnemies.Count(e => canonicalIds.Contains(e.enemyId));

            if (canonicalFound < 40)
                warnings.Add($"Only {canonicalFound}/40 canonical EnemyDataSO assets found. EnemyDataSO for the 13C roster must be created separately (SPEC 13B roster mismatch — see validation doc).");
            else
                passed.Add($"All 40 canonical EnemyDataSO assets found ({canonicalFound}).");

            passed.Add($"EnemyDataSO with valid ActionSetId wired: {wiredCount}/{allEnemies.Count}.");

            // ── 8. SPEC 13D not yet implemented ───────────────────────────────────
            passed.Add("SPEC 13D (EnemyBrain runtime) not yet implemented — correct for 13C.");

            // ── Print results ──────────────────────────────────────────────────────
            Debug.Log($"[SPEC 13C Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed)   Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors)   Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13C] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13C] Validation FAILED with {errors.Count} error(s).");
        }
    }
}
