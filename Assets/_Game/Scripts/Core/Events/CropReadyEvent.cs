using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma planta completa os dias de crescimento e fica pronta para colheita.
    /// </summary>
    public readonly struct CropReadyEvent
    {
        public string SeedId { get; }
        public Vector2Int TilePosition { get; }
        public int DaysGrown { get; }

        public CropReadyEvent(string seedId, Vector2Int tilePosition, int daysGrown)
        {
            SeedId = seedId;
            TilePosition = tilePosition;
            DaysGrown = daysGrown;
        }
    }
}
