using CindarsHope.Core.Data;
using System;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillTree_", menuName = "CindarsHope/Skills/SkillTree")]
    public class SkillTreeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string TreeName;
        [TextArea] public string Description;
        public SkillNodeDataSO[] Nodes;
        public int MaxActiveSlots = 4;
        public bool AllowRespeccing = true;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            MaxActiveSlots = Mathf.Max(1, MaxActiveSlots);
        }
    }

    [Serializable]
    public class SkillTreeSaveData
    {
        public string TreeId;
        public string[] UnlockedNodeIds;
        public string[] ActiveSlotNodeIds;
        public int TotalSpentPoints;
    }
}
