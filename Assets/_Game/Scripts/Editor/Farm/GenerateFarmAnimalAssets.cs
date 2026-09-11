#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.Economy;
using CindarsHope.Farm.Animals;
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Farm
{
    /// <summary>
    /// fable_12 — gera os assets de animais de fazenda: itens novos (filhotes/ração/produtos +
    /// variantes _silver/_gold do ITEM_CATALOG §18), os AnimalDataSO dos 3 animais, e as entradas de
    /// compra (filhotes + ração) no Shop_Eiran. IDs vêm de <see cref="FarmAnimalCatalog"/> — fonte
    /// única, sem duplicação. Itens criados pelo MESMO padrão de ItemDataInitializer (CreateAsset),
    /// não um gerador paralelo.
    /// </summary>
    public static class GenerateFarmAnimalAssets
    {
        private const string ItemsPath = "Assets/_Game/Data/Items/";
        private const string AnimalsPath = "Assets/_Game/Data/Animals/";
        private const string EiranShopPath = "Assets/_Game/Data/Economy/Shop_Eiran.asset";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        // BaseValues: produtos = ITEM_CATALOG §18 (egg 12 / goat_milk 28 / cow_milk 35).
        // Filhotes/ração não constam no §18 — valores de design coerentes com a economia early.
        private const int BvEgg = 12;
        private const int BvGoatMilk = 28;
        private const int BvCowMilk = 35;
        private const int BvFeed = 8;
        private const int BvChickChicken = 80;
        private const int BvKidGoat = 180;
        private const int BvLambSheep = BvKidGoat; // v19: mesmo default de conteúdo do cabrito.
        private const int BvCalfCow = 260;

        public static void Generate()
        {
            EnsureFolder("Assets/_Game/Data", "Animals");
            EnsureFolder("Assets/_Game/Data", "Items");

            var lamb = CreateItems();
            RegisterLambIfMissing(lamb);
            CreateAnimalDataAssets();
            FarmAnimalMotionAuthoring.Generate();
            AddEiranShopEntries();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GenerateFarmAnimalAssets] Animais de fazenda gerados (itens + SOs + Eiran).");
        }

        private static ItemDataSO CreateItems()
        {
            // Filhotes (compra no Eiran).
            CreateItem(FarmAnimalCatalog.ItemChickChicken, "Pintinho", ItemCategory.Misc, 99, BvChickChicken);
            CreateItem(FarmAnimalCatalog.ItemKidGoat, "Cabrito", ItemCategory.Misc, 99, BvKidGoat);
            // Conteúdo canônico da ovelha. O cordeiro fica disponível para release, sem ampliar a loja.
            var lamb = CreateItem(FarmAnimalCatalog.ItemLambSheep, "Cordeiro", ItemCategory.Misc, 99, BvLambSheep);
            CreateItem(FarmAnimalCatalog.ItemCalfCow, "Bezerro", ItemCategory.Misc, 99, BvCalfCow);

            // Ração.
            CreateItem(FarmAnimalCatalog.ItemFeed, "Racao Animal", ItemCategory.Material, 99, BvFeed);

            // Produtos base + variantes de qualidade (_silver ×1.5, _gold ×2.0).
            CreateProductWithQuality(FarmAnimalCatalog.ItemEgg, "Ovo", BvEgg);
            CreateProductWithQuality(FarmAnimalCatalog.ItemGoatMilk, "Leite de Cabra", BvGoatMilk);
            CreateProductWithQuality(FarmAnimalCatalog.ItemCowMilk, "Leite de Vaca", BvCowMilk);
            return lamb;
        }

        private static void CreateProductWithQuality(string baseId, string displayName, int baseValue)
        {
            CreateItem(baseId, displayName, ItemCategory.AnimalProduct, 99, baseValue);
            CreateItem(baseId + "_silver", displayName + " (Prata)", ItemCategory.AnimalProduct, 99, Mathf.RoundToInt(baseValue * 1.5f));
            CreateItem(baseId + "_gold", displayName + " (Ouro)", ItemCategory.AnimalProduct, 99, Mathf.RoundToInt(baseValue * 2.0f));
        }

        private static ItemDataSO CreateItem(string id, string displayName, ItemCategory category, int maxStack, int baseValue)
        {
            var path = $"{ItemsPath}{id}.asset";
            var itemAtCanonicalPath = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (itemAtCanonicalPath != null)
            {
                return itemAtCanonicalPath;
            }

            // Guard por Id (evita duplicar caso já exista sob outro nome de arquivo).
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemsPath.TrimEnd('/') }))
            {
                var existing = AssetDatabase.LoadAssetAtPath<ItemDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (existing != null && existing.Id == id)
                {
                    return existing;
                }
            }

            var asset = ScriptableObject.CreateInstance<ItemDataSO>();
            asset.Id = id;
            asset.DisplayName = displayName;
            asset.Description = $"Item: {displayName}";
            asset.Category = category;
            asset.ConsumableSubtype = ConsumableSubtype.None;
            asset.MaxStack = maxStack;
            asset.BaseValue = baseValue;
            asset.IsEquippable = false;
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void RegisterLambIfMissing(ItemDataSO lamb)
        {
            if (lamb == null) return;

            var database = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (database == null)
            {
                Debug.LogWarning($"[GenerateFarmAnimalAssets] ItemDatabase não encontrado em {ItemDatabasePath}; cordeiro criado, mas não registrado.");
                return;
            }

            var serialized = new SerializedObject(database);
            var items = serialized.FindProperty("_items");
            if (items == null || !items.isArray)
            {
                Debug.LogWarning("[GenerateFarmAnimalAssets] ItemDatabase não expõe o array serializado '_items'; cordeiro não registrado.");
                return;
            }

            for (var i = 0; i < items.arraySize; i++)
            {
                if (items.GetArrayElementAtIndex(i).objectReferenceValue is ItemDataSO existing &&
                    (existing == lamb || existing.Id == FarmAnimalCatalog.ItemLambSheep))
                {
                    return;
                }
            }

            items.arraySize++;
            items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = lamb;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
        }

        private static void CreateAnimalDataAssets()
        {
            foreach (var entry in FarmAnimalCatalog.GetAll())
            {
                var path = $"{AnimalsPath}AnimalData_{entry.AnimalId}.asset";
                if (AssetDatabase.LoadAssetAtPath<AnimalDataSO>(path) != null)
                {
                    continue;
                }

                var so = ScriptableObject.CreateInstance<AnimalDataSO>();
                so.Configure(
                    entry.AnimalId,
                    entry.DisplayName,
                    entry.Species,
                    entry.HousingType,
                    entry.PurchaseItemId,
                    entry.FeedItemId,
                    entry.ProductItemId,
                    entry.ProductIntervalDays,
                    FarmAnimalCatalog.NeglectDeathDays,
                    Color.white);
                AssetDatabase.CreateAsset(so, path);
            }
        }

        private static void AddEiranShopEntries()
        {
            var shop = AssetDatabase.LoadAssetAtPath<ShopDataSO>(EiranShopPath);
            if (shop == null)
            {
                Debug.LogWarning($"[GenerateFarmAnimalAssets] Shop_Eiran não encontrado em {EiranShopPath}; entradas não adicionadas.");
                return;
            }

            var entries = new List<ShopItemEntry>();
            if (shop.Items != null)
            {
                entries.AddRange(shop.Items);
            }

            // Filhotes + ração (idempotente: não duplica entradas já presentes).
            AddEntryIfMissing(entries, FarmAnimalCatalog.ItemChickChicken, 3);
            AddEntryIfMissing(entries, FarmAnimalCatalog.ItemKidGoat, 2);
            AddEntryIfMissing(entries, FarmAnimalCatalog.ItemCalfCow, 1);
            AddEntryIfMissing(entries, FarmAnimalCatalog.ItemFeed, 20);

            shop.Items = entries.ToArray();
            EditorUtility.SetDirty(shop);
        }

        private static void AddEntryIfMissing(List<ShopItemEntry> entries, string itemId, int dailyStock)
        {
            foreach (var e in entries)
            {
                if (e != null && e.ItemId == itemId)
                {
                    return;
                }
            }

            entries.Add(new ShopItemEntry
            {
                ItemId = itemId,
                BaseDailyStock = dailyStock,
                IsFiniteStock = true,
                BuyPriceOverride = 0,
                RequiredUnlockTag = string.Empty
            });
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
