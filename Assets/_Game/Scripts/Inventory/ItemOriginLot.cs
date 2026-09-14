using System;
using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Inventory
{
    // Zero is deliberately invalid: an omitted JSON Kind cannot become Earned.
    public enum ItemOriginKind { Invalid = 0, Earned = 1, Purchased = 2, RestrictedDerived = 3, LegacyUntracked = 4 }

    [Serializable]
    public sealed class ItemOriginLot
    {
        public ItemOriginKind Kind;
        public int Quantity;
        public int UnitPaidBasisGold;
        public int UnitResaleCapGold;

        public ItemOriginLot() { }
        public ItemOriginLot(ItemOriginKind kind, int quantity, int paidBasisGold = 0, int resaleCapGold = 0)
        {
            Kind = kind; Quantity = quantity; UnitPaidBasisGold = paidBasisGold; UnitResaleCapGold = resaleCapGold;
        }

        public ItemOriginLot Copy(int quantity) => new ItemOriginLot(Kind, quantity, UnitPaidBasisGold, UnitResaleCapGold);
        public ItemOriginLotSaveData ToSaveData() => new ItemOriginLotSaveData
        {
            Kind = (int)Kind, Quantity = Quantity, UnitPaidBasisGold = UnitPaidBasisGold,
            UnitResaleCapGold = UnitResaleCapGold
        };

        public static bool IsValid(IReadOnlyList<ItemOriginLot> lots, int expectedQuantity)
        {
            if (lots == null || expectedQuantity <= 0) return false;
            long total = 0;
            foreach (var lot in lots)
            {
                if (lot == null || lot.Quantity <= 0 || lot.Kind < ItemOriginKind.Invalid
                    || lot.Kind > ItemOriginKind.LegacyUntracked || lot.UnitPaidBasisGold < 0
                    || lot.UnitResaleCapGold < 0 || ((lot.Kind == ItemOriginKind.Earned
                    || lot.Kind == ItemOriginKind.LegacyUntracked || lot.Kind == ItemOriginKind.Invalid)
                    && (lot.UnitPaidBasisGold != 0 || lot.UnitResaleCapGold != 0))
                    || (lot.Kind == ItemOriginKind.Purchased && lot.UnitResaleCapGold
                        != Math.Max(0, lot.UnitPaidBasisGold - 1))) return false;
                total += lot.Quantity;
            }
            return total == expectedQuantity;
        }

        public static List<ItemOriginLot> CopyLots(IReadOnlyList<ItemOriginLot> lots)
        {
            var copy = new List<ItemOriginLot>();
            if (lots != null) foreach (var lot in lots) copy.Add(lot?.Copy(lot.Quantity));
            return copy;
        }

        public static List<ItemOriginLot> Invalid(int quantity) => new List<ItemOriginLot>
        { new ItemOriginLot(ItemOriginKind.Invalid, quantity) };

        public static List<ItemOriginLot> FromSaveData(IReadOnlyList<ItemOriginLotSaveData> data, int quantity)
        {
            if (data == null || quantity <= 0) return Invalid(quantity);
            long savedQuantity = 0;
            foreach (var lot in data)
            {
                if (lot == null || lot.Quantity <= 0) return Invalid(quantity);
                savedQuantity += lot.Quantity;
            }
            // Without a matching total there is no reliable mapping from saved units to the slot.
            if (savedQuantity != quantity) return Invalid(quantity);

            var lots = new List<ItemOriginLot>();
            foreach (var lot in data)
            {
                var candidate = new ItemOriginLot((ItemOriginKind)lot.Kind, lot.Quantity,
                    lot.UnitPaidBasisGold, lot.UnitResaleCapGold);
                if (!IsValid(new[] { candidate }, lot.Quantity))
                    candidate = new ItemOriginLot(ItemOriginKind.Invalid, lot.Quantity);
                Append(lots, candidate);
            }
            return lots;
        }

        public static List<ItemOriginLot> Purchased(int amount, int totalCost)
        {
            if (amount <= 0 || totalCost < 0) return null;
            var result = new List<ItemOriginLot>();
            var baseCost = totalCost / amount;
            var remainder = totalCost % amount;
            if (amount > remainder) result.Add(new ItemOriginLot(ItemOriginKind.Purchased,
                amount - remainder, baseCost, Math.Max(0, baseCost - 1)));
            if (remainder > 0) result.Add(new ItemOriginLot(ItemOriginKind.Purchased,
                remainder, baseCost + 1, Math.Max(0, baseCost)));
            return result;
        }

        internal static void Append(List<ItemOriginLot> target, ItemOriginLot value)
        {
            if (value.Quantity <= 0) return;
            if (target.Count > 0)
            {
                var last = target[target.Count - 1];
                if (last.Kind == value.Kind && last.UnitPaidBasisGold == value.UnitPaidBasisGold
                    && last.UnitResaleCapGold == value.UnitResaleCapGold
                    && last.Quantity <= int.MaxValue - value.Quantity)
                { last.Quantity += value.Quantity; return; }
            }
            target.Add(value.Copy(value.Quantity));
        }
    }
}
