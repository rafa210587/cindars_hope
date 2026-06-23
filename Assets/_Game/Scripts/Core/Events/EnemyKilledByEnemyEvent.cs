namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_78 — publicado quando um monstro mata outro (conflito inter-monstro). Dispara o caminho de
    /// corpo com loot/XP reduzidos, NÃO a rota normal de loot do jogador (EnemyKilledEvent permanece
    /// exclusivo de kills causados pelo jogador). Aditivo.
    /// </summary>
    public readonly struct EnemyKilledByEnemyEvent
    {
        public readonly string VictimInstanceId;
        public readonly string KillerInstanceId;
        public readonly int CaveLevel;

        public EnemyKilledByEnemyEvent(string victimInstanceId, string killerInstanceId, int caveLevel)
        {
            VictimInstanceId = victimInstanceId;
            KillerInstanceId = killerInstanceId;
            CaveLevel = caveLevel;
        }
    }
}
