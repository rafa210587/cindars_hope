using CindarsHope.Foundation;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider canônico de save do estado das quests. Fonte do estado: o runtime de quest
    /// registrado via <see cref="IQuestSaveRuntime"/> no <see cref="DomainManagerRegistry"/>.
    /// Fallback: seção existente do save ou seção vazia quando nenhum runtime está registrado
    /// (QuestService ainda não inicializado).
    ///
    /// arch: quebra do ciclo Quests|Save — relocado de CindarsHope.Quests.Save para
    /// CindarsHope.Save.Providers; Save deixa de nomear qualquer tipo de Quests (SaveManager.cs
    /// instancia esta classe em vez de CindarsHope.Quests.Save.QuestSectionProvider), mesmo
    /// precedente de OnboardingHintsSectionProvider (Save|UI).
    /// </summary>
    public sealed class QuestSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "quests";

        public object Capture(GameSaveData existingSaveData)
        {
            var runtime = DomainManagerRegistry.Get<IQuestSaveRuntime>();
            var liveSection = runtime?.CaptureSaveData();
            if (liveSection != null)
            {
                return liveSection;
            }

            // Runtime não inicializado: preserva o que já estava no save.
            return existingSaveData?.Quests ?? new QuestStateSectionSaveData();
        }

        public void Restore(object sectionData)
        {
            var runtime = DomainManagerRegistry.Get<IQuestSaveRuntime>();
            // sectionData null (save legado) = nenhuma quest em andamento.
            runtime?.RestoreFromSaveData(sectionData as QuestStateSectionSaveData);
        }
    }
}
