using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Bridges gameplay events (InventoryChangedEvent, ItemCraftedEvent) into
    /// QuestService objective progress updates.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
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
            GameEventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);
            _subscribed = true;
            Debug.Log("[QuestProgressEventBridge] Subscribed to gameplay events.");
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            GameEventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
            _subscribed = false;
        }

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
    }
}
