namespace CindarsHope.Core.Events
{
    public readonly struct SurvivalEncounterStartedEvent
    {
        public readonly string EncounterId;
        public readonly string ScopeId;
        public readonly string RunId;
        public readonly int CaveLevel;
        public readonly string FirstEnemyInstanceId;

        public SurvivalEncounterStartedEvent(string encounterId, string runId, int caveLevel)
            : this(encounterId, runId, caveLevel, string.Empty)
        {
        }

        public SurvivalEncounterStartedEvent(
            string encounterId,
            string scopeId,
            int caveLevel,
            string firstEnemyInstanceId)
        {
            EncounterId = encounterId ?? string.Empty;
            ScopeId = scopeId ?? string.Empty;
            RunId = ScopeId;
            CaveLevel = caveLevel;
            FirstEnemyInstanceId = firstEnemyInstanceId ?? string.Empty;
        }
    }

    public readonly struct SurvivalEncounterEnemyJoinedEvent
    {
        public readonly string EncounterId;
        public readonly string EnemyInstanceId;

        public SurvivalEncounterEnemyJoinedEvent(string encounterId, string enemyInstanceId)
        {
            EncounterId = encounterId ?? string.Empty;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
        }
    }

    public readonly struct SurvivalEncounterResolvedEvent
    {
        public readonly string EncounterId;
        public readonly string Reason;

        public SurvivalEncounterResolvedEvent(string encounterId, string reason)
        {
            EncounterId = encounterId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }
    }
}
