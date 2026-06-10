using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// IInteractable component for quest boards (fallback giver if no NPC available).
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Attach to an empty GameObject in TownScene as QuestBoard_FirstQuest_01.
    /// Wire boardId + postedQuestIds in the Inspector.
    ///
    /// Same pattern as QuestGiverInteractable but using boardId.
    /// Publishes QuestGiverInteractedEvent with boardId as npcId.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestBoardInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _boardId = "board_first_quest_01";
        [SerializeField] private string[] _postedQuestIds = System.Array.Empty<string>();
        [SerializeField] private string _interactionPrompt = "Ver missões disponíveis";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrEmpty(_boardId);
        }

        public void Interact(GameObject interactor)
        {
            if (string.IsNullOrEmpty(_boardId)) return;
            if (_postedQuestIds == null || _postedQuestIds.Length == 0) return;

            var service = QuestRuntimeBootstrap.QuestService;
            string questId = _postedQuestIds[0];

            var mode = QuestGiverInteractionMode.NoQuest;
            if (service != null)
            {
                foreach (var id in _postedQuestIds)
                {
                    if (service.CanTurnIn(id))
                    {
                        questId = id;
                        mode = QuestGiverInteractionMode.TurnIn;
                        break;
                    }
                }
                if (mode == QuestGiverInteractionMode.NoQuest)
                {
                    foreach (var id in _postedQuestIds)
                    {
                        var state = service.GetQuestState(id);
                        if (state == null)
                        {
                            questId = id;
                            mode = QuestGiverInteractionMode.Offer;
                            break;
                        }
                    }
                }
            }
            else
            {
                mode = QuestGiverInteractionMode.Offer;
            }

            GameEventBus.Publish(new QuestGiverInteractedEvent(_boardId, questId, mode));
        }
    }
}
