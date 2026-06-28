using CindarsHope.NPC.Services;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save das pendências dos serviços de NPC (encomendas, contratos, pratos, pasto).
    /// Fonte do estado: <see cref="NpcServiceRuntime.Instance"/> (singleton DontDestroyOnLoad).
    /// Fallback: sem pendências em saves legados sem a seção (CA-5).
    /// </summary>
    public class NpcServicesSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "npc_services";

        public object Capture(GameSaveData existingSaveData)
        {
            var runtime = NpcServiceRuntime.Instance;
            return runtime != null
                ? runtime.CaptureSaveData()
                : (existingSaveData?.NpcServices ?? new NpcServicesSaveData());
        }

        public void Restore(object sectionData)
        {
            var runtime = NpcServiceRuntime.Instance;
            if (runtime == null)
            {
                return;
            }

            // sectionData null (save legado) = sem encomendas/contrato/prato/pasto.
            runtime.RestoreFromSaveData(sectionData as NpcServicesSaveData);
        }
    }
}
