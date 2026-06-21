using CindarsHope.UI.Onboarding;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// fable_62 — provider de save dos hints de onboarding vistos, no padrao ISaveSectionProvider
    /// (precedente HotbarSectionProvider/SpellbookSectionProvider). Fonte do estado:
    /// <see cref="OnboardingHintService.Instance"/> (runtime singleton, padrao F07).
    /// Fallback: preserva a secao existente quando o servico ainda nao esta vivo.
    /// Default (sem instancia e sem save): lista vazia = todos os hints elegiveis (save legado OK,
    /// sem migration).
    /// </summary>
    public class OnboardingHintsSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "onboarding_hints";

        public object Capture(GameSaveData existingSaveData)
        {
            var service = OnboardingHintService.Instance;
            if (service == null)
            {
                // Servico nao inicializado: preserva o que ja estava no save.
                return existingSaveData?.OnboardingHints ?? new OnboardingHintsSaveData();
            }

            return new OnboardingHintsSaveData
            {
                SeenHintIds = service.Tracker.GetSeenHintIds()
            };
        }

        public void Restore(object sectionData)
        {
            var service = OnboardingHintService.Instance;
            if (service == null)
            {
                return;
            }

            // sectionData null (secao ausente em save legado) = lista vazia = todos elegiveis.
            var data = sectionData as OnboardingHintsSaveData;
            service.Tracker.RestoreSeen(data?.SeenHintIds);
        }
    }
}
