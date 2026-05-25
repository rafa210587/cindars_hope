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
        public int StaminaRestore;
        public string[] StatusEffectIds;
        public float BuffDurationSeconds;
        public bool IsEquippable;
        public int DurabilityRestoreAmount;
        public string WeaponId;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            MaxStack = Mathf.Max(1, MaxStack);
            BaseValue = Mathf.Max(0, BaseValue);
            HungerRestore = Mathf.Max(0, HungerRestore);
            StaminaRestore = Mathf.Max(0, StaminaRestore);
            BuffDurationSeconds = Mathf.Max(0, BuffDurationSeconds);
            DurabilityRestoreAmount = Mathf.Max(0, DurabilityRestoreAmount);
        }
    }
}
