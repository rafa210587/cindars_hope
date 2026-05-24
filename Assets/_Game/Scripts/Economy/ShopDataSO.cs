using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Economy
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "CindarsHope/Economy/Shop Data")]
    public class ShopDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string ShopKeeperId;
        public ShopItemEntry[] Items;
        public int BaseDailyStock = 5;
        public float PriceMultiplier = 1.0f;

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
        public int MaxStock = 5;
    }
}
