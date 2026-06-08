namespace CindarsHope.Player.Conditions
{
    public class FatigueState
    {
        public float FatigueValue { get; set; } = 0f; // 0..100
        public FatigueThreshold Threshold => FatigueThresholdExtensions.FromValue(FatigueValue);
        public int LastUpdatedDay { get; set; } = -1;
        public float SleepDebt { get; set; } = 0f;
        public float LastSleepQuality { get; set; } = 1.0f; // 0..1
        public bool IsExhausted => Threshold.IsExhausted();
        public bool HasPenalty => Threshold.HasPenalty();

        public void Clamp()
        {
            if (FatigueValue < 0f) FatigueValue = 0f;
            if (FatigueValue > 100f) FatigueValue = 100f;
        }
    }
}
