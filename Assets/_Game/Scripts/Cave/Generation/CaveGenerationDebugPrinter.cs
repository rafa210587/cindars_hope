using System.Text;
using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public static class CaveGenerationDebugPrinter
    {
        public static string ToAscii(CaveGeneratedLevel generated)
        {
            if (generated == null)
            {
                return "CaveGeneratedLevel: null";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Cave generated. Level: {generated.CaveLevel}");
            builder.AppendLine($"Biome: {generated.BiomeId}");
            builder.AppendLine($"Size: {generated.Width}x{generated.Height}");
            builder.AppendLine($"Rooms: {generated.Rooms.Count}");
            builder.AppendLine($"EnemyPoints: {generated.EnemySpawnPoints.Count}");
            builder.AppendLine($"ResourcePoints: {generated.ResourceSpawnPoints.Count}");

            for (var y = generated.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < generated.Width; x++)
                {
                    builder.Append(GetGlyph(generated, new Vector2Int(x, y)));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static char GetGlyph(CaveGeneratedLevel generated, Vector2Int position)
        {
            if (position == generated.Entrance)
            {
                return 'E';
            }

            if (position == generated.Exit)
            {
                return 'X';
            }

            foreach (var point in generated.EnemySpawnPoints)
            {
                if (point.Position == position)
                {
                    return 'M';
                }
            }

            foreach (var point in generated.ResourceSpawnPoints)
            {
                if (point.Position == position)
                {
                    return 'R';
                }
            }

            if (generated.WalkableTiles.Contains(position))
            {
                return '.';
            }

            return generated.WallTiles.Contains(position) ? '#' : ' ';
        }
    }
}
