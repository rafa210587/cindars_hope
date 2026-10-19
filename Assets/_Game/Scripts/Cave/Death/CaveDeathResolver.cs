using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave.Death
{
    public class CaveDeathResolver
    {
        private readonly CaveDeathPolicy _policy;
        // NAO readonly: o resolver nasce uma unica vez (DontDestroyOnLoad) quando ainda nao ha run
        // de caverna (CaveRunManager null fora da caverna). O dono atualiza a referencia viva no
        // momento da morte via SetCaveRunManager — so entao um corpo de caverna faz sentido.
        private CaveRunManager _caveRunManager;
        private readonly PlayerManager _playerManager;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly PlayerProgressionManager _progressionManager;
        private readonly CorpseRecoveryManager _recoveryManager;
        // fable_66: owner real do dia do jogo (TimeManager, mesmo que publica DayStartedEvent).
        // Opcional p/ compat — ausente degrada para o dia inicial 1 via CaveCorpseStamp (sem corromper).
        private readonly TimeManager _timeManager;

        public Corpse LastCreatedCorpse { get; private set; }

        public CaveDeathResolver(
            CaveDeathPolicy policy,
            CaveRunManager caveRunManager,
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            PlayerProgressionManager progressionManager,
            TimeManager timeManager = null,
            CorpseRecoveryManager recoveryManager = null)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            // caveRunManager pode ser null no boot (fora da caverna). E resolvido ao vivo na morte.
            _caveRunManager = caveRunManager;
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _equipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
            _progressionManager = progressionManager;
            _timeManager = timeManager;
            _recoveryManager = recoveryManager;
        }

        /// <summary>
        /// Atualiza a referencia viva do CaveRunManager (o resolver e DontDestroyOnLoad e nasce
        /// antes de qualquer run de caverna existir). O dono chama isto no momento da morte.
        /// </summary>
        public void SetCaveRunManager(CaveRunManager caveRunManager)
        {
            _caveRunManager = caveRunManager;
        }

        public bool IsDeathInCave(string deathSceneName)
        {
            return !string.IsNullOrEmpty(deathSceneName) && deathSceneName.Contains("Cave");
        }

        public void ResolveCaveDeath(string deathSceneName)
        {
            LastCreatedCorpse = null;
            if (!_policy.CreateCorpse)
            {
                return;
            }

            // Sem run de caverna ativo nao ha como carimbar o corpo (seed/nivel/hash). Acontece se a
            // morte for sinalizada como "na caverna" mas o CaveRunManager ainda nao existir. Sem corpo,
            // mas a tela de morte e o respawn continuam funcionando.
            if (_caveRunManager == null)
            {
                Debug.LogWarning(
                    "[CaveDeathResolver] Morte na caverna sem CaveRunManager ativo; corpo nao criado " +
                    $"(cena='{deathSceneName}').");
                return;
            }

            var corpse = CreateCorpse();
            if (corpse == null)
            {
                Debug.LogError("[CaveDeathResolver] Failed to create corpse");
                return;
            }

            var inventorySnapshot = _inventoryManager.CaptureSaveData();
            MoveEquipmentToCorpse(corpse, inventorySnapshot);
            MoveInventoryToCorpse(corpse, inventorySnapshot);
            MoveGoldToCorpse(corpse);

            PlayerManager.GoldMutationToken goldToken = default;
            if (_policy.RemoveAllGold && !_playerManager.TryStageGoldMutation(-corpse.GoldAmount, out goldToken))
            {
                Debug.LogError("[CaveDeathResolver] Gold mutation is busy; corpse transfer aborted.");
                return;
            }
            LastCreatedCorpse = corpse;
            var replacedCorpseId = _recoveryManager?.SetActiveCorpseDeferred(corpse);

            // The active corpse is complete before any equipment, inventory, gold or XP event.
            if (_policy.RemoveAllEquipment) _equipmentManager?.UnequipAll(false);
            if (_policy.RemoveAllInventoryItems) _inventoryManager.Clear(false);
            if (_policy.RemoveAllGold && !_playerManager.TryCommitGoldMutation(goldToken))
                throw new InvalidOperationException("Corpse gold transfer could not commit.");

            if (_policy.RemoveAllInventoryItems)
            {
                var totals = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var item in inventorySnapshot.Slots)
                {
                    if (item == null || item.Amount <= 0 || string.IsNullOrWhiteSpace(item.ItemId)) continue;
                    totals.TryGetValue(item.ItemId, out var previous);
                    totals[item.ItemId] = previous + item.Amount;
                }
                foreach (var item in totals)
                    GameEventBus.Publish(new InventoryChangedEvent(item.Key, -item.Value, 0));
            }
            if (_policy.RemoveAllEquipment)
                foreach (var item in corpse.EquipmentItems)
                    if (item.SourceSlotType > (int)EquipmentSlot.None
                        && Enum.IsDefined(typeof(EquipmentSlot), item.SourceSlotType))
                        GameEventBus.Publish(new EquipmentSlotChangedEvent(
                            (EquipmentSlot)item.SourceSlotType, null));
            if (replacedCorpseId != null)
                _recoveryManager.PublishReplacement(replacedCorpseId, corpse.CorpseId);
            ResetXp();

            PublishEvents(corpse);
            // fable_66: a substituição do corpse anterior (marcar Replaced + CorpseReplacedEvent) é
            // responsabilidade ÚNICA do CorpseRecoveryManager.SetActiveCorpse, chamado pelo handler de
            // morte (DeathSystemBootstrap/CaveDeathEventHandler) com LastCreatedCorpse. O resolver não
            // reimplementa replace (regra de não duplicação).
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

        private void MoveInventoryToCorpse(Corpse corpse, InventorySaveData inventorySnapshot)
        {
            if (_inventoryManager == null)
            {
                return;
            }

            foreach (var item in inventorySnapshot.Slots)
            {
                if (item == null || item.Amount <= 0) continue;
                var carriedByEquipment = false;
                foreach (var equipped in corpse.EquipmentItems)
                    if (!string.IsNullOrEmpty(item.ItemInstanceId)
                        && equipped.ItemInstanceId == item.ItemInstanceId)
                    { carriedByEquipment = true; break; }
                if (carriedByEquipment) continue;
                var corpseItem = new CorpseItem
                {
                    ItemId = item.ItemId,
                    Amount = item.Amount,
                    ItemInstanceId = item.ItemInstanceId,
                    SourceSlotIndex = item.SlotIndex,
                    OriginSchemaVersion = 1,
                    OriginLots = CopyOriginLots(item.OriginLots)
                };
                if (string.IsNullOrWhiteSpace(item.ItemInstanceId) && _equipmentManager != null)
                {
                    foreach (var equipped in _equipmentManager.GetAllEquippedItems())
                        if (equipped.ItemInstanceId == item.ItemId)
                        {
                            corpseItem.SourceSlotType = (int)equipped.SlotType;
                            break;
                        }
                }
                corpse.InventoryItems.Add(corpseItem);
            }

        }

        private void MoveEquipmentToCorpse(Corpse corpse, InventorySaveData inventorySnapshot)
        {
            if (_equipmentManager == null)
            {
                return;
            }

            var equippedItems = _equipmentManager.GetAllEquippedItems();
            foreach (var equippedItem in equippedItems)
            {
                // Bootstrap and legacy ammo can equip a bare catalog ID while the physical
                // unit lives in an inventory slot (or the ammo stack is already depleted).
                // Only a concrete instance binding can be moved as equipment.
                if (equippedItem.ItemInstanceId == ItemInstanceIdUtility.GetItemId(equippedItem.ItemInstanceId))
                    continue;
                InventorySlotSaveData source = null;
                foreach (var slot in inventorySnapshot.Slots)
                    if (slot != null && !string.IsNullOrEmpty(equippedItem.ItemInstanceId)
                        && slot.ItemInstanceId == equippedItem.ItemInstanceId)
                    { source = slot; break; }
                var corpseItem = new CorpseItem
                {
                    ItemId = source?.ItemId ?? ItemInstanceIdUtility.GetItemId(equippedItem.ItemInstanceId),
                    Amount = 1,
                    ItemInstanceId = equippedItem.ItemInstanceId,
                    DurabilityCurrent = equippedItem.DurabilityCurrent,
                    DurabilityMax = equippedItem.DurabilityMax,
                    IsBroken = equippedItem.IsBroken,
                    SourceSlotType = (int)equippedItem.SlotType,
                    SourceSlotIndex = source?.SlotIndex ?? equippedItem.SlotIndex,
                    OriginSchemaVersion = 1,
                    OriginLots = source != null ? CopyOriginLots(source.OriginLots)
                        : new List<ItemOriginLotSaveData> { new ItemOriginLotSaveData { Kind = (int)ItemOriginKind.Invalid, Quantity = 1 } }
                };
                corpse.EquipmentItems.Add(corpseItem);
            }

        }

        private static List<ItemOriginLotSaveData> CopyOriginLots(IReadOnlyList<ItemOriginLotSaveData> source)
        {
            var result = new List<ItemOriginLotSaveData>();
            if (source == null) return result;
            foreach (var lot in source)
                if (lot != null) result.Add(new ItemOriginLotSaveData
                {
                    Kind = lot.Kind,
                    Quantity = lot.Quantity,
                    UnitPaidBasisGold = lot.UnitPaidBasisGold,
                    UnitResaleCapGold = lot.UnitResaleCapGold
                });
            return result;
        }

        private void MoveGoldToCorpse(Corpse corpse)
        {
            corpse.GoldAmount = _playerManager.CurrentGold;

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

        private Vector2 GetPlayerPosition()
        {
            var playerTransform = PlayerController.ActiveInstance != null ? PlayerController.ActiveInstance.transform : null;
            return playerTransform != null ? playerTransform.position : Vector2.zero;
        }

        // fable_66: dia real do owner de tempo (TimeManager). Ausente ⇒ floor 1 (nunca grava 0).
        private int GetCurrentGameDay()
        {
            var ownerDay = _timeManager != null ? _timeManager.CurrentDay : 1;
            return CaveCorpseStamp.ResolveGameDay(ownerDay);
        }

        // fable_66: o projeto ainda não tem relógio intra-dia canônico (TimeManager só expõe o dia);
        // o owner repassa 0 até existir. Normalizado/saneado pelo helper. Sem stub eterno: o ponto de
        // troca para o futuro relógio é único (aqui).
        private float GetCurrentGameTime()
        {
            return CaveCorpseStamp.ResolveGameTime(0f);
        }

        // fable_66: hash de layout determinístico (ADR-0005) derivado das seeds reais da run + nível,
        // idêntico em revisita. Sem run ⇒ vazio. Substitui o stub eterno por fonte real e estável.
        private string GetCurrentLayoutHash()
        {
            return CaveCorpseStamp.ResolveLayoutHash(
                _caveRunManager.CaveWorldSeed,
                _caveRunManager.CaveRunSeed,
                _caveRunManager.CurrentCaveLevel);
        }
    }
}
