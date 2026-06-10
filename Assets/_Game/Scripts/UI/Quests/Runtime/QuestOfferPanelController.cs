using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Quests;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Runtime;
using UnityEngine;

namespace CindarsHope.UI.Quests.Runtime
{
    /// <summary>
    /// Headless IMGUI controller for the quest offer panel.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Opens when QuestGiverInteractedEvent is received with Mode=Offer.
    /// Shows quest title, description, objectives preview, reward preview.
    /// [Accept] calls QuestService.AcceptQuest.
    /// [Decline] closes the panel and returns focus.
    ///
    /// Canvas wiring: HUMAN_UNITY_ACTION_REQUIRED
    /// Until Canvas is wired this controller uses IMGUI fallback.
    /// </summary>
    public class QuestOfferPanelController : MonoBehaviour
    {
        /// <summary>Static instance to avoid FindObjectOfType at runtime.</summary>
        public static QuestOfferPanelController Instance { get; private set; }

        private bool _isOpen;
        private string _pendingQuestId;
        private string _giverNpcId;
        private QuestGiverInteractionMode _mode;

        // IMGUI display data (populated from QuestService/QuestRegistry)
        private string _questTitle = "";
        private string _questDescription = "";
        private string _objectiveSummary = "";
        private string _rewardSummary = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<QuestGiverInteractedEvent>(OnQuestGiverInteracted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<QuestGiverInteractedEvent>(OnQuestGiverInteracted);
        }

        private void OnQuestGiverInteracted(QuestGiverInteractedEvent evt)
        {
            _giverNpcId = evt.NpcId;
            _pendingQuestId = evt.QuestId;
            _mode = evt.Mode;

            if (evt.Mode == QuestGiverInteractionMode.Offer)
            {
                PopulateFromQuest(evt.QuestId);
                _isOpen = true;
            }
            else if (evt.Mode == QuestGiverInteractionMode.TurnIn)
            {
                HandleTurnIn(evt.QuestId);
            }
            else
            {
                Debug.Log($"[QuestOfferPanelController] NPC '{evt.NpcId}' has no quest available.");
            }
        }

        private void PopulateFromQuest(string questId)
        {
            var registry = QuestRuntimeBootstrap.QuestRegistry;
            if (registry == null || string.IsNullOrEmpty(questId))
            {
                _questTitle = "Quest não encontrada";
                _questDescription = "";
                _objectiveSummary = "";
                _rewardSummary = "";
                return;
            }

            if (!registry.TryGetQuest(questId, out var def))
            {
                _questTitle = $"Quest '{questId}' não registrada";
                return;
            }

            _questTitle = def.DisplayName ?? def.QuestId;
            _questDescription = def.Description ?? "";

            var objLines = new System.Text.StringBuilder();
            var questObjectives = registry.GetObjectives(questId);
            foreach (var obj in questObjectives)
                objLines.AppendLine($"• {obj.TargetId} (x{obj.RequiredAmount})");
            _objectiveSummary = objLines.ToString();

            var rewards = registry.GetRewards(questId);
            var rewardLines = new System.Text.StringBuilder();
            foreach (var r in rewards)
            {
                if (r.RewardType == QuestRewardType.Gold)
                    rewardLines.AppendLine($"Ouro: {r.Quantity}");
                else if (r.RewardType == QuestRewardType.Item)
                    rewardLines.AppendLine($"Item: {r.TargetId} x{r.Quantity}");
                else if (r.RewardType == QuestRewardType.QuestFlagGrant)
                    rewardLines.AppendLine($"Flag: {r.GrantedFlagId}");
            }
            _rewardSummary = rewardLines.ToString();
        }

        private void HandleTurnIn(string questId)
        {
            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null)
            {
                Debug.LogWarning("[QuestOfferPanelController] QuestService null — cannot turn in.");
                return;
            }

            var result = service.TurnIn(questId);
            if (result.Succeeded && !result.WasAlreadyCompleted)
                Debug.Log($"[QuestOfferPanelController] Turn-in SUCCESS: {questId} +{result.GoldGiven}g");
            else if (result.WasAlreadyCompleted)
                Debug.Log($"[QuestOfferPanelController] Quest already completed: {questId}");
            else
                Debug.LogWarning($"[QuestOfferPanelController] Turn-in FAILED: {result.FailureReason}");
        }

        private void AcceptQuest()
        {
            if (string.IsNullOrEmpty(_pendingQuestId)) return;

            var service = QuestRuntimeBootstrap.QuestService;
            if (service == null)
            {
                Debug.LogWarning("[QuestOfferPanelController] QuestService null — cannot accept.");
                return;
            }

            bool accepted = service.AcceptQuest(_pendingQuestId);
            if (accepted)
                Debug.Log($"[QuestOfferPanelController] Quest accepted: {_pendingQuestId}");

            _isOpen = false;
        }

        private void DeclineQuest()
        {
            _isOpen = false;
        }

        // IMGUI fallback until Canvas is wired by human
        private void OnGUI()
        {
            if (!_isOpen) return;

            var rect = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 220, 400, 440);
            GUI.Box(rect, "QUEST OFFER");

            float y = rect.y + 30;
            GUI.Label(new Rect(rect.x + 10, y, 380, 30), _questTitle);
            y += 35;
            GUI.Label(new Rect(rect.x + 10, y, 380, 60), _questDescription);
            y += 70;
            GUI.Label(new Rect(rect.x + 10, y, 380, 20), "Objetivos:");
            y += 22;
            GUI.Label(new Rect(rect.x + 10, y, 380, 80), _objectiveSummary);
            y += 90;
            GUI.Label(new Rect(rect.x + 10, y, 380, 20), "Recompensas:");
            y += 22;
            GUI.Label(new Rect(rect.x + 10, y, 380, 60), _rewardSummary);
            y += 70;

            if (GUI.Button(new Rect(rect.x + 10, y, 180, 30), "Aceitar"))
                AcceptQuest();
            if (GUI.Button(new Rect(rect.x + 210, y, 180, 30), "Recusar"))
                DeclineQuest();
        }
    }
}
