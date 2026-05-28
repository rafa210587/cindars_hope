using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 13B validator — Roster 40 EnemyDataSO assets.
    /// Run via: CindarsHope > Validation > Validate SPEC 13B - Enemy Roster
    /// </summary>
    public static class ValidateSpec13EnemyRoster
    {
        private static readonly string[] RequiredEnemyIds =
        {
            // Band 1
            "enemy_verdant_mite",
            "enemy_spore_crawler",
            "enemy_goblin_scrounger",
            "enemy_kobold_sentry",
            "enemy_rot_beetle",
            "enemy_pale_grub",
            "enemy_mushroom_puffball",
            // Band 2
            "enemy_goblin_shaman",
            "enemy_kobold_trapmaster",
            "enemy_orc_grunt",
            "enemy_cave_leaper",
            "enemy_duergar_crossbowman",
            "enemy_burrowing_maggot",
            "enemy_fungal_spreader",
            "enemy_drow_skirmisher",
            // Band 3
            "enemy_orc_berserker",
            "enemy_duergar_warder",
            "enemy_undead_shambler",
            "enemy_cultist_zealot",
            "enemy_gnome_tinkerer",
            "enemy_phase_stalker",
            "enemy_earth_elemental_minor",
            "enemy_cave_burrower_elite",
            // Band 4
            "enemy_drow_witch",
            "enemy_undead_knight",
            "enemy_construct_sentry",
            "enemy_abyssal_hound",
            "enemy_corrupted_vine_horror",
            "enemy_ninrorin_phantom",
            "enemy_gnome_wargolem",
            // Band 5
            "enemy_draconic_wyrmling",
            "enemy_abyssal_lurker",
            "enemy_corrupted_orc_champion",
            "enemy_earth_elemental_greater",
            "enemy_undead_lich_acolyte",
            "enemy_draconic_guardian",
            // MiniBosses
            "enemy_goblin_warchief",
            "enemy_orc_warlord",
            "enemy_abyssal_gatekeeper",
            // Bosses
            "enemy_cave_mite_queen",
            "enemy_fungal_patriarch",
            "enemy_duergar_artificer_lord",
            "enemy_void_herald",
            "enemy_draconic_elder",
        };

        [MenuItem("CindarsHope/Validation/Validate SPEC 13B - Enemy Roster")]
        public static void RunValidation()
        {
            var errors   = new List<string>();
            var warnings = new List<string>();
            var passed   = new List<string>();

            // ── Load all EnemyDataSO assets ────────────────────────────────────────
            var guids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var allEnemies = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            var foundIds = allEnemies.Select(e => e.enemyId).ToHashSet();

            passed.Add($"Total EnemyDataSO assets found: {allEnemies.Count}.");

            // ── 1. All required IDs present ────────────────────────────────────────
            foreach (var id in RequiredEnemyIds)
            {
                if (foundIds.Contains(id))
                    passed.Add($"[OK] {id}");
                else
                    errors.Add($"[MISSING] {id} — run CindarsHope > SPEC 13 > Create Roster 40 Enemy Data.");
            }

            // ── 2. Roster count ≥ 40 ──────────────────────────────────────────────
            var rosterCount = allEnemies.Count(e => RequiredEnemyIds.Contains(e.enemyId));
            if (rosterCount >= 40)
                passed.Add($"Roster count: {rosterCount}/40 (OK).");
            else
                errors.Add($"Roster count: {rosterCount}/40 — expected at least 40 required entries.");

            // ── 3. Data completeness checks ────────────────────────────────────────
            foreach (var e in allEnemies.Where(e => RequiredEnemyIds.Contains(e.enemyId)))
            {
                if (string.IsNullOrEmpty(e.FactionId))
                    errors.Add($"{e.enemyId}: FactionId is empty.");

                if (string.IsNullOrEmpty(e.SizeProfileId))
                    errors.Add($"{e.enemyId}: SizeProfileId is empty.");

                if (string.IsNullOrEmpty(e.MovementProfileId))
                    errors.Add($"{e.enemyId}: MovementProfileId is empty.");

                if (string.IsNullOrEmpty(e.VulnerabilityProfileId))
                    errors.Add($"{e.enemyId}: VulnerabilityProfileId is empty.");

                if (string.IsNullOrEmpty(e.BestiaryEntryId))
                    warnings.Add($"{e.enemyId}: BestiaryEntryId is empty (OK until SPEC 13E).");

                if (string.IsNullOrEmpty(e.PrimaryDamageTypeId))
                    warnings.Add($"{e.enemyId}: PrimaryDamageTypeId is empty.");

                if (e.xpReward <= 0)
                    warnings.Add($"{e.enemyId}: xpReward is 0.");

                if (e.maxHp <= 0)
                    errors.Add($"{e.enemyId}: maxHp must be > 0.");

                if (e.CaveBand < 1 || e.CaveBand > 6)
                    errors.Add($"{e.enemyId}: CaveBand={e.CaveBand} is out of range [1-6].");
            }

            // ── 4. Boss / MiniBoss checks ──────────────────────────────────────────
            var bosses    = allEnemies.Where(e => e.IsBoss    && RequiredEnemyIds.Contains(e.enemyId)).ToList();
            var minibosses = allEnemies.Where(e => e.IsMiniBoss && RequiredEnemyIds.Contains(e.enemyId)).ToList();

            if (bosses.Count >= 5)
                passed.Add($"Boss count: {bosses.Count} (≥5 OK).");
            else
                errors.Add($"Boss count: {bosses.Count} — expected at least 5 bosses.");

            if (minibosses.Count >= 3)
                passed.Add($"MiniBoss count: {minibosses.Count} (≥3 OK).");
            else
                errors.Add($"MiniBoss count: {minibosses.Count} — expected at least 3 mini-bosses.");

            // ── 5. Faction coverage ────────────────────────────────────────────────
            var coveredFactions = allEnemies
                .Where(e => RequiredEnemyIds.Contains(e.enemyId))
                .Select(e => e.FactionId)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            passed.Add($"Factions covered: {coveredFactions.Count} ({string.Join(", ", coveredFactions)}).");

            if (coveredFactions.Count < 10)
                warnings.Add($"Only {coveredFactions.Count} factions covered — expected ≥10 for good diversity.");

            // ── Print results ──────────────────────────────────────────────────────
            Debug.Log($"[SPEC 13B Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");

            foreach (var p in passed)   Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors)   Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[SPEC 13B] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13B] Validation FAILED with {errors.Count} error(s).");
        }
    }
}
