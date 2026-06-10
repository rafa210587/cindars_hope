using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Quests;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;
using UnityEngine;

namespace CindarsHope.UI.Quests.Runtime
{
    /// <summary>
    /// IMGUI fallback Quest Log panel controller.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Opened by J key (via QuestLogRuntimeBinder).
    /// Shows active quests with objective progress, completed quests, empty state.
    /// ReadyToComplete quests are highlighted.
    ///
    /// Canvas wiring: HUMAN_UNITY_ACTION_REQUIRED
    /// Until Canvas is wired, IMGUI is the display layer.
    ///
    /// Subscribes to QuestAcceptedEvent, QuestObjectiveProgressedEvent,
    /// QuestCompletedEvent to auto-refresh.
    /// </summary>
    public class QuestLogPanelController : MonoBehaviour
    {
        /// <summary>Static instance to avoid FindObjectOfType at runtime.</summary>
        public static QuestLogPanelController Instance { get; private set; }

        private bool _isOpen;
        private int _scrollY;

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
            GameEventBus.Subscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Subscribe<QuestObjectiveProgressedEvent>(OnObjectiveProgressed);
            GameEventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Subscribe<QuestReadyToCompleteEvent>(OnQuestReadyToComplete);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Unsubscribe<QuestObjectiveProgressedEvent>(OnObjectiveProgressed);
            GameEventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Unsubscribe<QuestReadyToCompleteEvent>(OnQuestReadyToComplete);
        }

        public void Open()
        {
            _isOpen = true;
        }

        public void Close()
        {
            _isOpen = false;
        }

        public bool IsOpen => _isOpen;

        private void OnQuestAccepted(QuestAcceptedEvent evt) { /* panel refreshes on OnGUI */ }
        private void OnObjectiveProgressed(QuestObjectiveProgressedEvent evt) { /* panel refreshes on OnGUI */ }
        private void OnQuestCompleted(QuestCompletedEvent evt) { /* panel refreshes on OnGUI */ }
        private void OnQuestReadyToComplete(QuestReadyToCompleteEvent evt) { /* panel refreshes on OnGUI */ }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
                Close();
        }

        private void OnGUI()
        {
            if (!_isOpen) return;

            var service = QuestRuntimeBootstrap.QuestService;
            var registry = QuestRuntimeBootstrap.QuestRegistry;

            var rect = new Rect(Screen.width / 2f - 250, 60, 500, Screen.height - 120);
            GUI.Box(rect, "DIÁRIO DE MISSÕES (J para fechar)");

            float y = rect.y + 30;

            if (service == null)
            {
                GUI.Label(new Rect(rect.x + 10, y, 480, 30), "Quest system initializing...");
                if (GUI.Button(new Rect(rect.x + 10, y + 35, 100, 25), "Fechar")) Close();
                return;
            }

            var active = service.GetActiveQuests();
            var completed = service.GetCompletedQuests();

            // Header
            GUI.Label(new Rect(rect.x + 10, y, 480, 20), $"=== MISSÕES ATIVAS ({active.Count}) ===");
            y += 25;

            if (active.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 10, y, 480, 20), "(nenhuma missão ativa)");
                y += 25;
            }

            foreach (var record in active)
            {
                if (y > rect.y + rect.height - 80) break;

                var state = (QuestStateStatus)record.State;
                string stateLabel = state == QuestStateStatus.ReadyToComplete ? " [PRONTA PARA ENTREGAR]" : "";
                Color prevColor = GUI.color;
                if (state == QuestStateStatus.ReadyToComplete) GUI.color = Color.yellow;

                string title = record.QuestId;
                if (registry != null && registry.TryGetQuest(record.QuestId, out var def))
                    title = def.DisplayName ?? def.QuestId;

                GUI.Label(new Rect(rect.x + 10, y, 480, 20), $"• {title}{stateLabel}");
                GUI.color = prevColor;
                y += 20;

                foreach (var obj in record.ObjectiveStates)
                {
                    if (y > rect.y + rect.height - 80) break;
                    string checkmark = obj.IsCompleted ? "✓" : "○";
                    string objLabel = obj.ObjectiveId;
                    if (registry != null)
                    {
                        var questObjectives = registry.GetObjectives(record.QuestId);
                        var objDef = questObjectives.FirstOrDefault(o => o.ObjectiveId == obj.ObjectiveId);
                        if (objDef != null) objLabel = $"{objDef.TargetId}";
                    }
                    GUI.Label(new Rect(rect.x + 25, y, 460, 18), $"  {checkmark} {objLabel}: {obj.CurrentProgress}/{obj.RequiredProgress}");
                    y += 18;
                }
                y += 5;
            }

            if (completed.Count > 0)
            {
                GUI.Label(new Rect(rect.x + 10, y, 480, 20), $"=== CONCLUÍDAS ({completed.Count}) ===");
                y += 25;
                foreach (var record in completed)
                {
                    if (y > rect.y + rect.height - 60) break;
                    string title = record.QuestId;
                    if (registry != null && registry.TryGetQuest(record.QuestId, out var def))
                        title = def.DisplayName ?? def.QuestId;
                    GUI.Label(new Rect(rect.x + 10, y, 480, 20), $"✓ {title}");
                    y += 20;
                }
            }

            y = rect.y + rect.height - 35;
            if (GUI.Button(new Rect(rect.x + 10, y, 100, 25), "Fechar"))
                Close();
        }
    }
}
