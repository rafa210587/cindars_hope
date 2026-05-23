using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave
{
    [CreateAssetMenu(fileName = "CaveEntry_", menuName = "CindarsHope/Cave/CaveEntryData")]
    public class CaveEntryDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string EntryName;
        public int CaveLevel;
        public int MaxLoadoutItems = 5;
        public int MaxEquipmentSlots = 5;
        public bool AllowFoodConsumption = true;
        public bool AllowPotionUsage = true;
        public int MinimumLevelRequired = 1;
        public float EntryHeatResistanceRequired;
        public float EntryColdResistanceRequired;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            CaveLevel = Mathf.Max(1, CaveLevel);
            MaxLoadoutItems = Mathf.Max(1, MaxLoadoutItems);
            MaxEquipmentSlots = Mathf.Max(1, MaxEquipmentSlots);
            MinimumLevelRequired = Mathf.Max(1, MinimumLevelRequired);
        }
    }
}
