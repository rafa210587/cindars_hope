namespace CindarsHope.Core.Events
{
    public readonly struct CaveRunIdentityStartedEvent
    {
        public readonly string RunId;
        public readonly string CaveRunSeed;
        public readonly int CaveLevel;
        public readonly string Reason;

        public CaveRunIdentityStartedEvent(string runId, string caveRunSeed, int caveLevel, string reason)
        {
            RunId = runId ?? string.Empty;
            CaveRunSeed = caveRunSeed ?? string.Empty;
            CaveLevel = caveLevel;
            Reason = reason ?? string.Empty;
        }
    }

    public readonly struct CaveRunIdentityEndedEvent
    {
        public readonly string RunId;
        public readonly int CaveLevel;
        public readonly string Reason;

        public CaveRunIdentityEndedEvent(string runId, int caveLevel, string reason)
        {
            RunId = runId ?? string.Empty;
            CaveLevel = caveLevel;
            Reason = reason ?? string.Empty;
        }
    }

    public readonly struct CaveRunLevelChangedEvent
    {
        public readonly string RunId;
        public readonly int PreviousLevel;
        public readonly int CurrentLevel;

        public CaveRunLevelChangedEvent(string runId, int previousLevel, int currentLevel)
        {
            RunId = runId ?? string.Empty;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }
    }
}
