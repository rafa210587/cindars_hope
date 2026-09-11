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
        [SerializeField] private int _pickupIndex;
        [SerializeField] private string _itemId;
        [SerializeField] private string _itemInstanceId;
        [SerializeField] private int _amount = 1;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => "Pegar";
        public int PickupIndex => _pickupIndex;
        public string ItemId => _itemId;
        public string ItemInstanceId => _itemInstanceId;
        public int Amount => _amount;
        public bool IsCollected { get; private set; }

        public void Configure(int pickupIndex, string itemId, int amount, InventoryManager inventoryManager)
            => Configure(pickupIndex, itemId, string.Empty, amount, inventoryManager);

        public void Configure(int pickupIndex, string itemId, string itemInstanceId, int amount, InventoryManager inventoryManager)
        {
            _pickupIndex = pickupIndex;
            _itemId = itemId;
            _itemInstanceId = itemInstanceId ?? string.Empty;
            _amount = Mathf.Max(1, amount);
            _inventoryManager = inventoryManager;
            EnsureComponents();
            SetCollected(IsCollected);
        }

        public ItemPickupSaveData CaptureSaveData()
        {
            return new ItemPickupSaveData
            {
                PickupIndex = _pickupIndex,
                ItemId = _itemId,
                ItemInstanceId = _itemInstanceId,
                Amount = _amount,
                Position = transform.position,
                IsCollected = IsCollected
            };
        }

        public void RestoreFromSaveData(ItemPickupSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning($"ItemPickup {_pickupIndex} cannot restore from null save data.", this);
                return;
            }

            _pickupIndex = saveData.PickupIndex;
            _itemId = string.IsNullOrWhiteSpace(saveData.ItemId) ? string.Empty : saveData.ItemId;
            _itemInstanceId = saveData.ItemInstanceId ?? string.Empty;
            _amount = Mathf.Max(1, saveData.Amount);
            transform.position = saveData.Position;
            SetCollected(saveData.IsCollected);
        }

        public void SetCollected(bool collected)
        {
            IsCollected = collected;
            EnsureComponents();

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = !IsCollected;
            }

            if (_collider != null)
            {
                _collider.enabled = !IsCollected;
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            if (IsCollected)
            {
                return false;
            }

            return _inventoryManager != null
                && !string.IsNullOrWhiteSpace(_itemId)
                && _amount > 0;
        }

        public void Interact(GameObject interactor)
        {
            if (IsCollected)
            {
                return;
            }

            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(ItemPickup)} on '{name}' cannot be picked up because it is not configured.", this);
                return;
            }

            var added = string.IsNullOrWhiteSpace(_itemInstanceId)
                ? _inventoryManager.AddItem(_itemId, _amount)
                : _amount == 1 && _inventoryManager.TryAddItemInstance(_itemId, _itemInstanceId).Success;
            if (!added)
            {
                Debug.LogWarning($"{nameof(ItemPickup)} could not add '{_itemId}' x{_amount} to inventory.", this);
                return;
            }

            GameEventBus.Publish(new ItemPickedUpEvent(_itemId, _amount));
            Debug.Log($"Picked up '{_itemId}' x{_amount}.", this);
            SetCollected(true);
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            _amount = Mathf.Max(1, _amount);
            EnsureComponents();
        }

        private void Awake()
        {
            EnsureComponents();
            SetCollected(IsCollected);
        }

        private void EnsureComponents()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }
    }
}
