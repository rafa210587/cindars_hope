using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "CindarsHope/Data/Equipment")]
    public class EquipmentDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public int BaseValue;
        public int DurabilityMax = 100;
        public int StrengthBonus;
        public int BaseDefense;
        public int BreathBonus;
        public int ColdResistance;
        public int HeatResistance;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseValue = Mathf.Max(0, BaseValue);
            DurabilityMax = Mathf.Max(1, DurabilityMax);
            StrengthBonus = Mathf.Max(0, StrengthBonus);
            BaseDefense = Mathf.Max(0, BaseDefense);
            BreathBonus = Mathf.Max(0, BreathBonus);
            ColdResistance = Mathf.Max(0, ColdResistance);
            HeatResistance = Mathf.Max(0, HeatResistance);
        }
    }
}
