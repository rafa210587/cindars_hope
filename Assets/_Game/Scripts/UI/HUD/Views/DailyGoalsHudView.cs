using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Farm.Runtime;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    /// <summary>
    /// fable_65 — widget compacto read-only das metas diárias no GameplayHudCanvas.
    /// Segue o padrão WI-23 (QuestTrackerHudView como referência): headless (Text/labels
    /// ligados por humano no Unity Editor), sem input próprio, respeita HudVisibilityController.
    ///
    /// Fonte de dados: snapshot FarmDailyGoalService.GetCurrentGoals() projetado por
    /// DailyGoalsHudProjection. Atualiza por DailyGoalProgressed/Completed + DayStarted.
    /// Zero estado de gameplay próprio. Unsubscribe pareado.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DailyGoalsHudView : MonoBehaviour
    {
        private readonly DailyGoalsHudProjection _projection = new DailyGoalsHudProjection();

        /// <summary>Projeção atual (read-only) — exposta para a camada visual e para testes.</summary>
        public DailyGoalsHudProjection Projection => _projection;

        private void OnEnable()
        {
            GameEventBus.Subscribe<DailyGoalProgressedEvent>(OnGoalProgressed);
            GameEventBus.Subscribe<DailyGoalCompletedEvent>(OnGoalCompleted);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);
            RebuildFromService();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DailyGoalProgressedEvent>(OnGoalProgressed);
            GameEventBus.Unsubscribe<DailyGoalCompletedEvent>(OnGoalCompleted);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);
        }

        private void OnGoalProgressed(DailyGoalProgressedEvent evt) => RebuildFromService();
        private void OnGoalCompleted(DailyGoalCompletedEvent evt) => RebuildFromService();
        private void OnDayStarted(DayStartedEvent evt) => RebuildFromService();

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);

        private void RebuildFromService()
        {
            var service = FarmDailyGoalService.Instance;
            if (service == null)
            {
                _projection.Rebuild(null);
                return;
            }

            _projection.Rebuild(service.GetCurrentGoals());
            Refresh();
        }

        private void Refresh()
        {
            // Headless: labels (rótulo + check + n/m por linha) ligados por humano no Unity Editor.
            // _projection.Rows alimenta a camada visual; _projection.CompletedCount/TotalCount o cabeçalho.
        }
    }
}
