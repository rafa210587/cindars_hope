using System;
using System.Collections.Generic;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Skills
{
    [CreateAssetMenu(fileName = "SkillTree_", menuName = "CindarsHope/Skills/SkillTree")]
    public class SkillTreeDataSO : ScriptableObject, IIdentifiedData
    {
        [Header("Identity")]
        public string TreeId;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Nodes")]
        public List<SkillNodeDataSO> Nodes = new List<SkillNodeDataSO>();
        public string CapstoneNodeId;

        string IIdentifiedData.Id => TreeId;
    }
}
