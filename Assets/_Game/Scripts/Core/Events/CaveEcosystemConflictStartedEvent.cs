namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_78 — publicado na materialização quando um nível entra com conflito inter-monstro ativo.
    /// HUD/SFX/telemetria assinam (toast obrigatório de feedback). Aditivo; não substitui eventos existentes.
    /// </summary>
    public readonly struct CaveEcosystemConflictStartedEvent
    {
        public readonly int CaveLevel;
        public readonly string FactionAEnemyId;
        public readonly string FactionBEnemyId;

        public CaveEcosystemConflictStartedEvent(int caveLevel, string factionAEnemyId, string factionBEnemyId)
        {
            CaveLevel = caveLevel;
            FactionAEnemyId = factionAEnemyId;
            FactionBEnemyId = factionBEnemyId;
        }
    }
}
