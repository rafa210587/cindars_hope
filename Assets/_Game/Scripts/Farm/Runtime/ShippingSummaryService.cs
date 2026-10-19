using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Exibe resumo de vendas no HUD via PlayerActionFeedbackEvent.
    /// Escuta EconomyTransactionCompletedEvent e publica feedback legível.
    /// Sem chamadas diretas — usa GameEventBus exclusivamente.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShippingSummaryService : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransaction);
        }

        private void OnEconomyTransaction(EconomyTransactionCompletedEvent evt)
        {
            if (!evt.WasSuccessful)
            {
                return;
            }

            if (evt.GoldDelta > 0)
            {
                var itemDisplay = string.IsNullOrWhiteSpace(evt.ItemId) ? "item" : evt.ItemId;
                var amountDisplay = evt.Amount > 1 ? $" x{evt.Amount}" : string.Empty;
                var msg = $"Vendido: {itemDisplay}{amountDisplay} por {evt.GoldDelta}g";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(msg, 3f));
            }
        }
    }
}
