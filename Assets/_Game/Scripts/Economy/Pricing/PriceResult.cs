using System.Collections.Generic;

namespace CindarsHope.Economy.Pricing
{
    public class PriceResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }
        public List<string> AppliedMultipliers { get; set; } = new List<string>();
        public bool RoundingApplied { get; set; }
        public List<PriceProtectionFlag> ProtectionFlags { get; set; } = new List<PriceProtectionFlag>();
        public bool IsLimitedException { get; set; } = false;
        public string DebugExplanation { get; set; }

        public static PriceResult Blocked(string reason, PriceProtectionFlag flag) =>
            new PriceResult
            {
                Success = false,
                FailureReason = reason,
                ProtectionFlags = new List<PriceProtectionFlag> { flag }
            };

        public static PriceResult Fail(string reason) =>
            new PriceResult { Success = false, FailureReason = reason };
    }
}
