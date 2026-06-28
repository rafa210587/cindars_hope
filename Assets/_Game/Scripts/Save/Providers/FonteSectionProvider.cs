using CindarsHope.Fonte;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da Fonte de Anya. Fonte do estado: <see cref="FonteRuntimeService.Instance"/>
    /// (singleton DontDestroyOnLoad). Fallback: Fonte dormante (seção default) quando o serviço não está
    /// inicializado ou em saves legados sem a seção.
    /// </summary>
    public class FonteSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "fonte";

        public object Capture(GameSaveData existingSaveData)
        {
            var fonte = FonteRuntimeService.Instance;
            if (fonte == null)
            {
                return existingSaveData?.Fonte ?? new FonteSaveData();
            }

            var data = new FonteSaveData
            {
                FonteState = (int)fonte.Section.FonteState,
                LivingWaterUnlocked = fonte.Section.LivingWater.Unlocked,
                LivingWaterCharges = fonte.Section.LivingWater.CurrentCharges,
                LastGrantDay = fonte.LastGrantDay
            };

            foreach (var fn in fonte.Section.UnlockedFunctions)
            {
                data.UnlockedFunctions.Add((int)fn);
            }

            foreach (var fragment in fonte.Progression.FragmentStates)
            {
                if (fragment.IsIntegrated())
                {
                    data.IntegratedFragments.Add((int)fragment.FragmentType);
                }
            }

            return data;
        }

        public void Restore(object sectionData)
        {
            var fonte = FonteRuntimeService.Instance;
            if (fonte == null)
            {
                return;
            }

            // sectionData null (save legado) = Fonte dormante padrão.
            var data = sectionData as FonteSaveData;
            if (data == null)
            {
                return;
            }

            fonte.RestoreFromSave(
                data.FonteState,
                data.UnlockedFunctions,
                data.LivingWaterUnlocked,
                data.LivingWaterCharges,
                data.LastGrantDay,
                data.IntegratedFragments);
        }
    }
}
