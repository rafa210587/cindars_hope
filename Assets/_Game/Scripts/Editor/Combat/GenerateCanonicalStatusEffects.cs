using System.Collections.Generic;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Combat
{
    /// <summary>
    /// F01 — gera/atualiza os 13 status canônicos (10 da direction + Slow/HeatStress/ColdStress
    /// da emenda) e registra todos no StatusEffectDatabase. Idempotente por ID.
    /// Hunger/Fatigue NÃO têm asset (geridos por HungerManager/F16).
    /// </summary>
    public static class GenerateCanonicalStatusEffects
    {
        private const string AssetFolder = "Assets/_Game/Data/Combat/StatusEffects";
        private const string DatabasePath = "Assets/_Game/Data/Combat/StatusEffectDatabase.asset";

        // spec_content_enemy_status_kit_ids_v1 — 12 IDs referenciados por 43 EnemyActionSO
        // (kits "minor"/buff) que nunca foram materializados no database. IDs como const
        // (rule id-stability), prefixo status_.
        public const string StatusSlowMinor = "status_slow_minor";
        public const string StatusBurnMinor = "status_burn_minor";
        public const string StatusChillMinor = "status_chill_minor";
        public const string StatusPoisonMinor = "status_poison_minor";
        public const string StatusBleedMinor = "status_bleed_minor";
        public const string StatusConfuseMinor = "status_confuse_minor";
        public const string StatusRootMinor = "status_root_minor";
        public const string StatusHaste = "status_haste";
        public const string StatusShield = "status_shield";
        public const string StatusFrenzy = "status_frenzy";
        public const string StatusRegen = "status_regen";
        public const string StatusGuard = "status_guard";

        // Fórmula "minor" (spec §20): minor = MinorEffectScale do efeito do status pai — placeholder
        // conservador; balance fino fica fora de escopo (skill economy-balance-tuning). Fallbacks só
        // disparam se um "_minor" referenciar um pai ausente do database (edge case §23; hoje não
        // ocorre para os 7 minors abaixo, todos com pai canônico já materializado).
        private const float MinorEffectScale = 0.60f;
        private const int FallbackMinorDamagePerTurn = 1;
        private const int FallbackMinorDurationTurns = 3;

        // Buffs (haste/shield/frenzy/regen/guard) não têm "_minor" nem pai no database canônico
        // (§23): duração default nomeada, sem efeito numérico de balance (fora de escopo).
        private const int BuffDurationTurns = 5;

        // Buffs de inimigo (haste/shield/frenzy/regen/guard) usam o tipo dedicado StatusEffectType.Buff
        // (adicionado ao enum nesta spec, em vez de reusar Vulnerable, que é debuff). O comportamento
        // runtime de buff é feature à parte; aqui só o tipo/ID são materializados (tipos não mapeados
        // no ticker seguem com default neutro — nenhum dano-por-turno indevido).
        private const StatusEffectType BuffMarkerType = StatusEffectType.Buff;

        private static readonly Color BuffColor = new Color(0.8f, 0.75f, 0.2f);

        private struct StatusSpec
        {
            public string Id;
            public string Name;
            public StatusEffectType Type;
            public int DurationTurns;
            public int DamagePerTurn;
            public float MoveSpeedMultiplier;
            public float BehaviorOverrideSeconds;
            public float DurabilityWearMultiplier;
            public Color Color;
        }

        // (Id do kit, Id do status pai canônico ou null p/ buffs, nome PT-BR de exibição).
        private static readonly (string Id, string ParentId, string Name)[] MinorAndBuffDefs =
        {
            (StatusSlowMinor, "status_slow", "Lentidao Menor"),
            (StatusBurnMinor, "status_burn", "Queimadura Menor"),
            (StatusChillMinor, "status_chill", "Congelamento Menor"),
            (StatusPoisonMinor, "status_poison", "Veneno Menor"),
            (StatusBleedMinor, "status_bleed", "Sangramento Menor"),
            (StatusConfuseMinor, "status_confusion_lite", "Confusao Menor"),
            (StatusRootMinor, "status_root", "Enraizamento Menor"),
            (StatusHaste, null, "Aceleracao"),
            (StatusShield, null, "Escudo"),
            (StatusFrenzy, null, "Frenesi"),
            (StatusRegen, null, "Regeneracao"),
            (StatusGuard, null, "Guarda"),
        };

        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(AssetFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data/Combat", "StatusEffects");
            }

            var specs = BuildCanonicalSpecs();
            var created = 0;
            var updated = 0;
            var assets = new List<StatusEffectSO>();

            foreach (var spec in specs)
            {
                var path = $"{AssetFolder}/{spec.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<StatusEffectSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                asset.Id = spec.Id;
                asset.DisplayName = spec.Name;
                asset.Description = $"Status canonico: {spec.Name}";
                asset.Type = spec.Type;
                asset.DurationTurns = spec.DurationTurns;
                asset.DamagePerTurn = spec.DamagePerTurn;
                asset.MoveSpeedMultiplier = spec.MoveSpeedMultiplier;
                asset.BehaviorOverrideSeconds = spec.BehaviorOverrideSeconds;
                asset.DurabilityWearMultiplier = spec.DurabilityWearMultiplier;
                asset.VisualColor = spec.Color;
                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            // spec_content_enemy_status_kit_ids_v1 — após o bloco dos 13 status canônicos, materializa
            // os 12 IDs "minor"/buff referenciados pelos kits de ataque de inimigo (Fase 1, §20).
            var canonicalSnapshot = new List<StatusEffectSO>(assets);
            foreach (var def in MinorAndBuffDefs)
            {
                var path = $"{AssetFolder}/{def.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<StatusEffectSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                ApplyMinorOrBuffFields(asset, def, canonicalSnapshot);
                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            RegisterInDatabase(assets);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GenerateCanonicalStatusEffects] {created} criados, {updated} atualizados, {assets.Count} registrados no database. Pasta: {AssetFolder}");
        }

        // Fórmula "minor"/buff (spec_content_enemy_status_kit_ids_v1 §20, adaptada ao schema real de
        // StatusEffectSO — a spec assumia campos Kind/Magnitude/MaxStacks que não existem; o
        // equivalente real é Type/DamagePerTurn/MoveSpeedMultiplier/BehaviorOverrideSeconds).
        private static void ApplyMinorOrBuffFields(
            StatusEffectSO asset,
            (string Id, string ParentId, string Name) def,
            List<StatusEffectSO> canonicalAssets)
        {
            bool isMinorVariant = !string.IsNullOrEmpty(def.ParentId);
            StatusEffectSO parent = isMinorVariant ? canonicalAssets.Find(a => a.Id == def.ParentId) : null;

            if (isMinorVariant && parent == null)
            {
                Debug.LogWarning($"[GenerateCanonicalStatusEffects] '{def.Id}': pai '{def.ParentId}' nao encontrado no database; aplicando fallback (edge case spec_content_enemy_status_kit_ids_v1 §23).");
            }

            asset.Id = def.Id;
            asset.DisplayName = def.Name;
            asset.Description = parent != null
                ? $"Variante menor de {parent.DisplayName} ({MinorEffectScale:P0} do efeito)."
                : "Buff de kit de inimigo (spec_content_enemy_status_kit_ids_v1); sem novo comportamento de aplicacao nesta spec.";
            asset.Type = parent != null ? parent.Type : BuffMarkerType;
            asset.DurationTurns = parent != null
                ? parent.DurationTurns
                : (isMinorVariant ? FallbackMinorDurationTurns : BuffDurationTurns);
            asset.DamagePerTurn = parent != null
                ? Mathf.RoundToInt(parent.DamagePerTurn * MinorEffectScale)
                : (isMinorVariant ? FallbackMinorDamagePerTurn : 0);
            asset.MoveSpeedMultiplier = parent != null
                ? Mathf.Clamp01(1f - (1f - parent.MoveSpeedMultiplier) * MinorEffectScale)
                : 1f;
            asset.BehaviorOverrideSeconds = parent != null ? parent.BehaviorOverrideSeconds * MinorEffectScale : 0f;
            asset.DurabilityWearMultiplier = parent != null ? parent.DurabilityWearMultiplier : 1f;
            asset.VisualColor = parent != null ? parent.VisualColor : BuffColor;
        }

        private static List<StatusSpec> BuildCanonicalSpecs()
        {
            return new List<StatusSpec>
            {
                new StatusSpec { Id = "status_bleed", Name = "Sangramento", Type = StatusEffectType.Bleed, DurationTurns = 5, DamagePerTurn = 2, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 1f, Color = new Color(0.7f, 0.1f, 0.1f) },
                new StatusSpec { Id = "status_burn", Name = "Queimadura", Type = StatusEffectType.Burn, DurationTurns = 4, DamagePerTurn = 3, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 1f, Color = new Color(1f, 0.45f, 0.1f) },
                new StatusSpec { Id = "status_chill", Name = "Congelamento", Type = StatusEffectType.Chill, DurationTurns = 3, DamagePerTurn = 0, MoveSpeedMultiplier = 0.5f, DurabilityWearMultiplier = 1f, Color = new Color(0.5f, 0.8f, 1f) },
                new StatusSpec { Id = "status_poison", Name = "Veneno", Type = StatusEffectType.Poison, DurationTurns = 6, DamagePerTurn = 2, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 1f, Color = new Color(0.4f, 0.8f, 0.2f) },
                new StatusSpec { Id = "status_stun", Name = "Atordoamento", Type = StatusEffectType.Stun, DurationTurns = 2, DamagePerTurn = 0, MoveSpeedMultiplier = 0f, DurabilityWearMultiplier = 1f, Color = new Color(1f, 0.95f, 0.4f) },
                new StatusSpec { Id = "status_root", Name = "Enraizamento", Type = StatusEffectType.Root, DurationTurns = 2, DamagePerTurn = 0, MoveSpeedMultiplier = 0f, DurabilityWearMultiplier = 1f, Color = new Color(0.45f, 0.3f, 0.15f) },
                new StatusSpec { Id = "status_fear", Name = "Medo", Type = StatusEffectType.Fear, DurationTurns = 2, DamagePerTurn = 0, MoveSpeedMultiplier = 1f, BehaviorOverrideSeconds = 1.5f, DurabilityWearMultiplier = 1f, Color = new Color(0.5f, 0.2f, 0.6f) },
                new StatusSpec { Id = "status_confusion_lite", Name = "Confusao Leve", Type = StatusEffectType.ConfusionLite, DurationTurns = 3, DamagePerTurn = 0, MoveSpeedMultiplier = 1f, BehaviorOverrideSeconds = 2f, DurabilityWearMultiplier = 1f, Color = new Color(0.9f, 0.6f, 0.9f) },
                new StatusSpec { Id = "status_durability_stress", Name = "Estresse de Durabilidade", Type = StatusEffectType.DurabilityStress, DurationTurns = 10, DamagePerTurn = 0, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 2f, Color = new Color(0.6f, 0.6f, 0.6f) },
                new StatusSpec { Id = "status_corruption", Name = "Corrupcao", Type = StatusEffectType.Corruption, DurationTurns = 8, DamagePerTurn = 1, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 1f, Color = new Color(0.2f, 0.05f, 0.3f) },
                // Emenda 2026-06-12: COMBAT_CORE §31 — 3 status adicionais.
                new StatusSpec { Id = "status_slow", Name = "Lentidao", Type = StatusEffectType.Slow, DurationTurns = 4, DamagePerTurn = 0, MoveSpeedMultiplier = 0.7f, DurabilityWearMultiplier = 1f, Color = new Color(0.7f, 0.7f, 0.5f) },
                new StatusSpec { Id = "status_heat_stress", Name = "Estresse de Calor", Type = StatusEffectType.HeatStress, DurationTurns = 6, DamagePerTurn = 1, MoveSpeedMultiplier = 1f, DurabilityWearMultiplier = 1f, Color = new Color(1f, 0.6f, 0.3f) },
                new StatusSpec { Id = "status_cold_stress", Name = "Estresse de Frio", Type = StatusEffectType.ColdStress, DurationTurns = 6, DamagePerTurn = 1, MoveSpeedMultiplier = 0.85f, DurabilityWearMultiplier = 1f, Color = new Color(0.6f, 0.8f, 0.95f) }
            };
        }

        private static void RegisterInDatabase(List<StatusEffectSO> canonicalAssets)
        {
            var database = AssetDatabase.LoadAssetAtPath<StatusEffectDatabaseSO>(DatabasePath);
            if (database == null)
            {
                // Self-healing: o asset pode existir em disco mas carregar como null quando o m_Script
                // guard aponta para um guid placeholder que nao casa com StatusEffectDatabaseSO.cs.meta.
                // Nesse caso, deletamos o asset quebrado e recriamos via API — o Unity atribui o guid
                // CORRETO do script no CreateAsset, e o merge-por-id abaixo repopula as entradas.
                if (AssetDatabase.LoadMainAssetAtPath(DatabasePath) != null || System.IO.File.Exists(DatabasePath))
                {
                    AssetDatabase.DeleteAsset(DatabasePath);
                }

                EnsureDatabaseFolder();
                database = ScriptableObject.CreateInstance<StatusEffectDatabaseSO>();
                AssetDatabase.CreateAsset(database, DatabasePath);
                Debug.Log("[GenerateCanonicalStatusEffects] StatusEffectDatabase recriado (script guid estava quebrado).");
            }

            var serialized = new SerializedObject(database);
            var itemsProperty = serialized.FindProperty("_items");

            // Preserva entradas existentes que não são canônicas; substitui/insere canônicas por ID.
            var merged = new List<StatusEffectSO>();
            for (var i = 0; i < itemsProperty.arraySize; i++)
            {
                var existing = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as StatusEffectSO;
                if (existing != null && !canonicalAssets.Exists(a => a.Id == existing.Id))
                {
                    merged.Add(existing);
                }
            }

            merged.AddRange(canonicalAssets);

            itemsProperty.arraySize = merged.Count;
            for (var i = 0; i < merged.Count; i++)
            {
                itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = merged[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        // Garante que Assets/_Game/Data/Combat existe antes de recriar o database asset.
        private static void EnsureDatabaseFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data/Combat"))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Combat");
            }
        }
    }
}
