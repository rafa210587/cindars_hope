using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount = 1;
        [SerializeField] private InventoryManager _inventoryManager;

        public string InteractionPrompt => "Pegar";

        public bool CanInteract(GameObject interactor)
        {
            return _inventoryManager != null
                && !string.IsNullOrWhiteSpace(_itemId)
                && _amount > 0;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(ItemPickup)} on '{name}' cannot be picked up because it is not configured.", this);
                return;
            }

            if (!_inventoryManager.AddItem(_itemId, _amount))
            {
                Debug.LogWarning($"{nameof(ItemPickup)} could not add '{_itemId}' x{_amount} to inventory.", this);
                return;
            }

            GameEventBus.Publish(new ItemPickedUpEvent(_itemId, _amount));
            Debug.Log($"Picked up '{_itemId}' x{_amount}.", this);
            Destroy(gameObject);
        }

        private void OnValidate()
        {
            _amount = Mathf.Max(1, _amount);
        }
    }
}
