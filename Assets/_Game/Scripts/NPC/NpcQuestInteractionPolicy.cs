using CindarsHope.Core.Events;
using CindarsHope.Quests.Runtime;

namespace CindarsHope.NPC
{
    /// <summary>
    /// Decisao pura do modo de interacao de quest de um NPC (Offer/TurnIn/NoQuest). Extraida de
    /// NpcController para ser testavel sem cena/MonoBehaviour (spec_arch_npc_quest_boundary_residual_v1 14.1/14.2).
    /// Comportamento identico ao anterior, incluindo o fallback Offer quando nao ha servico de quest.
    /// </summary>
    public static class NpcQuestInteractionPolicy
    {
        public static QuestGiverInteractionMode ResolveMode(string questId, IQuestInteractionQuery query)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return QuestGiverInteractionMode.NoQuest;
            }

            if (query == null)
            {
                return QuestGiverInteractionMode.Offer;
            }

            if (query.CanTurnIn(questId))
            {
                return QuestGiverInteractionMode.TurnIn;
            }

            return !query.HasQuestState(questId)
                ? QuestGiverInteractionMode.Offer
                : QuestGiverInteractionMode.NoQuest;
        }
    }
}
