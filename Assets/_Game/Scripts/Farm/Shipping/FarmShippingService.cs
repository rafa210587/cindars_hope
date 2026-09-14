using System;
using System.Collections.Generic;
using CindarsHope.Economy.Pricing;
using CindarsHope.Inventory;

namespace CindarsHope.Farm.Shipping
{
    public class DepositResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public PendingShippingEntry CreatedEntry { get; set; }

        public static DepositResult Fail(string reason) =>
            new DepositResult { Success = false, FailureReason = reason };
    }

    public class ItemSellabilityProvider
    {
        private readonly HashSet<string> _nonSellableItems;
        private readonly HashSet<string> _questItems;
        private readonly HashSet<string> _keyItems;

        public ItemSellabilityProvider(
            HashSet<string> nonSellableItems = null,
            HashSet<string> questItems = null,
            HashSet<string> keyItems = null)
        {
            _nonSellableItems = nonSellableItems ?? new HashSet<string>();
            _questItems = questItems ?? new HashSet<string>();
            _keyItems = keyItems ?? new HashSet<string>();
        }

        public string GetBlockReason(string itemId)
        {
            if (_questItems.Contains(itemId)) return "QuestItemProtected";
            if (_keyItems.Contains(itemId)) return "KeyItemProtected";
            if (_nonSellableItems.Contains(itemId)) return "ItemNotSellable";
            return null;
        }
    }

    public class FarmShippingService
    {
        private readonly List<PendingShippingEntry> _pendingEntries;
        private readonly ShippingPriceResolver _priceResolver;
        private readonly ItemSellabilityProvider _sellability;

        public FarmShippingService(
            List<PendingShippingEntry> pendingEntries,
            ShippingPriceResolver priceResolver,
            ItemSellabilityProvider sellability)
        {
            _pendingEntries = pendingEntries ?? new List<PendingShippingEntry>();
            _priceResolver = priceResolver ?? new ShippingPriceResolver();
            _sellability = sellability ?? new ItemSellabilityProvider();
        }

        public DepositResult Deposit(string sellPointId, string itemId, int quantity,
            int qualityTier, float baseValue, int currentDay,
            IReadOnlyList<ItemOriginLot> originLots = null)
        {
            if (string.IsNullOrEmpty(itemId))
                return DepositResult.Fail("ItemIdInvalid");

            if (quantity <= 0)
                return DepositResult.Fail("QuantityInvalid");
            if (currentDay <= 0 || currentDay == int.MaxValue)
                return DepositResult.Fail("DayInvalid");

            var lots = originLots == null
                ? new List<ItemOriginLot> { new ItemOriginLot(ItemOriginKind.Earned, quantity) }
                : ItemOriginLot.CopyLots(originLots);
            if (!ItemOriginLot.IsValid(lots, quantity))
                return DepositResult.Fail("OriginInvalid");

            var blockReason = _sellability.GetBlockReason(itemId);
            if (blockReason != null)
                return DepositResult.Fail(blockReason);

            if (!TryQuote(itemId, baseValue, qualityTier, lots, out var pricePreview))
                return DepositResult.Fail("PriceInvalid");

            var entry = new PendingShippingEntry
            {
                ShippingEntryId = Guid.NewGuid().ToString("N").Substring(0, 16),
                SellPointId = sellPointId,
                ItemId = itemId,
                Quantity = quantity,
                QualityTier = qualityTier,
                BaseValueSnapshot = baseValue,
                PricePreview = pricePreview,
                OriginLots = lots,
                DepositedDay = currentDay,
                ProcessOnDay = currentDay + 1,
                State = ShippingEntryState.Pending
            };

            _pendingEntries.Add(entry);
            return new DepositResult { Success = true, CreatedEntry = entry };
        }

        public ShippingBatch ProcessDayBatch(int currentDay)
        {
            var batch = new ShippingBatch
            {
                BatchId = $"batch_{currentDay}",
                ProcessDay = currentDay
            };

            long totalGold = 0;
            long itemCount = 0;
            foreach (var entry in _pendingEntries)
            {
                if (entry == null || !entry.IsPending || entry.ProcessOnDay > currentDay) continue;
                if (!TryQuote(entry.ItemId, entry.BaseValueSnapshot, entry.QualityTier,
                        entry.OriginLots, out var entryGold))
                {
                    batch.State = ShippingBatchState.FailedPartial;
                    batch.Entries.Clear();
                    return batch;
                }
                totalGold += entryGold;
                itemCount += entry.Quantity;
                if (totalGold > int.MaxValue || itemCount > int.MaxValue)
                {
                    batch.State = ShippingBatchState.FailedPartial;
                    batch.Entries.Clear();
                    return batch;
                }
                batch.Entries.Add(entry);
            }

            batch.TotalGoldChecked = (int)totalGold;
            batch.TotalGold = (float)totalGold;
            batch.ItemCount = (int)itemCount;
            return batch;
        }

        public bool TryCommitBatch(ShippingBatch batch)
        {
            if (batch == null || batch.State != ShippingBatchState.Pending || batch.Entries.Count == 0)
                return false;
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var entry in batch.Entries)
                if (entry == null || !entry.IsPending || !_pendingEntries.Contains(entry)
                    || !seen.Add(entry.ShippingEntryId)) return false;

            foreach (var entry in batch.Entries)
            {
                entry.State = ShippingEntryState.Processed;
                entry.ProcessedPaymentId = $"pay_{batch.BatchId}_{entry.ShippingEntryId}";
            }
            batch.State = ShippingBatchState.Processed;
            return true;
        }

        public bool TryRollbackBatch(ShippingBatch batch)
        {
            if (batch == null || batch.State != ShippingBatchState.Processed) return false;
            foreach (var entry in batch.Entries)
                if (entry == null || !_pendingEntries.Contains(entry) || !entry.IsProcessed)
                    return false;
            foreach (var entry in batch.Entries)
            {
                entry.State = ShippingEntryState.Pending;
                entry.ProcessedPaymentId = null;
            }
            batch.State = ShippingBatchState.Pending;
            return true;
        }

        public bool CancelDeposit(PendingShippingEntry entry)
            => entry != null && entry.IsPending && _pendingEntries.Remove(entry);

        private bool TryQuote(string itemId, float baseValue, int qualityTier,
            IReadOnlyList<ItemOriginLot> lots, out int gold)
        {
            gold = 0;
            if (string.IsNullOrWhiteSpace(itemId) || lots == null) return false;
            long quantity = 0;
            foreach (var lot in lots)
            {
                if (lot == null || lot.Kind == ItemOriginKind.Invalid) return false;
                quantity += lot.Quantity;
                if (quantity > int.MaxValue) return false;
            }
            if (!ItemOriginLot.IsValid(lots, (int)quantity)) return false;
            // Reject out-of-range raw values before the float resolver can lose integer range.
            var rawBase = (double)baseValue * 0.95d;
            if (qualityTier > 0) rawBase *= 1d + (double)qualityTier * 0.05d;
            if (double.IsNaN(rawBase) || double.IsInfinity(rawBase)
                || rawBase < 0d || rawBase > int.MaxValue) return false;
            var raw = _priceResolver.Resolve(new ShippingPriceInput
            {
                ItemId = itemId, BaseValue = baseValue,
                QualityTier = qualityTier, ChannelMultiplier = 0.95f
            });
            if (float.IsNaN(raw) || float.IsInfinity(raw) || raw < 0f
                || raw > int.MaxValue) return false;
            var unitGold = (int)System.Math.Round(raw, System.MidpointRounding.ToEven);
            var quote = OriginAwareSaleQuote.Calculate(lots,
                new RepeatedUnitPrices((int)quantity, unitGold));
            if (!quote.Success) return false;
            gold = quote.FinalGold;
            return true;
        }

        private sealed class RepeatedUnitPrices : IReadOnlyList<int>
        {
            private readonly int _value;
            public RepeatedUnitPrices(int count, int value) { Count = count; _value = value; }
            public int Count { get; }
            public int this[int index] => index >= 0 && index < Count
                ? _value : throw new System.ArgumentOutOfRangeException(nameof(index));
            public IEnumerator<int> GetEnumerator()
            {
                for (var i = 0; i < Count; i++) yield return _value;
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public List<PendingShippingEntry> GetPendingEntries() => _pendingEntries;
    }
}
