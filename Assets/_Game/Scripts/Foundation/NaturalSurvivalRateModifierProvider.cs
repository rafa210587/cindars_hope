namespace CindarsHope.Foundation
{
    public enum NaturalSurvivalRateChannel
    {
        HungerDrain = 0,
        FatigueGain = 1,
        StaminaRegen = 2,
        ManaRegen = 3,
        HealthRegen = 4
    }

    public interface INaturalSurvivalRateModifierRuntime
    {
        float ResolveMultiplier(NaturalSurvivalRateChannel channel, float worldX, float worldY);
    }

    /// <summary>Composes temporary survival zones with existing base rates.</summary>
    public static class NaturalSurvivalRateModifierProvider
    {
        public static INaturalSurvivalRateModifierRuntime Source;

        public static float Resolve(NaturalSurvivalRateChannel channel,
            float worldX, float worldY)
        {
            var value = Source != null
                ? Source.ResolveMultiplier(channel, worldX, worldY)
                : 1f;
            return float.IsNaN(value) || float.IsInfinity(value) || value < 0f
                ? 1f
                : value;
        }
    }
}
