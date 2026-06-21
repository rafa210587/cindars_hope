#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Magic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Magic
{
    /// <summary>
    /// fable_07 — gera 4 itens exemplares das fontes canônicas de magia e os registra no ItemDatabase.
    /// Idempotente por Id. NÃO altera SpellDataSO (catálogo). NÃO edita YAML manualmente — usa
    /// AssetDatabase/SerializedObject (rule unity-yaml-editing-policy).
    ///
    /// Itens:
    ///   scroll_learn_fire_spark  — LearnableScroll → ensina spell_fire_spark (consome).
    ///   scroll_cast_heal_minor   — CastScroll      → casta spell_heal_minor (consome, não ensina).
    ///   tome_ice_studies         — Tome            → ensina spell_ice_shard após 3 estudos.
    ///   wand_spark               — EquippedItem    → concede spell_spark enquanto equipada.
    ///
    /// As entradas de loja da Ozzra (Shop_Ozzra) são deixadas para o gerador de shop existente; este
    /// gerador foca nos itens (DEFERRED_SHOP_WIRING documentado no report).
    /// </summary>
    public static class GenerateSpellLearningItems
    {
        private const string ItemsPath = "Assets/_Game/Data/Items/";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        private struct SpellItemSpec
        {
            public string Id;
            public string DisplayName;
            public ItemCategory Category;
            public SpellSourceType Source;
            public string TaughtSpellId;
            public string EquippedSpellId;
            public int TomeUsesRequired;
            public string RequiredSkillNodeId;
            public int BaseValue;
            public int MaxStack;
            public bool IsEquippable;
        }

        public static void Generate()
        {
            var specs = BuildSpecs();
            var created = 0;
            var updated = 0;
            var assets = new List<ItemDataSO>();

            foreach (var spec in specs)
            {
                var path = $"{ItemsPath}{spec.Id}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<ItemDataSO>();
                    AssetDatabase.CreateAsset(asset, path);
                    created++;
                }
                else
                {
                    updated++;
                }

                asset.Id = spec.Id;
                asset.DisplayName = spec.DisplayName;
                asset.Description = $"fable_07 spell item: {spec.DisplayName} ({spec.Source}).";
                asset.Category = spec.Category;
                asset.MaxStack = spec.MaxStack;
                asset.BaseValue = spec.BaseValue;
                asset.IsEquippable = spec.IsEquippable;
                asset.SpellSource = spec.Source;
                asset.TaughtSpellId = spec.TaughtSpellId ?? string.Empty;
                asset.TomeUsesRequired = spec.TomeUsesRequired;
                asset.RequiredSkillNodeId = spec.RequiredSkillNodeId ?? string.Empty;
                // EquippedItem (wand): a magia vem do SpellId do item (caminho legado de cast).
                asset.SpellId = spec.EquippedSpellId ?? string.Empty;

                EditorUtility.SetDirty(asset);
                assets.Add(asset);
            }

            int registered = RegisterInItemDatabase(assets);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GenerateSpellLearningItems] Itens criados={created}, atualizados={updated}, registrados no ItemDatabase={registered}.");
        }

        private static IEnumerable<SpellItemSpec> BuildSpecs()
        {
            return new[]
            {
                new SpellItemSpec
                {
                    Id = "scroll_learn_fire_spark",
                    DisplayName = "Pergaminho: Faisca de Fogo",
                    Category = ItemCategory.Magic,
                    Source = SpellSourceType.LearnableScroll,
                    TaughtSpellId = "spell_fire_spark",
                    RequiredSkillNodeId = string.Empty,
                    BaseValue = 60,
                    MaxStack = 10,
                    IsEquippable = false
                },
                new SpellItemSpec
                {
                    Id = "scroll_cast_heal_minor",
                    DisplayName = "Pergaminho de Conjuracao: Cura Menor",
                    Category = ItemCategory.Magic,
                    Source = SpellSourceType.CastScroll,
                    TaughtSpellId = "spell_heal_minor",
                    BaseValue = 40,
                    MaxStack = 20,
                    IsEquippable = false
                },
                new SpellItemSpec
                {
                    Id = "tome_ice_studies",
                    DisplayName = "Tomo: Estudos do Gelo",
                    Category = ItemCategory.Magic,
                    Source = SpellSourceType.Tome,
                    TaughtSpellId = "spell_ice_shard",
                    TomeUsesRequired = 3,
                    BaseValue = 120,
                    MaxStack = 1,
                    IsEquippable = false
                },
                new SpellItemSpec
                {
                    Id = "wand_spark",
                    DisplayName = "Varinha de Faiscas",
                    Category = ItemCategory.Magic,
                    Source = SpellSourceType.EquippedItem,
                    // 'spell_spark' nunca existiu como SpellDataSO (era placeholder do exemplo fable_07),
                    // o que disparava SPELL_ID_NOT_IN_DB. Aponta para o bolt arcano básico real do
                    // SpellDatabase (arcane_projectile, gerado por GenerateShapeSpells) — fit de "faísca".
                    EquippedSpellId = "arcane_projectile",
                    BaseValue = 90,
                    MaxStack = 1,
                    IsEquippable = true
                }
            };
        }

        private static int RegisterInItemDatabase(List<ItemDataSO> assets)
        {
            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database == null)
            {
                Debug.LogWarning($"[GenerateSpellLearningItems] ItemDatabase nao encontrado em {ItemDatabasePath}; itens criados mas nao registrados.");
                return 0;
            }

            var so = new SerializedObject(database);
            var itemsProp = so.FindProperty("_items");
            if (itemsProp == null || !itemsProp.isArray)
            {
                Debug.LogWarning("[GenerateSpellLearningItems] Campo _items nao encontrado no ItemDatabase; registro pulado.");
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
    }
}
#endif
