using CindarsHope.Farm;
using UnityEngine.SceneManagement;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da fazenda (plots, árvores, animais, processamento, envio, forrageio).
    /// Captura os dados de plots/árvores apenas quando na FarmScene; fora dela preserva o save existente.
    /// Sub-seções aditivas (Processing, PendingShipping, ForageSpawns) são capturadas sempre via
    /// seus singletons DontDestroyOnLoad, independente da cena ativa.
    /// </summary>
    public class FarmSectionProvider : ISaveSectionProvider
    {
        private const string FarmSceneName = "FarmScene";

        private readonly FarmPlotRegistry _farmPlotRegistry;

        public FarmSectionProvider(FarmPlotRegistry farmPlotRegistry)
        {
            _farmPlotRegistry = farmPlotRegistry;
        }

        public string ProviderId => "farm";

        public object Capture(GameSaveData existingSaveData)
        {
            var activeScene = SceneManager.GetActiveScene();
            FarmSaveData farmSaveData;

            if (activeScene.name == FarmSceneName && _farmPlotRegistry != null)
            {
                farmSaveData = _farmPlotRegistry.CaptureSaveData();
            }
            else if (existingSaveData?.Farm != null)
            {
                farmSaveData = existingSaveData.Farm;
            }
            else
            {
                farmSaveData = new FarmSaveData();
            }

            // fable_55: jobs de processamento são DontDestroyOnLoad — sempre sobrescreve o campo aditivo.
            var processingService = Farm.Processing.FarmProcessingStationService.Instance;
            if (processingService != null)
            {
                farmSaveData.Processing = processingService.CaptureSaveData();
            }
            else if (farmSaveData.Processing == null)
            {
                farmSaveData.Processing = existingSaveData?.Farm?.Processing ?? new Farm.Processing.FarmProcessingSaveData();
            }

            // fable_54: batch de envio pendente — DontDestroyOnLoad.
            var shippingBin = Farm.Shipping.ShippingBinRuntimeService.Instance;
            if (shippingBin != null)
            {
                farmSaveData.PendingShipping = shippingBin.CaptureSaveData();
            }
            else if (farmSaveData.PendingShipping == null)
            {
                farmSaveData.PendingShipping = existingSaveData?.Farm?.PendingShipping ?? new Farm.Shipping.PendingShippingSaveData();
            }

            // fable_54: estado dos pontos de forrageio — DontDestroyOnLoad.
            var forageService = Farm.Forage.FarmForageRuntimeService.Instance;
            if (forageService != null)
            {
                farmSaveData.ForageSpawns = forageService.CaptureSaveData();
            }
            else if (farmSaveData.ForageSpawns == null)
            {
                farmSaveData.ForageSpawns = existingSaveData?.Farm?.ForageSpawns ?? new Farm.Forage.ForageSpawnsSaveData();
            }

            return farmSaveData;
        }

        public void Restore(object sectionData)
        {
            var data = sectionData as FarmSaveData;

            if (_farmPlotRegistry != null && data != null)
            {
                _farmPlotRegistry.RestoreFromSaveData(data);
            }

            // fable_55: restaura jobs de processamento; ausente em save legado = nenhuma estação em produção.
            if (Farm.Processing.FarmProcessingStationService.Instance != null)
            {
                Farm.Processing.FarmProcessingStationService.Instance.RestoreFromSaveData(data?.Processing);
            }

            // fable_54: restaura batch de envio pendente.
            if (Farm.Shipping.ShippingBinRuntimeService.Instance != null)
            {
                Farm.Shipping.ShippingBinRuntimeService.Instance.RestoreFromSaveData(data?.PendingShipping);
            }

            // fable_54: restaura estado dos pontos de forrageio.
            if (Farm.Forage.FarmForageRuntimeService.Instance != null)
            {
                Farm.Forage.FarmForageRuntimeService.Instance.RestoreFromSaveData(data?.ForageSpawns);
            }
        }
    }
}
