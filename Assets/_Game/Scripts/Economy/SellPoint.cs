using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
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
            var totalGold = 0;

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
                    Debug.LogWarning($"SellPoint skipped unknown item id '{itemId}'.", this);
                    continue;
                }

                var unitPrice = ItemPriceResolver.ResolveSellingPrice(
                    itemData.BaseValue, SellContext.Shipping, balanceConfig);
                var itemValue = unitPrice * amount;
                if (itemValue <= 0)
                {
                    Debug.Log($"SellPoint skipped '{itemId}' x{amount} because it has no sale value.", this);
                    continue;
                }

                if (!_inventoryManager.RemoveItem(itemId, amount))
                {
                    Debug.LogWarning($"SellPoint could not remove '{itemId}' x{amount} from inventory.", this);
                    continue;
                }

                totalGold += itemValue;
                Debug.Log($"Sold '{itemId}' x{amount} @ {unitPrice}/unit for {itemValue} gold.", this);
            }

            if (totalGold <= 0)
            {
                Debug.Log("SellPoint found no sellable items.", this);
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Nada para vender.", 2f));
                return;
            }

            _playerManager.AddGold(totalGold);
            Debug.Log($"SellPoint sale complete. Earned {totalGold} gold.", this);
            var feedbackMsg = $"Vendido! +{totalGold} ouro.";
            GameEventBus.Publish(new PlayerActionFeedbackEvent(feedbackMsg, 3f));
            GameEventBus.Publish(new EconomyTransactionCompletedEvent(true, "SellPoint", "all", 0, totalGold, feedbackMsg));
        }

        public void RebindRuntimeManagers(InventoryManager inventoryManager, PlayerManager playerManager)
        {
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
