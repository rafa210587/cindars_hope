using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Economy.Transactions;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public class SeedShopPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private string _seedId = "item_seed_wheat";
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

            var result = AtomicPurchaseTransaction.ExecutePurchased(
                _inventoryManager, _playerManager, _seedId, _amount, _totalCost);
            if (!result.Success)
            {
                Debug.LogWarning($"SeedShopPoint could not buy '{_seedId}' x{_amount}: {result.Failure}.", this);
                return;
            }

            Debug.Log($"Bought '{_seedId}' x{_amount} for {_totalCost} gold.", this);
        }

        // arch: quebra do par mutuo Player|SceneManagement (2026-07-15) — playerManagerRef agora e
        // MonoBehaviour (FarmSceneRuntimeReferenceInstaller so tem bootstrap.PlayerManager como
        // MonoBehaviour); cast para o tipo concreto aqui dentro.
        public void RebindRuntimeManagers(InventoryManager inventoryManager, MonoBehaviour playerManagerRef)
        {
            var playerManager = playerManagerRef as PlayerManager;

            if (inventoryManager == null)
            {
                Debug.LogWarning("SeedShopPoint received null InventoryManager for rebind.", this);
            }
            else
            {
                _inventoryManager = inventoryManager;
            }

            if (playerManager == null)
            {
                Debug.LogWarning("SeedShopPoint received null PlayerManager for rebind.", this);
            }
            else
            {
                _playerManager = playerManager;
            }
        }

        private void OnValidate()
        {
            _amount = Mathf.Max(1, _amount);
            _totalCost = Mathf.Max(0, _totalCost);
        }
    }
}
