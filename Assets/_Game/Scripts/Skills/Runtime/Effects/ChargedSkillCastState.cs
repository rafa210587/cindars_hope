using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public readonly struct ChargedSkillProfile
    {
        public float MinimumHoldSeconds { get; }
        public float MaximumHoldSeconds { get; }
        public int MinimumDamage { get; }
        public int MaximumDamage { get; }
        public float MinimumRange { get; }
        public float MaximumRange { get; }
        public float MinimumPostureMultiplier { get; }
        public float MaximumPostureMultiplier { get; }
        public float MinimumStaminaCost { get; }
        public float MaximumStaminaCost { get; }

        public ChargedSkillProfile(float minimumHoldSeconds, float maximumHoldSeconds,
            int minimumDamage, int maximumDamage, float minimumRange, float maximumRange,
            float minimumPostureMultiplier, float maximumPostureMultiplier,
            float minimumStaminaCost, float maximumStaminaCost)
        {
            MinimumHoldSeconds = Mathf.Max(0f, minimumHoldSeconds);
            MaximumHoldSeconds = Mathf.Max(MinimumHoldSeconds, maximumHoldSeconds);
            MinimumDamage = Mathf.Max(0, minimumDamage);
            MaximumDamage = Mathf.Max(MinimumDamage, maximumDamage);
            MinimumRange = Mathf.Max(0f, minimumRange);
            MaximumRange = Mathf.Max(MinimumRange, maximumRange);
            MinimumPostureMultiplier = Mathf.Max(0f, minimumPostureMultiplier);
            MaximumPostureMultiplier = Mathf.Max(MinimumPostureMultiplier, maximumPostureMultiplier);
            MinimumStaminaCost = Mathf.Max(0f, minimumStaminaCost);
            MaximumStaminaCost = Mathf.Max(MinimumStaminaCost, maximumStaminaCost);
        }
    }

    public readonly struct ChargedSkillResolution
    {
        public bool CanCommit { get; }
        public float NormalizedCharge { get; }
        public int Damage { get; }
        public float Range { get; }
        public float PostureMultiplier { get; }
        public int StaminaCost { get; }

        public ChargedSkillResolution(bool canCommit, float normalizedCharge, int damage,
            float range, float postureMultiplier, int staminaCost)
        {
            CanCommit = canCommit;
            NormalizedCharge = normalizedCharge;
            Damage = damage;
            Range = range;
            PostureMultiplier = postureMultiplier;
            StaminaCost = staminaCost;
        }
    }

    public sealed class ChargedSkillCastState
    {
        private ChargedSkillProfile _profile;
        public bool IsHolding { get; private set; }
        public float HeldSeconds { get; private set; }

        public void Begin(ChargedSkillProfile profile)
        {
            _profile = profile;
            IsHolding = true;
            HeldSeconds = 0f;
        }

        public void Tick(float scaledDeltaSeconds)
        {
            if (!IsHolding) return;
            HeldSeconds = Mathf.Min(_profile.MaximumHoldSeconds, HeldSeconds + Mathf.Max(0f, scaledDeltaSeconds));
        }

        public ChargedSkillResolution Release()
        {
            if (!IsHolding || HeldSeconds < _profile.MinimumHoldSeconds)
            {
                Cancel();
                return default;
            }

            float normalized = Mathf.InverseLerp(_profile.MinimumHoldSeconds, _profile.MaximumHoldSeconds, HeldSeconds);
            IsHolding = false;
            return new ChargedSkillResolution(
                true,
                normalized,
                Mathf.RoundToInt(Mathf.Lerp(_profile.MinimumDamage, _profile.MaximumDamage, normalized)),
                Mathf.Lerp(_profile.MinimumRange, _profile.MaximumRange, normalized),
                Mathf.Lerp(_profile.MinimumPostureMultiplier, _profile.MaximumPostureMultiplier, normalized),
                Mathf.RoundToInt(Mathf.Lerp(_profile.MinimumStaminaCost, _profile.MaximumStaminaCost, normalized)));
        }

        public void Cancel()
        {
            IsHolding = false;
            HeldSeconds = 0f;
        }
    }
}
