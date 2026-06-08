namespace CindarsHope.Player.Conditions
{
    public class SleepRecoveryContext
    {
        public int SleepStartHour { get; set; } // 0-23
        public int SleepEndHour { get; set; }
        public bool WentToBedLate { get; set; }     // after hour 22
        public float HungerAtSleep { get; set; }    // 0..1
        public bool HasBuffs { get; set; }
    }

    public class SleepRecoveryResult
    {
        public float FatigueReduction { get; set; }
        public float SleepQuality { get; set; } // 0..1
        public string QualityReason { get; set; }
    }

    public static class SleepRecoveryCalculator
    {
        private const float BaseRecovery = 70f; // default full-night recovery
        private const float LateNightPenalty = 0.3f; // lose 30% quality if late
        private const float HungerThreshold = 0.2f; // below 20% hunger reduces quality

        public static SleepRecoveryResult Calculate(SleepRecoveryContext ctx)
        {
            if (ctx == null)
                return new SleepRecoveryResult { FatigueReduction = 0f, SleepQuality = 0f };

            float quality = 1.0f;
            string reason = "Good sleep";

            if (ctx.WentToBedLate)
            {
                quality -= LateNightPenalty;
                reason = "Late to bed";
            }

            if (ctx.HungerAtSleep < HungerThreshold)
            {
                quality -= 0.2f;
                reason = "Hungry sleep";
            }

            // Buff bonus (future: food/potion/companion)
            if (ctx.HasBuffs)
                quality = System.Math.Min(1.0f, quality + 0.1f);

            quality = System.Math.Max(0f, System.Math.Min(1.0f, quality));

            return new SleepRecoveryResult
            {
                FatigueReduction = BaseRecovery * quality,
                SleepQuality = quality,
                QualityReason = reason
            };
        }
    }
}
