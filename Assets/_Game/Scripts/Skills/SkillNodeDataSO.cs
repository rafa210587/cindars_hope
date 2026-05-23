using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillNode_", menuName = "CindarsHope/Skills/SkillNode")]
    public class SkillNodeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string NodeName;
        [TextArea] public string Description;
        public Sprite Icon;
        public int SkillPointCost = 1;
        public int RequiredLevel = 1;
        public string RequiredSkillNodeId;
        public int StrengthBonus;
        public int DexterityBonus;
        public int IntelligenceBonus;
        public int WillpowerBonus;
        public int ConstitutionBonus;
        public int BreathBonus;
        public int DamageBonus;
        public int DefenseBonus;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            SkillPointCost = Mathf.Max(1, SkillPointCost);
            RequiredLevel = Mathf.Max(1, RequiredLevel);
        }
    }
}
