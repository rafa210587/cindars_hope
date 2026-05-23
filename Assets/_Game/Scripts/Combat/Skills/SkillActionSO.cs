using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat.Skills
{
    [CreateAssetMenu(fileName = "SkillAction_", menuName = "CindarsHope/Combat/SkillAction")]
    public class SkillActionSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string ActionName;
        [TextArea] public string Description;
        public Sprite Icon;
        public SkillActionType Type;
        public int BaseDamage;
        public int StaminaCost;
        public int ManaCost;
        public int CooldownMs;
        public int RequiredLevel;
        public int CastRangeMeters = 5;
        public float AreaOfEffectRadius = 0f;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseDamage = Mathf.Max(0, BaseDamage);
            StaminaCost = Mathf.Max(0, StaminaCost);
            ManaCost = Mathf.Max(0, ManaCost);
            CooldownMs = Mathf.Max(100, CooldownMs);
            RequiredLevel = Mathf.Max(1, RequiredLevel);
            CastRangeMeters = Mathf.Max(1, CastRangeMeters);
            AreaOfEffectRadius = Mathf.Max(0, AreaOfEffectRadius);
        }
    }

    public enum SkillActionType
    {
        None,
        Melee,
        Ranged,
        Magic,
        Utility,
        Heal,
        Dash,
        Block
    }
}
