namespace CindarsHope.Core.Events
{
    public readonly struct HudFeedbackUpdatedEvent
    {
        public readonly string Text;
        public readonly float Duration;
        public readonly int Priority;

        public HudFeedbackUpdatedEvent(string text, float duration, int priority = 1)
        {
            Text = text ?? string.Empty;
            Duration = duration;
            Priority = priority;
        }
    }

    public readonly struct HudVisibilityChangedEvent
    {
        public readonly bool IsVisible;
        public HudVisibilityChangedEvent(bool isVisible) { IsVisible = isVisible; }
    }
}
