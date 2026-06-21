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

            RegisterInDatabase(assets);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GenerateCanonicalStatusEffects] {created} criados, {updated} atualizados, {assets.Count} registrados no database. Pasta: {AssetFolder}");
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
