namespace CindarsHope.Core.Events
{
    public readonly struct SellAllRequestedEvent
    {
        public readonly string SourceId;

        public SellAllRequestedEvent(string sourceId)
        {
            SourceId = sourceId ?? string.Empty;
        }
    }
}
