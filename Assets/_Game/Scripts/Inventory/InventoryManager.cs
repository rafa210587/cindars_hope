using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
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

    /// <summary>Owns inventory state and adapts catalogs, bootstrap and gameplay events.</summary>
    /// <remarks>Add/remove and split/move delegate to InventorySlotOperations; these mutations rebuild totals before publishing.</remarks>
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour, IInventoryTransactionPort, IInventoryRuntime, IGameBootstrapRuntimeService
    {
        public const int DefaultCapacity = 40;
        public const int MaxCapacity = 40;

        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
        private readonly List<InventorySlot> _slots = new List<InventorySlot>(MaxCapacity);
        private ItemDatabaseSO _itemDatabase;
        private long _nextItemInstanceSequence = 1;

        public bool IsInitialized { get; private set; }
        public bool HasItemDatabase => _itemDatabase != null;
        public int Capacity => _slots.Count;
        public IReadOnlyDictionary<string, int> Items => _items;
        public IReadOnlyList<InventorySlot> Slots => _slots;

        // arch: quebra do par mutuo Core|Inventory (2026-07-15) — InventoryManager se anuncia via
        // DomainManagerRegistry sob a porta IInventoryRuntime (nao um static Instance/Active proprio,
        // proibido pela regra de ratchet GlobalInventoryAccess) para o GameBootstrap parar de segurar
        // referencia serializada direta a este tipo ou nomear CindarsHope.Inventory.
        // arch/bugfix (2026-08-11): guarda de duplicata no self-registro — ver PlayerManager para o
        // racional completo. Cada cena traz um GameBootstrap+InventoryManager; o InventoryManager da
        // duplicata (destruida pelo singleton do GameBootstrap) nao pode sobrescrever e depois remover
        // o registro do InventoryManager persistente, sob pena de IInventoryRuntime ficar nulo e o
        // NpcShopController._inventoryManager falhar no Interact.
        private bool _ownsRegistration;

        private void Awake()
        {
            var existing = DomainManagerRegistry.Get<IInventoryRuntime>();
            if (existing is UnityEngine.Object existingObject && existingObject != null && !ReferenceEquals(existing, this))
            {
                _ownsRegistration = false;
                return;
            }

            DomainManagerRegistry.Register<IInventoryRuntime>(this);
            _ownsRegistration = true;
        }

        private void OnDestroy()
        {
            if (!_ownsRegistration)
            {
                return;
            }

            DomainManagerRegistry.Unregister<IInventoryRuntime>(this);
        }

        // arch: quebra do par mutuo Core|Inventory (2026-07-15) — IGameBootstrapRuntimeService
        // (molde SaveManager/ShopManager): GameBootstrap.InitializeBootstrapRuntimeServices() chama
        // isto via GetComponents<MonoBehaviour>() (mesmo GameObject), sem precisar nomear
        // CindarsHope.Inventory.InventoryManager. Substitui a antiga chamada direta
        // inventoryManager.InitializeFromStartingItems(_playerData, _itemDatabase) feita por
        // GameBootstrap.InitializeManagers().
        public string BootstrapServiceId => "InventoryManager";

        public void InitializeFromBootstrap(GameBootstrapRuntimeContext context)
        {
            var itemDatabase = context.ItemDatabase as ItemDatabaseSO;
            // arch: quebra do par mutuo Core|Player (2026-07-15) — context.PlayerData agora e
            // ScriptableObject (Core nao pode mais nomear PlayerDataSO); castado aqui para o contrato
            // neutro IStartingItemsSource que PlayerDataSO ja implementa (molde do corte Inventory|Player).
            var startingItemsSource = context.PlayerData as IStartingItemsSource;
            if (startingItemsSource != null && itemDatabase != null)
            {
                InitializeFromStartingItems(startingItemsSource, itemDatabase);
            }
            else
            {
                Debug.LogWarning("InventoryManager.InitializeFromBootstrap is missing PlayerDataSO or ItemDatabaseSO. InventoryManager will initialize without starting items.", this);
                Initialize();
            }
        }

        public void ShutdownFromBootstrap() => Shutdown();

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
                    ItemInstanceId = ResolveSnapshotInstanceId(slot),
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
                Capacity = Capacity,
                NextItemInstanceSequence = System.Math.Max(1, _nextItemInstanceSequence)
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
                    ItemInstanceId = slot.ItemInstanceId,
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
                _nextItemInstanceSequence = 1;
                return;
            }

            _nextItemInstanceSequence = System.Math.Max(1, saveData.NextItemInstanceSequence);
            var capacity = saveData.Capacity > 0 ? saveData.Capacity : DefaultCapacity;
            EnsureCapacity(capacity);

            if (saveData.Slots != null && saveData.Slots.Count > 0)
            {
                foreach (var savedSlot in saveData.Slots)
                    AdvanceItemInstanceSequencePast(savedSlot?.ItemInstanceId);
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
            return InventorySlotOperations.GetAvailableCapacityFor(_slots, itemId, itemData.MaxStack) >= amount;
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
            if (itemData.MaxStack == 1)
            {
                var emptySlots = 0;
                foreach (var slot in _slots)
                    if (slot != null && slot.IsEmpty) emptySlots++;
                if (emptySlots < amount)
                    return new InventoryAddResult(false, itemId, amount, 0);

                var remaining = amount;
                foreach (var slot in _slots)
                {
                    if (slot == null || !slot.IsEmpty) continue;
                    slot.ItemId = itemId;
                    slot.ItemInstanceId = ReserveItemInstanceId(itemId);
                    slot.Amount = 1;
                    slot.IsEquipped = false;
                    slot.EquipmentBindingId = string.Empty;
                    if (--remaining == 0) break;
                }

                RebuildAggregate();
                GameEventBus.Publish(new InventoryChangedEvent(itemId, amount, previousAmount + amount));
                return new InventoryAddResult(true, itemId, amount, amount);
            }

            var result = InventorySlotOperations.TryAddItem(_slots, itemId, amount, itemData.MaxStack);
            if (!result.Success) return result;

            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, result.AddedAmount, previousAmount + result.AddedAmount));
            return result;
        }

        /// <summary>
        /// Adds one individually tracked item. Instance ids are namespaced by their canonical item
        /// id (`itemId#token`) so catalog consumers can resolve the definition without another
        /// runtime registry. Ordinary stack additions remain identity-free.
        /// </summary>
        public InventoryAddResult TryAddItemInstance(string itemId, string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(itemInstanceId)
                || !ItemInstanceIdUtility.IsForItem(itemId, itemInstanceId)
                || !TryGetItemData(itemId, out var itemData) || itemData.MaxStack != 1)
            {
                return new InventoryAddResult(false, itemId, 1, 0);
            }

            EnsureCapacity(DefaultCapacity);
            for (var index = 0; index < _slots.Count; index++)
            {
                var existing = _slots[index];
                if (existing != null && !existing.IsEmpty
                    && string.Equals(existing.ItemInstanceId, itemInstanceId, System.StringComparison.Ordinal))
                {
                    return new InventoryAddResult(false, itemId, 1, 0);
                }
            }

            var target = FindFirstEmptySlot();
            if (target == null)
                return new InventoryAddResult(false, itemId, 1, 0);

            var previousAmount = GetAmount(itemId);
            target.ItemId = itemId;
            target.ItemInstanceId = itemInstanceId;
            target.Amount = 1;
            target.IsEquipped = false;
            target.EquipmentBindingId = string.Empty;
            AdvanceItemInstanceSequencePast(itemInstanceId);
            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(itemId, 1, previousAmount + 1));
            return new InventoryAddResult(true, itemId, 1, 1);
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

            if (!InventorySlotOperations.TryRemoveItem(_slots, itemId, amount)) return false;

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
            if (!InventorySlotOperations.TrySplit(source, target))
            {
                return false;
            }

            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(source.ItemId, 0, GetAmount(source.ItemId)));
            return true;
        }

        /// <summary>
        /// Moves a stack to an empty slot, merges stacks of the same item, or swaps two distinct items.
        /// The operation is atomic: validation happens before either slot changes.
        /// </summary>
        public bool TryMoveOrMergeSlot(int sourceSlotIndex, int destinationSlotIndex, out string failureReason)
        {
            failureReason = string.Empty;
            EnsureCapacity(DefaultCapacity);

            if (sourceSlotIndex < 0 || sourceSlotIndex >= _slots.Count
                || destinationSlotIndex < 0 || destinationSlotIndex >= _slots.Count)
            {
                failureReason = "Slot de origem ou destino invalido.";
                return false;
            }

            if (sourceSlotIndex == destinationSlotIndex)
            {
                failureReason = "Escolha um slot de destino diferente.";
                return false;
            }

            var source = _slots[sourceSlotIndex];
            var destination = _slots[destinationSlotIndex];
            if (source == null || source.IsEmpty)
            {
                failureReason = "O slot de origem esta vazio.";
                return false;
            }

            if (source.IsEquipped || (destination != null && destination.IsEquipped))
            {
                failureReason = "Itens equipados nao podem ser movidos.";
                return false;
            }

            if (!TryGetItemData(source.ItemId, out var sourceItemData))
            {
                failureReason = "O item de origem nao e reconhecido.";
                return false;
            }

            if (destination == null)
            {
                failureReason = "O slot de destino nao existe.";
                return false;
            }

            if (!destination.IsEmpty && !TryGetItemData(destination.ItemId, out _))
            {
                failureReason = "O item de destino nao e reconhecido.";
                return false;
            }

            var sourceItemId = source.ItemId;
            var destinationItemId = destination.ItemId;
            if (!InventorySlotOperations.TryMoveOrMerge(source, destination, sourceItemData.MaxStack, out failureReason))
            {
                return false;
            }

            RebuildAggregate();
            PublishRefreshForAffectedItems(sourceItemId, destinationItemId);
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

        /// <summary>Atomically replaces one identified item with salvage rewards.</summary>
        public bool TryCommitSalvage(int slotIndex, string expectedItemId, string expectedInstanceId,
            IReadOnlyList<SalvageReward> rewards, System.Func<bool> commitSideEffect, out string failureReason)
        {
            failureReason = string.Empty;
            if (!TryGetSlot(slotIndex, out var source) || source.IsEmpty || source.IsEquipped || source.Amount != 1 ||
                string.IsNullOrWhiteSpace(source.ItemInstanceId) ||
                !string.Equals(source.ItemId, expectedItemId, System.StringComparison.Ordinal) ||
                !string.Equals(source.ItemInstanceId, expectedInstanceId, System.StringComparison.Ordinal))
            { failureReason = "Item indisponivel para salvage."; return false; }
            if (rewards == null || rewards.Count == 0) { failureReason = "Salvage sem retorno valido."; return false; }

            var copy = new List<InventorySlot>(_slots.Count);
            foreach (var slot in _slots)
                copy.Add(new InventorySlot { SlotIndex = slot.SlotIndex, ItemId = slot.ItemId,
                    ItemInstanceId = slot.ItemInstanceId, Amount = slot.Amount, IsEquipped = slot.IsEquipped,
                    EquipmentBindingId = slot.EquipmentBindingId });
            copy[slotIndex].Clear();
            foreach (var reward in rewards)
            {
                if (reward.Amount <= 0 || string.Equals(reward.ItemId, expectedItemId, System.StringComparison.Ordinal) ||
                    !TryGetItemData(reward.ItemId, out var data) ||
                    !InventorySlotOperations.TryAddItem(copy, reward.ItemId, reward.Amount, data.MaxStack).Success)
                { failureReason = "Inventario sem espaco para o retorno."; return false; }
            }
            if (commitSideEffect == null || !commitSideEffect())
            { failureReason = "Falha ao confirmar a transacao de salvage."; return false; }
            for (var i = 0; i < _slots.Count; i++)
            {
                _slots[i].ItemId = copy[i].ItemId; _slots[i].ItemInstanceId = copy[i].ItemInstanceId;
                _slots[i].Amount = copy[i].Amount; _slots[i].IsEquipped = copy[i].IsEquipped;
                _slots[i].EquipmentBindingId = copy[i].EquipmentBindingId;
            }
            RebuildAggregate();
            GameEventBus.Publish(new InventoryChangedEvent(expectedItemId, -1, GetAmount(expectedItemId)));
            foreach (var reward in rewards)
                GameEventBus.Publish(new InventoryChangedEvent(reward.ItemId, reward.Amount, GetAmount(reward.ItemId)));
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
            var itemInstanceId = slot.ItemInstanceId;

            if (!spawner.TryDropItem(itemId, itemInstanceId, amount, dropPosition))
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
            var restoredInstanceIds = new HashSet<string>(System.StringComparer.Ordinal);
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
                    var instanceId = maxStack == 1 ? savedSlot.ItemInstanceId : string.Empty;
                    if (maxStack == 1 && !ItemInstanceIdUtility.IsForItem(savedSlot.ItemId, instanceId))
                        instanceId = ReserveItemInstanceId(savedSlot.ItemId);
                    if (!string.IsNullOrWhiteSpace(instanceId) && !restoredInstanceIds.Add(instanceId))
                    {
                        Debug.LogWarning($"InventoryManager ignored duplicate saved item instance id '{instanceId}'.", this);
                        continue;
                    }
                    _slots[slotIndex].ItemId = savedSlot.ItemId;
                    _slots[slotIndex].ItemInstanceId = instanceId ?? string.Empty;
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

        private string ReserveItemInstanceId(string itemId)
        {
            return $"{itemId}#inventory-{_nextItemInstanceSequence++}";
        }

        private void AdvanceItemInstanceSequencePast(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            const string marker = "#inventory-";
            var markerIndex = itemInstanceId.LastIndexOf(marker, System.StringComparison.Ordinal);
            if (markerIndex < 1) return;
            if (long.TryParse(itemInstanceId.Substring(markerIndex + marker.Length), out var sequence)
                && sequence >= _nextItemInstanceSequence)
                _nextItemInstanceSequence = sequence + 1;
        }

        private string ResolveSnapshotInstanceId(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return string.Empty;
            if (!string.IsNullOrWhiteSpace(slot.ItemInstanceId)) return slot.ItemInstanceId;
            return TryGetItemData(slot.ItemId, out var itemData) && itemData.MaxStack == 1
                ? slot.ItemId
                : string.Empty;
        }

        private void PublishRefreshForAffectedItems(string firstItemId, string secondItemId)
        {
            if (!string.IsNullOrWhiteSpace(firstItemId))
            {
                GameEventBus.Publish(new InventoryChangedEvent(firstItemId, 0, GetAmount(firstItemId)));
            }

            if (!string.IsNullOrWhiteSpace(secondItemId) && secondItemId != firstItemId)
            {
                GameEventBus.Publish(new InventoryChangedEvent(secondItemId, 0, GetAmount(secondItemId)));
            }
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
