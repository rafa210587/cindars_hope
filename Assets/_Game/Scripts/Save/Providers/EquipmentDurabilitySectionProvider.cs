using CindarsHope.Equipment;
using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da durabilidade dos equipamentos. Fonte do estado:
    /// <see cref="EquipmentManager.DurabilityTracker"/>, via <see cref="EquipmentManager"/> injetado
    /// via constructor. Fallback: seção de durabilidade vazia quando o tracker não está disponível.
    /// </summary>
    public class EquipmentDurabilitySectionProvider : ISaveSectionProvider
    {
        private readonly EquipmentManager _equipmentManager;

        public EquipmentDurabilitySectionProvider(EquipmentManager equipmentManager)
        {
            _equipmentManager = equipmentManager;
        }

        public string ProviderId => "equipment_durability";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_equipmentManager != null && _equipmentManager.DurabilityTracker != null)
            {
                return _equipmentManager.DurabilityTracker.CaptureSaveData();
            }

            return new EquipmentDurabilitySaveData();
        }

        public void Restore(object sectionData)
        {
            if (_equipmentManager == null || _equipmentManager.DurabilityTracker == null)
            {
                return;
            }

            var data = sectionData as EquipmentDurabilitySaveData;
            if (data != null)
            {
                _equipmentManager.DurabilityTracker.LoadFromSaveData(data);
            }
        }
    }
}
