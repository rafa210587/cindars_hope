namespace CindarsHope.Core.Events
{
    public readonly struct PlayerActionFeedbackEvent
    {
        public readonly string Message;
        public readonly float DurationSeconds;

        public PlayerActionFeedbackEvent(string message, float durationSeconds = 2f)
        {
            Message = message ?? string.Empty;
            DurationSeconds = durationSeconds <= 0f ? 2f : durationSeconds;
        }
    }
}
