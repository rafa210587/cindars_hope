using CindarsHope.Save;
using CindarsHope.UI.Onboarding;

namespace CindarsHope.UI.Onboarding.Save
{
    /// <summary>
    /// fable_62 — provider de save dos hints de onboarding vistos, no padrao ISaveSectionProvider
    /// (precedente HotbarSectionProvider/SpellbookSectionProvider). Fonte do estado:
    /// <see cref="OnboardingHintService.Instance"/> (runtime singleton, padrao F07).
    /// Fallback: preserva a secao existente quando o servico ainda nao esta vivo.
    /// Default (sem instancia e sem save): lista vazia = todos os hints elegiveis (save legado OK,
    /// sem migration).
    /// arch: quebra do ciclo Save|UI (spec_arch_save_ui_cycle_reduction_v26) — provider relocado de
    /// CindarsHope.Save.Providers para o modulo dono (UI.Onboarding), mesma tecnica de
    /// spec_arch_quests_save_cycle_reduction_v24 (SaveManager constroi por nome totalmente
    /// qualificado, sem novo using de topo).
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
