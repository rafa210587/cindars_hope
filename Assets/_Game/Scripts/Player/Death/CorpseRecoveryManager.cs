using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Player;
using System;
using UnityEngine;

namespace CindarsHope.Player.Death
{
    public class CorpseRecoveryManager
    {
        private Corpse _activeCorpse;
        private readonly PlayerManager _playerManager;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;

        public Corpse ActiveCorpse => _activeCorpse;
        public bool HasActiveCorpse => _activeCorpse != null && _activeCorpse.Status == CorpseStatus.Active;

        public CorpseRecoveryManager(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager)
        {
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _equipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
        }

        public void SetActiveCorpse(Corpse corpse)
        {
            if (_activeCorpse != null && _activeCorpse.CorpseId != corpse.CorpseId)
            {
                // Replace old corpse
                var oldCorpseId = _activeCorpse.CorpseId;
                _activeCorpse.Status = CorpseStatus.Replaced;
                _activeCorpse.ReplacedByCorpseId = corpse.CorpseId;

                GameEventBus.Publish(new CorpseReplacedEvent
                {
                    OldCorpseId = oldCorpseId,
                    NewCorpseId = corpse.CorpseId
                });
            }

            _activeCorpse = corpse;
        }

        public void RecoverCorpse()
        {
            if (!HasActiveCorpse)
            {
                Debug.LogWarning("[CorpseRecoveryManager] No active corpse to recover");
                return;
            }

            bool fullyRecovered = RecoverGold() && RecoverItems();

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

        private bool RecoverGold()
        {
            if (_activeCorpse.GoldAmount <= 0)
            {
                return true;
            }

            _playerManager.AddGold(_activeCorpse.GoldAmount);
            _activeCorpse.GoldAmount = 0;
            return true;
        }

        private bool RecoverItems()
        {
            bool allRecovered = true;

            // Recover inventory items
            var inventoryItemsToRemove = new System.Collections.Generic.List<CorpseItem>();
            foreach (var corpseItem in _activeCorpse.InventoryItems)
            {
                if (_inventoryManager.TryAddItem(corpseItem.ItemId, corpseItem.Amount).Success)
                {
                    inventoryItemsToRemove.Add(corpseItem);
                }
                else
                {
                    allRecovered = false;
                }
            }

            foreach (var item in inventoryItemsToRemove)
            {
                _activeCorpse.InventoryItems.Remove(item);
            }

            // Recover equipment items
            var equipmentItemsToRemove = new System.Collections.Generic.List<CorpseItem>();
            foreach (var corpseItem in _activeCorpse.EquipmentItems)
            {
                if (corpseItem.SourceSlotType >= 0)
                {
                    _equipmentManager.EquipItem((EquipmentSlot)corpseItem.SourceSlotType, corpseItem.ItemInstanceId);
                    equipmentItemsToRemove.Add(corpseItem);
                }
                else if (_inventoryManager.TryAddItem(corpseItem.ItemId, 1).Success)
                {
                    equipmentItemsToRemove.Add(corpseItem);
                }
                else
                {
                    allRecovered = false;
                }
            }

            foreach (var item in equipmentItemsToRemove)
            {
                _activeCorpse.EquipmentItems.Remove(item);
            }

            return allRecovered;
        }
    }
}
