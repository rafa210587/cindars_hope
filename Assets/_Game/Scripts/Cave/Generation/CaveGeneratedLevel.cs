using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public sealed class CaveGeneratedLevel
    {
        public int CaveLevel;
        public string BiomeId;
        public int Width;
        public int Height;
        public List<CaveRoom> Rooms = new List<CaveRoom>();
        public Vector2Int Entrance;
        public Vector2Int Exit;
        public List<CaveGenerationPoint> EnemySpawnPoints = new List<CaveGenerationPoint>();
        public List<CaveGenerationPoint> ResourceSpawnPoints = new List<CaveGenerationPoint>();
        public HashSet<Vector2Int> WalkableTiles = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> WallTiles = new HashSet<Vector2Int>();
    }
}
