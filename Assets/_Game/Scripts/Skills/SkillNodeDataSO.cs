using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillNode_", menuName = "CindarsHope/Skills/SkillNode")]
    public class SkillNodeDataSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identity")]
        public string SkillNodeId;
        public string TreeId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Type")]
        public SkillNodeType NodeType = SkillNodeType.PassiveStat;
        public SkillCategory SkillCategory = SkillCategory.PassiveSkill;
        public bool IsCapstone;

        [Header("Cost & Prerequisites")]
        public int SkillPointCost = 1;
        public int MinimumPlayerLevel = 1;
        public List<string> PrerequisiteNodeIds = new List<string>();
        public int RequiredPurchasedNodesInTree = 0;

        [Header("Unlocks")]
        public string UnlockedSkillActionId;
        public string LinkedSpellId;

        [Header("Passive Modifiers")]
        public List<SkillPassiveModifier> PassiveModifiers = new List<SkillPassiveModifier>();

        // Legacy compatibility
        public string Id => SkillNodeId;
        string IIdentifiedData.Id => SkillNodeId;

        private void OnValidate()
        {
            SkillPointCost = Mathf.Max(1, SkillPointCost);
            MinimumPlayerLevel = Mathf.Max(1, MinimumPlayerLevel);
        }
    }
}
