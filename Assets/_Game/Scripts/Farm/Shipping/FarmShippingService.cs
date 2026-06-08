using System;
using System.Collections.Generic;

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
            int qualityTier, float baseValue, int currentDay)
        {
            if (string.IsNullOrEmpty(itemId))
                return DepositResult.Fail("ItemIdInvalid");

            if (quantity <= 0)
                return DepositResult.Fail("QuantityInvalid");

            var blockReason = _sellability.GetBlockReason(itemId);
            if (blockReason != null)
                return DepositResult.Fail(blockReason);

            var pricePreview = _priceResolver.Resolve(new ShippingPriceInput
            {
                ItemId = itemId, BaseValue = baseValue,
                QualityTier = qualityTier, ChannelMultiplier = 0.95f
            });

            var entry = new PendingShippingEntry
            {
                ShippingEntryId = Guid.NewGuid().ToString("N").Substring(0, 16),
                SellPointId = sellPointId,
                ItemId = itemId,
                Quantity = quantity,
                QualityTier = qualityTier,
                BaseValueSnapshot = baseValue,
                PricePreview = pricePreview,
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

            foreach (var entry in _pendingEntries)
            {
                if (entry.ProcessOnDay != currentDay) continue;
                if (entry.IsProcessed) continue; // idempotency — already processed

                var price = _priceResolver.Resolve(new ShippingPriceInput
                {
                    ItemId = entry.ItemId,
                    BaseValue = entry.BaseValueSnapshot,
                    QualityTier = entry.QualityTier,
                    ChannelMultiplier = 0.95f
                });

                batch.TotalGold += price * entry.Quantity;
                entry.State = ShippingEntryState.Processed;
                entry.ProcessedPaymentId = $"pay_{batch.BatchId}_{entry.ShippingEntryId}";
                batch.Entries.Add(entry);
            }

            batch.State = ShippingBatchState.Processed;
            return batch;
        }

        public List<PendingShippingEntry> GetPendingEntries() => _pendingEntries;
    }
}
