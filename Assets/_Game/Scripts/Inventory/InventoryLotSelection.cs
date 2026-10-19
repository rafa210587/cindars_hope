using System.Collections.Generic;

namespace CindarsHope.Inventory
{
    public sealed class InventoryLotSlice
    {
        public int SlotIndex { get; }
        public int LotIndex { get; }
        public int Quantity { get; }
        public ItemOriginKind Kind { get; }
        public int UnitPaidBasisGold { get; }
        public int UnitResaleCapGold { get; }
        public string ItemInstanceId { get; }

        internal InventoryLotSlice(int slotIndex, int lotIndex, int quantity, ItemOriginLot lot, string itemInstanceId)
        {
            SlotIndex = slotIndex; LotIndex = lotIndex; Quantity = quantity; Kind = lot.Kind;
            UnitPaidBasisGold = lot.UnitPaidBasisGold; UnitResaleCapGold = lot.UnitResaleCapGold;
            ItemInstanceId = itemInstanceId;
        }

        public ItemOriginLot ToLot() => new ItemOriginLot(Kind, Quantity, UnitPaidBasisGold, UnitResaleCapGold);
    }

    public sealed class InventoryLotSelection
    {
        private readonly List<InventoryLotSlice> _slices;
        internal readonly List<InventorySlot> SlotSnapshots;
        public string ItemId { get; }
        public int Quantity { get; }
        public long Revision { get; }
        public IReadOnlyList<InventoryLotSlice> Slices => _slices.AsReadOnly();

        internal InventoryLotSelection(string itemId, int quantity, long revision,
            List<InventoryLotSlice> slices, List<InventorySlot> slotSnapshots)
        {
            ItemId = itemId; Quantity = quantity; Revision = revision;
            _slices = new List<InventoryLotSlice>(slices);
            SlotSnapshots = slotSnapshots;
        }

        public List<ItemOriginLot> CopyLots()
        {
            var result = new List<ItemOriginLot>();
            foreach (var slice in _slices) ItemOriginLot.Append(result, slice.ToLot());
            return result;
        }
    }

    public sealed class InventoryLotBatchSelection
    {
        private readonly List<InventoryLotSelection> _selections;
        public long Revision { get; }
        public IReadOnlyList<InventoryLotSelection> Selections => _selections.AsReadOnly();

        internal InventoryLotBatchSelection(long revision, List<InventoryLotSelection> selections)
        {
            Revision = revision;
            _selections = new List<InventoryLotSelection>(selections);
        }
    }

    public sealed class InventoryBatchRemovalToken
    {
        internal InventoryManager Owner { get; }
        internal long RevisionAfterCommit { get; }
        internal List<InventorySlot> Before { get; }
        internal List<InventorySlot> After { get; }

        internal InventoryBatchRemovalToken(InventoryManager owner, long revisionAfterCommit,
            List<InventorySlot> before, List<InventorySlot> after)
        {
            Owner = owner; RevisionAfterCommit = revisionAfterCommit;
            Before = before; After = after;
        }
    }
}
