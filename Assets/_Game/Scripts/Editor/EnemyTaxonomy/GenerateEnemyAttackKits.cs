using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.EnemyTaxonomy
{
    /// <summary>
    /// spec_enemy_attack_kits_v1 — gera/realinha EnemyActionSO/EnemyActionSetSO para o universo
    /// completo de ~121 donos de kit (113 fichas canonicas das 7 bands + 7 NOVAS do Roster +
    /// enemy_meteor_ooze_king), conforme ENEMY_ATTACK_CATALOG_DIRECTION_v1.0 (v1.1) e
    /// ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0. NAO recria CreateEnemyActionsAndSets (que ja
    /// cobre os 60 IDs do Roster) — REALINHA in-place os assets ja existentes desses 60 (mesmos
    /// ActionId/ActionSetId, campos atualizados para o kit final do catalogo v1.1) e CRIA os
    /// assets que faltam para as 113 canonicas + 7 novas + meteor_ooze_king, usando o naming
    /// padrao `action_{enemyId sem prefixo}_{melee|ranged|special[_n]}` / `actionset_{enemyId}`.
    /// Idempotente: sempre localiza por ActionId/ActionSetId (AssetDatabase.LoadAssetAtPath) antes
    /// de criar. Wireia as 53 VARIANCIAS do Roster (EnemyDataSO.ActionSetId -> ActionSetId da mae),
    /// sem duplicar EnemyActionSO/EnemyActionSetSO.
    /// Registrado como RunStep em CindarsHope/Inicializar Projeto (FASE A), APOS
    /// GenerateAndWireSpec13GAssets (que gera o bestiario canonico + os 60 assets do Roster).
    /// Sem [MenuItem] proprio (rule editor-generation-orchestration).
    /// </summary>
    public static partial class GenerateEnemyAttackKits
    {
        private const string ActionsFolder = "Assets/_Game/Data/Enemies/Actions";
        private const string ActionSetsFolder = "Assets/_Game/Data/Enemies/ActionSets";

        // ── Multiplicadores nomeados por arquetipo (no-magic-balance-values) ───────────────────
        // Derivados de ENEMY_ATTACK_IMPLEMENTATION_DIRECTION_v1.0 §3 (range/windup por arquetipo)
        // e do catalogo §1b (melee de fallback ~60% do dano do ranged, sem status).

        private const float FallbackMeleeDamageMultiplier = 0.6f;
        private const float SpecialDamageMultiplier = 1.15f;
        private const float NormalDamageMultiplier = 1.0f;

        /// <summary>Um arquetipo mecanico compartilhado (§2 do catalogo): EnemyActionType + perfil de
        /// timing/range/telegraph. Os numeros de dano vem de BestiaryCreatureDef.Damage (por-criatura);
        /// aqui so o multiplicador e os campos nao-dano (range/windup/telegraph) sao fixos por arquetipo.</summary>
        private readonly struct Archetype
        {
            public readonly EnemyActionType ActionType;
            public readonly float Range;
            public readonly float MinRange;
            public readonly float AreaRadius;
            public readonly float Windup;
            public readonly float Recover;
            public readonly float Cooldown;
            public readonly float ProjectileSpeed;
            public readonly string TelegraphId;

            public Archetype(EnemyActionType actionType, float range, float windup, float recover, float cooldown,
                float areaRadius = 0f, float minRange = 0f, float projectileSpeed = 0f, string telegraphId = "telegraph_fast_melee")
            {
                ActionType = actionType;
                Range = range;
                MinRange = minRange;
                AreaRadius = areaRadius;
                Windup = windup;
                Recover = recover;
                Cooldown = cooldown;
                ProjectileSpeed = projectileSpeed;
                TelegraphId = telegraphId;
            }
        }

        // Slugs `atk_*` -> perfil mecanico (ENEMY_ATTACK_CATALOG_DIRECTION_v1.0 §2 + IMPLEMENTATION §3).
        private static readonly Dictionary<string, Archetype> Archetypes = new Dictionary<string, Archetype>
        {
            ["atk_slash"] = new Archetype(EnemyActionType.MeleeAttack, 1.2f, 0.4f, 0.35f, 2.0f, telegraphId: "telegraph_fast_melee"),
            ["atk_cleave"] = new Archetype(EnemyActionType.MeleeAttack, 1.4f, 0.8f, 0.55f, 2.5f, telegraphId: "telegraph_heavy_melee"),
            ["atk_thrust"] = new Archetype(EnemyActionType.MeleeAttack, 1.8f, 0.5f, 0.4f, 2.2f, telegraphId: "telegraph_fast_melee"),
            ["atk_bite"] = new Archetype(EnemyActionType.MeleeAttack, 0.9f, 0.25f, 0.3f, 1.6f, telegraphId: "telegraph_fast_melee"),
            ["atk_claw"] = new Archetype(EnemyActionType.MeleeAttack, 1.0f, 0.25f, 0.3f, 1.8f, telegraphId: "telegraph_fast_melee"),
            ["atk_slam"] = new Archetype(EnemyActionType.MeleeAttack, 1.3f, 1.0f, 0.6f, 2.8f, telegraphId: "telegraph_heavy_melee"),
            ["atk_bash"] = new Archetype(EnemyActionType.DebuffStrike, 1.3f, 0.5f, 0.45f, 3.0f, telegraphId: "telegraph_heavy_melee"),
            ["atk_charge"] = new Archetype(EnemyActionType.MultiHitCharge, 4.0f, 0.9f, 0.6f, 4.5f, telegraphId: "telegraph_leap"),
            ["atk_leap"] = new Archetype(EnemyActionType.LeapStrike, 2.5f, 0.45f, 0.5f, 3.2f, telegraphId: "telegraph_leap"),
            ["atk_whip"] = new Archetype(EnemyActionType.MeleeAttack, 1.8f, 0.5f, 0.45f, 2.4f, telegraphId: "telegraph_fast_melee"),
            ["atk_bow"] = new Archetype(EnemyActionType.RangedProjectile, 6.5f, 0.6f, 0.4f, 2.5f, minRange: 2.0f, projectileSpeed: 6.5f, telegraphId: "telegraph_ranged_projectile"),
            ["atk_throw"] = new Archetype(EnemyActionType.RangedProjectile, 4.5f, 0.4f, 0.35f, 2.3f, minRange: 1.5f, projectileSpeed: 4.5f, telegraphId: "telegraph_ranged_projectile"),
            ["atk_spit"] = new Archetype(EnemyActionType.RangedProjectile, 4.5f, 0.4f, 0.35f, 2.2f, minRange: 1.5f, projectileSpeed: 5.0f, telegraphId: "telegraph_ranged_projectile"),
            ["atk_cast"] = new Archetype(EnemyActionType.CastProjectile, 4.5f, 0.8f, 0.5f, 2.8f, minRange: 2.0f, projectileSpeed: 5.5f, telegraphId: "telegraph_caster_spell"),
            ["atk_breath"] = new Archetype(EnemyActionType.TelegraphedAoE, 3.0f, 0.9f, 0.65f, 4.5f, areaRadius: 3.0f, telegraphId: "telegraph_area_pulse"),
            ["atk_nova"] = new Archetype(EnemyActionType.TelegraphedAoE, 2.2f, 0.8f, 0.55f, 4.5f, areaRadius: 2.0f, telegraphId: "telegraph_area_pulse"),
            ["atk_scream"] = new Archetype(EnemyActionType.DebuffStrike, 2.5f, 0.6f, 0.5f, 5.0f, areaRadius: 2.5f, telegraphId: "telegraph_area_pulse"),
            ["atk_burrow"] = new Archetype(EnemyActionType.BurrowStrike, 1.0f, 0.7f, 0.6f, 4.0f, telegraphId: "telegraph_burrow_emerge"),
            ["atk_blink"] = new Archetype(EnemyActionType.BlinkStrike, 1.0f, 0.35f, 0.4f, 3.5f, telegraphId: "telegraph_phase"),
            ["atk_summon"] = new Archetype(EnemyActionType.SummonAdds, 3.0f, 0.7f, 0.6f, 6.0f, telegraphId: "telegraph_caster_spell"),
            ["atk_buff"] = new Archetype(EnemyActionType.SelfBuff, 0f, 0.5f, 0.5f, 8.0f, telegraphId: "telegraph_caster_spell"),
            ["atk_flurry"] = new Archetype(EnemyActionType.ComboStrike, 1.1f, 0.3f, 0.35f, 3.0f, telegraphId: "telegraph_fast_melee"),
            // Pseudo-arquetipo atk_debuff: reusa a folha do golpe base — aqui mapeado como DebuffStrike
            // de perfil melee curto (dano do golpe base + status garantido), conforme IMPLEMENTATION §3.
            ["atk_debuff"] = new Archetype(EnemyActionType.DebuffStrike, 1.0f, 0.3f, 0.35f, 2.5f, telegraphId: "telegraph_fast_melee"),
        };

        /// <summary>Um kit por criatura: arquetipo Normal (melee OU ranged), arquetipo Especial, e
        /// parametros opcionais das 4 primitivas novas quando o Especial as usa. Dist == "R" gera
        /// kit de 3 (Normal ranged + fallback melee + Especial); caso contrario kit de 2.</summary>
        private struct KitEntry
        {
            public string EnemyId;
            public string NormalArchetype;
            public string SpecialArchetype;
            public bool RangedNormal;
            public string StatusId;
            public System.Action<EnemyActionSO> ConfigureSpecial;
        }

        // Plano de corpo -> arquetipo de melee fallback (catalogo §1b). Chave = Family (BestiaryCreatureDef).
        private static string FallbackMeleeArchetypeForFamily(string family)
        {
            switch ((family ?? string.Empty).ToLowerInvariant())
            {
                case "beast":
                case "dragon":
                case "dragon-kin":
                    return "atk_bite";
                case "construct":
                    return "atk_slam";
                case "plant":
                    return "atk_whip";
                case "elemental":
                case "aberration":
                case "undead":
                    return "atk_claw";
                case "humanoid":
                default:
                    return "atk_slash";
            }
        }

        public static void GenerateAll()
        {
            EnsureFolder(ActionsFolder);
            EnsureFolder(ActionSetsFolder);

            var enemyDataById = AssetDatabase.FindAssets("t:EnemyDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<EnemyDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.enemyId))
                .GroupBy(e => e.enemyId)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => AssetDatabase.GetAssetPath(e)).First());

            var bestiaryById = CanonicalBestiaryCatalog.All.ToDictionary(c => c.EnemyId, c => c);

            int createdActions = 0, realignedActions = 0, createdSets = 0, realignedSets = 0, skippedNoAsset = 0, wiredVariance = 0;

            foreach (var kit in BuildKitTable())
            {
                if (!enemyDataById.TryGetValue(kit.EnemyId, out var enemyData))
                {
                    skippedNoAsset++;
                    Debug.LogWarning($"[GenerateEnemyAttackKits] EnemyDataSO nao materializado para '{kit.EnemyId}' — kit pulado (fora de escopo criar o asset).");
                    continue;
                }

                bestiaryById.TryGetValue(kit.EnemyId, out var ficha);
                int baseDamage = ficha.Hp > 0 ? Mathf.Max(1, ficha.Damage) : Mathf.Max(1, enemyData.contactDamage * 4);
                if (baseDamage <= 1 && enemyData.maxHp > 0)
                    baseDamage = Mathf.Max(2, Mathf.RoundToInt(enemyData.maxHp * 0.15f));
                string damageTypeId = !string.IsNullOrWhiteSpace(ficha.PrimaryDamageTypeId) ? ficha.PrimaryDamageTypeId
                    : (!string.IsNullOrWhiteSpace(enemyData.PrimaryDamageTypeId) ? enemyData.PrimaryDamageTypeId : "physical");
                string family = ficha.Family;

                var actionIds = new List<string>();
                string enemyIdNoPrefix = kit.EnemyId.StartsWith("enemy_") ? kit.EnemyId.Substring("enemy_".Length) : kit.EnemyId;

                if (kit.RangedNormal)
                {
                    // Kit de 3: Normal ranged + melee de fallback + Especial.
                    string fallbackArchetype = FallbackMeleeArchetypeForFamily(family);
                    var fallbackId = $"action_{enemyIdNoPrefix}_melee";
                    UpsertAction(fallbackId, "MeleeFallback", fallbackArchetype, Mathf.Max(1, Mathf.RoundToInt(baseDamage * FallbackMeleeDamageMultiplier)),
                        damageTypeId, null, ref createdActions, ref realignedActions);
                    actionIds.Add(fallbackId);

                    var normalId = $"action_{enemyIdNoPrefix}_ranged";
                    UpsertAction(normalId, "Normal", kit.NormalArchetype, Mathf.RoundToInt(baseDamage * NormalDamageMultiplier),
                        damageTypeId, null, ref createdActions, ref realignedActions);
                    actionIds.Add(normalId);
                }
                else
                {
                    var normalId = $"action_{enemyIdNoPrefix}_melee";
                    UpsertAction(normalId, "Normal", kit.NormalArchetype, Mathf.RoundToInt(baseDamage * NormalDamageMultiplier),
                        damageTypeId, null, ref createdActions, ref realignedActions);
                    actionIds.Add(normalId);
                }

                var specialId = $"action_{enemyIdNoPrefix}_special";
                UpsertAction(specialId, "Special", kit.SpecialArchetype, Mathf.RoundToInt(baseDamage * SpecialDamageMultiplier),
                    damageTypeId, kit, ref createdActions, ref realignedActions);
                actionIds.Add(specialId);

                var setId = $"actionset_{kit.EnemyId}";
                UpsertActionSet(setId, actionIds.ToArray(), ref createdSets, ref realignedSets);

                SetEnemyActionSetId(enemyData, setId);
            }

            // Wireia as 53 variancias do Roster para a ActionSetId da mae (sem duplicar dados).
            foreach (var pair in VarianceToMotherCrosswalk)
            {
                string rosterId = "enemy_" + pair.Key;
                string motherId = "enemy_" + pair.Value;
                if (!enemyDataById.TryGetValue(rosterId, out var rosterData))
                {
                    Debug.LogWarning($"[GenerateEnemyAttackKits] Variancia '{rosterId}' sem EnemyDataSO — pulada.");
                    continue;
                }

                string motherSetId = $"actionset_{motherId}";
                if (AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>($"{ActionSetsFolder}/{motherSetId}.asset") == null)
                {
                    Debug.LogWarning($"[GenerateEnemyAttackKits] Mae '{motherId}' de '{rosterId}' nao tem actionset gerado ainda — variancia pulada nesta rodada.");
                    continue;
                }

                SetEnemyActionSetId(rosterData, motherSetId);
                wiredVariance++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GenerateEnemyAttackKits] Concluido. Actions criadas={createdActions}, realinhadas={realignedActions}; " +
                      $"ActionSets criados={createdSets}, realinhados={realignedSets}; Variancias wireadas={wiredVariance}; " +
                      $"Fichas sem EnemyDataSO (puladas)={skippedNoAsset}.");
        }

        // ── Upsert helpers (idempotentes — carregam por path antes de criar) ───────────────────

        private static void UpsertAction(string actionId, string label, string archetypeSlug, int baseDamage, string damageTypeId,
            KitEntry? specialConfig, ref int created, ref int realigned)
        {
            if (!Archetypes.TryGetValue(archetypeSlug, out var arch))
            {
                Debug.LogError($"[GenerateEnemyAttackKits] Arquetipo desconhecido '{archetypeSlug}' para acao '{actionId}'.");
                return;
            }

            string path = $"{ActionsFolder}/{actionId}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyActionSO>(path);
            bool isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<EnemyActionSO>();
                so.ActionId = actionId;
            }

            so.DisplayName = $"{label} ({archetypeSlug})";
            so.ActionType = arch.ActionType;
            so.DamageType = damageTypeId;
            so.BaseDamage = Mathf.Max(0, baseDamage);
            so.Range = arch.Range;
            so.MinRange = arch.MinRange;
            so.AreaRadius = arch.AreaRadius;
            so.CooldownSeconds = arch.Cooldown;
            so.WindupSeconds = arch.Windup;
            so.RecoverSeconds = arch.Recover;
            so.ProjectileSpeed = arch.ProjectileSpeed;
            so.TelegraphProfileId = arch.TelegraphId;
            so.TriggersVulnerabilityWindow = arch.ActionType != EnemyActionType.SelfBuff;

            // Especial: aplica configuracao das 4 primitivas novas (Rise-once/AllyHeal-Buff/Hazard/Pull)
            // quando o kit especifica (ConfigureSpecial), preservando defaults neutros nas demais acoes.
            if (specialConfig.HasValue)
                specialConfig.Value.ConfigureSpecial?.Invoke(so);

            if (isNew)
            {
                AssetDatabase.CreateAsset(so, path);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(so);
                realigned++;
            }
        }

        private static void UpsertActionSet(string setId, string[] actionIds, ref int created, ref int realigned)
        {
            string path = $"{ActionSetsFolder}/{setId}.asset";
            var so = AssetDatabase.LoadAssetAtPath<EnemyActionSetSO>(path);
            bool isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<EnemyActionSetSO>();
                so.ActionSetId = setId;
            }

            so.DisplayName = setId;
            so.ActionIds = actionIds;
            so.FallbackActionId = actionIds.Length > 0 ? actionIds[0] : string.Empty;

            if (isNew)
            {
                AssetDatabase.CreateAsset(so, path);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(so);
                realigned++;
            }
        }

        private static void SetEnemyActionSetId(EnemyDataSO enemyData, string actionSetId)
        {
            var so = new SerializedObject(enemyData);
            var prop = so.FindProperty("ActionSetId");
            if (prop == null)
            {
                Debug.LogError($"[GenerateEnemyAttackKits] EnemyDataSO '{enemyData.enemyId}' sem campo ActionSetId serializado.");
                return;
            }

            prop.stringValue = actionSetId;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enemyData);
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
