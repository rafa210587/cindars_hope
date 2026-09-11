using System;

namespace CindarsHope.Foundation
{
    public enum CavebornStatusFamily
    {
        Other = 0,
        Cold = 1,
        Heat = 2,
        Toxic = 3,
        Fear = 4,
        Confusion = 5
    }

    /// <summary>
    /// Neutral read port for the run-bound Nascido da Caverna buff. The Skills runtime owns
    /// activation and expiry; Player and Combat only consume the resolved modifiers.
    /// </summary>
    public interface ICavebornCapstoneModifierRuntime
    {
        bool IsActive { get; }
        float CostMultiplier { get; }
        float DodgeDistanceBonus { get; }
        float HealthRegenMultiplier { get; }
        int ResolveIncomingDamage(int rawDamage, DamageType damageType);
        float ResolveStatusDuration(float durationSeconds, CavebornStatusFamily family);
    }

    public static class CavebornCapstoneProvider
    {
        public static ICavebornCapstoneModifierRuntime Source;

        public static float ResolveCostMultiplier()
            => Sanitize(Source?.CostMultiplier ?? 1f, 1f);

        public static float ResolveDodgeDistanceBonus()
            => Math.Max(0f, Sanitize(Source?.DodgeDistanceBonus ?? 0f, 0f));

        public static float ResolveHealthRegenMultiplier()
            => Math.Max(0f, Sanitize(Source?.HealthRegenMultiplier ?? 1f, 1f));

        public static int ResolveIncomingDamage(int rawDamage, DamageType damageType)
            => Source != null ? Math.Max(0, Source.ResolveIncomingDamage(rawDamage, damageType)) : rawDamage;

        public static float ResolveStatusDuration(float durationSeconds, CavebornStatusFamily family)
            => Source != null
                ? Math.Max(0f, Source.ResolveStatusDuration(durationSeconds, family))
                : Math.Max(0f, durationSeconds);

        private static float Sanitize(float value, float fallback)
            => float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
    }

    public readonly struct CavebornCapstoneActivatedEvent
    {
        public string RunId { get; }
        public int Rank { get; }
        public float DurationSeconds { get; }

        public CavebornCapstoneActivatedEvent(string runId, int rank, float durationSeconds)
        {
            RunId = runId ?? string.Empty;
            Rank = rank;
            DurationSeconds = Math.Max(0f, durationSeconds);
        }
    }
}
