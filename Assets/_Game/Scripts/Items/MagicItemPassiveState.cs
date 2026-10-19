using System.Collections.Generic;

namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 — PURE projection of the continuous (presence-based) magic-item effects. Given the set of
    /// item ids currently present in the inventory, it computes which named passive flags are active and the
    /// extra inventory-slot bonus granted by Pouch of Holding. No Unity refs ⇒ fully EditMode-testable.
    ///
    /// "Ponto único por efeito": each passive flag is derived in exactly one place (here), so no ifs leak
    /// into the light/HUD/reveal/inventory systems. Consumers READ flags; they don't recompute them.
    /// </summary>
    public sealed class MagicItemPassiveState
    {
        private readonly HashSet<string> _activeFlags = new HashSet<string>();
        private int _slotBonus;

        public int SlotBonus => _slotBonus;

        public IReadOnlyCollection<string> ActiveFlags => _activeFlags;

        public bool IsFlagActive(string flag) =>
            !string.IsNullOrWhiteSpace(flag) && _activeFlags.Contains(flag);

        /// <summary>
        /// Recomputes flags and slot bonus from the ids currently present (qty &gt; 0). Returns true when the
        /// resulting state changed (so a tracker can publish/refresh only on change).
        /// </summary>
        public bool Recompute(IEnumerable<string> presentItemIds)
        {
            var previousBonus = _slotBonus;
            var changed = false;

            var nextFlags = new HashSet<string>();
            var nextBonus = 0;

            if (presentItemIds != null)
            {
                foreach (var itemId in presentItemIds)
                {
                    if (string.IsNullOrWhiteSpace(itemId) || !MagicItemCatalog.IsPassiveItem(itemId))
                    {
                        continue;
                    }

                    var flag = MagicItemCatalog.GetPassiveFlag(itemId);
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        nextFlags.Add(flag);
                    }

                    if (itemId == MagicItemCatalog.PouchOfHolding)
                    {
                        nextBonus += MagicItemCatalog.PouchSlotBonus;
                    }
                }
            }

            if (!_activeFlags.SetEquals(nextFlags))
            {
                _activeFlags.Clear();
                foreach (var f in nextFlags)
                {
                    _activeFlags.Add(f);
                }
                changed = true;
            }

            _slotBonus = nextBonus;
            if (_slotBonus != previousBonus)
            {
                changed = true;
            }

            return changed;
        }
    }
}
