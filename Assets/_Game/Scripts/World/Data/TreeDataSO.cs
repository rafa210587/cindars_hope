using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.World.Data
{
    [CreateAssetMenu(fileName = "TreeData", menuName = "CindarsHope/Data/Tree")]
    public class TreeDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        public string WoodItemId = "item_wood";
        public int WoodAmount = 2;
        public int RequiredHits = 3;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            WoodAmount = Mathf.Max(1, WoodAmount);
            RequiredHits = Mathf.Max(1, RequiredHits);

            if (string.IsNullOrWhiteSpace(Id))
            {
                Debug.LogWarning($"{nameof(TreeDataSO)} '{name}' has an empty Id.", this);
            }
        }
    }
}
