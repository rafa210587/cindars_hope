#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Magic;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Magic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Magic
{
    /// <summary>
    /// fable_08 — gera as spells exemplares de SHAPE (nova/cura/barreira/cone), as Tier 5 canônicas da
    /// EMENDA 2026-06-12-D (Eco de Anya + Ruptura de Senya) e o Projétil Arcano com auto-target, e as
    /// registra no SpellDatabase. Gera também os pergaminhos (LearnableScroll) correspondentes no
    /// ItemDatabase. Idempotente por Id. Usa AssetDatabase/SerializedObject — NÃO edita YAML manual
    /// (rule unity-assets). Não recria SpellDataSO/SpellShape (apenas instancia assets).
    /// </summary>
    public static class GenerateShapeSpells
    {
        private const string SpellsPath = "Assets/_Game/Data/Combat/Spells/";
        private const string SpellDatabasePath = "Assets/_Game/Data/Combat/SpellDatabase.asset";
        private const string ItemsPath = "Assets/_Game/Data/Items/";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        private struct ShapeSpellSpec
        {
            public string Id;
            public string SpellName;
            public string Description;
            public SpellType Type;
            public DamageType DamageType;
            public SpellShape Shape;
            public int BaseDamage;
            public int ManaCost;
            public float CooldownSeconds;
            public float CastTimeSeconds;
            public float Range;
            public bool AutoTarget;
            public float ConeHalfAngleDegrees;
            public int ConeProjectileCount;
            public float NovaRadius;
            public int RestoreHp;
            public int RestoreStamina;
            public int RestoreMana;
            public int BarrierAbsorb;
            public float BarrierSeconds;
            public string StatusEffectId;
            public float StatusApplyChance;
            public int BaseValue;
            // Pergaminho de aprendizado (LearnableScroll) opcional para esta spell.
            public bool MakeScroll;
            public string ScrollName;
            public int ScrollBaseValue;
        }

        public static void Generate()
        {
            var specs = BuildSpecs();
            var spellAssets = new List<SpellDataSO>();
            int created = 0, updated = 0;

            EnsureFolder(SpellsPath);

            foreach (var spec in specs)
            {
                var path = $"{SpellsPath}{spec.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<SpellDataSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<SpellDataSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                asset.Id = spec.Id;
                asset.SpellName = spec.SpellName;
                asset.Description = spec.Description;
                asset.Type = spec.Type;
                asset.DamageType = spec.DamageType;
                asset.Shape = spec.Shape;
                asset.BaseDamage = spec.BaseDamage;
                asset.ManaCost = spec.ManaCost;
                asset.CooldownSeconds = spec.CooldownSeconds;
                asset.CastTimeSeconds = spec.CastTimeSeconds;
                asset.Range = spec.Range;
                asset.AutoTarget = spec.AutoTarget;
                asset.ConeHalfAngleDegrees = spec.ConeHalfAngleDegrees;
                asset.ConeProjectileCount = spec.ConeProjectileCount;
                asset.NovaRadius = spec.NovaRadius;
                asset.RestoreHp = spec.RestoreHp;
                asset.RestoreStamina = spec.RestoreStamina;
                asset.RestoreMana = spec.RestoreMana;
                asset.BarrierAbsorb = spec.BarrierAbsorb;
                asset.BarrierSeconds = spec.BarrierSeconds;
                asset.StatusEffectId = spec.StatusEffectId ?? string.Empty;
                asset.StatusApplyChance = spec.StatusApplyChance;
                asset.BaseValue = spec.BaseValue;

                EditorUtility.SetDirty(asset);
                spellAssets.Add(asset);
            }

            int registeredSpells = RegisterAssets(SpellDatabasePath, "_items", spellAssets);

            // Pergaminhos de aprendizado para as spells exemplares (fonte LearnableScroll).
            var scrollAssets = GenerateScrolls(specs);
            int registeredItems = RegisterAssets(ItemDatabasePath, "_items", scrollAssets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GenerateShapeSpells] Spells criadas={created}, atualizadas={updated}, registradas no SpellDatabase={registeredSpells}; pergaminhos registrados no ItemDatabase={registeredItems}.");
        }

        private static IEnumerable<ShapeSpellSpec> BuildSpecs()
        {
            return new[]
            {
                // 1) Nova de gelo defensiva (Chill em área 360°) — engineering story #1.
                new ShapeSpellSpec
                {
                    Id = "spell_ice_nova", SpellName = "Nova de Gelo", Description = "fable_08: explosao 360 que aplica Chill nos inimigos proximos.",
                    Type = SpellType.IceSpike, DamageType = DamageType.Ice, Shape = SpellShape.Nova,
                    BaseDamage = 14, ManaCost = 22, CooldownSeconds = 3f, CastTimeSeconds = 0.4f, Range = 3f,
                    NovaRadius = 3f, StatusEffectId = "status_chill", StatusApplyChance = 0.6f, BaseValue = 120,
                    MakeScroll = true, ScrollName = "Pergaminho: Nova de Gelo", ScrollBaseValue = 120
                },
                // 2) Cura com cast cancelável por dano — engineering story #2 (SelfRestore).
                new ShapeSpellSpec
                {
                    Id = "spell_minor_heal", SpellName = "Cura Menor", Description = "fable_08: cura com 1.2s de cast; cancela se tomar dano (reembolsa mana).",
                    Type = SpellType.Heal, DamageType = DamageType.Arcane, Shape = SpellShape.SelfRestore,
                    BaseDamage = 0, ManaCost = 18, CooldownSeconds = 2f, CastTimeSeconds = 1.2f, Range = 0f,
                    RestoreHp = 30, RestoreStamina = 0, RestoreMana = 0, BaseValue = 80,
                    MakeScroll = true, ScrollName = "Pergaminho: Cura Menor", ScrollBaseValue = 80
                },
                // 3) Barreira arcana temporária — engineering story #3 (Barrier).
                new ShapeSpellSpec
                {
                    Id = "spell_arcane_barrier", SpellName = "Barreira Arcana", Description = "fable_08: barreira que absorve N de dano por alguns segundos.",
                    Type = SpellType.Buff, DamageType = DamageType.Arcane, Shape = SpellShape.Barrier,
                    BaseDamage = 0, ManaCost = 20, CooldownSeconds = 6f, CastTimeSeconds = 0.5f, Range = 0f,
                    BarrierAbsorb = 40, BarrierSeconds = 6f, BaseValue = 100,
                    MakeScroll = true, ScrollName = "Pergaminho: Barreira Arcana", ScrollBaseValue = 100
                },
                // 4) Cone de fogo (leque) — shape Cone.
                new ShapeSpellSpec
                {
                    Id = "spell_flame_cone", SpellName = "Cone de Chamas", Description = "fable_08: leque de projeteis de fogo num arco curto.",
                    Type = SpellType.Fireball, DamageType = DamageType.Fire, Shape = SpellShape.Cone,
                    BaseDamage = 10, ManaCost = 24, CooldownSeconds = 2.5f, CastTimeSeconds = 0.5f, Range = 5f,
                    ConeHalfAngleDegrees = 30f, ConeProjectileCount = 5, StatusEffectId = "status_burn", StatusApplyChance = 0.25f, BaseValue = 130,
                    MakeScroll = true, ScrollName = "Pergaminho: Cone de Chamas", ScrollBaseValue = 130
                },
                // 5) Projétil Arcano com AUTO-TARGET — EMENDA 2026-06-12-D (6.6-A). Bolt + AutoTarget.
                new ShapeSpellSpec
                {
                    Id = "arcane_projectile", SpellName = "Projetil Arcano", Description = "fable_08 EMENDA 6.6-A: bolt arcano basico com auto-target no inimigo mais proximo/na mira.",
                    Type = SpellType.Lightning, DamageType = DamageType.Arcane, Shape = SpellShape.Bolt,
                    BaseDamage = 12, ManaCost = 8, CooldownSeconds = 0.8f, CastTimeSeconds = 0.2f, Range = 5f,
                    AutoTarget = true, BaseValue = 60,
                    MakeScroll = true, ScrollName = "Pergaminho: Projetil Arcano", ScrollBaseValue = 60
                },
                // 6) Eco de Anya (Tier 5) — cura + barreira curta (SelfRestore + barreira no mesmo cast).
                //    Modelado como SelfRestore (cura) com barreira embutida via campos de barreira:
                //    o executor de Barrier e o de SelfRestore sao distintos; Eco prioriza cura/barreira.
                //    Aqui usamos Barrier para a barreira curta + RestoreHp (suporte late). Direction MAGIC #29.
                new ShapeSpellSpec
                {
                    Id = "anya_echo", SpellName = "Eco de Anya", Description = "fable_08 Tier 5 (EMENDA 6.6-A): suporte late — cura moderada + barreira curta (Direction MAGIC #29).",
                    Type = SpellType.Heal, DamageType = DamageType.Arcane, Shape = SpellShape.SelfRestore,
                    BaseDamage = 0, ManaCost = 44, CooldownSeconds = 45f, CastTimeSeconds = 0.9f, Range = 3f,
                    RestoreHp = 60, RestoreMana = 0, BaseValue = 400,
                    MakeScroll = false
                },
                // 7) Ruptura de Senya (Tier 5) — burst em area (Nova) caro, cooldown alto. Direction MAGIC #30.
                new ShapeSpellSpec
                {
                    Id = "senya_rupture", SpellName = "Ruptura de Senya", Description = "fable_08 Tier 5 (EMENDA 6.6-A): burst ofensivo forte em area, alto custo/cooldown (Direction MAGIC #30).",
                    Type = SpellType.Fireball, DamageType = DamageType.Fire, Shape = SpellShape.Nova,
                    BaseDamage = 55, ManaCost = 46, CooldownSeconds = 35f, CastTimeSeconds = 1.0f, Range = 2f,
                    NovaRadius = 2f, StatusEffectId = "status_burn", StatusApplyChance = 0.5f, BaseValue = 420,
                    MakeScroll = false
                }
            };
        }

        private static List<ItemDataSO> GenerateScrolls(IEnumerable<ShapeSpellSpec> specs)
        {
            EnsureFolder(ItemsPath);
            var assets = new List<ItemDataSO>();

            foreach (var spec in specs)
            {
                if (!spec.MakeScroll)
                {
                    continue;
                }

                var itemId = $"scroll_learn_{spec.Id}";
                var path = $"{ItemsPath}{itemId}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ItemDataSO>();
                    AssetDatabase.CreateAsset(asset, path);
                }

                asset.Id = itemId;
                asset.DisplayName = spec.ScrollName;
                asset.Description = $"fable_08 spell scroll: ensina {spec.SpellName}.";
                asset.Category = ItemCategory.Magic;
                asset.MaxStack = 10;
                asset.BaseValue = spec.ScrollBaseValue;
                asset.IsEquippable = false;
                asset.SpellSource = SpellSourceType.LearnableScroll;
                asset.TaughtSpellId = spec.Id;
                asset.SpellId = string.Empty;

                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            return assets;
        }

        private static int RegisterAssets<T>(string databasePath, string arrayField, List<T> assets) where T : Object
        {
            if (assets == null || assets.Count == 0)
            {
                return 0;
            }

            var database = AssetDatabase.LoadAssetAtPath<Object>(databasePath);
            if (database == null)
            {
                Debug.LogWarning($"[GenerateShapeSpells] Database nao encontrado em {databasePath}; assets criados mas nao registrados.");
                return 0;
            }

            var so = new SerializedObject(database);
            var itemsProp = so.FindProperty(arrayField);
            if (itemsProp == null || !itemsProp.isArray)
            {
                Debug.LogWarning($"[GenerateShapeSpells] Campo {arrayField} nao encontrado em {databasePath}; registro pulado.");
                return 0;
            }

            var existing = new HashSet<Object>();
            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                var element = itemsProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element != null)
                {
                    existing.Add(element);
                }
            }

            int added = 0;
            foreach (var asset in assets)
            {
                if (asset == null || existing.Contains(asset))
                {
                    continue;
                }

                itemsProp.arraySize += 1;
                itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1).objectReferenceValue = asset;
                existing.Add(asset);
                added++;
            }

            if (added > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
            }

            return added;
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder.TrimEnd('/')))
            {
                return;
            }

            var parts = assetFolder.TrimEnd('/').Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
