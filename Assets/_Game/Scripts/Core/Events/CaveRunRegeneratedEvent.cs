namespace CindarsHope.Core.Events
{
    public readonly struct CaveRunRegeneratedEvent
    {
        public readonly string CaveRunSeed;
        public readonly string Reason;

        public CaveRunRegeneratedEvent(string caveRunSeed, string reason)
        {
            CaveRunSeed = caveRunSeed ?? string.Empty;
            Reason = reason ?? string.Empty;
        }
    }
}
