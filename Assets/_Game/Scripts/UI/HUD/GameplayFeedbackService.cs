using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public sealed class GameplayFeedbackService : MonoBehaviour
    {
        private readonly Queue<GameplayFeedbackMessage> _queue = new Queue<GameplayFeedbackMessage>();
        private GameplayFeedbackMessage _active;
        private float _activeUntil;

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerActionFeedbackEvent>(OnPlayerFeedback);
            GameEventBus.Subscribe<GameSavedEvent>(OnGameSaved);
            GameEventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            GameEventBus.Subscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Subscribe<QuestRewardClaimedEvent>(OnQuestRewardClaimed);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Subscribe<NotificationToastRequestedEvent>(OnToastRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerActionFeedbackEvent>(OnPlayerFeedback);
            GameEventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
            GameEventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            GameEventBus.Unsubscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Unsubscribe<QuestRewardClaimedEvent>(OnQuestRewardClaimed);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Unsubscribe<NotificationToastRequestedEvent>(OnToastRequested);
        }

        private void Update()
        {
            if (_active != null && Time.time >= _activeUntil)
            {
                _active = null;
                AdvanceQueue();
            }
        }

        public GameplayFeedbackMessage CurrentMessage => _active;

        private void Enqueue(string text, float duration, FeedbackMessagePriority priority = FeedbackMessagePriority.Normal)
        {
            var msg = new GameplayFeedbackMessage(text, duration, priority);
            if (_active == null)
            {
                Activate(msg);
                return;
            }
            if (priority == FeedbackMessagePriority.Important)
            {
                Activate(msg);
                return;
            }
            _queue.Enqueue(msg);
        }

        private void Activate(GameplayFeedbackMessage msg)
        {
            _active = msg;
            _activeUntil = Time.time + msg.Duration;
            GameEventBus.Publish(new HudFeedbackUpdatedEvent(msg.Text, msg.Duration, (int)msg.Priority));
        }

        private void AdvanceQueue()
        {
            if (_queue.Count == 0) return;
            Activate(_queue.Dequeue());
        }

        private void OnPlayerFeedback(PlayerActionFeedbackEvent evt) =>
            Enqueue(evt.Message, evt.DurationSeconds, FeedbackMessagePriority.Normal);

        private void OnGameSaved(GameSavedEvent evt) =>
            Enqueue(evt.WasSuccessful ? "Jogo salvo." : $"Falha: {evt.Message}", 3f, FeedbackMessagePriority.Normal);

        private void OnGameLoaded(GameLoadedEvent evt) =>
            Enqueue(evt.WasSuccessful ? "Jogo carregado." : $"Falha ao carregar: {evt.Message}", 3f, FeedbackMessagePriority.Normal);

        private void OnQuestAccepted(QuestAcceptedEvent evt) =>
            Enqueue($"Quest aceita: {evt.QuestId}", 4f, FeedbackMessagePriority.Important);

        private void OnQuestCompleted(QuestCompletedEvent evt) =>
            Enqueue($"Quest concluída: {evt.QuestId}", 4f, FeedbackMessagePriority.Important);

        private void OnQuestRewardClaimed(QuestRewardClaimedEvent evt) =>
            Enqueue(evt.GoldGiven > 0 ? $"Recompensa: {evt.GoldGiven}g" : $"Recompensa recebida.", 3f, FeedbackMessagePriority.Normal);

        private void OnEnemyKilled(EnemyKilledEvent evt) =>
            Enqueue(string.IsNullOrEmpty(evt.DropItemId)
                ? $"Inimigo derrotado."
                : $"Loot: {evt.DropItemId} x{evt.DropAmount}", 2f, FeedbackMessagePriority.Low);

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt) =>
            Enqueue($"Caverna — nível {evt.CaveLevel}", 3f, FeedbackMessagePriority.Normal);

        private void OnCaveExited(CaveExitedEvent evt) =>
            Enqueue("Retornando à superfície...", 3f, FeedbackMessagePriority.Normal);

        private void OnToastRequested(NotificationToastRequestedEvent evt) =>
            Enqueue(evt.Message, 3f, FeedbackMessagePriority.Normal);
    }
}
