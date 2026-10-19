using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class FeedbackToastHudView : MonoBehaviour
    {
        private GameplayFeedbackService _feedbackService;

        public void Initialize(GameplayFeedbackService feedbackService) => _feedbackService = feedbackService;

        private void OnEnable() => GameEventBus.Subscribe<HudFeedbackUpdatedEvent>(OnFeedbackUpdated);
        private void OnDisable() => GameEventBus.Unsubscribe<HudFeedbackUpdatedEvent>(OnFeedbackUpdated);

        private void OnFeedbackUpdated(HudFeedbackUpdatedEvent evt)
        {
            // Headless: Text ref wired by human in Unity Editor.
            // evt.Text, evt.Duration, evt.Priority drive the toast display.
            Debug.Log($"[FeedbackToastHudView] Toast: '{evt.Text}' ({evt.Duration}s)");
        }
    }
}
