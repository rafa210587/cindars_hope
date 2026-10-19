using UnityEngine;

namespace CindarsHope.Combat
{
    public readonly struct ControlDiminishingReturnsResult
    {
        public bool CanApply { get; }
        public float Multiplier { get; }
        public float EffectiveDurationSeconds { get; }
        public float ImmunityUntil { get; }

        public ControlDiminishingReturnsResult(bool canApply, float multiplier,
            float effectiveDurationSeconds, float immunityUntil)
        {
            CanApply = canApply;
            Multiplier = multiplier;
            EffectiveDurationSeconds = effectiveDurationSeconds;
            ImmunityUntil = immunityUntil;
        }
    }

    /// <summary>Transient per-target diminishing returns shared by strong control effects.</summary>
    public sealed class ControlDiminishingReturnsState
    {
        public const float ImmunitySeconds = 4f;
        private static readonly float[] Multipliers = { 1f, .6f, .3f };
        private int _eligibleApplications;
        private float _immunityUntil;

        public int EligibleApplications => _eligibleApplications;
        public float ImmunityUntil => _immunityUntil;

        public ControlDiminishingReturnsResult TryApply(float baseDurationSeconds, float now)
        {
            if (baseDurationSeconds <= 0f)
                return new ControlDiminishingReturnsResult(false, 0f, 0f, _immunityUntil);

            if (_immunityUntil > 0f)
            {
                if (now < _immunityUntil)
                    return new ControlDiminishingReturnsResult(false, 0f, 0f, _immunityUntil);

                _eligibleApplications = 0;
                _immunityUntil = 0f;
            }

            int index = Mathf.Clamp(_eligibleApplications, 0, Multipliers.Length - 1);
            float multiplier = Multipliers[index];
            _eligibleApplications++;
            if (_eligibleApplications >= Multipliers.Length)
                _immunityUntil = now + ImmunitySeconds;

            return new ControlDiminishingReturnsResult(
                true,
                multiplier,
                baseDurationSeconds * multiplier,
                _immunityUntil);
        }

        public void Reset()
        {
            _eligibleApplications = 0;
            _immunityUntil = 0f;
        }
    }
}
