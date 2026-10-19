using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save dos hints de onboarding vistos.
    /// Consulta o runtime UI via port puro registrado no DomainManagerRegistry.
    /// </summary>
    public sealed class OnboardingHintsSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "onboarding_hints";

        public object Capture(GameSaveData existingSaveData)
        {
            var runtime = DomainManagerRegistry.Get<IOnboardingHintsRuntime>();
            if (runtime == null)
            {
                return existingSaveData?.OnboardingHints ?? new OnboardingHintsSaveData();
            }

            return new OnboardingHintsSaveData
            {
                SeenHintIds = new List<string>(runtime.GetSeenHintIds())
            };
        }

        public void Restore(object sectionData)
        {
            var runtime = DomainManagerRegistry.Get<IOnboardingHintsRuntime>();
            if (runtime == null)
            {
                return;
            }

            var data = sectionData as OnboardingHintsSaveData;
            runtime.RestoreSeenHintIds(data?.SeenHintIds);
        }
    }
}
