namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 CA-2 — Pure decision for the "Pouch of Holding" overflow-safe rule.
    ///
    /// The pouch grants +<see cref="MagicItemCatalog.PouchSlotBonus"/> effective slots while present. The
    /// risk is LOSING the pouch while those bonus slots are occupied: shrinking capacity must NEVER destroy
    /// the player's items. The rule: if dropping/removing the pouch would leave more occupied slots than the
    /// post-shrink capacity, the drop is BLOCKED ("mochila transbordando") — nothing is destroyed.
    ///
    /// Pure (no Unity refs) so CA-2 (gain / loss / overflow) is fully EditMode-testable.
    /// </summary>
    public static class PouchCapacityGuard
    {
        public readonly struct DropDecision
        {
            /// <summary>True when removing the pouch is safe and may proceed.</summary>
            public bool Allowed { get; }
            /// <summary>Effective capacity after the pouch is removed.</summary>
            public int CapacityAfter { get; }
            /// <summary>How many occupied slots would have no home after the shrink (0 when allowed).</summary>
            public int Overflow { get; }

            public DropDecision(bool allowed, int capacityAfter, int overflow)
            {
                Allowed = allowed;
                CapacityAfter = capacityAfter;
                Overflow = overflow;
            }
        }

        /// <summary>
        /// Effective capacity = base capacity + pouch bonus when the pouch is present.
        /// </summary>
        public static int EffectiveCapacity(int baseCapacity, bool pouchPresent)
        {
            var bonus = pouchPresent ? MagicItemCatalog.PouchSlotBonus : 0;
            return baseCapacity + bonus;
        }

        /// <summary>
        /// Decides whether the player may drop/lose the pouch given how many slots are currently occupied.
        /// </summary>
        /// <param name="occupiedSlots">Distinct slots in use right now (with the pouch still present).</param>
        /// <param name="baseCapacity">Inventory capacity WITHOUT the pouch bonus.</param>
        public static DropDecision EvaluateDropPouch(int occupiedSlots, int baseCapacity)
        {
            var capacityAfter = baseCapacity; // pouch is being removed → no bonus
            var overflow = occupiedSlots - capacityAfter;
            if (overflow <= 0)
            {
                return new DropDecision(true, capacityAfter, 0);
            }

            // Would overflow: block the drop, keep everything intact.
            return new DropDecision(false, capacityAfter, overflow);
        }
    }
}
