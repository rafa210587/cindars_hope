using CindarsHope.Farm.Runtime;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save das metas diárias da fazenda. Fonte do estado:
    /// <see cref="FarmDailyGoalService.Instance"/> (singleton DontDestroyOnLoad).
    /// Fallback: seção default (sem metas registradas) quando o serviço não está ativo.
    /// </summary>
    public class DailyGoalsSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "daily_goals";

        public object Capture(GameSaveData existingSaveData)
        {
            var service = FarmDailyGoalService.Instance;
            return service != null
                ? service.CaptureSaveData()
                : (existingSaveData?.DailyGoals ?? new FarmDailyGoalsSaveData());
        }

        public void Restore(object sectionData)
        {
            var service = FarmDailyGoalService.Instance;
            if (service == null)
            {
                return;
            }

            // sectionData null (save legado) = sem metas registradas.
            var data = sectionData as FarmDailyGoalsSaveData;
            if (data != null)
            {
                service.RestoreFromSaveData(data);
            }
        }
    }
}
