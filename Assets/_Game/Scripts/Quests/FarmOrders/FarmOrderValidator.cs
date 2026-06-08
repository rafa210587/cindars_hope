using System.Collections.Generic;

namespace CindarsHope.Quests.FarmOrders
{
    public class FarmOrderValidationIssue
    {
        public string OrderId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsBlocker { get; set; }
    }

    public class FarmOrderValidator
    {
        public List<FarmOrderValidationIssue> Validate(FarmOrderDefinition order)
        {
            var issues = new List<FarmOrderValidationIssue>();
            if (order == null) { issues.Add(B(null, "ORDER_NULL", "Null order definition")); return issues; }

            if (string.IsNullOrEmpty(order.FarmOrderId))
                issues.Add(B(order.FarmOrderId, "ORDER_NO_ID", "FarmOrderId is empty"));

            if (string.IsNullOrEmpty(order.QuestId))
                issues.Add(B(order.FarmOrderId, "ORDER_NO_QUEST_ID", "QuestId is empty — FarmOrder must reference a canonical quest"));

            if (order.RequiredItems == null || order.RequiredItems.Count == 0)
                issues.Add(B(order.FarmOrderId, "ORDER_NO_ITEMS", "RequiredItems list is empty"));

            if (order.CanExpire() && order.DeadlinePolicy.PolicyType == FarmOrderDeadlinePolicyType.DaysFromAccept
                && (!order.DeadlinePolicy.DaysFromAccept.HasValue || order.DeadlinePolicy.DaysFromAccept.Value <= 0))
                issues.Add(B(order.FarmOrderId, "ORDER_INVALID_DEADLINE", "DaysFromAccept deadline must be > 0"));

            if (order.CanRepeat() && order.RepeatPolicy.MaxRepeatCount < 0)
                issues.Add(B(order.FarmOrderId, "ORDER_INVALID_REPEAT_BOUND", "MaxRepeatCount cannot be negative"));

            // Economy guard: high-reward orders without a bounding constraint
            if (order.EconomyRiskTag == EconomyRiskTag.HighRisk && !order.CanExpire() && !order.CanRepeat() == false)
                issues.Add(W(order.FarmOrderId, "ORDER_ECONOMY_HIGH_RISK_UNBOUNDED", "High risk order with no expiry or repeat bound — review reward balance"));

            // Main quest protection: FarmOrder cannot block main progression
            foreach (var item in (order.RequiredItems ?? new List<RequiredOrderItem>()))
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.ItemId))
                    issues.Add(B(order.FarmOrderId, "ORDER_ITEM_NO_ID", "RequiredOrderItem has empty ItemId"));
                if (item.RequiredAmount <= 0)
                    issues.Add(B(order.FarmOrderId, "ORDER_ITEM_ZERO_AMOUNT", $"RequiredAmount <= 0 for item {item.ItemId}"));
            }

            return issues;
        }

        private FarmOrderValidationIssue B(string id, string code, string msg) =>
            new FarmOrderValidationIssue { OrderId = id, Code = code, Message = msg, IsBlocker = true };

        private FarmOrderValidationIssue W(string id, string code, string msg) =>
            new FarmOrderValidationIssue { OrderId = id, Code = code, Message = msg, IsBlocker = false };
    }
}
