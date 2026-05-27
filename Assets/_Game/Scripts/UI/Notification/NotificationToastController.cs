using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.Notification
{
    /// <summary>
    /// Queues and displays short notification toasts. Subscribes to
    /// PlayerActionFeedbackEvent and NotificationToastRequestedEvent.
    /// Uses IMGUI as a functional implementation; replace with Canvas prefab for polish.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NotificationToastController : MonoBehaviour
    {
        [SerializeField] private float _toastDurationSeconds = 2.5f;
        [SerializeField] private int _maxVisible = 4;

        private readonly Queue<ToastEntry> _queue = new Queue<ToastEntry>();
        private readonly List<ToastEntry> _active = new List<ToastEntry>();

        private struct ToastEntry
        {
            public string Message;
            public float ExpiresAt;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerActionFeedbackEvent>(OnFeedback);
            GameEventBus.Subscribe<NotificationToastRequestedEvent>(OnToastRequested);
            GameEventBus.Subscribe<ItemCraftedEvent>(OnItemCrafted);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnTransaction);
            GameEventBus.Subscribe<CaveCheckpointUnlockedEvent>(OnCheckpointUnlocked);
            GameEventBus.Subscribe<CaveBossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerActionFeedbackEvent>(OnFeedback);
            GameEventBus.Unsubscribe<NotificationToastRequestedEvent>(OnToastRequested);
            GameEventBus.Unsubscribe<ItemCraftedEvent>(OnItemCrafted);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnTransaction);
            GameEventBus.Unsubscribe<CaveCheckpointUnlockedEvent>(OnCheckpointUnlocked);
            GameEventBus.Unsubscribe<CaveBossDefeatedEvent>(OnBossDefeated);
        }

        private void Update()
        {
            var now = Time.unscaledTime;

            // Promote from queue to active
            while (_active.Count < _maxVisible && _queue.Count > 0)
            {
                var entry = _queue.Dequeue();
                entry.ExpiresAt = now + _toastDurationSeconds;
                _active.Add(entry);
            }

            // Expire old toasts
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                if (now >= _active[i].ExpiresAt)
                {
                    _active.RemoveAt(i);
                }
            }
        }

        private void OnGUI()
        {
            if (_active.Count == 0) return;

            var baseY = Screen.height - 60f;
            var width = 320f;
            var height = 28f;
            var x = Screen.width * 0.5f - width * 0.5f;

            for (var i = 0; i < _active.Count; i++)
            {
                var rect = new Rect(x, baseY - i * (height + 4f), width, height);
                GUI.Box(rect, _active[i].Message);
            }
        }

        public void ShowToast(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            _queue.Enqueue(new ToastEntry { Message = message });
        }

        private void OnFeedback(PlayerActionFeedbackEvent evt)
        {
            ShowToast(evt.Message);
        }

        private void OnToastRequested(NotificationToastRequestedEvent evt)
        {
            ShowToast(evt.Message);
        }

        private void OnItemCrafted(ItemCraftedEvent evt)
        {
            ShowToast($"Crafted: {evt.RecipeId}");
        }

        private void OnTransaction(EconomyTransactionCompletedEvent evt)
        {
            ShowToast($"Transaction: {evt.ItemId} x{evt.Amount}");
        }

        private void OnCheckpointUnlocked(CaveCheckpointUnlockedEvent evt)
        {
            ShowToast($"Checkpoint unlocked: Level {evt.CaveLevel}");
        }

        private void OnBossDefeated(CaveBossDefeatedEvent evt)
        {
            ShowToast($"Boss defeated: {evt.BossGateId}");
        }
    }
}
