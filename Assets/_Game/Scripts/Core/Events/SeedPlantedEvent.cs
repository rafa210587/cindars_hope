using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma semente é plantada em um canteiro.
    /// SeedId é persistível; TilePosition identifica a célula/canteiro no mapa.
    /// </summary>
    public readonly struct SeedPlantedEvent
    {
        public string SeedId { get; }
        public Vector2Int TilePosition { get; }
        public int DayNumber { get; }

        public SeedPlantedEvent(string seedId, Vector2Int tilePosition, int dayNumber)
        {
            SeedId = seedId;
            TilePosition = tilePosition;
            DayNumber = dayNumber;
        }
    }
}
