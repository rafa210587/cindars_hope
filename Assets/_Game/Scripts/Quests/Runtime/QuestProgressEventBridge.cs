using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Bridges gameplay events into QuestService objective progress updates.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///   Original events: InventoryChangedEvent, ItemCraftedEvent
    ///
    /// WAVE_INTEGRATION_26 — Questline Expansion + Objective Variety
    ///   New events: CropHarvestedEvent (HarvestCrop),
    ///               EconomyTransactionCompletedEvent (SellItem),
    ///               NpcInteractionStartedEvent (TalkToNpc),
    ///               CaveLevelEnteredEvent (ReachCaveDepth),
    ///               EnemyKilledEvent (DefeatEnemy)
    ///
    /// Uses GameEventBus subscribe pattern. Must be initialized once and kept alive
    /// (DontDestroyOnLoad via QuestRuntimeBootstrap).
    ///
    /// Does NOT hold any Unity scene refs.
    /// </summary>
    public class QuestProgressEventBridge
    {
        private readonly QuestService _questService;
        private bool _subscribed;

        public QuestProgressEventBridge(QuestService questService)
        {
            _questService = questService;
        }

        public void Subscribe()
        {
            if (_subscribed) return;

            // WAVE15 — original subscriptions
            GameEventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);

            // WAVE26 — new objective type subscriptions
            GameEventBus.Subscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
            GameEventBus.Subscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);

            _subscribed = true;
            Debug.Log("[QuestProgressEventBridge] Subscribed to gameplay events (WAVE15 + WAVE26 objective variety).");
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;

            // WAVE15
            GameEventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);

            // WAVE26
            GameEventBus.Unsubscribe<CropHarvestedEvent>(OnCropHarvested);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
            GameEventBus.Unsubscribe<NpcInteractionStartedEvent>(OnNpcInteractionStarted);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);

            _subscribed = false;
        }

        // ─── WAVE15 handlers ──────────────────────────────────────────────────────

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (string.IsNullOrEmpty(evt.ItemId)) return;
            _questService?.OnInventoryChanged(evt.ItemId);
        }

        private void OnItemCrafted(ItemCraftedEvent evt)
        {
            if (string.IsNullOrEmpty(evt.ItemId)) return;
            _questService?.OnItemCrafted(evt.ItemId);
        }

        // ─── WAVE26 handlers ──────────────────────────────────────────────────────

        private void OnCropHarvested(CropHarvestedEvent evt)
        {
            _questService?.OnCropHarvested(evt.SeedId, evt.ItemId);
        }

        private void OnEconomyTransactionCompleted(EconomyTransactionCompletedEvent evt)
        {
            if (!evt.WasSuccessful) return;
            _questService?.OnItemSold(evt.ItemId, evt.TransactionType, evt.GoldDelta);
        }

        private void OnNpcInteractionStarted(NpcInteractionStartedEvent evt)
        {
            _questService?.OnNpcTalkedTo(evt.NpcId);
        }

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            _questService?.OnCaveLevelEntered(evt.CaveLevel);
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            _questService?.OnEnemyKilled(evt.EnemyId);
        }
    }
}
