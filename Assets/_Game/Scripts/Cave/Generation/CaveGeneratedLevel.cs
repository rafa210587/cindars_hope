using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public sealed class CaveGeneratedLevel
    {
        public int CaveLevel;
        public string BiomeId;
        // fable_09: banda do perfil de layout aplicado (stone/fungal/ice/fire/ruins/deep/void).
        // Informativo; a mudança estrutural (dimensões/salas) já entra no LayoutHash.
        public string LayoutProfileBandId = string.Empty;
        public int Width;
        public int Height;
        public string LayoutHash;
        public List<CaveRoom> Rooms = new List<CaveRoom>();
        public Vector2Int Entrance;
        public Vector2Int Exit;
        public List<CaveGenerationPoint> EnemySpawnPoints = new List<CaveGenerationPoint>();
        public List<CaveGenerationPoint> ResourceSpawnPoints = new List<CaveGenerationPoint>();
        public HashSet<Vector2Int> WalkableTiles = new HashSet<Vector2Int>();
        public HashSet<Vector2Int> WallTiles = new HashSet<Vector2Int>();

        public void ComputeLayoutHash()
        {
            var sb = new StringBuilder();
            sb.Append($"{CaveLevel}_{Width}_{Height}_{Entrance}_{Exit}");
            sb.Append("_rooms:");
            foreach (var room in Rooms)
            {
                sb.Append($"{room.X}_{room.Y}_{room.Width}_{room.Height};");
            }
            sb.Append("_walkable:");
            var walkableList = new List<Vector2Int>(WalkableTiles);
            walkableList.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            foreach (var tile in walkableList)
            {
                sb.Append($"{tile.x},{tile.y};");
            }
            LayoutHash = ComputeHash(sb.ToString());
        }

        private static string ComputeHash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLower();
            }
        }
    }
}
