namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_09 — publicado quando um baú de sala de tesouro da caverna é aberto (1x por chestId).
    /// chestId é estável por run/level/posição (cave-stable-run); o estado de aberto persiste no
    /// VisitedLevelSnapshot (OpenedChestIds).
    /// </summary>
    public sealed class TreasureChestOpenedEvent
    {
        public readonly string ChestId;
        public readonly int CaveLevel;

        public TreasureChestOpenedEvent(string chestId, int caveLevel)
        {
            ChestId = chestId;
            CaveLevel = caveLevel;
        }
    }
}
