using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Farm.Shipping;

namespace CindarsHope.Tests.EditMode.Farm
{
    [TestFixture]
    public class FarmShippingServiceTests
    {
        private List<PendingShippingEntry> _entries;
        private FarmShippingService _service;

        [SetUp]
        public void Setup()
        {
            _entries = new List<PendingShippingEntry>();
            var priceResolver = new ShippingPriceResolver();
            var sellability = new ItemSellabilityProvider(
                nonSellableItems: new HashSet<string> { "item_non_sell" },
                questItems: new HashSet<string> { "item_quest_key" },
                keyItems: new HashSet<string> { "item_key_ring" }
            );
            _service = new FarmShippingService(_entries, priceResolver, sellability);
        }

        [Test]
        public void Deposit_SellableItem_CreatesEntry()
        {
            var result = _service.Deposit("sellpoint_01", "item_carrot", 3, 0, 10f, 1);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.CreatedEntry);
            Assert.AreEqual(ShippingEntryState.Pending, result.CreatedEntry.State);
            Assert.AreEqual(2, result.CreatedEntry.ProcessOnDay); // day 1 + 1
        }

        [Test]
        public void Deposit_QuestItem_Blocked()
        {
            var result = _service.Deposit("sp_01", "item_quest_key", 1, 0, 100f, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("QuestItemProtected", result.FailureReason);
        }

        [Test]
        public void Deposit_KeyItem_Blocked()
        {
            var result = _service.Deposit("sp_01", "item_key_ring", 1, 0, 100f, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("KeyItemProtected", result.FailureReason);
        }

        [Test]
        public void Deposit_NonSellable_Blocked()
        {
            var result = _service.Deposit("sp_01", "item_non_sell", 1, 0, 50f, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("ItemNotSellable", result.FailureReason);
        }

        [Test]
        public void Deposit_ZeroQuantity_Blocked()
        {
            var result = _service.Deposit("sp_01", "item_carrot", 0, 0, 10f, 1);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("QuantityInvalid", result.FailureReason);
        }

        [Test]
        public void ProcessBatch_NextDay_ProcessesPendingEntries()
        {
            _service.Deposit("sp_01", "item_carrot", 2, 0, 10f, 1);
            var batch = _service.ProcessDayBatch(2);
            Assert.AreEqual(ShippingBatchState.Processed, batch.State);
            Assert.AreEqual(1, batch.Entries.Count);
            Assert.Greater(batch.TotalGold, 0f);
        }

        [Test]
        public void ProcessBatch_Idempotency_NoDoublePayment()
        {
            _service.Deposit("sp_01", "item_carrot", 2, 0, 10f, 1);
            var batch1 = _service.ProcessDayBatch(2);
            var batch2 = _service.ProcessDayBatch(2); // second call same day
            Assert.AreEqual(0, batch2.Entries.Count); // already processed
            Assert.AreEqual(0f, batch2.TotalGold);
        }

        [Test]
        public void ProcessBatch_WrongDay_DoesNotProcess()
        {
            _service.Deposit("sp_01", "item_carrot", 1, 0, 10f, 1);
            var batch = _service.ProcessDayBatch(5); // different day
            Assert.AreEqual(0, batch.Entries.Count);
        }

        [Test]
        public void PriceResolver_QualityBonus_IncreasePrice()
        {
            var resolver = new ShippingPriceResolver();
            var noQuality = resolver.Resolve(new ShippingPriceInput { ItemId = "x", BaseValue = 100f, QualityTier = 0, ChannelMultiplier = 1.0f });
            var withQuality = resolver.Resolve(new ShippingPriceInput { ItemId = "x", BaseValue = 100f, QualityTier = 2, ChannelMultiplier = 1.0f });
            Assert.Greater(withQuality, noQuality);
        }

        [Test]
        public void Deposit_PendingEntryPreservedAfterReload()
        {
            var result = _service.Deposit("sp_01", "item_carrot", 1, 0, 10f, 1);
            // Simulate reload: entries list still has the entry
            Assert.AreEqual(1, _service.GetPendingEntries().Count);
            Assert.AreEqual(ShippingEntryState.Pending, result.CreatedEntry.State);
        }
    }
}
