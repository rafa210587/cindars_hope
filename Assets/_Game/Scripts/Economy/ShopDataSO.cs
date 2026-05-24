using System;
using CindarsHope.Core.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace CindarsHope.Economy
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "CindarsHope/Economy/Shop Data")]
    public class ShopDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [FormerlySerializedAs("ShopKeeperId")]
        public string NpcId;
        public ShopItemEntry[] Items;
        public int BaseDailyStock = 5;
        [FormerlySerializedAs("PriceMultiplier")]
        public float BuyPriceMultiplier = 1.0f;
        public float SellPriceMultiplier = 0.6f;
        public bool DailyRestock = true;
        public bool FutureAffinityPriceModifierEnabled;

        public string ShopKeeperId
        {
            get => NpcId;
            set => NpcId = value;
        }

        public float PriceMultiplier
        {
            get => BuyPriceMultiplier;
            set => BuyPriceMultiplier = value;
        }

        string IIdentifiedData.Id => Id;

        public ShopItemEntry GetEntry(string itemId)
        {
            if (Items == null)
                return null;

            foreach (var entry in Items)
            {
                if (entry != null && entry.ItemId == itemId)
                    return entry;
            }

            return null;
        }
    }

    [Serializable]
    public class ShopItemEntry
    {
        public string ItemId;
        [FormerlySerializedAs("MaxStock")]
        public int BaseDailyStock = 5;
        public bool IsFiniteStock = true;
        public int BuyPriceOverride;
        public string RequiredUnlockTag;

        public int MaxStock
        {
            get => BaseDailyStock;
            set => BaseDailyStock = value;
        }
    }
}
