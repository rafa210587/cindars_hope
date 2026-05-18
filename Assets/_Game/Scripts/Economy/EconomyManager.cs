using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class EconomyManager : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private PlayerManager _playerManager;

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

            if (_playerManager.CurrentGold < evt.TotalCost)
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, $"Ouro insuficiente para comprar '{evt.ItemId}' x{evt.Amount}.");
                return;
            }

            if (evt.TotalCost > 0 && !_playerManager.TrySpendGold(evt.TotalCost))
            {
                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, $"Compra falhou ao gastar {evt.TotalCost}g.");
                return;
            }

            if (!_inventoryManager.AddItem(evt.ItemId, evt.Amount))
            {
                if (evt.TotalCost > 0)
                {
                    _playerManager.AddGold(evt.TotalCost);
                }

                PublishTransaction(false, "Purchase", evt.ItemId, evt.Amount, 0, $"Compra falhou: nao foi possivel adicionar '{evt.ItemId}' x{evt.Amount}. Ouro reembolsado.");
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
