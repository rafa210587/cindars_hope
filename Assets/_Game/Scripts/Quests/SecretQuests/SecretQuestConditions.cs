using System;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — pure condition checks for the secret quests that the generic CollectItem objective
    /// cannot express on its own. EditMode-testable; no Unity, no item-database dependency (rarity is
    /// resolved through an injected lookup so this stays in the Quests assembly without coupling to Items).
    /// </summary>
    public static class SecretQuestConditions
    {
        /// <summary>Rarity threshold for scq_scrounger_bargain (item catalog fable_32: Rare = 2).</summary>
        public const int RarePlusThreshold = 2;

        /// <summary>
        /// True when an item qualifies as "Rare or better" for the Scrounger bargain. The rarity lookup
        /// returns the numeric ItemRarity (Common=0 … Unique=5); an unknown item (lookup &lt; 0) never
        /// qualifies.
        /// </summary>
        public static bool IsRarePlus(string itemId, Func<string, int> rarityLookup)
        {
            if (string.IsNullOrEmpty(itemId) || rarityLookup == null) return false;
            int rarity = rarityLookup(itemId);
            return rarity >= RarePlusThreshold;
        }

        /// <summary>
        /// Counts how many of the offered item ids qualify as Rare+, capped at <paramref name="cap"/>
        /// (the Scrounger bargain needs 3). Used to drive the bargain's count objective by rarity, not
        /// by a fixed item id.
        /// </summary>
        public static int CountRarePlus(System.Collections.Generic.IEnumerable<string> itemIds,
            Func<string, int> rarityLookup, int cap)
        {
            if (itemIds == null) return 0;
            int count = 0;
            foreach (var id in itemIds)
            {
                if (IsRarePlus(id, rarityLookup)) count++;
                if (cap > 0 && count >= cap) return cap;
            }
            return count;
        }
    }
}
