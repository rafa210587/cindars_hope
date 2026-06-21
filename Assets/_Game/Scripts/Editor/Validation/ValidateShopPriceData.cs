using System;
using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateShopPriceData
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string ShopFolder = "Assets/_Game/Data/Economy";

        private static readonly string[] RequiredWave12CShops =
        {
            "shop_tovin",
            "shop_savra",
            "shop_dagna",
            "shop_thalindra",
            "shop_yael",
            "shop_ozzra",
            "shop_gurd",
            "shop_nimble",
            "shop_mara",
        };

        public static void Run()
        {
            var errors = Validate();
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError(error);
                }

                throw new InvalidOperationException($"ValidateShopPriceData failed with {errors.Count} issue(s).");
            }

            Debug.Log("ValidateShopPriceData passed: all ShopDataSO items exist and have valid buy prices.");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase == null)
            {
                errors.Add($"ItemDatabaseSO not found at '{ItemDatabasePath}'.");
                return errors;
            }

            var shops = LoadShops();
            if (shops.Count == 0)
            {
                errors.Add($"No ShopDataSO assets found in '{ShopFolder}'.");
                return errors;
            }

            ValidateRequiredWave12CShops(shops, errors);

            foreach (var shop in shops.OrderBy(shop => shop.Id, StringComparer.Ordinal))
            {
                ValidateShop(shop, itemDatabase, errors);
            }

            return errors;
        }

        private static List<ShopDataSO> LoadShops()
        {
            var shops = new List<ShopDataSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ShopDataSO", new[] { ShopFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                if (shop != null)
                {
                    shops.Add(shop);
                }
            }

            return shops;
        }

        private static void ValidateRequiredWave12CShops(IReadOnlyCollection<ShopDataSO> shops, List<string> errors)
        {
            foreach (var requiredShopId in RequiredWave12CShops)
            {
                if (!shops.Any(shop => shop != null && shop.Id == requiredShopId))
                {
                    errors.Add($"Required WAVE12C shop '{requiredShopId}' was not found.");
                }
            }
        }

        private static void ValidateShop(ShopDataSO shop, ItemDatabaseSO itemDatabase, List<string> errors)
        {
            var path = AssetDatabase.GetAssetPath(shop);
            if (string.IsNullOrWhiteSpace(shop.Id))
            {
                errors.Add($"ShopDataSO '{path}' has empty Id.");
            }

            if (shop.Items == null || shop.Items.Length == 0)
            {
                errors.Add($"ShopDataSO '{path}' shopId '{shop.Id}' has no stock items.");
                return;
            }

            for (var i = 0; i < shop.Items.Length; i++)
            {
                var entry = shop.Items[i];
                if (entry == null)
                {
                    errors.Add($"ShopDataSO '{path}' shopId '{shop.Id}' stock[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    errors.Add($"ShopDataSO '{path}' shopId '{shop.Id}' stock[{i}] has empty ItemId.");
                    continue;
                }

                if (!itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                {
                    errors.Add($"ShopDataSO '{path}' shopId '{shop.Id}' stock[{i}] item '{entry.ItemId}' is absent from ItemDatabaseSO.");
                    continue;
                }

                if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                {
                    errors.Add($"ShopDataSO '{path}' shopId '{shop.Id}' stock[{i}] item '{entry.ItemId}' has invalid price. BaseValue={itemData.BaseValue}, BuyPriceOverride={entry.BuyPriceOverride}.");
                }
            }
        }
    }
}
