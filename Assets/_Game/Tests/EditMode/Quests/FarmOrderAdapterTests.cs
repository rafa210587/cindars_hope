using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests.FarmOrders;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class FarmOrderAdapterTests
    {
        private FarmOrderDeliveryService _service;
        private FarmOrderValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _service = new FarmOrderDeliveryService();
            _validator = new FarmOrderValidator();
        }

        private FarmOrderDefinition BasicOrder(FarmOrderType type = FarmOrderType.DeliverItem) => new FarmOrderDefinition
        {
            FarmOrderId = "order_001", QuestId = "quest_order_001", OrderType = type,
            RequiredItems = new List<RequiredOrderItem>
            {
                new RequiredOrderItem { ItemId = "item_carrot", RequiredAmount = 5, AcceptAnyQuality = true, ConsumeOnDelivery = true }
            },
            DeadlinePolicy = new FarmOrderDeadlinePolicy { PolicyType = FarmOrderDeadlinePolicyType.NoDeadline },
            RepeatPolicy = new FarmOrderRepeatPolicy { PolicyType = FarmOrderRepeatPolicyType.Never }
        };

        private List<DeliveryItemContext> Items(string id, int amount, int quality = 1) =>
            new List<DeliveryItemContext> { new DeliveryItemContext { ItemId = id, Amount = amount, Quality = quality } };

        [Test]
        public void HappyPath_DeliverItem_Success()
        {
            var result = _service.EvaluateDelivery(BasicOrder(), Items("item_carrot", 5), 10, 8, false);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.Expired);
        }

        [Test]
        public void InsufficientAmount_PartialProgress()
        {
            var result = _service.EvaluateDelivery(BasicOrder(), Items("item_carrot", 2), 10, 8, false);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.PartialProgress);
        }

        [Test]
        public void WrongItem_Fails()
        {
            var result = _service.EvaluateDelivery(BasicOrder(), Items("item_onion", 5), 10, 8, false);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("ITEM_NOT_FOUND"));
        }

        [Test]
        public void Expired_DaysFromAccept_Fails()
        {
            var order = BasicOrder();
            order.DeadlinePolicy = new FarmOrderDeadlinePolicy { PolicyType = FarmOrderDeadlinePolicyType.DaysFromAccept, DaysFromAccept = 3 };
            var result = _service.EvaluateDelivery(order, Items("item_carrot", 5), currentDay: 15, orderAcceptedDay: 8, false);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Expired);
        }

        [Test]
        public void NotExpired_WithinDeadline_Success()
        {
            var order = BasicOrder();
            order.DeadlinePolicy = new FarmOrderDeadlinePolicy { PolicyType = FarmOrderDeadlinePolicyType.DaysFromAccept, DaysFromAccept = 10 };
            var result = _service.EvaluateDelivery(order, Items("item_carrot", 5), currentDay: 15, orderAcceptedDay: 8, false);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Idempotency_AlreadyRewarded_Fails()
        {
            var result = _service.EvaluateDelivery(BasicOrder(), Items("item_carrot", 5), 10, 8, alreadyRewarded: true);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("REWARD_ALREADY_APPLIED", result.FailureReason);
        }

        [Test]
        public void QualityMinimum_MetOrHigher_Success()
        {
            var order = BasicOrder();
            order.RequiredItems[0].AcceptAnyQuality = false;
            order.RequiredItems[0].MinimumQuality = 2;
            order.RequiredItems[0].AcceptHigherQuality = true;
            var result = _service.EvaluateDelivery(order, Items("item_carrot", 5, quality: 3), 10, 8, false);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void QualityMinimum_TooLow_Fails()
        {
            var order = BasicOrder();
            order.RequiredItems[0].AcceptAnyQuality = false;
            order.RequiredItems[0].MinimumQuality = 3;
            order.RequiredItems[0].AcceptHigherQuality = true;
            var result = _service.EvaluateDelivery(order, Items("item_carrot", 5, quality: 1), 10, 8, false);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("QUALITY_REJECTED"));
        }

        [Test]
        public void RepeatAllowed_WithinBound()
        {
            var order = BasicOrder();
            order.RepeatPolicy = new FarmOrderRepeatPolicy { PolicyType = FarmOrderRepeatPolicyType.DailyTable, MaxRepeatCount = 3 };
            Assert.IsTrue(_service.IsRepeatAllowed(order, 2));
        }

        [Test]
        public void RepeatBlocked_AtMax()
        {
            var order = BasicOrder();
            order.RepeatPolicy = new FarmOrderRepeatPolicy { PolicyType = FarmOrderRepeatPolicyType.DailyTable, MaxRepeatCount = 3 };
            Assert.IsFalse(_service.IsRepeatAllowed(order, 3));
        }

        [Test]
        public void RepeatInstanceId_Deterministic()
        {
            var id1 = _service.ComputeRepeatInstanceId("order_001", 2);
            var id2 = _service.ComputeRepeatInstanceId("order_001", 2);
            Assert.AreEqual(id1, id2);
            Assert.IsTrue(id1.Contains("order_001"));
        }

        [Test]
        public void ObjectiveAdapter_DeliverItem_EventName()
        {
            Assert.AreEqual("OnItemDelivered", FarmOrderObjectiveAdapter.GetTriggerEventName(FarmOrderType.DeliverItem));
        }

        [Test]
        public void ObjectiveAdapter_ShipItem_RequiresDayTransition()
        {
            Assert.IsTrue(FarmOrderObjectiveAdapter.RequiresDayTransition(FarmOrderType.ShipItem));
            Assert.IsFalse(FarmOrderObjectiveAdapter.RequiresDayTransition(FarmOrderType.DeliverItem));
        }

        [Test]
        public void Validator_NoQuestId_Blocker()
        {
            var order = BasicOrder();
            order.QuestId = null;
            var issues = _validator.Validate(order);
            Assert.IsTrue(issues.Exists(i => i.Code == "ORDER_NO_QUEST_ID" && i.IsBlocker));
        }

        [Test]
        public void Validator_EmptyItems_Blocker()
        {
            var order = BasicOrder();
            order.RequiredItems.Clear();
            var issues = _validator.Validate(order);
            Assert.IsTrue(issues.Exists(i => i.Code == "ORDER_NO_ITEMS"));
        }

        [Test]
        public void Validator_ValidOrder_NoIssues()
        {
            var issues = _validator.Validate(BasicOrder());
            Assert.AreEqual(0, issues.Count);
        }
    }
}
