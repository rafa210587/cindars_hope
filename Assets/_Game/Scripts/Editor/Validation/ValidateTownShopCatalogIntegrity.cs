using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// FIX-001 — Town Shop Catalog Integrity Validator.
    ///
    /// Validates that the 5 core Town NPC shops (shop_thalindra, shop_corvus,
    /// shop_savra, shop_mirela, shop_hund) are present and that every ItemId
    /// in their catalogs exists in the ItemDatabaseSO.
    ///
    /// Prevents regression of shop-catalog mismatch errors that caused
    /// Debug.LogError at ShopManager.InitializeShop() runtime.
    ///
    /// Run via: CindarsHope/Validate/Validate Town Shop Catalog Integrity
    /// </summary>
    public static class ValidateTownShopCatalogIntegrity
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string ShopFolder = "Assets/_Game/Data/Economy";

        private static readonly string[] TownShopIds =
        {
            "shop_thalindra",
            "shop_corvus",
            "shop_savra",
            "shop_mirela",
            "shop_hund",
        };

        [MenuItem("CindarsHope/Validate/Validate Town Shop Catalog Integrity")]
        public static void Run()
        {
            var errors = Validate();
            foreach (var e in errors)
                Debug.LogError(e);

            if (errors.Count == 0)
                Debug.Log("ValidateTownShopCatalogIntegrity PASS: all 5 town shop catalogs are valid.");
            else
                throw new System.InvalidOperationException(
                    $"ValidateTownShopCatalogIntegrity failed with {errors.Count} issue(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();

            // Load ItemDatabase
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase == null)
            {
                errors.Add($"ItemDatabaseSO not found at '{ItemDatabasePath}'.");
                return errors;
            }

            // Load all ShopDataSO assets
            var allShops = LoadAllShops();
            var shopById = allShops.ToDictionary(s => s.Id, s => s);

            // Validate each required town shop
            foreach (var requiredId in TownShopIds)
            {
                if (!shopById.TryGetValue(requiredId, out var shop))
                {
                    errors.Add($"Required town shop '{requiredId}' not found in '{ShopFolder}'.");
                    continue;
                }

                ValidateShopCatalog(shop, itemDatabase, errors);
            }

            return errors;
        }

        private static List<ShopDataSO> LoadAllShops()
        {
            var shops = new List<ShopDataSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ShopDataSO", new[] { ShopFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                if (shop != null)
                    shops.Add(shop);
            }
            return shops;
        }

        private static void ValidateShopCatalog(
            ShopDataSO shop,
            ItemDatabaseSO itemDatabase,
            List<string> errors)
        {
            var path = AssetDatabase.GetAssetPath(shop);

            if (shop.Items == null || shop.Items.Length == 0)
            {
                errors.Add($"Shop '{shop.Id}' at '{path}' has no stock items.");
                return;
            }

            for (var i = 0; i < shop.Items.Length; i++)
            {
                var entry = shop.Items[i];
                if (entry == null)
                {
                    errors.Add($"Shop '{shop.Id}' stock[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    errors.Add($"Shop '{shop.Id}' stock[{i}] has empty ItemId.");
                    continue;
                }

                if (!itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                {
                    errors.Add(
                        $"Shop '{shop.Id}' stock[{i}] ItemId '{entry.ItemId}' " +
                        $"is absent from ItemDatabaseSO at '{ItemDatabasePath}'. " +
                        "Add the item asset to the ItemDatabase registry to fix this.");
                    continue;
                }

                if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                {
                    errors.Add(
                        $"Shop '{shop.Id}' stock[{i}] ItemId '{entry.ItemId}' has no valid price. " +
                        $"BaseValue={itemData.BaseValue}, BuyPriceOverride={entry.BuyPriceOverride}. " +
                        "Set either BaseValue on ItemDataSO or BuyPriceOverride on ShopDataSO.");
                }
            }
        }
    }
}
