using CindarsHope.Combat;
using CindarsHope.Core.Data;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Skills
{
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
        public float CooldownSeconds = 1f;
        public float StaminaCost = 0f;
        public float ManaCost = 0f;
        public string LinkedSpellId;

        [Header("Damage")]
        public int BaseDamage = 0;
        public DamageType DamageType = DamageType.Physical;
        public float Range = 5f;

        [Header("Projectile")]
        public int ProjectileCount = 1;
        public float ProjectileSpreadDegrees = 0f;
        public int LinePierceCount = 0;
        public float ChargeTimeSeconds = 0f;

        [Header("Block / Dodge / Leap")]
        public float BlockDurationSeconds = 0f;
        public float DashDistance = 0f;
        public float LeapDistance = 0f;

        string IIdentifiedData.Id => SkillActionId;
    }
}
