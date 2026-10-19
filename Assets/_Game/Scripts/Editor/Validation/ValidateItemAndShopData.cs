#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.Player.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools
{
    public static class ValidateItemAndShopData
    {
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";
        private const string PlayerDataPath = "Assets/_Game/Data/Config/PlayerData.asset";
        private const string ShopFolder = "Assets/_Game/Data/Economy";

        public static bool Run()
        {
            var errors = new List<string>();

            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase == null)
            {
                Debug.LogError($"[ValidateItemAndShopData] ItemDatabaseSO not found at '{ItemDatabasePath}'.");
                return false;
            }

            ValidateShops(itemDatabase, errors);
            ValidatePlayerData(itemDatabase, errors);

            if (errors.Count == 0)
            {
                Debug.Log("[ValidateItemAndShopData] All checks passed — 0 errors.");
                return true;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[ValidateItemAndShopData] {errors.Count} error(s) found:");
            foreach (var e in errors)
            {
                sb.AppendLine(e);
            }

            Debug.LogError(sb.ToString());
            return false;
        }

        private static void ValidateShops(ItemDatabaseSO itemDatabase, List<string> errors)
        {
            var guids = AssetDatabase.FindAssets("t:ShopDataSO", new[] { ShopFolder });
            if (guids.Length == 0)
            {
                errors.Add($"No ShopDataSO assets found in '{ShopFolder}'.");
                return;
            }

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(path);
                if (shop == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(shop.Id))
                {
                    errors.Add($"[ValidateItemAndShopData] ShopDataSO '{path}' has empty Id.");
                    continue;
                }

                if (shop.Items == null || shop.Items.Length == 0)
                {
                    errors.Add($"[ValidateItemAndShopData] ShopDataSO '{path}' shopId '{shop.Id}' has no items.");
                    continue;
                }

                foreach (var entry in shop.Items)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                    {
                        errors.Add($"[ValidateItemAndShopData] ShopDataSO '{path}' shopId '{shop.Id}' has an entry with empty ItemId.");
                        continue;
                    }

                    if (!itemDatabase.TryGetById(entry.ItemId, out var itemData) || itemData == null)
                    {
                        var suggestion = FindClosestId(entry.ItemId, itemDatabase);
                        var hint = suggestion != null ? $" Suggested existing match: '{suggestion}'" : string.Empty;
                        errors.Add($"[ValidateItemAndShopData] ShopDataSO '{path}' shopId '{shop.Id}' references missing itemId '{entry.ItemId}'.{hint}");
                        continue;
                    }

                    if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                    {
                        errors.Add($"[ValidateItemAndShopData] ShopDataSO '{path}' shopId '{shop.Id}' item '{entry.ItemId}' has no valid price (BaseValue=0 and BuyPriceOverride=0).");
                    }
                }
            }
        }

        private static void ValidatePlayerData(ItemDatabaseSO itemDatabase, List<string> errors)
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(PlayerDataPath);
            if (playerData == null)
            {
                errors.Add($"[ValidateItemAndShopData] PlayerDataSO not found at '{PlayerDataPath}'.");
                return;
            }

            if (playerData.StartingItems == null)
            {
                return;
            }

            for (var i = 0; i < playerData.StartingItems.Length; i++)
            {
                var entry = playerData.StartingItems[i];
                if (entry.Item == null)
                {
                    errors.Add($"[ValidateItemAndShopData] PlayerDataSO StartingItems[{i}] has null Item reference.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.Item.Id))
                {
                    errors.Add($"[ValidateItemAndShopData] PlayerDataSO StartingItems[{i}] item '{entry.Item.name}' has empty Id.");
                    continue;
                }

                if (!itemDatabase.TryGetById(entry.Item.Id, out _))
                {
                    errors.Add($"[ValidateItemAndShopData] PlayerDataSO StartingItems[{i}] itemId '{entry.Item.Id}' is absent from ItemDatabaseSO.");
                }
            }
        }

        private static string FindClosestId(string missingId, ItemDatabaseSO itemDatabase)
        {
            var bestScore = 0;
            string bestId = null;

            foreach (var item in itemDatabase.All)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                var score = CommonPrefixLength(missingId, item.Id);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestId = item.Id;
                }
            }

            return bestScore >= 4 ? bestId : null;
        }

        private static int CommonPrefixLength(string a, string b)
        {
            var len = Mathf.Min(a.Length, b.Length);
            var count = 0;
            for (var i = 0; i < len; i++)
            {
                if (a[i] == b[i])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }
    }
}
#endif
