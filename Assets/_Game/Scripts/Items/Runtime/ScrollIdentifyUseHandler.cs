using CindarsHope.Cave.Runtime;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — F08 use-pipeline handler for <c>item_consumable_scroll_identify</c>. Using the scroll
    /// identifies one unidentified trinket via the shared <see cref="ItemIdentificationService"/> (the same
    /// service F25/Veska consumes), i.e. a 1:1 swap. Returns true (scroll consumed) only when an unidentified
    /// trinket was actually present and identified — otherwise the scroll is NOT wasted.
    ///
    /// The reveal id is resolved by <see cref="InventorySwapAdapter"/> from the active cave run/level so the
    /// outcome is deterministic and stable-run safe.
    /// </summary>
    public sealed class ScrollIdentifyUseHandler : ItemUseHandler
    {
        private InventoryManager _inventory;

        public void Configure(InventoryManager inventory)
        {
            _inventory = inventory;
        }

        public override bool CanUseItem(string itemId, int amount)
        {
            return string.Equals(itemId, MagicItemCatalog.ScrollIdentifyId, System.StringComparison.Ordinal)
                   && amount > 0
                   && _inventory != null
                   && _inventory.GetAmount(MagicItemCatalog.UnidentifiedTrinketId) > 0;
        }

        public override bool TryUseItem(string itemId, int amount, GameObject user)
        {
            if (!string.Equals(itemId, MagicItemCatalog.ScrollIdentifyId, System.StringComparison.Ordinal)
                || _inventory == null)
            {
                return false;
            }

            var adapter = new InventorySwapAdapter(_inventory, ResolveRevealContext);
            var service = new ItemIdentificationService(adapter);
            var result = service.IdentifyItem(MagicItemCatalog.UnidentifiedTrinketId);
            if (!result.Success)
            {
                Debug.Log($"CombatLog: ScrollIdentifyNoOp. Reason={result.Outcome}.", this);
                return false; // do not consume the scroll if nothing was identified
            }

            Debug.Log($"CombatLog: ScrollIdentified. Revealed={result.RevealedItemId}.", this);
            return true;
        }

        private (string runSeed, int level, string salt) ResolveRevealContext()
        {
            var run = CaveRunManager.Instance;
            if (run == null)
            {
                return (string.Empty, 1, MagicItemCatalog.UnidentifiedTrinketId);
            }

            return (run.CaveRunSeed, run.CurrentCaveLevel, MagicItemCatalog.UnidentifiedTrinketId);
        }
    }
}
