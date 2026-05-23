using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "CindarsHope/Data/Item")]
    public class ItemDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public ItemCategory Category;
        public ConsumableSubtype ConsumableSubtype;
        public int MaxStack = 1;
        public int BaseValue;
        public int HungerRestore;
        public bool IsEquippable;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            MaxStack = Mathf.Max(1, MaxStack);
            BaseValue = Mathf.Max(0, BaseValue);
            HungerRestore = Mathf.Max(0, HungerRestore);
        }
    }
}
