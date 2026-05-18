using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ResourceNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _nodeId = "stone_node_1";
        [SerializeField] private string _itemId = "ore_copper";
        [SerializeField] private int _amount = 1;
        [SerializeField] private string _interactionPrompt = "Minerar";

        private InventoryManager _inventoryManager;
        private bool _isCollected;

        public string InteractionPrompt => _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !_isCollected && _inventoryManager != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"ResourceNode '{_nodeId}' cannot be interacted.", this);
                return;
            }

            if (_inventoryManager.AddItem(_itemId, _amount))
            {
                _isCollected = true;
                gameObject.SetActive(false);
                Debug.Log($"ResourceNode '{_nodeId}' harvested: {_amount}x {_itemId}.", this);
            }
            else
            {
                Debug.LogWarning($"ResourceNode '{_nodeId}' could not add item to inventory.", this);
            }
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }
            else
            {
                Debug.LogWarning($"ResourceNode '{_nodeId}' received null InventoryManager.", this);
            }
        }

        public void SetCollected(bool collected)
        {
            _isCollected = collected;
            if (collected)
            {
                gameObject.SetActive(false);
            }
        }

        public bool IsCollected => _isCollected;
        public string NodeId => _nodeId;
    }
}
