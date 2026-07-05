using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// spec_enemy_attack_kits_v1 — validator read-only do universo completo de kits de ataque
    /// (113 fichas canonicas + 7 novas do Roster + enemy_meteor_ooze_king + 53 variancias).
    /// NAO muta nenhum asset. Registrado como RunStep em CindarsHope/Validar Projeto.
    /// Distinto de ValidateSpec13EnemyActions (escopo antigo: 40 IDs hardcoded do Roster) —
    /// os dois continuam rodando; este cobre o universo completo do catalogo v1.1.
    /// </summary>
    public static class ValidateEnemyAttackKits
    {
        // Threshold acima do qual uma acao Normal e considerada "ranged" e exige fallback melee +
        // MinRange (kit de 3), por simetria com o §1 do catalogo (Dist R = kit de 3).
        private const float RangedNormalRangeThreshold = 2.5f;

        public static void RunValidation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var passed = new List<string>();

            var actionsById = AssetDatabase.FindAssets("t:EnemyActionSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyActionSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .GroupBy(a => a.ActionId)
                .ToDictionary(g => g.Key, g => g.First());

            var setsById = AssetDatabase.FindAssets("t:EnemyActionSetSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null)
                .GroupBy(s => s.ActionSetId)
                .ToDictionary(g => g.Key, g => g.First());

            var enemyDataById = AssetDatabase.FindAssets("t:EnemyDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.enemyId))
                .GroupBy(e => e.enemyId)
                .ToDictionary(g => g.Key, g => g.First());

            var statusIds = AssetDatabase.FindAssets("t:StatusEffectSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<CindarsHope.Combat.StatusEffect.StatusEffectSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null)
                .Select(s => s.Id)
                .ToHashSet();

            var telegraphIds = AssetDatabase.FindAssets("t:EnemyTelegraphProfileSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyTelegraphProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(t => t != null)
                .Select(t => t.TelegraphProfileId)
                .ToHashSet();

            passed.Add($"EnemyActionSO total: {actionsById.Count}.");
            passed.Add($"EnemyActionSetSO total: {setsById.Count}.");

            // ── The Four (§4B.8) — devem permanecer SEM ActionSetId (DORMANTE) ────────────────
            string[] theFour = { "boss_vel_karaum", "boss_cindrathel", "boss_archivist_of_silence", "boss_ithryndor" };
            foreach (var bossId in theFour)
            {
                if (enemyDataById.TryGetValue(bossId, out var bossData) && !string.IsNullOrWhiteSpace(bossData.ActionSetId))
                    errors.Add($"The Four '{bossId}' tem ActionSetId '{bossData.ActionSetId}' — deve permanecer DORMANTE (vazio).");
            }
            passed.Add("The Four (4 fichas) checados — devem permanecer sem ActionSetId.");

            // ── Kit owners (~121): todo dono tem actionset com >=1 normal + >=1 especial ───────
            int kitOwnersChecked = 0;
            int missingAsset = 0;
            foreach (var kit in CindarsHope.Editor.EnemyTaxonomy.GenerateEnemyAttackKits.GetKitOwnerIds())
            {
                if (!enemyDataById.TryGetValue(kit, out var enemyData))
                {
                    warnings.Add($"Ficha '{kit}' nao tem EnemyDataSO materializado ainda (fora de escopo criar aqui).");
                    missingAsset++;
                    continue;
                }

                kitOwnersChecked++;

                if (string.IsNullOrWhiteSpace(enemyData.ActionSetId))
                {
                    errors.Add($"'{kit}': EnemyDataSO.ActionSetId vazio — sem kit gerado.");
                    continue;
                }

                if (!setsById.TryGetValue(enemyData.ActionSetId, out var actionSet))
                {
                    errors.Add($"'{kit}': ActionSetId '{enemyData.ActionSetId}' nao resolve para um EnemyActionSetSO existente.");
                    continue;
                }

                ValidateActionSetKit(kit, actionSet, actionsById, errors, warnings, passed);
            }

            passed.Add($"Kit owners checados: {kitOwnersChecked} (fichas sem EnemyDataSO: {missingAsset}).");

            // ── Variancias (53): ActionSetId deve ser identico ao da mae ───────────────────────
            int variancesChecked = 0;
            foreach (var pair in CindarsHope.Editor.EnemyTaxonomy.GenerateEnemyAttackKits.VarianceToMotherCrosswalk)
            {
                string rosterId = "enemy_" + pair.Key;
                string motherId = "enemy_" + pair.Value;

                if (!enemyDataById.TryGetValue(rosterId, out var rosterData))
                {
                    warnings.Add($"Variancia '{rosterId}' sem EnemyDataSO materializado.");
                    continue;
                }

                string expectedSetId = $"actionset_{motherId}";
                if (rosterData.ActionSetId != expectedSetId)
                {
                    errors.Add($"Variancia '{rosterId}': ActionSetId='{rosterData.ActionSetId}' difere do esperado (mae='{expectedSetId}').");
                    continue;
                }

                variancesChecked++;
            }

            passed.Add($"Variancias checadas: {variancesChecked}/53.");

            // ── StatusApplicationIds e TelegraphProfileId resolvem ─────────────────────────────
            foreach (var action in actionsById.Values)
            {
                if (action.StatusApplicationIds != null)
                {
                    foreach (var sid in action.StatusApplicationIds)
                    {
                        if (!string.IsNullOrWhiteSpace(sid) && !statusIds.Contains(sid))
                            errors.Add($"Action '{action.ActionId}': StatusApplicationId '{sid}' nao existe no StatusEffectDatabaseSO.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(action.AllyBuffStatusId) && !statusIds.Contains(action.AllyBuffStatusId))
                    errors.Add($"Action '{action.ActionId}': AllyBuffStatusId '{action.AllyBuffStatusId}' nao existe no StatusEffectDatabaseSO.");

                if (!string.IsNullOrWhiteSpace(action.HazardStatusId) && !statusIds.Contains(action.HazardStatusId))
                    errors.Add($"Action '{action.ActionId}': HazardStatusId '{action.HazardStatusId}' nao existe no StatusEffectDatabaseSO.");

                if (!string.IsNullOrWhiteSpace(action.DebuffStatusId) && !statusIds.Contains(action.DebuffStatusId))
                    errors.Add($"Action '{action.ActionId}': DebuffStatusId '{action.DebuffStatusId}' nao existe no StatusEffectDatabaseSO.");

                if (!string.IsNullOrWhiteSpace(action.TelegraphProfileId) && !telegraphIds.Contains(action.TelegraphProfileId))
                    errors.Add($"Action '{action.ActionId}': TelegraphProfileId '{action.TelegraphProfileId}' nao existe no EnemyTelegraphProfileDatabaseSO.");
            }

            passed.Add("StatusApplicationIds/AllyBuffStatusId/HazardStatusId/DebuffStatusId/TelegraphProfileId checados em todas as EnemyActionSO.");

            // ── Print results ──────────────────────────────────────────────────────────────────
            Debug.Log($"[EnemyAttackKits Validation] PASSED: {passed.Count} | WARNINGS: {warnings.Count} | ERRORS: {errors.Count}");
            foreach (var p in passed) Debug.Log($"  [OK]   {p}");
            foreach (var w in warnings) Debug.LogWarning($"  [WARN] {w}");
            foreach (var e in errors) Debug.LogError($"  [ERR]  {e}");

            if (errors.Count == 0)
                Debug.Log("[EnemyAttackKits] Validation PASSED.");
            else
                Debug.LogError($"[EnemyAttackKits] Validation FAILED with {errors.Count} error(s).");
        }

        private static void ValidateActionSetKit(
            string enemyId,
            EnemyActionSetSO actionSet,
            IReadOnlyDictionary<string, EnemyActionSO> actionsById,
            List<string> errors,
            List<string> warnings,
            List<string> passed)
        {
            if (actionSet.ActionIds == null || actionSet.ActionIds.Length == 0)
            {
                errors.Add($"'{enemyId}': ActionSet '{actionSet.ActionSetId}' nao tem ActionIds.");
                return;
            }

            var resolvedActions = new List<EnemyActionSO>();
            foreach (var actionId in actionSet.ActionIds)
            {
                if (!actionsById.TryGetValue(actionId, out var action))
                {
                    errors.Add($"'{enemyId}': ActionSet '{actionSet.ActionSetId}' referencia ActionId '{actionId}' que nao existe.");
                    continue;
                }
                resolvedActions.Add(action);
            }

            if (resolvedActions.Count == 0) return;

            bool hasNormal = resolvedActions.Any(a => a.ActionType != EnemyActionType.SelfBuff && !IsSpecialByNaming(a.ActionId));
            bool hasSpecial = resolvedActions.Any(a => IsSpecialByNaming(a.ActionId));

            if (!hasNormal)
                errors.Add($"'{enemyId}': ActionSet '{actionSet.ActionSetId}' nao tem nenhuma acao Normal (melee/ranged/cast).");
            if (!hasSpecial)
                errors.Add($"'{enemyId}': ActionSet '{actionSet.ActionSetId}' nao tem nenhuma acao Especial.");

            // Kit de 3 (ranged): toda acao com Range acima do threshold precisa de fallback melee no
            // mesmo set + MinRange > 0 configurado nela.
            foreach (var rangedCandidate in resolvedActions.Where(a =>
                         (a.ActionType == EnemyActionType.RangedProjectile || a.ActionType == EnemyActionType.CastProjectile)
                         && a.Range > RangedNormalRangeThreshold
                         && !IsSpecialByNaming(a.ActionId)))
            {
                if (rangedCandidate.MinRange <= 0f)
                    errors.Add($"'{enemyId}': acao ranged Normal '{rangedCandidate.ActionId}' sem MinRange configurado (kit de 3 exige fallback ativavel).");

                bool hasFallbackMelee = resolvedActions.Any(a => a.ActionType == EnemyActionType.MeleeAttack && !IsSpecialByNaming(a.ActionId));
                if (!hasFallbackMelee)
                    errors.Add($"'{enemyId}': acao ranged Normal '{rangedCandidate.ActionId}' sem melee de fallback no mesmo actionset.");
            }

            passed.Add($"'{enemyId}': ActionSet '{actionSet.ActionSetId}' com {resolvedActions.Count} acao(oes) — kit valido.");
        }

        private static bool IsSpecialByNaming(string actionId)
        {
            return !string.IsNullOrEmpty(actionId) && actionId.EndsWith("_special");
        }
    }
}
