using CindarsHope.Combat;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillAction_", menuName = "CindarsHope/Skills/Skill Action")]
    public class SkillActionSO : ScriptableObject, IIdentifiedData
    {
        public string SkillActionId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public SkillActionType SkillActionType = SkillActionType.DamageSkill;
        public float CooldownSeconds = 1f;
        public float StaminaCost = 0f;
        public float ManaCost = 0f;
        public int BaseDamage = 0;
        public DamageType DamageType = DamageType.Physical;
        public float Range = 5f;
        public string LinkedSpellId;

        string IIdentifiedData.Id => SkillActionId;
    }

    public enum SkillActionType
    {
        DamageSkill,
        SelfBuffSkill,
        LinkedSpellSkill
    }
}
