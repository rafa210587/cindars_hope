using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// SPEC 13A validator — Enemy taxonomy, profiles and contracts.
    /// Run via: CindarsHope > Validation > Validate SPEC 13A Enemy Taxonomy
    /// </summary>
    public static class ValidateSpec13EnemyTaxonomyProfiles
    {
        private static readonly string[] RequiredMovementProfileIds =
        {
            "movement_ground_chase",
            "movement_ground_patrol",
            "movement_guard_stationary",
            "movement_kite_ranged",
            "movement_caster_keep_away",
            "movement_burrow_ambush",
            "movement_swarm_erratic",
            "movement_tank_slow_push",
            "movement_phase_short_blink",
            "movement_leaper"
        };

        private static readonly string[] RequiredSizeProfileIds =
        {
            "size_tiny",
            "size_small",
            "size_medium",
            "size_large",
            "size_huge",
            "size_boss"
        };

        private static readonly string[] RequiredVulnerabilityProfileIds =
        {
            "vuln_swarm_after_bite",
            "vuln_chaser_charge",
            "vuln_ranged_after_volley",
            "vuln_caster_after_cast",
            "vuln_burrow_emerge",
            "vuln_guard_shield_drop",
            "vuln_tank_recover",
            "vuln_phase_arrival",
            "vuln_leaper_landing",
            "vuln_corrupted_enrage_pulse"
        };

        private static readonly string[] RequiredFactionIds =
        {
            "faction_beast", "faction_fungal", "faction_goblin", "faction_kobold",
            "faction_orc", "faction_duergar", "faction_drow", "faction_gnome",
            "faction_ninrorin", "faction_undead", "faction_cultist", "faction_elemental",
            "faction_construct", "faction_abyssal", "faction_corrupted", "faction_draconic"
        };

        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            // ── 1. EnemyRole: Phase must NOT exist ─────────────────────────────
            var roleNames = Enum.GetNames(typeof(EnemyRole));
            if (roleNames.Contains("Phase", StringComparer.OrdinalIgnoreCase))
                errors.Add("EnemyRole contains 'Phase' — must not exist. Use MovementProfileId = 'movement_phase_short_blink'.");
            else
                passed.Add("EnemyRole does not contain Phase.");

            // ── 2. All required roles exist ────────────────────────────────────
            var requiredRoles = new[] { "Chaser", "Guard", "Ranged", "Caster", "Burrower", "Swarm", "Tank", "Elite", "MiniBoss", "Boss" };
            foreach (var role in requiredRoles)
            {
                if (roleNames.Contains(role, StringComparer.OrdinalIgnoreCase))
                    passed.Add($"EnemyRole.{role} exists.");
                else
                    errors.Add($"EnemyRole.{role} is missing from enum.");
            }

            // ── 3. EnemyMovementType: PhaseShortBlink exists, Flying not active ─
            var movTypeNames = Enum.GetNames(typeof(EnemyMovementType));
            if (movTypeNames.Contains("PhaseShortBlink", StringComparer.OrdinalIgnoreCase))
                passed.Add("EnemyMovementType.PhaseShortBlink exists.");
            else
                errors.Add("EnemyMovementType.PhaseShortBlink is missing.");

            if (movTypeNames.Contains("Flying", StringComparer.OrdinalIgnoreCase))
                warnings.Add("EnemyMovementType.Flying exists — must not be active in MVP runtime.");
            else
                passed.Add("EnemyMovementType.Flying not present (correct for MVP).");

            // ── 4. Movement profile assets ─────────────────────────────────────
            var movGuids = AssetDatabase.FindAssets("t:EnemyMovementProfileSO");
            var movIds = movGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyMovementProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => a.MovementProfileId)
                .ToHashSet();

            foreach (var id in RequiredMovementProfileIds)
            {
                if (movIds.Contains(id))
                    passed.Add($"MovementProfile '{id}' found.");
                else
                    errors.Add($"MovementProfile '{id}' not found — run CindarsHope > SPEC 13 > Create Default Enemy Profiles.");
            }

            // Verify no movement profile has CanFly = true
            foreach (var guid in movGuids)
            {
                var mp = AssetDatabase.LoadAssetAtPath<EnemyMovementProfileSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (mp != null && mp.CanFly)
                    errors.Add($"MovementProfile '{mp.MovementProfileId}' has CanFly=true — Flying is not active in MVP.");
            }

            // ── 5. Size profile assets ─────────────────────────────────────────
            var sizeGuids = AssetDatabase.FindAssets("t:EnemySizeProfileSO");
            var sizeIds = sizeGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemySizeProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => a.SizeProfileId)
                .ToHashSet();

            foreach (var id in RequiredSizeProfileIds)
            {
                if (sizeIds.Contains(id))
                    passed.Add($"SizeProfile '{id}' found.");
                else
                    errors.Add($"SizeProfile '{id}' not found — run CindarsHope > SPEC 13 > Create Default Enemy Profiles.");
            }

            // ── 6. Vulnerability profile assets ───────────────────────────────
            var vulnGuids = AssetDatabase.FindAssets("t:EnemyVulnerabilityProfileSO");
            var vulnIds = vulnGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyVulnerabilityProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => a.VulnerabilityProfileId)
                .ToHashSet();

            foreach (var id in RequiredVulnerabilityProfileIds)
            {
                if (vulnIds.Contains(id))
                    passed.Add($"VulnerabilityProfile '{id}' found.");
                else
                    errors.Add($"VulnerabilityProfile '{id}' not found — run CindarsHope > SPEC 13 > Create Default Enemy Profiles.");
            }

            // ── 7. Faction assets ──────────────────────────────────────────────
            var factionGuids = AssetDatabase.FindAssets("t:EnemyFactionSO");
            var factionIds = factionGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyFactionSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => a.factionId)
                .ToHashSet();

            foreach (var id in RequiredFactionIds)
            {
                if (factionIds.Contains(id))
                    passed.Add($"Faction '{id}' found.");
                else
                    errors.Add($"Faction '{id}' not found — run CindarsHope > SPEC 13 > Create Default Enemy Profiles.");
            }

            // ── 8. EnemyDataSO compatibility check ────────────────────────────
            var enemyGuids = AssetDatabase.FindAssets("t:EnemyDataSO");
            var enemyAssets = enemyGuids
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToList();

            passed.Add($"EnemyDataSO assets found: {enemyAssets.Count}. Backwards-compatible.");

            foreach (var enemy in enemyAssets)
            {
                if (string.IsNullOrEmpty(enemy.enemyId))
                    warnings.Add($"EnemyDataSO '{enemy.name}' has empty enemyId.");
            }

            // ── 9. Confirm SPEC 13B not yet implemented ────────────────────────
            var rosterCount = enemyAssets.Count;
            if (rosterCount >= 40)
                warnings.Add($"Found {rosterCount} EnemyDataSO assets — SPEC 13B may already be implemented.");
            else
                passed.Add($"Roster has {rosterCount}/40 entries — SPEC 13B not yet implemented (correct for 13A).");

            // ── Print results ──────────────────────────────────────────────────
            var totalErrors   = errors.Count;
            var totalWarnings = warnings.Count;
            var totalPassed   = passed.Count;

            Debug.Log($"[SPEC 13A Validation] PASSED: {totalPassed} | WARNINGS: {totalWarnings} | ERRORS: {totalErrors}");

            foreach (var p in passed)   Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors)   Debug.LogError($"  [ERR]  {e}");

            if (totalErrors == 0)
                Debug.Log("[SPEC 13A] Validation PASSED.");
            else
                Debug.LogError($"[SPEC 13A] Validation FAILED with {totalErrors} error(s).");
        }
    }
}
