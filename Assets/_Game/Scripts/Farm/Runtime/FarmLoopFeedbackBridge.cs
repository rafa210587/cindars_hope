using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Bridge entre eventos de farm e o sistema de feedback HUD.
    /// Escuta CropHarvestedEvent e DailyGoalProgressedEvent e publica PlayerActionFeedbackEvent.
    /// Sem chamadas diretas — usa GameEventBus exclusivamente.
    ///
    /// fable_65: o toast de CONCLUSÃO de meta deixou de ser emitido aqui — passou a ser o toast
    /// rico de recompensa ("Meta concluída: +X ouro, +Y XP") emitido por FarmDailyGoalService no
    /// pagamento. Evita toast duplicado no mesmo DailyGoalCompletedEvent.
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmLoopFeedbackBridge : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEventBus.Subscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Subscribe<DailyGoalProgressedEvent>(OnDailyGoalProgressed);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Unsubscribe<DailyGoalProgressedEvent>(OnDailyGoalProgressed);
        }

        private void OnCropHarvested(CropHarvestedEvent evt)
        {
            var msg = evt.Amount > 1
                ? $"Colheita: {evt.ItemId} x{evt.Amount} adicionado ao inventário."
                : $"Colheita: {evt.ItemId} adicionado ao inventário.";
            GameEventBus.Publish(new PlayerActionFeedbackEvent(msg, 2.5f));
        }

        private void OnDailyGoalProgressed(DailyGoalProgressedEvent evt)
        {
            if (evt.Current < evt.Required)
            {
                var msg = $"Meta [{evt.GoalId}]: {evt.Current}/{evt.Required}";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(msg, 2f));
            }
        }
    }
}
