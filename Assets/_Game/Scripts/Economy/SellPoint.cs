using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Economy.Transactions;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public class SellPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private PlayerManager _playerManager;

        public string InteractionPrompt => "Vender";

        public bool CanInteract(GameObject interactor)
        {
            return _inventoryManager != null && _playerManager != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning("SellPoint cannot sell because InventoryManager or PlayerManager is missing.", this);
                return;
            }

            var balanceConfig = Resources.Load<EconomyBalanceConfigSO>("EconomyBalanceConfig");
            var snapshot = new List<KeyValuePair<string, int>>(_inventoryManager.Items);
            var amounts = new Dictionary<string, int>();
            var unitPrices = new Dictionary<string, int>();
            long normalGold = 0;
            long retainedAmount = 0;

            foreach (var entry in snapshot)
            {
                var itemId = entry.Key;
                var amount = _inventoryManager.GetEligibleAmount(itemId);
                retainedAmount += entry.Value - amount;
                if (amount <= 0 || !SellableItemPolicy.IsSellable(itemId))
                {
                    continue;
                }

                if (!_inventoryManager.TryGetItemData(itemId, out var itemData))
                {
                    Debug.LogWarning($"SellPoint skipped unknown item id '{itemId}'.", this);
                    continue;
                }

                var multiplier = balanceConfig != null
                    ? balanceConfig.ShippingBuybackMultiplier : 1f;
                var rawUnitPrice = (double)itemData.BaseValue * multiplier;
                if (double.IsNaN(rawUnitPrice) || double.IsInfinity(rawUnitPrice)
                    || rawUnitPrice < 0 || rawUnitPrice > int.MaxValue)
                {
                    Debug.LogWarning($"SellPoint quote overflow for '{itemId}' x{amount}.", this);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Venda falhou: valor invalido.", 2f));
                    return;
                }
                var unitPrice = ItemPriceResolver.ResolveSellingPrice(
                    itemData.BaseValue, SellContext.Shipping, balanceConfig);
                var itemValue = (long)unitPrice * amount;
                if (itemValue <= 0)
                {
                    Debug.Log($"SellPoint skipped '{itemId}' x{amount} because it has no sale value.", this);
                    continue;
                }

                if (itemValue > int.MaxValue || normalGold + itemValue > int.MaxValue)
                {
                    Debug.LogWarning($"SellPoint quote overflow for '{itemId}' x{amount}.", this);
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Venda falhou: valor invalido.", 2f));
                    return;
                }

                amounts[itemId] = amount;
                unitPrices[itemId] = unitPrice;
                normalGold += itemValue;
            }

            if (amounts.Count == 0)
            {
                Debug.Log("SellPoint found no sellable items.", this);
                GameEventBus.Publish(new PlayerActionFeedbackEvent(
                    retainedAmount > 0 ? $"Nada para vender. {retainedAmount} unidade(s) retida(s)."
                        : "Nada para vender.", 2f));
                return;
            }

            if (!_inventoryManager.TryPrepareBatchRemoval(amounts, out var batch)
                || !AtomicOriginSaleTransaction.TryQuoteBatch(batch, unitPrices,
                    (int)normalGold, out var totalGold, out var limitedUnits)
                || !AtomicOriginSaleTransaction.TryCommitBatch(_inventoryManager,
                    _playerManager, batch, totalGold))
            {
                Debug.LogWarning("SellPoint sale failed before commit.", this);
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Venda falhou.", 2f));
                return;
            }

            Debug.Log($"SellPoint sale complete. Earned {totalGold} gold.", this);
            var feedbackMsg = $"Vendido! +{totalGold} ouro."
                + (limitedUnits > 0 ? $" {limitedUnits} unidade(s) com limite de compra." : string.Empty)
                + (retainedAmount > 0 ? $" {retainedAmount} unidade(s) retida(s)." : string.Empty);
            GameEventBus.Publish(new PlayerActionFeedbackEvent(feedbackMsg, 3f));
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "SellPoint", "all", 0, totalGold, feedbackMsg));
        }

        // arch: quebra do par mutuo Player|SceneManagement (2026-07-15) — playerManagerRef agora e
        // MonoBehaviour; cast para o tipo concreto aqui dentro.
        public void RebindRuntimeManagers(InventoryManager inventoryManager, MonoBehaviour playerManagerRef)
        {
            var playerManager = playerManagerRef as PlayerManager;

            if (inventoryManager == null)
            {
                Debug.LogWarning("SellPoint received null InventoryManager for rebind.", this);
            }
            else
            {
                _inventoryManager = inventoryManager;
            }

            if (playerManager == null)
            {
                Debug.LogWarning("SellPoint received null PlayerManager for rebind.", this);
            }
            else
            {
                _playerManager = playerManager;
            }
        }
    }
}
