namespace CindarsHope.Player.Conditions
{
    public enum FatigueGainSource
    {
        TimePassing = 0,
        StaminaSpend = 1,
        HungerLow = 2,
        RepeatedPhysicalAction = 3,
        CaveExploration = 4,
        StatusEffect = 5,
        MagicFuture = 99,
        Debug = 100
    }

    public class FatigueGainContext
    {
        public FatigueGainSource Source { get; set; }
        public float BaseAmount { get; set; }
        public float Multiplier { get; set; } = 1.0f;
        public string SceneContext { get; set; }
        public string ActionId { get; set; }
        public float FinalAmount => BaseAmount * Multiplier;
    }
}
