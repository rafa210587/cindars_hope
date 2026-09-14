using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _pickupIndex;
        [SerializeField] private string _itemId;
        [SerializeField] private string _itemInstanceId;
        [SerializeField] private int _amount = 1;
        [SerializeField] private bool _isDynamic;
        [SerializeField] private string _runtimeGeneratedPickupId;
        [SerializeField] private string _originSceneName;
        [SerializeField] private List<ItemOriginLot> _originLots = new List<ItemOriginLot>();
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => "Pegar";
        public int PickupIndex => _pickupIndex;
        public string ItemId => _itemId;
        public string ItemInstanceId => _itemInstanceId;
        public int Amount => _amount;
        public bool IsCollected { get; private set; }
        public bool IsDynamic => _isDynamic;
        public string RuntimeGeneratedPickupId => _runtimeGeneratedPickupId;
        public string OriginSceneName => _isDynamic ? _originSceneName : gameObject.scene.name;
        public IReadOnlyList<ItemOriginLot> OriginLots => _originLots;

        public void Configure(int pickupIndex, string itemId, int amount, InventoryManager inventoryManager)
            => Configure(pickupIndex, itemId, string.Empty, amount, inventoryManager);

        public void Configure(int pickupIndex, string itemId, string itemInstanceId, int amount, InventoryManager inventoryManager)
            => ConfigureWithLots(pickupIndex, itemId, itemInstanceId, amount,
                new[] { new ItemOriginLot(ItemOriginKind.Earned, amount) }, inventoryManager, false);

        public bool ConfigureWithLots(int pickupIndex, string itemId, string itemInstanceId, int amount,
            IReadOnlyList<ItemOriginLot> originLots, InventoryManager inventoryManager, bool isDynamic)
        {
            if (amount <= 0 || !ItemOriginLot.IsValid(originLots, amount)) return false;
            _pickupIndex = pickupIndex;
            _itemId = itemId;
            _itemInstanceId = itemInstanceId ?? string.Empty;
            _amount = amount;
            _originLots = ItemOriginLot.CopyLots(originLots);
            _isDynamic = isDynamic;
            _runtimeGeneratedPickupId = isDynamic ? Guid.NewGuid().ToString("N") : string.Empty;
            _originSceneName = isDynamic ? SceneManager.GetActiveScene().name : string.Empty;
            _inventoryManager = inventoryManager;
            EnsureComponents();
            SetCollected(IsCollected);
            return true;
        }

        public ItemPickupSaveData CaptureSaveData()
        {
            EnsureSceneAuthoredOrigin();
            return new ItemPickupSaveData
            {
                PickupIndex = _pickupIndex,
                ItemId = _itemId,
                ItemInstanceId = _itemInstanceId,
                Amount = _amount,
                Position = transform.position,
                IsCollected = IsCollected,
                IsDynamic = _isDynamic,
                RuntimeGeneratedPickupId = _runtimeGeneratedPickupId,
                SceneName = OriginSceneName,
                OriginSchemaVersion = 1,
                OriginLots = CaptureOriginLots()
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
            _isDynamic = saveData.IsDynamic;
            _runtimeGeneratedPickupId = saveData.RuntimeGeneratedPickupId ?? string.Empty;
            _originSceneName = saveData.IsDynamic ? saveData.SceneName ?? string.Empty : string.Empty;
            _originLots = saveData.OriginSchemaVersion == 1
                ? ItemOriginLot.FromSaveData(saveData.OriginLots, _amount)
                : ItemOriginLot.Invalid(_amount);
            transform.position = saveData.Position;
            SetCollected(saveData.IsCollected);
        }

        public List<ItemOriginLot> CopyOriginLots() => ItemOriginLot.CopyLots(_originLots);

        private List<ItemOriginLotSaveData> CaptureOriginLots()
        {
            var result = new List<ItemOriginLotSaveData>();
            foreach (var lot in _originLots) result.Add(lot.ToSaveData());
            return result;
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
            EnsureSceneAuthoredOrigin();
            if (IsCollected)
            {
                return false;
            }

            return _inventoryManager != null
                && !string.IsNullOrWhiteSpace(_itemId)
                && _amount > 0
                && ItemOriginLot.IsValid(_originLots, _amount);
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

            var originInventory = _inventoryManager as IOriginInventoryRuntime;
            if (originInventory == null || (!string.IsNullOrWhiteSpace(_itemInstanceId) && _amount != 1)
                || !originInventory.TryStageOriginAddition(_itemId, _itemInstanceId,
                    _amount, CaptureOriginLots(), out var token))
            {
                Debug.LogWarning($"{nameof(ItemPickup)} could not add '{_itemId}' x{_amount} to inventory.", this);
                return;
            }

            // A save listener sees both sides committed: item in inventory and pickup consumed.
            SetCollected(true);
            if (!originInventory.TryCommitOriginAddition(token))
            {
                if (originInventory.TryRollbackOriginAddition(token)) SetCollected(false);
                else Debug.LogError($"{nameof(ItemPickup)} could not roll back staged pickup '{_itemId}'.", this);
                return;
            }
            GameEventBus.Publish(new ItemPickedUpEvent(_itemId, _amount));
            Debug.Log($"Picked up '{_itemId}' x{_amount}.", this);
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
            EnsureSceneAuthoredOrigin();
            SetCollected(IsCollected);
        }

        private void EnsureSceneAuthoredOrigin()
        {
            // Existing authored FarmScene pickups predate the serialized origin field. Only a
            // never-restored static scene pickup may acquire Earned origin from that absence.
            if (!_isDynamic && _amount > 0 && (_originLots == null || _originLots.Count == 0))
                _originLots = new List<ItemOriginLot> { new ItemOriginLot(ItemOriginKind.Earned, _amount) };
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
