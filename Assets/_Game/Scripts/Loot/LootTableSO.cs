using System;
using UnityEngine;

namespace CindarsHope.Loot
{
    [CreateAssetMenu(fileName = "LootTable_", menuName = "CindarsHope/Loot/LootTable")]
    public class LootTableSO : ScriptableObject
    {
        public LootEntry[] Entries;

        public string GetRandomLoot()
        {
            if (Entries == null || Entries.Length == 0)
            {
                return null;
            }

            float totalWeight = 0;
            foreach (var entry in Entries)
            {
                totalWeight += entry.Weight;
            }

            float roll = UnityEngine.Random.value * totalWeight;
            foreach (var entry in Entries)
            {
                roll -= entry.Weight;
                if (roll <= 0)
                {
                    return entry.ItemId;
                }
            }

            return Entries[Entries.Length - 1].ItemId;
        }
    }

    [Serializable]
    public class LootEntry
    {
        public string ItemId;
        public int MinAmount = 1;
        public int MaxAmount = 1;
        public float Weight = 1f;
    }
}