using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public class SeedShopPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private string _seedId = "seed_wheat";
        [SerializeField] private int _amount = 3;
        [SerializeField] private int _totalCost = 5;

        public string InteractionPrompt => "Comprar sementes";

        public bool CanInteract(GameObject interactor)
        {
            return _inventoryManager != null && _playerManager != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning("SeedShopPoint cannot sell because InventoryManager or PlayerManager is missing.", this);
                return;
            }

            if (_playerManager.CurrentGold < _totalCost)
            {
                Debug.Log($"SeedShopPoint requires {_totalCost} gold to buy '{_seedId}' x{_amount}.", this);
                return;
            }

            if (!_playerManager.TrySpendGold(_totalCost))
            {
                Debug.LogWarning($"SeedShopPoint could not spend {_totalCost} gold.", this);
                return;
            }

            if (!_inventoryManager.AddItem(_seedId, _amount))
            {
                _playerManager.AddGold(_totalCost);
                Debug.LogWarning($"SeedShopPoint could not add '{_seedId}' x{_amount}. Gold was refunded.", this);
                return;
            }

            Debug.Log($"Bought '{_seedId}' x{_amount} for {_totalCost} gold.", this);
        }

        private void OnValidate()
        {
            _amount = Mathf.Max(1, _amount);
            _totalCost = Mathf.Max(0, _totalCost);
        }
    }
}
