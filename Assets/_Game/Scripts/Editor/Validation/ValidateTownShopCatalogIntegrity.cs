using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace CindarsHope.Editor.Validation
{
    /// <summary>
    /// FIX-001B — Town Shop Catalog Integrity Validator (expanded scope).
    ///
    /// Validates ALL ShopDataSO assets in Assets/_Game/Data/Economy:
    ///   - Each shop has a non-empty Id
    ///   - No duplicate shop Ids
    ///   - Each shop has at least 1 stock item
    ///   - Every ItemId in all catalogs exists in the ItemDatabaseSO
    ///   - Every item has a valid price (BuyPriceOverride > 0 OR itemData.BaseValue > 0)
    ///
    /// This covers all 25 NPC shop assets including all NPCs referenced by
    /// CreateMvpTownScene.RefinedCanonicalTownNpcSpecs and additional shop
    /// assets (Blacksmith, Cave_Supplies, General_Store, Seeds_Tools, Weapons_Armor).
    ///
    /// Prevents regression of shop-catalog mismatch errors that caused
    /// Debug.LogError at ShopManager.InitializeShop() runtime.
    ///
    /// Previous scope (FIX-001): 5 hardcoded town shops.
    /// Current scope (FIX-001B): ALL ShopDataSO assets in Data/Economy folder.
    ///
    /// Run via: CindarsHope/Validate/Validate Town Shop Catalog Integrity
    /// </summary>
    public static class ValidateTownShopCatalogIntegrity
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string ShopFolder = "Assets/_Game/Data/Economy";

        public static void Run()
        {
            var errors = Validate();
            foreach (var e in errors)
                Debug.LogError(e);

            if (errors.Count == 0)
                Debug.Log($"ValidateTownShopCatalogIntegrity PASS: all shop catalogs in '{ShopFolder}' are valid.");
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

            // Load ALL ShopDataSO assets in the Economy folder
            var allShops = LoadAllShops();
            if (allShops.Count == 0)
            {
                errors.Add($"No ShopDataSO assets found in '{ShopFolder}'.");
                return errors;
            }

            // Track duplicate Ids
            var seenIds = new HashSet<string>();

            // Validate each shop
            foreach (var shop in allShops)
            {
                var path = AssetDatabase.GetAssetPath(shop);

                // Id validation
                if (string.IsNullOrWhiteSpace(shop.Id))
                {
                    errors.Add($"Shop at '{path}' has empty Id.");
                }
                else if (!seenIds.Add(shop.Id))
                {
                    errors.Add($"Duplicate shop Id '{shop.Id}' found at '{path}'.");
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
            var shopLabel = string.IsNullOrWhiteSpace(shop.Id) ? path : shop.Id;

            if (shop.Items == null || shop.Items.Length == 0)
            {
                errors.Add($"Shop '{shopLabel}' at '{path}' has no stock items.");
                return;
            }

            for (var i = 0; i < shop.Items.Length; i++)
            {
                var entry = shop.Items[i];
                if (entry == null)
                {
                    errors.Add($"Shop '{shopLabel}' stock[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    errors.Add($"Shop '{shopLabel}' stock[{i}] has empty ItemId.");
                    continue;
                }

                if (!itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                {
                    errors.Add(
                        $"Shop '{shopLabel}' stock[{i}] ItemId '{entry.ItemId}' " +
                        $"is absent from ItemDatabaseSO at '{ItemDatabasePath}'. " +
                        "Add the item asset to the ItemDatabase registry to fix this.");
                    continue;
                }

                if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                {
                    errors.Add(
                        $"Shop '{shopLabel}' stock[{i}] ItemId '{entry.ItemId}' has no valid price. " +
                        $"BaseValue={itemData.BaseValue}, BuyPriceOverride={entry.BuyPriceOverride}. " +
                        "Set either BaseValue on ItemDataSO or BuyPriceOverride on ShopDataSO.");
                }
            }
        }

        /// <summary>
        /// fable_76 T5 — Price range bounds check across all ShopDataSO assets.
        /// Warns if effective price > 3x BaseValue; errors if effective price < 0.1x BaseValue.
        /// Returns (warnings, errors) logged counts.
        /// </summary>
        public static void RunPriceRanges()
        {
            var (warnings, errors) = ValidatePriceRanges();
            if (errors == 0 && warnings == 0)
                Debug.Log("ValidateShopPriceRanges PASS: all shop prices within canonical bounds.");
            else
                Debug.Log($"ValidateShopPriceRanges: {warnings} warning(s), {errors} error(s). See log above.");
        }

        public static (int warnings, int errors) ValidatePriceRanges()
        {
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase == null)
            {
                Debug.LogError($"[ValidateShopPriceRanges] ItemDatabaseSO not found at '{ItemDatabasePath}'.");
                return (0, 1);
            }

            var configGuids = AssetDatabase.FindAssets("t:EconomyBalanceConfigSO");
            EconomyBalanceConfigSO config = null;
            if (configGuids.Length > 0)
                config = AssetDatabase.LoadAssetAtPath<EconomyBalanceConfigSO>(
                    AssetDatabase.GUIDToAssetPath(configGuids[0]));

            float maxMult = config != null ? config.ShopMaxPriceMultiplier : 3.0f;
            float minMult = config != null ? config.ShopMinPriceMultiplier : 0.1f;

            var allShops = LoadAllShops();
            int warnings = 0, errors = 0;

            foreach (var shop in allShops)
            {
                if (shop.Items == null) continue;
                var shopLabel = string.IsNullOrWhiteSpace(shop.Id)
                    ? AssetDatabase.GetAssetPath(shop) : shop.Id;

                foreach (var entry in shop.Items)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId)) continue;
                    if (!itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null) continue;
                    if (itemData.BaseValue <= 0) continue;

                    int effectivePrice = entry.BuyPriceOverride > 0
                        ? entry.BuyPriceOverride
                        : (int)(itemData.BaseValue * shop.BuyPriceMultiplier);

                    if (effectivePrice <= 0) continue; // free item — design intent, skip

                    if (effectivePrice > itemData.BaseValue * maxMult)
                    {
                        Debug.LogWarning(
                            $"[ShopPriceRange] WARN — Shop '{shopLabel}' item '{entry.ItemId}' " +
                            $"effectivePrice={effectivePrice} > {maxMult}x BaseValue={itemData.BaseValue}.");
                        warnings++;
                    }
                    else if (effectivePrice < itemData.BaseValue * minMult)
                    {
                        Debug.LogError(
                            $"[ShopPriceRange] ERROR — Shop '{shopLabel}' item '{entry.ItemId}' " +
                            $"effectivePrice={effectivePrice} < {minMult}x BaseValue={itemData.BaseValue}.");
                        errors++;
                    }
                }
            }

            return (warnings, errors);
        }
    }
}
