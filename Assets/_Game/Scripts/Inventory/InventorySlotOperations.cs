using System;
using System.Collections.Generic;

namespace CindarsHope.Inventory
{
    /// <summary>Mutates stack contents while preserving slot identity and item totals.</summary>
    /// <remarks>
    /// Owns no inventory state. The caller validates catalog entries, rebuilds aggregate totals
    /// and publishes changes after success. Rejected operations leave slots unchanged.
    /// Collections must contain distinct slot instances; slot positions are never replaced.
    /// </remarks>
    public static class InventorySlotOperations
    {
        public static int GetAvailableCapacityFor(IReadOnlyList<InventorySlot> slots, string itemId, int maxStack)
        {
            if (slots == null || string.IsNullOrWhiteSpace(itemId)) return 0;
            maxStack = Math.Max(1, maxStack);
            var available = 0;
            for (var index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];
                if (slot == null) continue;
                if (slot.IsEmpty) available += maxStack;
                else if (slot.ItemId == itemId && string.IsNullOrWhiteSpace(slot.ItemInstanceId)
                    && slot.Amount < maxStack)
                    available += maxStack - slot.Amount;
            }
            return available;
        }

        public static InventoryAddResult TryAddItem(IReadOnlyList<InventorySlot> slots, string itemId,
            int amount, int maxStack)
        {
            if (slots == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0
                || GetAvailableCapacityFor(slots, itemId, maxStack) < amount)
                return new InventoryAddResult(false, itemId, amount, 0);

            maxStack = Math.Max(1, maxStack);
            var remaining = amount;
            // Fill existing stacks before occupying an earlier empty position.
            for (var index = 0; index < slots.Count && remaining > 0; index++)
            {
                var slot = slots[index];
                if (slot == null || slot.IsEmpty || slot.ItemId != itemId
                    || !string.IsNullOrWhiteSpace(slot.ItemInstanceId) || slot.Amount >= maxStack) continue;
                var added = Math.Min(remaining, maxStack - slot.Amount);
                slot.Amount += added;
                remaining -= added;
            }
            for (var index = 0; index < slots.Count && remaining > 0; index++)
            {
                var slot = slots[index];
                if (slot == null || !slot.IsEmpty) continue;
                var added = Math.Min(remaining, maxStack);
                slot.ItemId = itemId;
                slot.ItemInstanceId = string.Empty;
                slot.Amount = added;
                remaining -= added;
            }
            return new InventoryAddResult(true, itemId, amount, amount);
        }

        public static bool TryRemoveItem(IReadOnlyList<InventorySlot> slots, string itemId, int amount)
        {
            if (slots == null || string.IsNullOrWhiteSpace(itemId) || amount <= 0) return false;
            var available = 0L;
            for (var index = 0; index < slots.Count && available < amount; index++)
            {
                var slot = slots[index];
                if (slot != null && !slot.IsEmpty && slot.ItemId == itemId) available += slot.Amount;
            }
            if (available < amount) return false;

            var remaining = amount;
            for (var index = slots.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var slot = slots[index];
                if (slot == null || slot.IsEmpty || slot.ItemId != itemId) continue;
                var removed = Math.Min(remaining, slot.Amount);
                slot.Amount -= removed;
                remaining -= removed;
                if (slot.Amount <= 0) slot.Clear();
            }
            return true;
        }

        public static bool TrySplit(InventorySlot source, InventorySlot destination)
        {
            if (source == null || source.IsEmpty || source.Amount < 2
                || !string.IsNullOrWhiteSpace(source.ItemInstanceId)
                || destination == null || !destination.IsEmpty || ReferenceEquals(source, destination))
            {
                return false;
            }

            var splitAmount = source.Amount / 2;
            source.Amount -= splitAmount;
            destination.ItemId = source.ItemId;
            destination.ItemInstanceId = string.Empty;
            destination.Amount = splitAmount;
            return true;
        }

        public static bool TryMoveOrMerge(InventorySlot source, InventorySlot destination,
            int maxStack, out string failureReason)
        {
            failureReason = string.Empty;
            if (source == null || source.IsEmpty)
            {
                failureReason = "O slot de origem esta vazio.";
                return false;
            }

            if (ReferenceEquals(source, destination))
            {
                failureReason = "Escolha um slot de destino diferente.";
                return false;
            }

            if (source.IsEquipped || (destination != null && destination.IsEquipped))
            {
                failureReason = "Itens equipados nao podem ser movidos.";
                return false;
            }

            if (destination == null)
            {
                failureReason = "O slot de destino nao existe.";
                return false;
            }

            if (destination.IsEmpty)
            {
                CopyContents(source, destination);
                source.Clear();
            }
            else if (source.ItemId == destination.ItemId
                && string.IsNullOrWhiteSpace(source.ItemInstanceId)
                && string.IsNullOrWhiteSpace(destination.ItemInstanceId))
            {
                var available = Math.Max(1, maxStack) - destination.Amount;
                if (available <= 0)
                {
                    failureReason = "A pilha de destino ja esta cheia.";
                    return false;
                }

                var transferred = Math.Min(source.Amount, available);
                source.Amount -= transferred;
                destination.Amount += transferred;
                if (source.Amount <= 0)
                {
                    source.Clear();
                }
            }
            else
            {
                var itemId = source.ItemId;
                var itemInstanceId = source.ItemInstanceId;
                var amount = source.Amount;
                var isEquipped = source.IsEquipped;
                var bindingId = source.EquipmentBindingId;
                CopyContents(destination, source);
                destination.ItemId = itemId;
                destination.ItemInstanceId = itemInstanceId;
                destination.Amount = amount;
                destination.IsEquipped = isEquipped;
                destination.EquipmentBindingId = bindingId;
            }

            return true;
        }

        private static void CopyContents(InventorySlot source, InventorySlot destination)
        {
            destination.ItemId = source.ItemId;
            destination.ItemInstanceId = source.ItemInstanceId;
            destination.Amount = source.Amount;
            destination.IsEquipped = source.IsEquipped;
            destination.EquipmentBindingId = source.EquipmentBindingId;
        }
    }
}
