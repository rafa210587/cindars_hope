using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// IInteractable component that connects an NPC to the quest giver flow.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Attach to npc_thalindra or any quest-giving NPC GameObject.
    /// Wire npcId + offeredQuestIds in the Inspector.
    ///
    /// Publishes QuestGiverInteractedEvent; does NOT accept quests directly.
    /// QuestOfferPanelController (UI side) listens and calls QuestService.AcceptQuest.
    ///
    /// Does NOT use GameObject.Find/FindObjectOfType.
    /// </summary>
    [DisallowMultipleComponent]
    public class QuestGiverInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _npcId = "";
        [SerializeField] private string[] _offeredQuestIds = System.Array.Empty<string>();
        [SerializeField] private string _noQuestPrompt = "Não há nada para você agora.";
        [SerializeField] private string _offerPrompt = "Interagir com";
        [SerializeField] private string _turnInPrompt = "Entregar quest para";

        public string InteractionPrompt
        {
            get
            {
                var mode = DetermineMode();
                return mode switch
                {
                    QuestGiverInteractionMode.Offer => $"{_offerPrompt} {_npcId}",
                    QuestGiverInteractionMode.TurnIn => $"{_turnInPrompt} {_npcId}",
                    _ => _noQuestPrompt
                };
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrEmpty(_npcId);
        }

        public void Interact(GameObject interactor)
        {
            if (string.IsNullOrEmpty(_npcId)) return;

            var mode = DetermineMode();
            var questId = FindRelevantQuestId(mode);

            GameEventBus.Publish(new QuestGiverInteractedEvent(_npcId, questId ?? "", mode));
        }

        private QuestGiverInteractionMode DetermineMode()
        {
            if (_offeredQuestIds == null || _offeredQuestIds.Length == 0)
                return QuestGiverInteractionMode.NoQuest;

            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null)
                return QuestGiverInteractionMode.NoQuest;

            // Priority 1: ReadyToComplete quest
            foreach (var questId in _offeredQuestIds)
            {
                if (service.CanTurnIn(questId))
                    return QuestGiverInteractionMode.TurnIn;
            }

            // Priority 2: Offer quest not yet accepted
            foreach (var questId in _offeredQuestIds)
            {
                var state = service.GetQuestState(questId);
                if (state == null)
                    return QuestGiverInteractionMode.Offer;
            }

            return QuestGiverInteractionMode.NoQuest;
        }

        private string FindRelevantQuestId(QuestGiverInteractionMode mode)
        {
            if (_offeredQuestIds == null || _offeredQuestIds.Length == 0) return null;
            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null) return _offeredQuestIds.Length > 0 ? _offeredQuestIds[0] : null;

            if (mode == QuestGiverInteractionMode.TurnIn)
            {
                foreach (var questId in _offeredQuestIds)
                    if (service.CanTurnIn(questId)) return questId;
            }
            else if (mode == QuestGiverInteractionMode.Offer)
            {
                foreach (var questId in _offeredQuestIds)
                {
                    var state = service.GetQuestState(questId);
                    if (state == null) return questId;
                }
            }

            return _offeredQuestIds.Length > 0 ? _offeredQuestIds[0] : null;
        }
    }
}
