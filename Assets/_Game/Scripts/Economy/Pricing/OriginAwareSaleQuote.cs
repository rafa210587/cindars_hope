using System.Collections.Generic;
using CindarsHope.Inventory;

namespace CindarsHope.Economy.Pricing
{
    /// <summary>Applies persisted origin caps after a channel has computed its final unit prices.</summary>
    public readonly struct OriginAwareSaleQuote
    {
        private OriginAwareSaleQuote(bool success, int normalGold, int finalGold, int limitedUnits)
        {
            Success = success;
            NormalGold = normalGold;
            FinalGold = finalGold;
            LimitedUnits = limitedUnits;
        }

        public bool Success { get; }
        public int NormalGold { get; }
        public int FinalGold { get; }
        public int LimitedUnits { get; }
        public int LimitedGold => NormalGold - FinalGold;

        public static OriginAwareSaleQuote Calculate(InventoryLotSelection selection,
            IReadOnlyList<int> finalNormalUnitGold)
        {
            if (selection == null) return default;
            return Calculate(selection.CopyLots(), finalNormalUnitGold);
        }

        public static OriginAwareSaleQuote Calculate(IReadOnlyList<ItemOriginLot> lots,
            IReadOnlyList<int> finalNormalUnitGold)
        {
            if (lots == null || finalNormalUnitGold == null || finalNormalUnitGold.Count <= 0)
                return default;

            long normalTotal = 0;
            long finalTotal = 0;
            var limitedUnits = 0;
            var unitIndex = 0;
            foreach (var lot in lots)
            {
                if (lot == null || lot.Quantity <= 0 || lot.Kind == ItemOriginKind.Invalid)
                    return default;

                for (var i = 0; i < lot.Quantity; i++)
                {
                    if (unitIndex >= finalNormalUnitGold.Count) return default;
                    var normal = finalNormalUnitGold[unitIndex++];
                    if (normal < 0) return default;

                    var final = normal;
                    if (lot.Kind == ItemOriginKind.Purchased
                        || lot.Kind == ItemOriginKind.RestrictedDerived)
                    {
                        if (lot.UnitResaleCapGold < 0 || lot.UnitPaidBasisGold < 0)
                            return default;
                        if (lot.Kind == ItemOriginKind.Purchased
                            && lot.UnitResaleCapGold != (lot.UnitPaidBasisGold > 0
                                ? lot.UnitPaidBasisGold - 1 : 0))
                            return default;
                        final = normal < lot.UnitResaleCapGold ? normal : lot.UnitResaleCapGold;
                        if (final < normal) limitedUnits++;
                    }
                    else if (lot.Kind != ItemOriginKind.Earned
                        && lot.Kind != ItemOriginKind.LegacyUntracked)
                        return default;

                    normalTotal += normal;
                    finalTotal += final;
                    if (normalTotal > int.MaxValue || finalTotal > int.MaxValue)
                        return default;
                }
            }

            if (unitIndex != finalNormalUnitGold.Count) return default;
            return new OriginAwareSaleQuote(true, (int)normalTotal, (int)finalTotal, limitedUnits);
        }

        /// <summary>
        /// Distributes an existing aggregate bonus into stable, nonnegative unit prices.
        /// Callers sort units by itemId/slot/lot before invoking; ties keep that order.
        /// </summary>
        public static bool TryAllocateAggregateBonus(IReadOnlyList<int> baseUnitGold,
            int totalWithBonus, out int[] finalUnitGold)
        {
            finalUnitGold = null;
            if (baseUnitGold == null || baseUnitGold.Count == 0 || totalWithBonus < 0)
                return false;

            long baseTotal = 0;
            foreach (var value in baseUnitGold)
            {
                if (value < 0) return false;
                baseTotal += value;
                if (baseTotal > int.MaxValue) return false;
            }
            if (baseTotal <= 0) return false;

            var allocated = new int[baseUnitGold.Count];
            var remainders = new long[baseUnitGold.Count];
            long allocatedTotal = 0;
            for (var i = 0; i < baseUnitGold.Count; i++)
            {
                var numerator = (long)baseUnitGold[i] * totalWithBonus;
                allocated[i] = (int)(numerator / baseTotal);
                remainders[i] = numerator % baseTotal;
                allocatedTotal += allocated[i];
            }

            var remaining = totalWithBonus - allocatedTotal;
            for (var n = 0L; n < remaining; n++)
            {
                var best = -1;
                for (var i = 0; i < remainders.Length; i++)
                {
                    if (best < 0 || remainders[i] > remainders[best]) best = i;
                }
                if (best < 0) return false;
                allocated[best]++;
                remainders[best] = -1;
            }

            finalUnitGold = allocated;
            return true;
        }
    }
}
