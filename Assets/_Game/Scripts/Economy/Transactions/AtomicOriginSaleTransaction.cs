using System.Collections.Generic;
using CindarsHope.Economy.Pricing;
using CindarsHope.Inventory;
using CindarsHope.Player;

namespace CindarsHope.Economy.Transactions
{
    public static class AtomicOriginSaleTransaction
    {
        public static bool TryQuoteBatch(InventoryLotBatchSelection batch,
            IReadOnlyDictionary<string, int> normalUnitGold, int totalAfterBonuses,
            out int finalGold, out int limitedUnits)
        {
            finalGold = 0;
            limitedUnits = 0;
            if (batch == null || normalUnitGold == null || batch.Selections.Count == 0)
                return false;

            var orderedUnits = new List<SaleUnit>();
            var unitPricesBySelection = new int[batch.Selections.Count][];
            for (var selectionIndex = 0; selectionIndex < batch.Selections.Count; selectionIndex++)
            {
                var selection = batch.Selections[selectionIndex];
                if (!normalUnitGold.TryGetValue(selection.ItemId, out var unitGold)
                    || unitGold <= 0) return false;
                unitPricesBySelection[selectionIndex] = new int[selection.Quantity];
                var unitIndex = 0;
                foreach (var slice in selection.Slices)
                    for (var i = 0; i < slice.Quantity; i++)
                    {
                        if (unitIndex >= selection.Quantity) return false;
                        orderedUnits.Add(new SaleUnit(selection.ItemId, slice.SlotIndex,
                            slice.LotIndex, selectionIndex, unitIndex++, unitGold));
                    }
                if (unitIndex != selection.Quantity) return false;
            }
            orderedUnits.Sort((a, b) =>
            {
                var order = System.StringComparer.Ordinal.Compare(a.ItemId, b.ItemId);
                if (order != 0) return order;
                order = a.SlotIndex.CompareTo(b.SlotIndex);
                return order != 0 ? order : a.LotIndex.CompareTo(b.LotIndex);
            });
            var baseUnits = new List<int>(orderedUnits.Count);
            foreach (var unit in orderedUnits) baseUnits.Add(unit.BaseGold);
            if (!OriginAwareSaleQuote.TryAllocateAggregateBonus(baseUnits,
                totalAfterBonuses, out var allocated)) return false;
            for (var i = 0; i < orderedUnits.Count; i++)
            {
                var unit = orderedUnits[i];
                unitPricesBySelection[unit.SelectionIndex][unit.UnitIndex] = allocated[i];
            }

            long total = 0;
            for (var selectionIndex = 0; selectionIndex < batch.Selections.Count; selectionIndex++)
            {
                var selection = batch.Selections[selectionIndex];
                var quote = OriginAwareSaleQuote.Calculate(selection,
                    unitPricesBySelection[selectionIndex]);
                if (!quote.Success) return false;
                total += quote.FinalGold;
                limitedUnits += quote.LimitedUnits;
                if (total > int.MaxValue) return false;
            }
            finalGold = (int)total;
            return true;
        }

        private readonly struct SaleUnit
        {
            public readonly string ItemId;
            public readonly int SlotIndex;
            public readonly int LotIndex;
            public readonly int SelectionIndex;
            public readonly int UnitIndex;
            public readonly int BaseGold;

            public SaleUnit(string itemId, int slotIndex, int lotIndex,
                int selectionIndex, int unitIndex, int baseGold)
            {
                ItemId = itemId;
                SlotIndex = slotIndex;
                LotIndex = lotIndex;
                SelectionIndex = selectionIndex;
                UnitIndex = unitIndex;
                BaseGold = baseGold;
            }
        }

        public static bool TryCommitSingle(InventoryManager inventory, PlayerManager wallet,
            InventoryLotSelection selection, int payoutGold)
        {
            if (inventory == null || wallet == null || selection == null || payoutGold < 0)
                return false;
            if (!wallet.TryStageGoldMutation(payoutGold, out var walletToken))
                return false;

            var inventoryRemoved = false;
            var committed = false;
            try
            {
                if (!inventory.TryCommitRemoval(selection, false)) return false;
                inventoryRemoved = true;
                // GoldChanged listeners now see both the credited wallet and removed lots.
                if (!wallet.TryCommitGoldMutation(walletToken)) return false;
                committed = true;
            }
            finally
            {
                if (!committed)
                {
                    var inventoryRestored = !inventoryRemoved
                        || inventory.TryRestoreRemoval(selection, false);
                    var walletRestored = wallet.TryRollbackGoldMutation(walletToken);
                    if (!inventoryRestored || !walletRestored)
                        throw new System.InvalidOperationException("Sale rollback did not restore inventory and wallet.");
                }
            }

            inventory.PublishCommittedInventoryChange(selection.ItemId, -selection.Quantity);
            return true;
        }

        public static bool TryCommitBatch(InventoryManager inventory, PlayerManager wallet,
            InventoryLotBatchSelection batch, int payoutGold)
        {
            if (inventory == null || wallet == null || batch == null || payoutGold < 0)
                return false;
            if (!wallet.TryStageGoldMutation(payoutGold, out var walletToken))
                return false;

            InventoryBatchRemovalToken removalToken = null;
            var committed = false;
            try
            {
                if (!inventory.TryCommitBatchRemoval(batch, out removalToken)) return false;
                // One credit and one snapshot: a listener cannot observe a partial SKU batch.
                if (!wallet.TryCommitGoldMutation(walletToken)) return false;
                committed = true;
            }
            finally
            {
                if (!committed)
                {
                    var inventoryRestored = removalToken == null
                        || inventory.TryRollbackBatchRemoval(removalToken);
                    var walletRestored = wallet.TryRollbackGoldMutation(walletToken);
                    if (!inventoryRestored || !walletRestored)
                        throw new System.InvalidOperationException("Sale batch rollback did not restore inventory and wallet.");
                }
            }

            foreach (var selection in batch.Selections)
                inventory.PublishCommittedInventoryChange(selection.ItemId, -selection.Quantity);
            return true;
        }

        // Preserve an existing channel's aggregate integer quote, allocating its remainder
        // in stable unit order so origin caps apply after the channel's historic rounding.
        public static int[] SplitAggregatePrice(int totalGold, int amount)
        {
            if (totalGold < 0 || amount <= 0) return null;
            var units = new int[amount];
            var each = totalGold / amount;
            var remainder = totalGold % amount;
            for (var i = 0; i < amount; i++)
                units[i] = each + (i >= amount - remainder ? 1 : 0);
            return units;
        }
    }
}
