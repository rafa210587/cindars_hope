using System.Collections.Generic;

namespace CindarsHope.Quests.FarmOrders
{
    public class DeliveryItemContext
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
        public int Quality { get; set; }
        public bool IsTrackedByQuest { get; set; }
    }

    public class FarmOrderDeliveryResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public List<string> ItemsConsumed { get; set; } = new List<string>();
        public string AcceptedQualitySummary { get; set; }
        public bool RewardApplied { get; set; }
        public bool Expired { get; set; }
        public bool PartialProgress { get; set; }
        public List<string> DebugNotes { get; set; } = new List<string>();

        public static FarmOrderDeliveryResult Fail(string reason) =>
            new FarmOrderDeliveryResult { Success = false, FailureReason = reason };

        public static FarmOrderDeliveryResult Expire() =>
            new FarmOrderDeliveryResult { Success = false, Expired = true, FailureReason = "ORDER_EXPIRED" };
    }

    // Pure C# service — no Unity/MonoBehaviour, no live scene state
    public class FarmOrderDeliveryService
    {
        // Evaluates whether delivery items satisfy the order's required items and quality policy
        public FarmOrderDeliveryResult EvaluateDelivery(
            FarmOrderDefinition order,
            List<DeliveryItemContext> deliveredItems,
            int currentDay,
            int orderAcceptedDay,
            bool alreadyRewarded)
        {
            if (order == null) return FarmOrderDeliveryResult.Fail("ORDER_NULL");

            // Idempotency guard — reward already applied
            if (alreadyRewarded)
                return FarmOrderDeliveryResult.Fail("REWARD_ALREADY_APPLIED");

            // Expiry check
            if (IsExpired(order, currentDay, orderAcceptedDay))
                return FarmOrderDeliveryResult.Expire();

            // Validate each required item
            foreach (var required in order.RequiredItems)
            {
                var match = FindMatch(deliveredItems, required);
                if (match == null)
                    return FarmOrderDeliveryResult.Fail($"ITEM_NOT_FOUND:{required.ItemId}");

                if (match.Amount < required.RequiredAmount)
                    return new FarmOrderDeliveryResult
                    {
                        Success = false, PartialProgress = true,
                        FailureReason = $"INSUFFICIENT_AMOUNT:{required.ItemId}"
                    };

                var qualityCheck = CheckQuality(required, match.Quality);
                if (!qualityCheck.Accepted)
                    return FarmOrderDeliveryResult.Fail($"QUALITY_REJECTED:{required.ItemId}:{qualityCheck.Reason}");
            }

            var result = new FarmOrderDeliveryResult { Success = true };
            foreach (var req in order.RequiredItems)
                result.ItemsConsumed.Add(req.ItemId);
            result.AcceptedQualitySummary = order.AcceptedQualityPolicy.ToString();
            return result;
        }

        private DeliveryItemContext FindMatch(List<DeliveryItemContext> items, RequiredOrderItem required)
        {
            foreach (var item in items)
                if (item.ItemId == required.ItemId)
                    return item;
            return null;
        }

        private (bool Accepted, string Reason) CheckQuality(RequiredOrderItem required, int actualQuality)
        {
            if (required.AcceptAnyQuality)
                return (true, null);

            switch (required.AcceptHigherQuality ? QualityAcceptancePolicyType.MinimumQuality : QualityAcceptancePolicyType.ExactQuality)
            {
                case QualityAcceptancePolicyType.MinimumQuality:
                    if (required.MinimumQuality.HasValue && actualQuality < required.MinimumQuality.Value)
                        return (false, "BELOW_MINIMUM");
                    return (true, null);

                case QualityAcceptancePolicyType.ExactQuality:
                    if (required.MinimumQuality.HasValue && actualQuality != required.MinimumQuality.Value)
                        return (false, "NOT_EXACT");
                    return (true, null);

                default:
                    return (true, null);
            }
        }

        private bool IsExpired(FarmOrderDefinition order, int currentDay, int acceptedDay)
        {
            var policy = order.DeadlinePolicy;
            if (policy == null || policy.PolicyType == FarmOrderDeadlinePolicyType.NoDeadline)
                return false;

            if (policy.PolicyType == FarmOrderDeadlinePolicyType.DaysFromAccept)
                return policy.DaysFromAccept.HasValue && currentDay > acceptedDay + policy.DaysFromAccept.Value;

            if (policy.PolicyType == FarmOrderDeadlinePolicyType.SpecificDay)
                return policy.SpecificDay.HasValue && currentDay > policy.SpecificDay.Value;

            return false;
        }

        // Computes a new RepeatInstanceId for repeat orders (deterministic, not GUID-based)
        public string ComputeRepeatInstanceId(string farmOrderId, int repeatCount) =>
            $"{farmOrderId}_repeat_{repeatCount}";

        // Guards repeat reward exploit — bounded table check
        public bool IsRepeatAllowed(FarmOrderDefinition order, int completionCount)
        {
            if (order.RepeatPolicy == null || order.RepeatPolicy.PolicyType == FarmOrderRepeatPolicyType.Never)
                return false;

            if (order.RepeatPolicy.MaxRepeatCount > 0 && completionCount >= order.RepeatPolicy.MaxRepeatCount)
                return false;

            return true;
        }
    }
}
