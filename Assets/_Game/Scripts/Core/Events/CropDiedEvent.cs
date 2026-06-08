using UnityEngine;

namespace CindarsHope.Core.Events
{
    public readonly struct CropDiedEvent
    {
        public string SeedId { get; }
        public Vector2Int TilePosition { get; }
        public int DayNumber { get; }

        public CropDiedEvent(string seedId, Vector2Int tilePosition, int dayNumber)
        {
            SeedId = seedId;
            TilePosition = tilePosition;
            DayNumber = dayNumber;
        }
    }
}
