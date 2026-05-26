using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave.Death
{
    public class CaveDeathResolver
    {
        private readonly CaveDeathPolicy _policy;
        private readonly CaveRunManager _caveRunManager;
        private readonly PlayerManager _playerManager;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly PlayerProgressionManager _progressionManager;

        public Corpse LastCreatedCorpse { get; private set; }

        public CaveDeathResolver(
            CaveDeathPolicy policy,
            CaveRunManager caveRunManager,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            PlayerProgressionManager progressionManager)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _caveRunManager = caveRunManager ?? throw new ArgumentNullException(nameof(caveRunManager));
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _equipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
            _progressionManager = progressionManager;
        }

        public bool IsDeathInCave(string deathSceneName)
        {
            return !string.IsNullOrEmpty(deathSceneName) && deathSceneName.Contains("Cave");
        }

        public void ResolveCaveDeath(string deathSceneName)
        {
            if (!_policy.CreateCorpse)
            {
                return;
            }

            var corpse = CreateCorpse();
            if (corpse == null)
            {
                Debug.LogError("[CaveDeathResolver] Failed to create corpse");
                return;
            }

            LastCreatedCorpse = corpse;

            MoveInventoryToCorpse(corpse);
            MoveEquipmentToCorpse(corpse);
            MoveGoldToCorpse(corpse);
            ResetXp();

            PublishEvents(corpse);
            ReplaceActiveCorpse(corpse);
        }

        private Corpse CreateCorpse()
        {
            var corpse = new Corpse(Guid.NewGuid().ToString("N"));
            corpse.Status = CorpseStatus.Active;
            corpse.RunId = _caveRunManager.State.CaveRunSeed;
            corpse.CaveSeed = _caveRunManager.State.CaveRunSeed;
            corpse.CaveLevel = _caveRunManager.CurrentCaveLevel;
            corpse.SceneName = SceneManager.GetActiveScene().name;
            corpse.Position = GetPlayerPosition();
            corpse.CreatedAtGameDay = GetCurrentGameDay();
            corpse.CreatedAtGameTime = GetCurrentGameTime();
            corpse.SnapshotLayoutHash = GetCurrentLayoutHash();

            return corpse;
        }

        private void MoveInventoryToCorpse(Corpse corpse)
        {
            if (_inventoryManager == null)
            {
                return;
            }

            var items = _inventoryManager.GetAllItems();
            foreach (var item in items)
            {
                var corpseItem = new CorpseItem
                {
                    ItemId = item.ItemId,
                    Amount = item.Amount,
                    ItemInstanceId = item.ItemInstanceId,
                    DurabilityCurrent = item.DurabilityCurrent,
                    DurabilityMax = item.DurabilityMax,
                    IsBroken = item.IsBroken
                };
                corpse.InventoryItems.Add(corpseItem);
            }

            if (_policy.RemoveAllInventoryItems)
            {
                _inventoryManager.Clear();
            }
        }

        private void MoveEquipmentToCorpse(Corpse corpse)
        {
            if (_equipmentManager == null)
            {
                return;
            }

            var equippedItems = _equipmentManager.GetAllEquippedItems();
            foreach (var equippedItem in equippedItems)
            {
                var corpseItem = new CorpseItem
                {
                    ItemId = equippedItem.ItemId,
                    Amount = 1,
                    ItemInstanceId = equippedItem.ItemInstanceId,
                    DurabilityCurrent = equippedItem.DurabilityCurrent,
                    DurabilityMax = equippedItem.DurabilityMax,
                    IsBroken = equippedItem.IsBroken,
                    SourceSlotType = (int)equippedItem.SlotType,
                    SourceSlotIndex = equippedItem.SlotIndex
                };
                corpse.EquipmentItems.Add(corpseItem);
            }

            if (_policy.RemoveAllEquipment)
            {
                _equipmentManager.UnequipAll();
            }
        }

        private void MoveGoldToCorpse(Corpse corpse)
        {
            corpse.GoldAmount = _playerManager.CurrentGold;

            if (_policy.RemoveAllGold)
            {
                _playerManager.SetGold(0);
            }
        }

        private void ResetXp()
        {
            if (!_policy.ResetXpToLevelStart || _progressionManager == null)
            {
                return;
            }

            var currentLevel = _progressionManager.Level;
            var lostXp = _progressionManager.CurrentXp;

            _progressionManager.ResetCurrentLevelXp();

            GameEventBus.Publish(new XpResetToLevelStartEvent
            {
                Level = currentLevel,
                XpLost = lostXp
            });
        }

        private void PublishEvents(Corpse corpse)
        {
            GameEventBus.Publish(new CorpseCreatedEvent
            {
                CorpseId = corpse.CorpseId,
                SceneName = corpse.SceneName
            });

            if (_caveRunManager != null)
            {
                GameEventBus.Publish(new CavePlayerDeathResolvedEvent
                {
                    CorpseId = corpse.CorpseId,
                    CaveRunId = _caveRunManager.State.CaveRunSeed,
                    CaveLevel = _caveRunManager.CurrentCaveLevel
                });

                if (_policy.RedistributeEnemies)
                {
                    GameEventBus.Publish(new CaveEnemiesRedistributionRequestedEvent());
                }
            }
        }

        private void ReplaceActiveCorpse(Corpse newCorpse)
        {
            // TODO: Replace old corpse with new one
            // This will be integrated with CorpseRecoveryManager
        }

        private Vector2 GetPlayerPosition()
        {
            var playerTransform = _playerManager?.gameObject.transform;
            return playerTransform != null ? playerTransform.position : Vector2.zero;
        }

        private int GetCurrentGameDay()
        {
            // TODO: Get from game time manager
            return 1;
        }

        private float GetCurrentGameTime()
        {
            // TODO: Get from game time manager
            return 0f;
        }

        private string GetCurrentLayoutHash()
        {
            // TODO: Get from cave runtime
            return string.Empty;
        }
    }
}
