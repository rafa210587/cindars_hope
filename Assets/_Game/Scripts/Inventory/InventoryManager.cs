using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Foundation.Transactions;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Inventory
{
    // arch: contrato neutro para quebrar o par mutuo Inventory|Player (spec_arch_inventory_player_pair_reduction) —
    // InventoryManager consome isto em vez de CindarsHope.Player.Data.PlayerDataSO diretamente.
    // PlayerDataSO implementa via interface explicita; nenhum outro consumidor precisa mudar.
    public interface IStartingItemsSource
    {
        System.Collections.Generic.IReadOnlyList<Data.StartingItem> StartingItems { get; }
    }

    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour, IInventoryTransactionPort
    {
        public const int DefaultCapacity = 40;
        public const int MaxCapacity = 40;

        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        private readonly List<InventorySlot> _slots = new List<InventorySlot>(MaxCapacity);
        private ItemDatabaseSO _itemDatabase;

        public bool IsInitialized { get; private set; }
        public bool HasItemDatabase => _itemDatabase != null;
        public int Capacity => _slots.Count;
        public IReadOnlyDictionary<string, int> Items => _items;
        public IReadOnlyList<InventorySlot> Slots => _slots;

        // arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) —
        // InventoryManager se anuncia via DomainManagerRegistry (nao um static Instance/Active proprio,
        // proibido pela regra de ratchet GlobalInventoryAccess) para o GameBootstrap parar de segurar
        // referencia serializada direta a este tipo.
        private void Awake()
        {
            DomainManagerRegistry.Register(this);
        }

        private void OnDestroy()
        {
            DomainManagerRegistry.Unregister(this);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            EnsureCapacity(DefaultCapacity);
            IsInitialized = true;
        }

        public void Initialize(ItemDatabaseSO itemDatabase)
        {
            Initialize();

            if (itemDatabase == null)
            {
                Debug.LogWarning("InventoryManager initialized without ItemDatabaseSO. Item operations will reject unknown ids until a database is assigned.", this);
                return;
            }

            _itemDatabase = itemDatabase;
        }

        public void InitializeFromStartingItems(IStartingItemsSource playerData, ItemDatabaseSO itemDatabase)
        {
            Initialize(itemDatabase);
            CindarsHope.DebugTools.CombatLog.Log("CombatLog: StarterInventoryCheckStarted.", this);

            if (playerData == null)
            {
                Debug.LogWarning("CombatLog: StarterInventoryApplied=False. Reason=PlayerDataMissing.", this);
                return;
            }

            if (itemDatabase == null)
            {
                Debug.LogWarning("CombatLog: StarterInventoryApplied=False. Reason=ItemDatabaseMissing.", this);
                return;
            }

            if (playerData.StartingItems == null || playerData.StartingItems.Count == 0)
            {
                Debug.LogWarning("CombatLog: StarterInventoryApplied=False. Reason=StartingItemsEmpty.", this);
                return;
            }

            var reason = _items.Count == 0 ? "NewGameOrEmptyInventory" : "RepairMissingItems";
            EnsureStarterItemsPresent(playerData, itemDatabase, reason);
        }

        public void EnsureStarterItemsPresent(IStartingItemsSource playerData, ItemDatabaseSO itemDatabase, string reason)
        {
            Initialize(itemDatabase);
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: StarterInventoryCheckStarted. Reason={reason}.", this);

            if (playerData == null)
            {
                Debug.LogWarning($"CombatLog: StarterInventoryApplied=False. Reason={reason}. StarterInventoryReason=PlayerDataMissing.", this);
                return;
            }

            if (itemDatabase == null)
            {
                Debug.LogWarning($"CombatLog: StarterInventoryApplied=False. Reason={reason}. StarterInventoryReason=ItemDatabaseMissing.", this);
                return;
            }

            if (playerData.StartingItems == null || playerData.StartingItems.Count == 0)
            {
                Debug.LogWarning($"CombatLog: StarterInventoryApplied=False. Reason={reason}. StarterInventoryReason=StartingItemsEmpty.", this);
                return;
            }

            var added = new List<string>();
            var skipped = new List<string>();
            foreach (var startingItem in playerData.StartingItems)
            {
                if (startingItem.Item == null)
                {
                    skipped.Add("<null-item>:null-reference");
                    continue;
                }
                if (startingItem.Amount <= 0)
                {
                    skipped.Add($"{startingItem.Item.Id}:invalid-amount-{startingItem.Amount}");
                    continue;
                }
                if (!itemDatabase.TryGetById(startingItem.Item.Id, out _))
                {
                    skipped.Add($"{startingItem.Item.Id}:not-in-itemdatabase");
                    continue;
                }
                if (GetAmount(startingItem.Item.Id) >= startingItem.Amount)
                {
                    skipped.Add($"{startingItem.Item.Id}:already-have-{GetAmount(startingItem.Item.Id)}");
                    continue;
                }

                int desired = startingItem.Amount;
                int currentlyHave = GetAmount(startingItem.Item.Id);
                int toAdd = desired - currentlyHave;
                if (toAdd <= 0) { skipped.Add($"{startingItem.Item.Id}:nothing-to-add"); continue; }
                if (AddItem(startingItem.Item.Id, toAdd)) added.Add($"{startingItem.Item.Id}x{toAdd}");
                else skipped.Add($"{startingItem.Item.Id}:add-failed");
            }

            var appliedReason = added.Count > 0 ? reason : "AlreadyPresent";
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: StarterInventoryApplied={added.Count > 0}. StarterInventoryReason={appliedReason}. ItemsAdded=[{string.Join(", ", added)}]. Skipped=[{string.Join(", ", skipped)}].", this);
        }

        // SPEC 14A-FIX14: clear hotbar bindings that point to items not present in the inventory.
        // Returns the number of cleared bindings.
        public int ClearHotbarBindingsForMissingItems(System.Func<int, string> getSlotItemId, System.Action<int, string> setSlot, int slotCount)
        {
            int cleared = 0;
            for (int i = 0; i < slotCount; i++)
            {
                var id = getSlotItemId(i);
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (GetAmount(id) <= 0)
                {
                    CindarsHope.DebugTools.CombatLog.Log($"CombatLog: HotbarInvalidBindingCleared. Slot={i}, ItemId='{id}', Reason=NotInInventory.", this);
                    setSlot(i, string.Empty);
                    cleared++;
                }
            }
            CindarsHope.DebugTools.CombatLog.Log($"CombatLog: HotbarConsistencyCheck. Cleared={cleared}/{slotCount}.", this);
            return cleared;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            Clear();
            IsInitialized = false;
        }

        public void Clear()
        {
            EnsureCapacity(DefaultCapacity);

            if (_items.Count > 0)
            {
                var removedItems = new List<KeyValuePair<string, int>>(_items);
                foreach (var item in removedItems)
                {
                    GameEventBus.Publish(new InventoryChangedEvent(item.Key, -item.Value, 0));
                }
            }

            foreach (var slot in _slots)
            {
                slot.Clear();
            }

            _items.Clear();
        }

        public int GetAmount(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) || !_items.TryGetValue(itemId, out var amount) ? 0 : amount;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            return GetAmount(itemId) >= amount;
        }

        public bool IsKnownItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out _);
        }

        public bool TryGetItemData(string itemId, out ItemDataSO itemData)
        {
            itemData = null;
            return !string.IsNullOrWhiteSpace(itemId)
                && _itemDatabase != null
                && _itemDatabase.TryGetById(itemId, out itemData)
                && itemData != null;
        }

        public bool TryGetSlot(int slotIndex, out InventorySlot slot)
        {
            EnsureCapacity(DefaultCapacity);
            if (slotIndex < 0 || slotIndex >= _slots.Count)
            {
                slot = null;
                return false;
            }

            slot = _slots[slotIndex];
            return true;
        }

        public List<InventoryItemSnapshot> GetAllItems()
        {
            var result = new List<InventoryItemSnapshot>();

            foreach (var slot in _slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                result.Add(new InventoryItemSnapshot
                {
                    ItemId = slot.ItemId,
                    Amount = slot.Amount,
                    ItemInstanceId = slot.EquipmentBindingId ?? string.Empty,
                    DurabilityCurrent = 1f,
                    DurabilityMax = 1f,
                    IsBroken = false
                });
            }

            return result;
        }

        public InventorySaveData CaptureSaveData()
        {
            RebuildAggregate();
            var saveData = new InventorySaveData
            {
                Capacity = Capacity
            };

            foreach (var slot in _slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                saveData.Slots.Add(new InventorySlotSaveData
                {
                    SlotIndex = slot.SlotIndex,
                    ItemId = slot.ItemId,
                    Amount = slot.Amount,
                    IsEquipped = slot.IsEquipped,
                    EquipmentBindingId = slot.EquipmentBindingId
                });
            }

            foreach (var item in _items)
            {
                if (string.IsNullOrWhiteSpace(item.Key) || item.Value <= 0)
                {
                    continue;
                }

                saveData.Items.Add(new InventoryItemSaveData
                {
                    ItemId = item.Key,
                    Amount = item.Value
                });
            }

            return saveData;
        }

        public void RestoreFromSaveData(InventorySaveData saveData)
        {
            Clear();

            if (saveData == null)
            {
                return;
            }

            var capacity = saveData.Capacity > 0 ? saveData.Capacity : DefaultCapacity;
            EnsureCapacity(capacity);

            if (saveData.Slots != null && saveData.Slots.Count > 0)
            {
                RestoreSlots(saveData.Slots);
            }
            else if (saveData.Items != null)
            {
                RestoreLegacyItems(saveData.Items);
            }

            RebuildAggregateAndPublishRefresh();
        }

        public bool AddItem(string itemId, int amount)
        {
            return TryAddItem(itemId, amount).Success;
        }

        public bool CanAddItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0 || !TryGetItemData(itemId, out var itemData))
            {
                return false;
            }

            EnsureCapacity(DefaultCapacity);
            return GetAvailableCapacityFor(itemId, Mathf.Max(1, itemData.MaxStack)) >= amount;
        }

        public InventoryAddResult TryAddItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return new InventoryAddResult(false, itemId, amount, 0);
            }

            if (!TryGetItemData(itemId, out var itemData))
            {
                Debug.LogWarning($"InventoryManager rejected unknown item id '{itemId}'.", this);
                return new InventoryAddResult(false, itemId, amount, 0);
            }

            EnsureCapacity(DefaultCapacity);
            var previousAmount = GetAmount(itemId);
            var remaining = amount;
            var maxStack = Mathf.Max(1, itemData.MaxStack);
            var available = GetAvailableCapacityFor(itemId, maxStack);
            if (available < amount)
            {
                return new InventoryAddResult(false, itemId, amount, 0);
            }

            foreach (var slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.IsEmpty || slot.ItemId != itemId || slot.Amount >= maxStack)
                {
                    continue;
                }

                var added = Mathf.Min(remaining, maxStack - slot.Amount);
                slot.Amount += added;
                remaining -= added;
            }

            foreach (var slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (!slot.IsEmpty)
                {
                    continue;
                }

                var added = Mathf.Min(remaining, maxStack);
                slot.ItemId = itemId;
                slot.Amount = added;
                remaining -= added;
            }

            var addedAmount = amount - remaining;
            if (addedAmount <= 0)
            {
                return new InventoryAddResult(false, itemId, amount, 0);
            }

            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, addedAmount, previousAmount + addedAmount));
            return new InventoryAddResult(remaining == 0, itemId, amount, addedAmount);
        }

        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return false;
            }

            var currentAmount = GetAmount(itemId);
            if (currentAmount < amount)
            {
                return false;
            }

            var remaining = amount;
            for (var index = _slots.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var slot = _slots[index];
                if (slot.IsEmpty || slot.ItemId != itemId)
                {
                    continue;
                }

                var removed = Mathf.Min(remaining, slot.Amount);
                slot.Amount -= removed;
                remaining -= removed;

                if (slot.Amount <= 0)
                {
                    slot.Clear();
                }
            }

            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, -amount, currentAmount - amount));
            return true;
        }

        public bool SplitSlot(int slotIndex)
        {
            if (!TryGetSlot(slotIndex, out var source) || source.IsEmpty || source.Amount < 2)
            {
                return false;
            }

            var target = FindFirstEmptySlot();
            if (target == null)
            {
                return false;
            }

            var splitAmount = source.Amount / 2;
            source.Amount -= splitAmount;
            target.ItemId = source.ItemId;
            target.Amount = splitAmount;

            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(source.ItemId, 0, GetAmount(source.ItemId)));
            return true;
        }

        public bool DestroySlot(int slotIndex)
        {
            if (!TryGetSlot(slotIndex, out var slot) || slot.IsEmpty)
            {
                return false;
            }

            var itemId = slot.ItemId;
            var amount = slot.Amount;
            slot.Clear();
            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, -amount, GetAmount(itemId)));
            return true;
        }

        public bool MarkSlotEquipped(int slotIndex, string equipmentBindingId)
        {
            if (!TryGetSlot(slotIndex, out var slot) || slot.IsEmpty || !TryGetItemData(slot.ItemId, out var itemData) || !itemData.IsEquippable)
            {
                return false;
            }

            slot.IsEquipped = true;
            slot.EquipmentBindingId = string.IsNullOrWhiteSpace(equipmentBindingId) ? "equipment" : equipmentBindingId;
            return true;
        }

        public bool MarkSlotEquipped(int slotIndex, EquipmentSlot equipmentSlot)
        {
            return equipmentSlot != EquipmentSlot.None
                && MarkSlotEquipped(slotIndex, GetEquipmentSlotBindingId(equipmentSlot));
        }

        public bool ClearEquippedBindingAtSlot(int slotIndex)
        {
            if (!TryGetSlot(slotIndex, out var slot) || slot.IsEmpty || !slot.IsEquipped)
            {
                return false;
            }

            slot.IsEquipped = false;
            slot.EquipmentBindingId = string.Empty;
            return true;
        }

        public bool ClearEquippedBinding(EquipmentSlot equipmentSlot, string legacyItemId = null)
        {
            var bindingId = GetEquipmentSlotBindingId(equipmentSlot);
            foreach (var slot in _slots)
            {
                if (slot != null && !slot.IsEmpty && slot.IsEquipped && slot.EquipmentBindingId == bindingId)
                {
                    slot.IsEquipped = false;
                    slot.EquipmentBindingId = string.Empty;
                    return true;
                }
            }

            // Legacy SPEC 17 bindings stored item IDs; clear one matching stack only.
            if (!string.IsNullOrWhiteSpace(legacyItemId))
            {
                foreach (var slot in _slots)
                {
                    if (slot != null && !slot.IsEmpty && slot.IsEquipped && slot.ItemId == legacyItemId)
                    {
                        slot.IsEquipped = false;
                        slot.EquipmentBindingId = string.Empty;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ClearEquippedBinding(string equipmentBindingId)
        {
            var changed = false;
            foreach (var slot in _slots)
            {
                if (slot == null || slot.IsEmpty || !slot.IsEquipped)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(equipmentBindingId) && slot.EquipmentBindingId != equipmentBindingId)
                {
                    continue;
                }

                slot.IsEquipped = false;
                slot.EquipmentBindingId = string.Empty;
                changed = true;
            }

            return changed;
        }

        private static string GetEquipmentSlotBindingId(EquipmentSlot equipmentSlot)
        {
            return $"equipment-slot:{equipmentSlot}";
        }

        public bool DropItem(int slotIndex, Vector3 dropPosition)
        {
            if (!TryGetSlot(slotIndex, out var slot) || slot.IsEmpty)
            {
                return false;
            }

            var spawner = World.ItemDropSpawner.Instance;
            if (spawner == null)
            {
                Debug.LogWarning("InventoryManager: ItemDropSpawner not available.", this);
                return false;
            }

            var itemId = slot.ItemId;
            var amount = slot.Amount;

            if (!spawner.TryDropItem(itemId, amount, dropPosition))
            {
                Debug.LogWarning($"InventoryManager: Failed to drop item '{itemId}' x{amount}.", this);
                return false;
            }

            var previousAmount = GetAmount(itemId);
            slot.Clear();
            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, -amount, previousAmount - amount));
            return true;
        }

        private void RestoreSlots(List<InventorySlotSaveData> savedSlots)
        {
            foreach (var savedSlot in savedSlots)
            {
                if (savedSlot == null || string.IsNullOrWhiteSpace(savedSlot.ItemId) || savedSlot.Amount <= 0)
                {
                    Debug.LogWarning("InventoryManager skipped invalid saved inventory slot.", this);
                    continue;
                }

                if (!TryGetItemData(savedSlot.ItemId, out var itemData))
                {
                    Debug.LogWarning($"InventoryManager skipped unknown saved item id '{savedSlot.ItemId}'.", this);
                    continue;
                }

                var slotIndex = Mathf.Clamp(savedSlot.SlotIndex, 0, MaxCapacity - 1);
                EnsureCapacity(slotIndex + 1);
                var maxStack = Mathf.Max(1, itemData.MaxStack);
                var remaining = savedSlot.Amount;

                if (_slots[slotIndex].IsEmpty)
                {
                    var placed = Mathf.Min(remaining, maxStack);
                    _slots[slotIndex].ItemId = savedSlot.ItemId;
                    _slots[slotIndex].Amount = placed;
                    _slots[slotIndex].IsEquipped = savedSlot.IsEquipped;
                    _slots[slotIndex].EquipmentBindingId = savedSlot.EquipmentBindingId ?? string.Empty;
                    remaining -= placed;
                }

                if (remaining > 0)
                {
                    TryAddItem(savedSlot.ItemId, remaining);
                }
            }
        }

        private void RestoreLegacyItems(List<InventoryItemSaveData> savedItems)
        {
            foreach (var item in savedItems)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.ItemId) || item.Amount <= 0)
                {
                    Debug.LogWarning("InventoryManager skipped invalid saved inventory item.", this);
                    continue;
                }

                if (!TryAddItem(item.ItemId, item.Amount).Success)
                {
                    Debug.LogWarning($"InventoryManager could not fully restore item '{item.ItemId}' x{item.Amount}.", this);
                }
            }
        }

        private InventorySlot FindFirstEmptySlot()
        {
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    return slot;
                }
            }

            return null;
        }

        private int GetAvailableCapacityFor(string itemId, int maxStack)
        {
            var available = 0;
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    available += maxStack;
                }
                else if (slot.ItemId == itemId && slot.Amount < maxStack)
                {
                    available += maxStack - slot.Amount;
                }
            }

            return available;
        }

        private void EnsureCapacity(int requestedCapacity)
        {
            var capacity = Mathf.Clamp(requestedCapacity, DefaultCapacity, MaxCapacity);
            while (_slots.Count < capacity)
            {
                _slots.Add(new InventorySlot
                {
                    SlotIndex = _slots.Count
                });
            }
        }

        private void RebuildAggregateAndPublishRefresh()
        {
            RebuildAggregate();
            foreach (var item in _items)
            {
                GameEventBus.Publish(new InventoryChangedEvent(item.Key, 0, item.Value));
            }
        }

        private void RebuildAggregate()
        {
            _items.Clear();
            foreach (var slot in _slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (_items.TryGetValue(slot.ItemId, out var currentAmount))
                {
                    _items[slot.ItemId] = currentAmount + slot.Amount;
                }
                else
                {
                    _items[slot.ItemId] = slot.Amount;
                }
            }
        }
    }

    public sealed class InventoryItemSnapshot
    {
        public string ItemId;
        public int Amount;
        public string ItemInstanceId;
        public float DurabilityCurrent;
        public float DurabilityMax;
        public bool IsBroken;
    }
}
