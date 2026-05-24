using System;
using UnityEngine;

namespace CindarsHope.Loot
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "CindarsHope/Data/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        public LootTableEntry[] Entries;

        public bool TryRoll(out string itemId, out int amount)
        {
            itemId = string.Empty;
            amount = 0;

            if (Entries == null || Entries.Length == 0)
            {
                return false;
            }

            var totalWeight = 0;
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0)
                {
                    continue;
                }

                totalWeight += entry.Weight;
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            var roll = UnityEngine.Random.Range(1, totalWeight + 1);
            var cursor = 0;
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Weight <= 0)
                {
                    continue;
                }

                cursor += entry.Weight;
                if (roll > cursor)
                {
                    continue;
                }

                itemId = entry.ItemId;
                amount = UnityEngine.Random.Range(Mathf.Max(1, entry.MinAmount), Mathf.Max(entry.MinAmount, entry.MaxAmount) + 1);
                return amount > 0;
            }

            return false;
        }
    }

    [Serializable]
    public class LootTableEntry
    {
        public string ItemId;
        public int MinAmount = 1;
        public int MaxAmount = 1;
        public int Weight = 1;
        public string[] RequiredTags;
    }
}
