namespace CindarsHope.Player.Conditions
{
    public enum FatigueThreshold
    {
        Rested = 0,        // 0-24
        SlightlyTired = 1, // 25-49
        Tired = 2,         // 50-74
        VeryTired = 3,     // 75-89
        Exhausted = 4      // 90-100
    }

    public static class FatigueThresholdExtensions
    {
        public static FatigueThreshold FromValue(float fatigueValue)
        {
            if (fatigueValue < 25f) return FatigueThreshold.Rested;
            if (fatigueValue < 50f) return FatigueThreshold.SlightlyTired;
            if (fatigueValue < 75f) return FatigueThreshold.Tired;
            if (fatigueValue < 90f) return FatigueThreshold.VeryTired;
            return FatigueThreshold.Exhausted;
        }

        public static bool HasPenalty(this FatigueThreshold threshold) =>
            threshold >= FatigueThreshold.Tired;

        public static bool IsExhausted(this FatigueThreshold threshold) =>
            threshold == FatigueThreshold.Exhausted;
    }
}
