namespace CindarsHope.Save
{
    /// <summary>
    /// Port puro para persistir o estado de quests sem acoplar Save ao módulo Quests.
    /// Vive em CindarsHope.Save porque <see cref="QuestStateSectionSaveData"/> já é um tipo
    /// canônico deste módulo (SaveData.cs) — não precisa de Foundation nem de nenhum tipo
    /// definido em Quests. Implementado pelo runtime de quest (Quests) e resolvido via
    /// DomainManagerRegistry, seguindo o mesmo precedente de IOnboardingHintsRuntime (Save|UI).
    /// </summary>
    public interface IQuestSaveRuntime
    {
        QuestStateSectionSaveData CaptureSaveData();
        void RestoreFromSaveData(QuestStateSectionSaveData saveData);
    }
}
