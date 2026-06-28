using CindarsHope.Quests.Runtime;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado das quests. Fonte do estado: <see cref="QuestRuntimeBootstrap"/>
    /// (static — acesso direto sem singleton). Fallback: seção existente do save ou seção vazia quando
    /// o QuestService ainda não foi inicializado.
    /// </summary>
    public class QuestSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "quests";

        public object Capture(GameSaveData existingSaveData)
        {
            var liveSection = QuestRuntimeBootstrap.CaptureSaveData();
            if (liveSection != null)
            {
                return liveSection;
            }

            // QuestService não inicializado: preserva o que já estava no save.
            return existingSaveData?.Quests ?? new QuestStateSectionSaveData();
        }

        public void Restore(object sectionData)
        {
            // sectionData null (save legado) = nenhuma quest em andamento.
            QuestRuntimeBootstrap.RestoreFromSaveData(sectionData as QuestStateSectionSaveData);
        }
    }
}
