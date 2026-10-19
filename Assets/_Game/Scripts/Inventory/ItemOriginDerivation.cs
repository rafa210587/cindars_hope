using System;
using System.Collections.Generic;

namespace CindarsHope.Inventory
{
    /// <summary>One-time resale budget for an output batch; never reprice inherited lots.</summary>
    public static class ItemOriginDerivation
    {
        public readonly struct Input
        {
            public readonly IReadOnlyList<ItemOriginLot> Lots;
            public readonly int NormalUnitLiquidationGold;

            public Input(IReadOnlyList<ItemOriginLot> lots, int normalUnitLiquidationGold)
            {
                Lots = lots;
                NormalUnitLiquidationGold = normalUnitLiquidationGold;
            }
        }

        public static bool TryDerive(IReadOnlyList<Input> inputs, int outputQuantity,
            out List<ItemOriginLot> outputLots)
        {
            outputLots = null;
            if (inputs == null || inputs.Count == 0 || outputQuantity <= 0) return false;

            long budget = 0;
            long paidBasis = 0;
            var hasRestrictedInput = false;
            var hasLegacyInput = false;
            try
            {
                checked
                {
                    foreach (var input in inputs)
                    {
                        if (input.Lots == null || input.Lots.Count == 0
                            || input.NormalUnitLiquidationGold < 0) return false;
                        long quantity = 0;
                        foreach (var lot in input.Lots)
                        {
                            if (lot == null || !ItemOriginLot.IsValid(new[] { lot }, lot.Quantity)
                                || lot.Kind == ItemOriginKind.Invalid) return false;
                            quantity += lot.Quantity;
                            if (lot.Kind == ItemOriginKind.Purchased
                                || lot.Kind == ItemOriginKind.RestrictedDerived)
                            {
                                hasRestrictedInput = true;
                                budget += (long)lot.UnitResaleCapGold * lot.Quantity;
                                paidBasis += (long)lot.UnitPaidBasisGold * lot.Quantity;
                            }
                            else
                            {
                                if (lot.Kind == ItemOriginKind.LegacyUntracked)
                                    hasLegacyInput = true;
                                budget += (long)input.NormalUnitLiquidationGold * lot.Quantity;
                            }
                        }
                        if (quantity <= 0 || quantity > int.MaxValue) return false;
                    }
                }
            }
            catch (OverflowException) { return false; }

            if (budget > int.MaxValue || paidBasis > int.MaxValue) return false;
            var derived = new List<ItemOriginLot>();
            if (!hasRestrictedInput)
            {
                derived.Add(new ItemOriginLot(hasLegacyInput
                    ? ItemOriginKind.LegacyUntracked : ItemOriginKind.Earned, outputQuantity));
                outputLots = derived;
                return true;
            }

            var basisUnit = (int)(paidBasis / outputQuantity);
            var basisRemainder = (int)(paidBasis % outputQuantity);
            var capUnit = (int)(budget / outputQuantity);
            var capRemainder = (int)(budget % outputQuantity);
            var split1 = Math.Min(basisRemainder, capRemainder);
            var split2 = Math.Max(basisRemainder, capRemainder);
            AppendRange(0, split1);
            AppendRange(split1, split2);
            AppendRange(split2, outputQuantity);
            outputLots = derived;
            return ItemOriginLot.IsValid(derived, outputQuantity);

            void AppendRange(int start, int end)
            {
                if (end <= start) return;
                var basis = basisUnit + (start < basisRemainder ? 1 : 0);
                var cap = capUnit + (start < capRemainder ? 1 : 0);
                ItemOriginLot.Append(derived,
                    new ItemOriginLot(ItemOriginKind.RestrictedDerived, end - start, basis, cap));
            }
        }
    }
}
