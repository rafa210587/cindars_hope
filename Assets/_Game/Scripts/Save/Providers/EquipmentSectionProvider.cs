using CindarsHope.Equipment;
using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do equipamento equipado pelo jogador. Fonte do estado:
    /// <see cref="EquipmentManager"/> injetado via constructor.
    /// Fallback: seção de equipamento vazia quando o manager não está disponível.
    /// </summary>
    public class EquipmentSectionProvider : ISaveSectionProvider
    {
        private readonly EquipmentManager _equipmentManager;

        public EquipmentSectionProvider(EquipmentManager equipmentManager)
        {
            _equipmentManager = equipmentManager;
        }

        public string ProviderId => "equipment";

        public object Capture(GameSaveData existingSaveData)
        {
            return _equipmentManager != null
                ? _equipmentManager.CaptureSaveData()
                : new EquipmentSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_equipmentManager == null)
            {
                return;
            }

            _equipmentManager.RestoreFromSaveData(sectionData as EquipmentSaveData ?? new EquipmentSaveData());
        }
    }
}
