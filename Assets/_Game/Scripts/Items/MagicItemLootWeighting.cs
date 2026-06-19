namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 CA-1 — PURE, DETERMINISTIC gate for dropping an unidentified trinket from a rare cave chest.
    ///
    /// Rather than introduce a second loot resolver, this exposes a small weighted decision that the existing
    /// cave loot path (F06) can consult when materializing a rare chest: at cave level ≥ <see cref="MinCaveLevel"/>,
    /// the chest has a weighted chance to yield <see cref="MagicItemCatalog.UnidentifiedTrinketId"/>. The roll is
    /// seeded from the cave run seed + level + chest instance id, so within a run the SAME chest always yields
    /// the SAME result (stable-run, ADR-0005). Below the level gate the chance is always zero.
    /// </summary>
    public static class MagicItemLootWeighting
    {
        /// <summary>Rare unidentified trinkets only appear from level 10 onward.</summary>
        public const int MinCaveLevel = 10;

        /// <summary>Default weight of the trinket vs. the rest of the rare-chest table (out of 100).</summary>
        public const int DefaultTrinketWeight = 18;

        /// <summary>True when this cave level is deep enough for unidentified trinkets to appear at all.</summary>
        public static bool IsEligibleLevel(int caveLevel) => caveLevel >= MinCaveLevel;

        /// <summary>
        /// Deterministically decides whether a specific rare chest yields the unidentified trinket.
        /// </summary>
        /// <param name="caveRunSeed">Active CaveRunManager.CaveRunSeed (stable within a run).</param>
        /// <param name="caveLevel">The chest's cave level.</param>
        /// <param name="chestInstanceId">Stable per-chest id so two chests on the same level differ.</param>
        /// <param name="trinketWeight">Weight out of 100 (clamped to 0..100).</param>
        public static bool RollUnidentifiedTrinket(
            string caveRunSeed, int caveLevel, string chestInstanceId, int trinketWeight = DefaultTrinketWeight)
        {
            if (!IsEligibleLevel(caveLevel))
            {
                return false;
            }

            var weight = trinketWeight < 0 ? 0 : (trinketWeight > 100 ? 100 : trinketWeight);
            if (weight <= 0)
            {
                return false;
            }
            if (weight >= 100)
            {
                return true;
            }

            var hash = MagicItemCatalog.StableHash($"{caveRunSeed}|{caveLevel}|{chestInstanceId}|trinket_roll");
            var roll = (int)((uint)hash % 100u); // 0..99
            return roll < weight;
        }
    }
}
