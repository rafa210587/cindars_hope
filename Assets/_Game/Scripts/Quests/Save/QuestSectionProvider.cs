using CindarsHope.Quests.Runtime;
using CindarsHope.Save;

namespace CindarsHope.Quests.Save
{
    /// <summary>
    /// Provider de save do estado das quests. Fonte do estado: <see cref="QuestRuntimeBootstrap"/>
    /// (static — acesso direto sem singleton). Fallback: seção existente do save ou seção vazia quando
    /// o QuestService ainda não foi inicializado.
    ///
    /// arch: quebra do ciclo Quests|Save (spec_arch_quests_save_cycle_reduction_v24) — relocado de
    /// CindarsHope.Save.Providers para CindarsHope.Quests.Save; Save deixa de nomear qualquer tipo
    /// de Quests (SaveManager.cs referencia esta classe por nome totalmente qualificado, sem novo
    /// `using`, mesmo precedente de spec_arch_npc_save_cycle_reduction_v23).
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
