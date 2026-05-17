using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma planta pronta é colhida.
    /// Usa IDs estáveis para semente e item colhido.
    /// </summary>
    public readonly struct CropHarvestedEvent
    {
        public string SeedId { get; }
        public string ItemId { get; }
        public int Amount { get; }
        public Vector2Int TilePosition { get; }

        public CropHarvestedEvent(string seedId, string itemId, int amount, Vector2Int tilePosition)
        {
            SeedId = seedId;
            ItemId = itemId;
            Amount = amount;
            TilePosition = tilePosition;
        }
    }
}
