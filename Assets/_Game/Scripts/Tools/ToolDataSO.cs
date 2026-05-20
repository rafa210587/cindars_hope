using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "CindarsHope/Tools/Tool Data")]
    public class ToolDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        public ToolType ToolType;
        public ToolTier Tier = ToolTier.Basic;
        public string ItemId;

        string IIdentifiedData.Id => Id;
    }
}
