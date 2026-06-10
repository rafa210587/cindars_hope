using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.NPC;
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
        private const string ThalindraNpcId = "npc_thalindra";
        private const string FirstSuppliesQuestId = "quest_first_supplies_for_cindar";

        [SerializeField] private string _npcId = "";
        [SerializeField] private string[] _offeredQuestIds = System.Array.Empty<string>();
        [SerializeField] private string _noQuestPrompt = "Não há nada para você agora.";
        [SerializeField] private string _offerPrompt = "Interagir com";
        [SerializeField] private string _turnInPrompt = "Entregar quest para";

        private static readonly string[] ThalindraDefaultQuestIds = { FirstSuppliesQuestId };

        public string InteractionPrompt
        {
            get
            {
                var npcId = ResolveNpcId();
                var mode = DetermineMode();
                return mode switch
                {
                    QuestGiverInteractionMode.Offer => $"{_offerPrompt} {npcId}",
                    QuestGiverInteractionMode.TurnIn => $"{_turnInPrompt} {npcId}",
                    _ => _noQuestPrompt
                };
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrEmpty(ResolveNpcId());
        }

        public void Interact(GameObject interactor)
        {
            var npcId = ResolveNpcId();
            if (string.IsNullOrEmpty(npcId)) return;

            var mode = DetermineMode();
            var questId = FindRelevantQuestId(mode);

            GameEventBus.Publish(new QuestGiverInteractedEvent(npcId, questId ?? "", mode));
        }

        private string ResolveNpcId()
        {
            if (!string.IsNullOrWhiteSpace(_npcId))
            {
                return _npcId;
            }

            var npcController = GetComponent<NpcController>();
            return npcController != null && npcController.NpcData != null
                ? npcController.NpcData.NpcId
                : string.Empty;
        }

        private string[] ResolveOfferedQuestIds()
        {
            if (_offeredQuestIds != null)
            {
                for (var i = 0; i < _offeredQuestIds.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(_offeredQuestIds[i]))
                    {
                        return _offeredQuestIds;
                    }
                }
            }

            return ResolveNpcId() == ThalindraNpcId
                ? ThalindraDefaultQuestIds
                : System.Array.Empty<string>();
        }

        private QuestGiverInteractionMode DetermineMode()
        {
            var offeredQuestIds = ResolveOfferedQuestIds();
            if (offeredQuestIds == null || offeredQuestIds.Length == 0)
                return QuestGiverInteractionMode.NoQuest;

            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null)
                return QuestGiverInteractionMode.Offer;

            // Priority 1: ReadyToComplete quest
            foreach (var questId in offeredQuestIds)
            {
                if (!string.IsNullOrWhiteSpace(questId) && service.CanTurnIn(questId))
                    return QuestGiverInteractionMode.TurnIn;
            }

            // Priority 2: Offer quest not yet accepted
            foreach (var questId in offeredQuestIds)
            {
                if (string.IsNullOrWhiteSpace(questId))
                    continue;

                var state = service.GetQuestState(questId);
                if (state == null)
                    return QuestGiverInteractionMode.Offer;
            }

            return QuestGiverInteractionMode.NoQuest;
        }

        private string FindRelevantQuestId(QuestGiverInteractionMode mode)
        {
            var offeredQuestIds = ResolveOfferedQuestIds();
            if (offeredQuestIds == null || offeredQuestIds.Length == 0) return null;
            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null) return FirstNonEmptyQuestId(offeredQuestIds);

            if (mode == QuestGiverInteractionMode.TurnIn)
            {
                foreach (var questId in offeredQuestIds)
                    if (!string.IsNullOrWhiteSpace(questId) && service.CanTurnIn(questId)) return questId;
            }
            else if (mode == QuestGiverInteractionMode.Offer)
            {
                foreach (var questId in offeredQuestIds)
                {
                    if (string.IsNullOrWhiteSpace(questId))
                        continue;

                    var state = service.GetQuestState(questId);
                    if (state == null) return questId;
                }
            }

            return FirstNonEmptyQuestId(offeredQuestIds);
        }

        private static string FirstNonEmptyQuestId(string[] questIds)
        {
            if (questIds == null) return null;

            for (var i = 0; i < questIds.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(questIds[i]))
                {
                    return questIds[i];
                }
            }

            return null;
        }
    }
}