using CindarsHope.Combat;
using CindarsHope.Core.Data;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Skills
{
    [System.Serializable]
    public sealed class SkillActionRankData
    {
        public int Rank = 1;
        public int Damage;
        public float StaminaCost;
        public float ManaCost;
        public float CooldownSeconds;
        public float Range;

        public SkillActionRankData Copy()
        {
            return (SkillActionRankData)MemberwiseClone();
        }
    }

    public enum SkillActionType
    {
        DamageSkill,
        SelfBuffSkill,
        LinkedSpellSkill,
        BlockSkill,
        DashSkill,
        LeapSkill,
        ProjectileSkill,
        AreaSkill
    }

    [CreateAssetMenu(fileName = "SkillAction_", menuName = "CindarsHope/Skills/Skill Action")]
    public class SkillActionSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identity")]
        public string SkillActionId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Execution")]
        public SkillActionType SkillActionType = SkillActionType.DamageSkill;
        public string TimingProfileId;
        [Min(0f)] public float WindupSeconds;
        [Min(0f)] public float ActiveSeconds;
        [Min(0f)] public float RecoverySeconds;
        public float CooldownSeconds = 1f;
        public float StaminaCost = 0f;
        public float ManaCost = 0f;
        public string LinkedSpellId;
        public SpellDiscipline SpellDiscipline = SpellDiscipline.None;

        [Header("Damage")]
        public int BaseDamage = 0;
        public DamageType DamageType = DamageType.Physical;
        public float Range = 5f;
        public int DamagePerRank;

        [Header("Projectile")]
        public int ProjectileCount = 1;
        public float ProjectileSpreadDegrees = 0f;
        public int LinePierceCount = 0;
        [Min(0f)] public float ChargeMinimumHoldSeconds = 0f;
        public float ChargeTimeSeconds = 0f;
        [Min(0)] public int ChargeMaximumDamage = 0;
        [Min(0f)] public float ChargeMaximumRange = 0f;
        [Min(0f)] public float ChargeMaximumPostureDamageMultiplier = 0f;
        [Min(0f)] public float ChargeMaximumStaminaCost = 0f;
        public float ProjectileSpeed = 10f;

        [Header("Target Modifier")]
        [Min(0f)] public float EffectDurationSeconds = 0f;
        [Min(0f)] public float EffectDurationPerRank = 0f;
        [Min(0f)] public float RangedDamageBonusFraction = 0f;
        [Min(0f)] public float RangedDamageBonusPerRank = 0f;

        [Header("Utility / Survival")]
        public float[] EffectMagnitudeByRank = new float[0];
        public float[] SecondaryMagnitudeByRank = new float[0];
        public float[] SecondaryDurationByRank = new float[0];
        [Min(0f)] public float EffectRadius = 0f;

        [Header("Persistent / Chain / Control")]
        [Min(0)] public int PulseCount = 0;
        [Min(0f)] public float ChainJumpRange = 0f;
        [Min(0f)] public float TargetingRange = 0f;
        public float[] TargetDamageMultipliers = new float[0];
        public float[] ControlStrengthByRank = new float[0];
        [Min(0)] public int EffectHitCharges = 0;
        [Range(0f, 1f)] public float StrongSlowFraction = 0f;
        [Min(0f)] public float StrongSlowDurationSeconds = 0f;

        [Header("Shape & Hit Policy")]
        public float ArcDegrees = 0f;
        public int MaxTargets = 0;
        public int FullDamageTargetCount = 0;
        [Range(0f, 1f)] public float AdditionalTargetDamageMultiplier = 1f;
        public float KnockbackForce = 2.5f;
        public float PostureDamageMultiplier = 1f;
        public string StatusEffectId;
        [Range(0f, 1f)] public float StatusApplyChance;

        [Header("Readiness")]
        public bool NotYetExecutable;

        [Header("Block / Dodge / Leap")]
        public float BlockDurationSeconds = 0f;
        public float DashDistance = 0f;
        public float LeapDistance = 0f;

        public SkillActionRankData ResolveRank(int rank)
        {
            int safeRank = Mathf.Max(1, rank);
            return new SkillActionRankData
            {
                Rank = safeRank,
                Damage = Mathf.Max(0, BaseDamage + DamagePerRank * (safeRank - 1)),
                StaminaCost = Mathf.Max(0f, StaminaCost),
                ManaCost = Mathf.Max(0f, ManaCost),
                CooldownSeconds = Mathf.Max(0f, CooldownSeconds),
                Range = Mathf.Max(0f, Range)
            };
        }

        public int ResolveDamageForTargetIndex(int rank, int targetIndex)
        {
            int damage = ResolveRank(rank).Damage;
            return FullDamageTargetCount > 0 && targetIndex >= FullDamageTargetCount
                ? Mathf.RoundToInt(damage * Mathf.Clamp01(AdditionalTargetDamageMultiplier))
                : damage;
        }

        public float ResolveEffectDuration(int rank)
            => Mathf.Max(0f, EffectDurationSeconds + EffectDurationPerRank * (Mathf.Max(1, rank) - 1));

        public float ResolveRangedDamageBonus(int rank)
            => Mathf.Max(0f, RangedDamageBonusFraction + RangedDamageBonusPerRank * (Mathf.Max(1, rank) - 1));

        public float ResolveControlStrength(int rank)
        {
            if (ControlStrengthByRank == null || ControlStrengthByRank.Length == 0) return 0f;
            return Mathf.Clamp01(ControlStrengthByRank[Mathf.Clamp(rank - 1, 0, ControlStrengthByRank.Length - 1)]);
        }

        public float ResolveEffectMagnitude(int rank)
            => ResolveRankValue(EffectMagnitudeByRank, rank);

        public float ResolveSecondaryMagnitude(int rank)
            => ResolveRankValue(SecondaryMagnitudeByRank, rank);

        public float ResolveSecondaryDuration(int rank)
            => ResolveRankValue(SecondaryDurationByRank, rank);

        private static float ResolveRankValue(float[] values, int rank)
        {
            if (values == null || values.Length == 0) return 0f;
            return Mathf.Max(0f, values[Mathf.Clamp(rank - 1, 0, values.Length - 1)]);
        }

        string IIdentifiedData.Id => SkillActionId;
    }
}
