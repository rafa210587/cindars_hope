using System;

namespace CindarsHope.Foundation
{
    /// <summary>Movement actions that may receive a directional survival modifier.</summary>
    public enum MobilityActionKind
    {
        SprintTick = 0,
        DodgeCommit = 1
    }

    /// <summary>
    /// Neutral, engine-free result consumed by player movement. Cost and movement remain
    /// independent so an effect cannot silently change an unrelated stamina channel.
    /// </summary>
    public readonly struct DirectionalMobilityModifier
    {
        public static readonly DirectionalMobilityModifier Neutral =
            new DirectionalMobilityModifier(1f, 1f);

        public float CostMultiplier { get; }
        public float SpeedMultiplier { get; }

        public DirectionalMobilityModifier(float costMultiplier, float speedMultiplier)
        {
            CostMultiplier = SanitizeMultiplier(costMultiplier);
            SpeedMultiplier = SanitizeMultiplier(speedMultiplier);
        }

        private static float SanitizeMultiplier(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 1f;
            }

            return Math.Max(0f, value);
        }
    }

    /// <summary>
    /// Implemented by the owner of the temporary effect. The owner keeps threat selection and
    /// expiry private; sprint/dodge only provide their intended world-space direction.
    /// </summary>
    public interface IDirectionalMobilityModifierRuntime
    {
        DirectionalMobilityModifier Resolve(
            MobilityActionKind actionKind,
            float directionX,
            float directionY);
    }

    /// <summary>
    /// Foundation seam read by player movement without taking a dependency on Skills. Missing
    /// runtime wiring is deliberately neutral.
    /// </summary>
    public static class DirectionalMobilityModifierProvider
    {
        public static IDirectionalMobilityModifierRuntime Source;

        public static DirectionalMobilityModifier Resolve(
            MobilityActionKind actionKind,
            float directionX,
            float directionY)
        {
            return Source != null
                ? Source.Resolve(actionKind, directionX, directionY)
                : DirectionalMobilityModifier.Neutral;
        }
    }
}
