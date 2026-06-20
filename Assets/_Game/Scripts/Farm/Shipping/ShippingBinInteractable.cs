using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Economy;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm.Shipping
{
    /// <summary>
    /// fable_54 — caixa de envio FISICA na Zone_ShippingSellpoint. Interagir deposita os itens
    /// vendaveis atualmente no inventario para envio overnight (canal "deposita hoje, recebe amanha
    /// de manha"). Reusa SellableItemPolicy (sem segundo canal de venda) para decidir o que pode ir,
    /// e o preco do servico orfao (ShippingPriceResolver, canal 0.95) no pagamento da manha seguinte.
    ///
    /// Cada deposito bem-sucedido remove o item do inventario SO apos Deposit.Success (recusa nunca
    /// perde item) e progride a meta diaria de colheita. Comunicacao de gameplay via GameEventBus.
    /// InventoryManager via ref de bootstrap (wiring), nunca FindObjectOfType de gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShippingBinInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _interactionPrompt = "Depositar para envio";

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return ShippingBinRuntimeService.Instance != null && ResolveInventory() != null;
        }

        public void Interact(GameObject interactor)
        {
            var bin = ShippingBinRuntimeService.Instance;
            var inventory = ResolveInventory();
            if (bin == null || inventory == null)
            {
                Debug.LogWarning("[ShippingBin] serviço ou inventário ausente.", this);
                return;
            }

            // Snapshot dos itens vendaveis (mesma politica da venda imediata; nao mexe no que e
            // protegido). Itera sobre copia para mutar o inventario com seguranca.
            var snapshot = new List<KeyValuePair<string, int>>(inventory.Items);
            var depositedItems = 0;
            var depositedStacks = 0;

            foreach (var entry in snapshot)
            {
                var itemId = entry.Key;
                var amount = entry.Value;
                if (amount <= 0 || !SellableItemPolicy.IsSellable(itemId)) continue;
                if (!inventory.TryGetItemData(itemId, out var itemData) || itemData == null) continue;
                if (itemData.BaseValue <= 0) continue;

                var result = bin.Deposit(itemId, amount, 0, itemData.BaseValue);
                if (!result.Success) continue;

                // Remove do inventario SO apos Deposit.Success.
                if (!inventory.RemoveItem(itemId, amount))
                {
                    // Falha rara: nao foi possivel remover; deixa a entrada pendente cancelada para
                    // nao pagar por item nao entregue.
                    if (result.CreatedEntry != null)
                    {
                        result.CreatedEntry.State = ShippingEntryState.Cancelled;
                    }
                    continue;
                }

                depositedItems += amount;
                depositedStacks++;

                // Meta diaria de colheita: cada deposito de colheita conta (WI-24).
                var goals = Farm.Runtime.FarmDailyGoalService.Instance;
                goals?.ProgressHarvestGoal(1);
            }

            if (depositedStacks > 0)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(
                    $"Depositado para envio: {depositedItems} itens. Pagamento amanhã de manhã.", 3f));
            }
            else
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Nada para depositar.", 2f));
            }
        }

        private InventoryManager ResolveInventory()
        {
            var bootstrap = Core.Bootstrap.GameBootstrap.Instance;
            return bootstrap != null ? bootstrap.InventoryManager : null;
        }
    }
}
