using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Core.Time;
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
        // NAO readonly: o resolver nasce uma unica vez (DontDestroyOnLoad) quando ainda nao ha run
        // de caverna (CaveRunManager null fora da caverna). O dono atualiza a referencia viva no
        // momento da morte via SetCaveRunManager — so entao um corpo de caverna faz sentido.
        private CaveRunManager _caveRunManager;
        private readonly PlayerManager _playerManager;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly PlayerProgressionManager _progressionManager;
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
            TimeManager timeManager = null)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            // caveRunManager pode ser null no boot (fora da caverna). E resolvido ao vivo na morte.
            _caveRunManager = caveRunManager;
            _playerManager = playerManager ?? throw new ArgumentNullException(nameof(playerManager));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _equipmentManager = equipmentManager ?? throw new ArgumentNullException(nameof(equipmentManager));
            _progressionManager = progressionManager;
            _timeManager = timeManager;
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

            LastCreatedCorpse = corpse;

            MoveInventoryToCorpse(corpse);
            MoveEquipmentToCorpse(corpse);
            MoveGoldToCorpse(corpse);
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

        private Vector2 GetPlayerPosition()
        {
            var playerTransform = _playerManager?.gameObject.transform;
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
