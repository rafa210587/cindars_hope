#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.Crafting;
using CindarsHope.Economy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// fable_49 CA-1 — valida que NENHUM ShopDataSO vende item de gear tier alto (Mithril/Bromecian/
    /// Blackstone/Meteoric). Regra dura do ITEM_CATALOG §12: tier alto SOMENTE craft/têmpera, nunca loja.
    /// Erro por ocorrência (shop + itemId). Espelha o padrão dos validadores de catálogo (MenuItem +
    /// contagem de erros + throw em falha).
    ///
    /// Run via: CindarsHope/Validate/Validate High-Tier Gear Not In Shops (fable_49)
    /// </summary>
    public static class ValidateHighTierGearNotInShops
    {
        private const string ShopFolder = "Assets/_Game/Data/Economy";

        [MenuItem("CindarsHope/Validate/Validate High-Tier Gear Not In Shops (fable_49)")]
        public static void Run()
        {
            var errors = Validate();
            foreach (var e in errors)
                Debug.LogError(e);

            if (errors.Count == 0)
                Debug.Log($"ValidateHighTierGearNotInShops PASS: no high-tier gear sold in any shop under '{ShopFolder}'.");
            else
                throw new System.InvalidOperationException(
                    $"ValidateHighTierGearNotInShops failed with {errors.Count} issue(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            var highTier = HighTierGearCatalog.ItemIdSet();

            var guids = AssetDatabase.FindAssets("t:ShopDataSO", new[] { ShopFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                if (shop == null || shop.Items == null)
                {
                    continue;
                }

                foreach (var entry in shop.Items)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                    {
                        continue;
                    }

                    if (highTier.Contains(entry.ItemId))
                    {
                        errors.Add($"Shop '{shop.Id}' ({path}) sells high-tier gear '{entry.ItemId}' — forbidden by ITEM_CATALOG §12 (craft/temper only).");
                    }
                }
            }

            return errors;
        }
    }
}
#endif
