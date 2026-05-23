using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [CreateAssetMenu(fileName = "Equipment_", menuName = "CindarsHope/Equipment/EquipmentData")]
    public class EquipmentDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public EquipmentType Type;
        public int BaseDefense;
        public int StrengthBonus;
        public int DexterityBonus;
        public int IntelligenceBonus;
        public int WillpowerBonus;
        public int ConstitutionBonus;
        public int BreathBonus;
        public int HeatResistance;
        public int ColdResistance;
        public int Weight;
        public int BaseValue;
        public int DurabilityMax = 100;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseDefense = Mathf.Max(0, BaseDefense);
            Weight = Mathf.Max(0, Weight);
            BaseValue = Mathf.Max(0, BaseValue);
            DurabilityMax = Mathf.Max(1, DurabilityMax);
        }
    }

    public enum EquipmentType
    {
        None,
        Helmet,
        Armor,
        Gloves,
        Boots,
        Accessory,
        Weapon,
        Shield
    }
}
