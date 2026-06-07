using CindarsHope.Core;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Core.Events
{
    /// <summary>
    /// SPEC 01.02: Game Event Contracts Audit and Hardening
    /// Tests validate GameEventBus publish/subscribe/unsubscribe contracts and lifecycle safety.
    /// </summary>
    public class GameEventBusTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear all subscribers before each test to avoid cross-test pollution
            GameEventBus.ClearAll();
        }

        [Test]
        public void Subscribe_AddsHandler()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            GameEventBus.Subscribe<DayStartedEvent>(Handler);

            Assert.AreEqual(1, GameEventBus.CountSubscribers<DayStartedEvent>());
        }

        [Test]
        public void Unsubscribe_RemovesHandler()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            GameEventBus.Subscribe<DayStartedEvent>(Handler);
            GameEventBus.Unsubscribe<DayStartedEvent>(Handler);

            Assert.AreEqual(0, GameEventBus.CountSubscribers<DayStartedEvent>());
        }

        [Test]
        public void Publish_CallsAllSubscribers()
        {
            int count = 0;
            void Handler1(DayStartedEvent evt) => count++;
            void Handler2(DayStartedEvent evt) => count += 10;

            GameEventBus.Subscribe<DayStartedEvent>(Handler1);
            GameEventBus.Subscribe<DayStartedEvent>(Handler2);

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt);

            Assert.AreEqual(11, count);
        }

        [Test]
        public void Publish_PassesEventPayload()
        {
            int receivedDay = -1;
            void Handler(DayStartedEvent evt) => receivedDay = evt.DayNumber;

            GameEventBus.Subscribe<DayStartedEvent>(Handler);

            var evt = new DayStartedEvent(42);
            GameEventBus.Publish(evt);

            Assert.AreEqual(42, receivedDay);
        }

        [Test]
        public void Unsubscribe_PreventsHandlerInvocation()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            GameEventBus.Subscribe<DayStartedEvent>(Handler);
            GameEventBus.Unsubscribe<DayStartedEvent>(Handler);

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt);

            Assert.AreEqual(0, count);
        }

        [Test]
        public void SubscribeOnce_OnlyCallsHandlerOnce()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            GameEventBus.SubscribeOnce<DayStartedEvent>(Handler);

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt);
            GameEventBus.Publish(evt);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void DuplicateSubscribe_IsPreventedWithinSameHandler()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            GameEventBus.Subscribe<DayStartedEvent>(Handler);
            GameEventBus.Subscribe<DayStartedEvent>(Handler); // Duplicate

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt);

            // Handler should only be called once due to duplicate check
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, GameEventBus.CountSubscribers<DayStartedEvent>());
        }

        [Test]
        public void HasSubscribers_ReturnsTrueWhenSubscribed()
        {
            void Handler(DayStartedEvent evt) { }

            Assert.IsFalse(GameEventBus.HasSubscribers<DayStartedEvent>());

            GameEventBus.Subscribe<DayStartedEvent>(Handler);
            Assert.IsTrue(GameEventBus.HasSubscribers<DayStartedEvent>());

            GameEventBus.Unsubscribe<DayStartedEvent>(Handler);
            Assert.IsFalse(GameEventBus.HasSubscribers<DayStartedEvent>());
        }

        [Test]
        public void IDisposable_UnsubscribesWhenDisposed()
        {
            int count = 0;
            void Handler(DayStartedEvent evt) => count++;

            IDisposable subscription = GameEventBus.Subscribe<DayStartedEvent>(Handler);
            subscription.Dispose();

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt);

            Assert.AreEqual(0, count);
            Assert.AreEqual(0, GameEventBus.CountSubscribers<DayStartedEvent>());
        }

        [Test]
        public void ExceptionInHandler_IsLoggedButDoesNotStopOtherHandlers()
        {
            int safeCount = 0;
            void SafeHandler(DayStartedEvent evt) => safeCount++;
            void ThrowingHandler(DayStartedEvent evt) => throw new System.Exception("Test exception");

            GameEventBus.Subscribe<DayStartedEvent>(ThrowingHandler);
            GameEventBus.Subscribe<DayStartedEvent>(SafeHandler);

            var evt = new DayStartedEvent(1);
            GameEventBus.Publish(evt); // Should not throw

            Assert.AreEqual(1, safeCount); // Safe handler was still called
        }

        [Test]
        public void InventoryChangedEvent_ContainsStableItemId()
        {
            string receivedItemId = null;
            int receivedDelta = 0;
            void Handler(InventoryChangedEvent evt)
            {
                receivedItemId = evt.ItemId;
                receivedDelta = evt.Delta;
            }

            GameEventBus.Subscribe<InventoryChangedEvent>(Handler);

            var evt = new InventoryChangedEvent("item_seed_wheat", 5, 10);
            GameEventBus.Publish(evt);

            Assert.AreEqual("item_seed_wheat", receivedItemId);
            Assert.AreEqual(5, receivedDelta);
        }

        [Test]
        public void ItemPickedUpEvent_ContainsStableItemId()
        {
            string receivedItemId = null;
            int receivedAmount = 0;
            void Handler(ItemPickedUpEvent evt)
            {
                receivedItemId = evt.ItemId;
                receivedAmount = evt.Amount;
            }

            GameEventBus.Subscribe<ItemPickedUpEvent>(Handler);

            var evt = new ItemPickedUpEvent("item_fish_common", 3);
            GameEventBus.Publish(evt);

            Assert.AreEqual("item_fish_common", receivedItemId);
            Assert.AreEqual(3, receivedAmount);
        }

        [Test]
        public void SpellCastSucceededEvent_ContainsStableSpellId()
        {
            string receivedSpellId = null;
            void Handler(SpellCastSucceededEvent evt)
            {
                receivedSpellId = evt.SpellId;
            }

            GameEventBus.Subscribe<SpellCastSucceededEvent>(Handler);

            var evt = new SpellCastSucceededEvent("spell_ice_spike");
            GameEventBus.Publish(evt);

            Assert.AreEqual("spell_ice_spike", receivedSpellId);
        }

        [Test]
        public void SkillNodePurchasedEvent_ContainsStableNodeAndTreeIds()
        {
            string receivedNodeId = null;
            string receivedTreeId = null;
            void Handler(SkillNodePurchasedEvent evt)
            {
                receivedNodeId = evt.NodeId;
                receivedTreeId = evt.TreeId;
            }

            GameEventBus.Subscribe<SkillNodePurchasedEvent>(Handler);

            var evt = new SkillNodePurchasedEvent("node_slash_1", "tree_warrior", 2);
            GameEventBus.Publish(evt);

            Assert.AreEqual("node_slash_1", receivedNodeId);
            Assert.AreEqual("tree_warrior", receivedTreeId);
        }

        [Test]
        public void ClearAll_RemovesAllSubscriptions()
        {
            void Handler1(DayStartedEvent evt) { }
            void Handler2(InventoryChangedEvent evt) { }

            GameEventBus.Subscribe<DayStartedEvent>(Handler1);
            GameEventBus.Subscribe<InventoryChangedEvent>(Handler2);

            GameEventBus.ClearAll();

            Assert.IsFalse(GameEventBus.HasSubscribers<DayStartedEvent>());
            Assert.IsFalse(GameEventBus.HasSubscribers<InventoryChangedEvent>());
        }
    }
}
