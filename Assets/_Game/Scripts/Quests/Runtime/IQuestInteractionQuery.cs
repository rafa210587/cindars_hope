namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Contrato minimo de consulta de quest usado pela resolucao de modo de interacao de NPC
    /// (Offer/TurnIn/NoQuest). Expoe apenas o subset real consumido — sem vazar QuestStateRecord —
    /// para que a decisao seja testavel sem o QuestService concreto (spec_arch_npc_quest_boundary_residual_v1,
    /// criterios 14.1/14.2).
    /// </summary>
    public interface IQuestInteractionQuery
    {
        bool CanTurnIn(string questId);

        /// <summary>True quando ja existe estado registrado para a quest (equivalente a GetQuestState(id) != null).</summary>
        bool HasQuestState(string questId);
    }
}
