using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using System.Collections.Generic;
using CindarsHope.Player;
using System;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    // arch: quebra do par mutuo Core|Inventory (2026-07-15) — recebe IInventoryRuntime (porta,
    // Foundation) em vez do tipo concreto CindarsHope.Inventory.InventoryManager, para que
    // GameBootstrap (Core) construa esta classe sem nomear CindarsHope.Inventory.
    public class CorpseRecoveryManager
    {
        private Corpse _activeCorpse;
        private readonly PlayerManager _playerManager;
        private readonly IInventoryRuntime _inventoryManager;
        private readonly IEquipmentRuntime _equipmentRuntime;

        public Corpse ActiveCorpse => _activeCorpse;
        public bool HasActiveCorpse => _activeCorpse != null
            && (_activeCorpse.Status == CorpseStatus.Active
                || _activeCorpse.Status == CorpseStatus.PartiallyRecovered);

        public CorpseRecoveryManager(
            PlayerManager playerManager,
            IInventoryRuntime inventoryManager,
            IEquipmentRuntime equipmentRuntime)
        {
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _equipmentRuntime = equipmentRuntime ?? throw new ArgumentNullException(nameof(equipmentRuntime));
        }

        public void SetActiveCorpse(Corpse corpse)
        {
            var oldCorpseId = SetActiveCorpseDeferred(corpse);
            if (oldCorpseId != null)
                PublishReplacement(oldCorpseId, corpse.CorpseId);
        }

        // Death resolver installs the complete body before mutating resources, then publishes
        // replacement only when all resource owners expose their final state.
        public string SetActiveCorpseDeferred(Corpse corpse)
        {
            if (corpse == null) { _activeCorpse = null; return null; }
            string oldCorpseId = null;
            if (_activeCorpse != null && _activeCorpse.CorpseId != corpse.CorpseId)
            {
                // Replace old corpse
                oldCorpseId = _activeCorpse.CorpseId;
                _activeCorpse.Status = CorpseStatus.Replaced;
                _activeCorpse.ReplacedByCorpseId = corpse.CorpseId;
            }

            _activeCorpse = corpse;
            return oldCorpseId;
        }

        public void PublishReplacement(string oldCorpseId, string newCorpseId)
        {
            if (string.IsNullOrWhiteSpace(oldCorpseId) || _activeCorpse?.CorpseId != newCorpseId)
                return;
            GameEventBus.Publish(new CorpseReplacedEvent
            {
                OldCorpseId = oldCorpseId,
                NewCorpseId = newCorpseId
            });
        }

        public void RecoverCorpse()
        {
            if (!HasActiveCorpse)
            {
                Debug.LogWarning("[CorpseRecoveryManager] No active corpse to recover");
                return;
            }

            var goldRecovered = RecoverGold();
            var itemsRecovered = RecoverItems();
            bool fullyRecovered = goldRecovered && itemsRecovered;

            if (fullyRecovered)
            {
                _activeCorpse.Status = CorpseStatus.Recovered;
                GameEventBus.Publish(new CorpseRecoveredEvent { CorpseId = _activeCorpse.CorpseId });
                _activeCorpse = null;
            }
            else
            {
                _activeCorpse.Status = CorpseStatus.PartiallyRecovered;
                GameEventBus.Publish(new CorpsePartiallyRecoveredEvent { CorpseId = _activeCorpse.CorpseId });
            }
        }

        public int GetRevivalTearCount(string corpseId)
        {
            if (!HasActiveCorpse || string.IsNullOrEmpty(corpseId) || _activeCorpse.CorpseId != corpseId) return 0;
            var count = 0;
            foreach (var item in _activeCorpse.InventoryItems)
                if (item != null && item.ItemId == DeathRevivalDecision.GoddessTearItemId)
                    count += Math.Max(0, item.Amount);
            return count;
        }

        public bool TryConsumeRevivalTear(string corpseId)
        {
            if (GetRevivalTearCount(corpseId) <= 0) return false;
            var items = _activeCorpse.InventoryItems;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null || item.ItemId != DeathRevivalDecision.GoddessTearItemId || item.Amount <= 0) continue;
                if (item.OriginSchemaVersion == 1)
                {
                    // Keep paid/resale provenance consistent with the remaining physical units.
                    var lots = item.OriginLots;
                    var total = 0;
                    if (lots != null)
                        foreach (var lot in lots)
                            if (lot != null && lot.Quantity > 0) total += lot.Quantity;
                    if (total != item.Amount) return false;
                    for (var j = 0; j < lots.Count; j++)
                    {
                        if (lots[j] == null || lots[j].Quantity <= 0) continue;
                        lots[j].Quantity--;
                        if (lots[j].Quantity == 0) lots.RemoveAt(j);
                        break;
                    }
                }
                item.Amount--;
                if (item.Amount == 0) items.RemoveAt(i);
                return true;
            }
            return false;
        }

        private bool RecoverGold()
        {
            if (_activeCorpse.GoldAmount <= 0)
            {
                return true;
            }

            var amount = _activeCorpse.GoldAmount;
            if (!_playerManager.TryStageGoldMutation(amount, out var token)) return false;
            _activeCorpse.GoldAmount = 0;
            if (_playerManager.TryCommitGoldMutation(token)) return true;
            _activeCorpse.GoldAmount = amount;
            _playerManager.TryRollbackGoldMutation(token);
            return false;
        }

        private bool RecoverItems()
        {
            if (!(_inventoryManager is IOriginInventoryRuntime originInventory)) return false;
            var allRecovered = true;
            foreach (var item in new List<CorpseItem>(_activeCorpse.InventoryItems))
                if (!RecoverItem(originInventory, _activeCorpse.InventoryItems, item, false))
                    allRecovered = false;
            foreach (var item in new List<CorpseItem>(_activeCorpse.EquipmentItems))
                if (!RecoverItem(originInventory, _activeCorpse.EquipmentItems, item, true))
                    allRecovered = false;
            return allRecovered;
        }

        private bool RecoverItem(IOriginInventoryRuntime originInventory, List<CorpseItem> source,
            CorpseItem item, bool wasEquipment)
        {
            if (item == null || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.ItemId)) return false;
            var sourceSlot = (EquipmentSlot)item.SourceSlotType;
            var reEquip = wasEquipment && item.SourceSlotType > (int)EquipmentSlot.None
                && Enum.IsDefined(typeof(EquipmentSlot), item.SourceSlotType)
                && !string.IsNullOrWhiteSpace(item.ItemInstanceId);
            var silentEquipment = _equipmentRuntime as IEquipmentSilentMutationRuntime;
            if (reEquip && silentEquipment == null) return false;
            if (reEquip && !string.IsNullOrEmpty(_equipmentRuntime.GetEquippedItem(sourceSlot)))
                return false;

            IReadOnlyList<ItemOriginLotSaveData> lots = item.OriginSchemaVersion == 1
                ? item.OriginLots
                : new[] { new ItemOriginLotSaveData { Kind = 0, Quantity = item.Amount } };
            if (!originInventory.TryStageOriginAddition(item.ItemId, item.ItemInstanceId,
                item.Amount, lots, out var token)) return false;

            if (reEquip)
            {
                if (!silentEquipment.TryEquipSilently(sourceSlot, item.ItemInstanceId))
                {
                    if (!originInventory.TryRollbackOriginAddition(token))
                        Debug.LogError("[CorpseRecoveryManager] Origin addition rollback failed after equip refusal; item remains in inventory.");
                    return false;
                }
            }

            // Inventory and equipment are staged silently before the corpse item leaves the body.
            source.Remove(item);

            var inventoryCommitted = reEquip
                ? originInventory.TryCommitEquippedOriginAddition(token, item.ItemInstanceId, sourceSlot)
                : originInventory.TryCommitOriginAddition(token);
            if (inventoryCommitted)
            {
                if (reEquip) silentEquipment.PublishCommittedEquip(sourceSlot, item.ItemInstanceId);
                return true;
            }
            if (reEquip && !silentEquipment.TryUnequipSilently(sourceSlot, item.ItemInstanceId))
                Debug.LogError("[CorpseRecoveryManager] Silent equipment rollback failed after inventory commit refusal.");
            if (originInventory.TryRollbackOriginAddition(token)) source.Add(item);
            else Debug.LogError("[CorpseRecoveryManager] Origin addition rollback failed after commit refusal; item remains in inventory and was removed from corpse.");
            return false;
        }
    }
}
