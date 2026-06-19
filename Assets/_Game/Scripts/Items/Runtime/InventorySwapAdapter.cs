using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — Adapts the runtime <see cref="InventoryManager"/> to the pure <see cref="IItemSwapInventory"/>
    /// used by <see cref="ItemIdentificationService"/>. Reveal resolution reads the unidentified item's
    /// additive <c>IdentifiedItemId</c> field (the explicit pair); when that is empty it falls back to the
    /// deterministic catalog reveal seeded by the active cave run/level (stable-run).
    /// </summary>
    public sealed class InventorySwapAdapter : IItemSwapInventory
    {
        private readonly InventoryManager _inventory;
        private readonly System.Func<(string runSeed, int level, string salt)> _revealContext;

        public InventorySwapAdapter(
            InventoryManager inventory,
            System.Func<(string runSeed, int level, string salt)> revealContext = null)
        {
            _inventory = inventory;
            _revealContext = revealContext;
        }

        public int GetAmount(string itemId) =>
            _inventory != null ? _inventory.GetAmount(itemId) : 0;

        public bool RemoveItem(string itemId, int amount) =>
            _inventory != null && _inventory.RemoveItem(itemId, amount);

        public bool AddItem(string itemId, int amount) =>
            _inventory != null && _inventory.AddItem(itemId, amount);

        public bool TryResolveRevealedId(string unidentifiedItemId, out string revealedItemId)
        {
            revealedItemId = string.Empty;
            if (_inventory == null || string.IsNullOrWhiteSpace(unidentifiedItemId))
            {
                return false;
            }

            // 1) Explicit pair declared on the unidentified item's data (preferred).
            if (_inventory.TryGetItemData(unidentifiedItemId, out ItemDataSO data)
                && data != null
                && data.IsUnidentified
                && !string.IsNullOrWhiteSpace(data.IdentifiedItemId))
            {
                revealedItemId = data.IdentifiedItemId;
                return true;
            }

            // 2) Generic unidentified trinket → deterministic catalog reveal (stable-run by run/level).
            if (MagicItemCatalog.IsUnidentifiedTrinket(unidentifiedItemId))
            {
                var ctx = _revealContext != null
                    ? _revealContext()
                    : (runSeed: string.Empty, level: 1, salt: unidentifiedItemId);
                revealedItemId = MagicItemCatalog.ResolveRevealedItemId(ctx.runSeed, ctx.level, ctx.salt);
                return !string.IsNullOrWhiteSpace(revealedItemId);
            }

            return false;
        }
    }
}
