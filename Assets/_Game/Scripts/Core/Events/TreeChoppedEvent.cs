using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma árvore é cortada em um nível de corte.
    /// WoodItemId permite adicionar madeira sem acoplar com ItemDataSO.
    /// </summary>
    public readonly struct TreeChoppedEvent
    {
        public string TreeId { get; }
        public string WoodItemId { get; }
        public int WoodAmount { get; }
        public int NewChopLevel { get; }
        public Vector2Int TilePosition { get; }

        public TreeChoppedEvent(string treeId, string woodItemId, int woodAmount, int newChopLevel, Vector2Int tilePosition)
        {
            TreeId = treeId;
            WoodItemId = woodItemId;
            WoodAmount = woodAmount;
            NewChopLevel = newChopLevel;
            TilePosition = tilePosition;
        }
    }
}
