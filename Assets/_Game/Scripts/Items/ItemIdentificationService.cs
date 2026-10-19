using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.Items
{
    /// <summary>
    /// Minimal inventory surface required by <see cref="ItemIdentificationService"/>. Implemented by the
    /// real InventoryManager and by test fakes — keeps identification logic pure/testable (no MonoBehaviour).
    /// </summary>
    public interface IItemSwapInventory
    {
        int GetAmount(string itemId);
        bool RemoveItem(string itemId, int amount);
        bool AddItem(string itemId, int amount);
        bool TryResolveRevealedId(string unidentifiedItemId, out string revealedItemId);
    }

    public enum IdentifyOutcome
    {
        Success = 0,
        NotPresent = 1,        // the unidentified item is not in the inventory
        NoRevealMapping = 2,   // could not resolve the real item id (invalid/missing pair)
        AddFailed = 3          // remove succeeded conceptually but the real item could not be added (rolled back)
    }

    public readonly struct IdentifyResult
    {
        public IdentifyOutcome Outcome { get; }
        public string UnidentifiedItemId { get; }
        public string RevealedItemId { get; }
        public bool Success => Outcome == IdentifyOutcome.Success;

        public IdentifyResult(IdentifyOutcome outcome, string unidentifiedItemId, string revealedItemId)
        {
            Outcome = outcome;
            UnidentifiedItemId = unidentifiedItemId;
            RevealedItemId = revealedItemId;
        }
    }

    /// <summary>
    /// fable_31 — Identification by the PAR DE ITENS pattern: a 1:1 swap of an unidentified item for the
    /// real item it reveals. Zero per-instance metadata, zero new save schema. Consumed by F25 (Veska's
    /// 120g service) and by the scroll_identify consumable. Publishes <see cref="ItemIdentifiedEvent"/>.
    ///
    /// Pure logic over <see cref="IItemSwapInventory"/> so it is fully EditMode-testable.
    /// </summary>
    public sealed class ItemIdentificationService
    {
        private readonly IItemSwapInventory _inventory;

        public ItemIdentificationService(IItemSwapInventory inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// Identifies one instance of <paramref name="unidentifiedItemId"/>: removes 1 of it and adds 1 of the
        /// resolved real item. Safe and atomic — if the real item cannot be added, the removal is rolled back
        /// and the unidentified item stays in the inventory (no item is ever destroyed).
        /// </summary>
        public IdentifyResult IdentifyItem(string unidentifiedItemId)
        {
            if (_inventory == null || string.IsNullOrWhiteSpace(unidentifiedItemId))
            {
                return new IdentifyResult(IdentifyOutcome.NotPresent, unidentifiedItemId, string.Empty);
            }

            if (_inventory.GetAmount(unidentifiedItemId) <= 0)
            {
                return new IdentifyResult(IdentifyOutcome.NotPresent, unidentifiedItemId, string.Empty);
            }

            if (!_inventory.TryResolveRevealedId(unidentifiedItemId, out var revealedItemId)
                || string.IsNullOrWhiteSpace(revealedItemId))
            {
                return new IdentifyResult(IdentifyOutcome.NoRevealMapping, unidentifiedItemId, string.Empty);
            }

            if (!_inventory.RemoveItem(unidentifiedItemId, 1))
            {
                return new IdentifyResult(IdentifyOutcome.NotPresent, unidentifiedItemId, revealedItemId);
            }

            if (!_inventory.AddItem(revealedItemId, 1))
            {
                // Roll back so nothing is lost (e.g. inventory full, or invalid revealed id).
                _inventory.AddItem(unidentifiedItemId, 1);
                return new IdentifyResult(IdentifyOutcome.AddFailed, unidentifiedItemId, revealedItemId);
            }

            GameEventBus.Publish(new ItemIdentifiedEvent(unidentifiedItemId, revealedItemId));
            return new IdentifyResult(IdentifyOutcome.Success, unidentifiedItemId, revealedItemId);
        }
    }
}
