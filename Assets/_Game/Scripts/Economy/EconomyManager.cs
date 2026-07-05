using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Economy.Transactions;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class EconomyManager : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private PlayerManager _playerManager;

        // fable_19 (CA-2): ponto de venda URBANO da cidade. Apenas este source exige a licença de
        // barraca (license_market_stall). Cave merchant (shop_cave_wandering_merchant_l*) e venda
        // direta a NPC NÃO passam por este gate; o shipping da fazenda usa outro fluxo.
        private const string UrbanSellSourceId = "shop_town_sell_box";

        public bool IsInitialized { get; private set; }

        private void OnEnable()
        {
            GameEventBus.Subscribe<ItemPurchaseRequestedEvent>(HandleItemPurchaseRequested);
            GameEventBus.Subscribe<SellAllRequestedEvent>(HandleSellAllRequested);
            Initialize();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ItemPurchaseRequestedEvent>(HandleItemPurchaseRequested);
            GameEventBus.Unsubscribe<SellAllRequestedEvent>(HandleSellAllRequested);
            Shutdown();
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }

        private void HandleItemPurchaseRequested(ItemPurchaseRequestedEvent evt)
        {
            if (!HasRequiredManagers("compra", out var failureMessage))
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, failureMessage);
                return;
            }

            if (string.IsNullOrWhiteSpace(evt.ItemId))
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, "Compra falhou: item vazio.");
                return;
            }

            if (evt.Amount <= 0)
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, $"Compra falhou: quantidade invalida para '{evt.ItemId}'.");
                return;
            }

            if (evt.TotalCost < 0)
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, $"Compra falhou: custo invalido para '{evt.ItemId}'.");
                return;
            }

            PurchaseTransactionResult transaction = AtomicPurchaseTransaction.Execute(
                _inventoryManager,
                _playerManager,
                evt.ItemId,
                evt.Amount,
                evt.TotalCost);
            if (!transaction.Success)
            {
                string failure = transaction.Failure switch
                {
                    PurchaseTransactionFailure.InsufficientFunds =>
                        $"Ouro insuficiente para comprar '{evt.ItemId}' x{evt.Amount}.",
                    PurchaseTransactionFailure.InventoryFull =>
                        $"Compra falhou: inventario sem espaco para '{evt.ItemId}' x{evt.Amount}.",
                    PurchaseTransactionFailure.DebitFailed =>
                        $"Compra falhou ao gastar {evt.TotalCost}g.",
                    PurchaseTransactionFailure.InventoryWriteFailed =>
                        $"Compra falhou: nao foi possivel adicionar '{evt.ItemId}' x{evt.Amount}. Ouro reembolsado.",
                    _ => $"Compra falhou: transacao invalida para '{evt.ItemId}' x{evt.Amount}."
                };
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, failure);
                return;
            }

            PublishTransaction(true, "Purchase", evt.ItemId, evt.Amount, -evt.TotalCost, $"Compra concluida: '{evt.ItemId}' x{evt.Amount} por {evt.TotalCost}g.");
        }

        private void HandleSellAllRequested(SellAllRequestedEvent evt)
        {
            if (!HasRequiredManagers("venda", out var failureMessage))
            {
                PublishTransaction(false, "SellAll", string.Empty, 0, 0, failureMessage);
                return;
            }

            // fable_19 (CA-2): o ponto de venda URBANO recusa sem a licença de barraca. Ponto único
            // de gate (sem if espalhado): consulta a fachada CityServiceAccess (fail-closed). Venda a
            // NPC e ao mercador errante da caverna seguem livres (source diferente).
            if (string.Equals(evt.SourceId, UrbanSellSourceId, System.StringComparison.Ordinal)
                && !CindarsHope.City.Services.CityServiceAccess.UrbanSellAllowed())
            {
                PublishTransaction(false, "SellAll", string.Empty, 0, 0,
                    "Voce precisa da Licenca de Barraca de Mercado (Tovin, 200g) para vender neste ponto.");
                return;
            }

            var snapshot = new List<KeyValuePair<string, int>>(_inventoryManager.Items);
            var totalGold = 0;
            var totalAmount = 0;

            foreach (var entry in snapshot)
            {
                var itemId = entry.Key;
                var amount = entry.Value;
                if (amount <= 0 || !SellableItemPolicy.IsSellable(itemId))
                {
                    continue;
                }

                if (!_inventoryManager.TryGetItemData(itemId, out var itemData))
                {
                    Debug.LogWarning($"{nameof(EconomyManager)} skipped unknown sellable item id '{itemId}'.", this);
                    continue;
                }

                var itemValue = itemData.BaseValue * amount;
                if (itemValue <= 0)
                {
                    Debug.Log($"{nameof(EconomyManager)} skipped '{itemId}' x{amount} because it has no sale value.", this);
                    continue;
                }

                if (!_inventoryManager.RemoveItem(itemId, amount))
                {
                    Debug.LogWarning($"{nameof(EconomyManager)} could not remove '{itemId}' x{amount} from inventory.", this);
                    continue;
                }

                totalGold += itemValue;
                totalAmount += amount;
            }

            if (totalGold <= 0)
            {
                PublishTransaction(false, "SellAll", string.Empty, 0, 0, "Venda concluida: nenhum item vendavel encontrado.");
                return;
            }

            // fable_23 (CA-2): ponto ÚNICO nomeado do bônus de ouro em vendas de acessório (Anel de
            // Finan +5%). Consulta síncrona ao AccessoryEffectRouter; sem acessório => neutro (sem if
            // espalhado). Não-stack já garantido no roteador (2 anéis iguais != +10%).
            totalGold = CindarsHope.Equipment.AccessoryEffectRouter.ApplyGoldGain(totalGold);

            // fable_37 (CA-2 economia): ponto ÚNICO nomeado do multiplicador de venda por evento de mundo
            // (pico de Lua Âmbar +10% e/ou cultivo em alta +25%). Lê WorldEventHooks (resolução do dia);
            // sem evento ativo => multiplicador neutro 1.0. O EconomyManager não conhece o WorldEventService.
            totalGold = CindarsHope.World.Events.WorldEventHooks.ApplySellGold(totalGold);

            _playerManager.AddGold(totalGold);
            PublishTransaction(true, "SellAll", string.Empty, totalAmount, totalGold, $"Venda concluida: {totalAmount} itens por {totalGold}g.");
        }

        private bool HasRequiredManagers(string operationName, out string failureMessage)
        {
            if (_inventoryManager == null || _playerManager == null)
            {
                failureMessage = $"Economia falhou na {operationName}: InventoryManager ou PlayerManager ausente.";
                return false;
            }

            failureMessage = string.Empty;
            return true;
        }

        private void PublishTransaction(bool wasSuccessful, string transactionType, string itemId, int amount, int goldDelta, string message)
        {
            if (wasSuccessful)
            {
                Debug.Log(message, this);
            }
            else
            {
                Debug.LogWarning(message, this);
            }

            GameEventBus.Publish(new EconomyTransactionCompletedEvent(wasSuccessful, transactionType, itemId, amount, goldDelta, message));
        }
    }
}
