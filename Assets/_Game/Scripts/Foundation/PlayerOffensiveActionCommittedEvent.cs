namespace CindarsHope.Foundation
{
    /// <summary>
    /// Published at the irreversible commit point of a player offensive action. Payload remains
    /// primitive so Skills can react without Combat, Equipment or Unity object references.
    /// </summary>
    public readonly struct PlayerOffensiveActionCommittedEvent
    {
        public string ActionId { get; }
        public string SourceKind { get; }

        public PlayerOffensiveActionCommittedEvent(string actionId, string sourceKind)
        {
            ActionId = actionId ?? string.Empty;
            SourceKind = sourceKind ?? string.Empty;
        }
    }
}
