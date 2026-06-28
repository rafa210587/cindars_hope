using CindarsHope.Farm.Lots;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da posse dos lotes de expansão da fazenda. Fonte do estado:
    /// <see cref="FarmLotService.Instance"/> (singleton DontDestroyOnLoad).
    /// Domínio global independente de cena. Fallback: todos os lotes bloqueados (CA-4) em saves legados.
    /// </summary>
    public class FarmLotsSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "farm_lots";

        public object Capture(GameSaveData existingSaveData)
        {
            var service = FarmLotService.Instance;
            return service != null
                ? service.CaptureSaveData()
                : (existingSaveData?.FarmLots ?? new FarmLotsSaveData());
        }

        public void Restore(object sectionData)
        {
            var service = FarmLotService.Instance;
            if (service == null)
            {
                return;
            }

            // sectionData null (save legado) = todos os lotes Locked (CA-4), sem erro.
            service.RestoreFromSaveData(sectionData as FarmLotsSaveData);
        }
    }
}
