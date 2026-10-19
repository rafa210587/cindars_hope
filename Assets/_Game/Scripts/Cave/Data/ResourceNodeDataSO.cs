using CindarsHope.Core.Data;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "ResourceNodeData", menuName = "CindarsHope/Cave/Resource Node Data")]
    public sealed class ResourceNodeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        public ToolType RequiredToolType = ToolType.Pickaxe;
        public ToolTier RequiredToolTier = ToolTier.Basic;
        public int StaminaCost = 1;
        public int HitsRequired = 2;
        public string PrimaryDropItemId;
        public int PrimaryDropAmount = 1;
        public bool RespawnsDaily;
        public string FallbackItemId = "item_material_stone";
        public int FallbackAmount = 1;
        public bool FallbackDepletesNode;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            StaminaCost = Mathf.Max(0, StaminaCost);
            HitsRequired = Mathf.Max(1, HitsRequired);
            PrimaryDropAmount = Mathf.Max(0, PrimaryDropAmount);
            FallbackAmount = Mathf.Max(0, FallbackAmount);
        }
    }
}
