namespace CindarsHope.Core.Events
{
    public readonly struct InteractionPromptChangedEvent
    {
        public readonly bool HasCandidate;
        public readonly string Prompt;

        public InteractionPromptChangedEvent(bool hasCandidate, string prompt)
        {
            HasCandidate = hasCandidate;
            Prompt = prompt ?? string.Empty;
        }
    }
}
