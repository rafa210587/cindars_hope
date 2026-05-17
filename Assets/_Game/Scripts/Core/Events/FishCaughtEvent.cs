using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma pesca é concluída com sucesso.
    /// MVP: normalmente FishItemId = item_fish_common e Amount = 1.
    /// </summary>
    public readonly struct FishCaughtEvent
    {
        public string FishItemId { get; }
        public int Amount { get; }
        public Vector2Int FishingSpotPosition { get; }

        public FishCaughtEvent(string fishItemId, int amount, Vector2Int fishingSpotPosition)
        {
            FishItemId = fishItemId;
            Amount = amount;
            FishingSpotPosition = fishingSpotPosition;
        }
    }
}
