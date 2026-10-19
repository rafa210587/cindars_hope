namespace CindarsHope.UI.HUD
{
    public enum FeedbackMessagePriority { Low = 0, Normal = 1, Important = 2 }

    public class GameplayFeedbackMessage
    {
        public string Text { get; set; }
        public float Duration { get; set; }
        public FeedbackMessagePriority Priority { get; set; }

        public GameplayFeedbackMessage(string text, float duration = 3f, FeedbackMessagePriority priority = FeedbackMessagePriority.Normal)
        {
            Text = text ?? string.Empty;
            Duration = duration;
            Priority = priority;
        }
    }
}
