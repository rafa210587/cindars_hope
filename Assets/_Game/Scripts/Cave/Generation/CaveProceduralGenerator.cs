using System;
using System.Collections.Generic;
using CindarsHope.Cave.Data;
using UnityEngine;

namespace CindarsHope.Cave.Generation
{
    public sealed class CaveProceduralGenerator
    {
        private const int PlacementAttemptsMultiplier = 8;

        public CaveGeneratedLevel Generate(
            CaveGenerationConfigSO config,
            int caveLevel,
            string caveWorldSeed,
            string caveRunSeed,
            string biomeId)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var width = Mathf.Max(8, config.TargetWidth);
            var height = Mathf.Max(8, config.TargetHeight);
            var random = new System.Random(BuildSeed(caveWorldSeed, caveRunSeed, caveLevel));
            var generated = new CaveGeneratedLevel
            {
                CaveLevel = Mathf.Max(1, caveLevel),
                BiomeId = biomeId ?? string.Empty,
                Width = width,
                Height = height
            };

            GenerateRooms(config, random, generated);
            ConnectRooms(config, random, generated);
            PlaceEntranceAndExit(generated);
            PlaceGenerationPoints(config, random, generated);
            BuildWalls(generated);

            return generated;
        }

        private static void GenerateRooms(CaveGenerationConfigSO config, System.Random random, CaveGeneratedLevel generated)
        {
            var targetRooms = random.Next(config.MinRooms, config.MaxRooms + 1);
            var attempts = Mathf.Max(targetRooms * PlacementAttemptsMultiplier, targetRooms);

            for (var attempt = 0; attempt < attempts && generated.Rooms.Count < targetRooms; attempt++)
            {
                var roomWidth = random.Next(config.MinRoomWidth, config.MaxRoomWidth + 1);
                var roomHeight = random.Next(config.MinRoomHeight, config.MaxRoomHeight + 1);
                var maxX = Mathf.Max(2, generated.Width - roomWidth - 2);
                var maxY = Mathf.Max(2, generated.Height - roomHeight - 2);
                var room = new CaveRoom(
                    random.Next(1, maxX),
                    random.Next(1, maxY),
                    roomWidth,
                    roomHeight);

                if (OverlapsAny(room, generated.Rooms))
                {
                    continue;
                }

                generated.Rooms.Add(room);
                CarveRoom(room, generated.WalkableTiles);
            }

            if (generated.Rooms.Count == 0)
            {
                var fallback = new CaveRoom(2, 2, Mathf.Min(10, generated.Width - 4), Mathf.Min(8, generated.Height - 4));
                generated.Rooms.Add(fallback);
                CarveRoom(fallback, generated.WalkableTiles);
            }
        }

        private static void ConnectRooms(CaveGenerationConfigSO config, System.Random random, CaveGeneratedLevel generated)
        {
            generated.Rooms.Sort((a, b) => a.Center.x == b.Center.x ? a.Center.y.CompareTo(b.Center.y) : a.Center.x.CompareTo(b.Center.x));

            for (var i = 1; i < generated.Rooms.Count; i++)
            {
                CarveCorridor(generated.Rooms[i - 1].Center, generated.Rooms[i].Center, generated.WalkableTiles, random.Next(0, 2) == 0);
            }

            for (var i = 0; i < generated.Rooms.Count; i++)
            {
                for (var j = i + 2; j < generated.Rooms.Count; j++)
                {
                    if (random.Next(0, 100) >= config.ExtraConnectionChancePercent)
                    {
                        continue;
                    }

                    CarveCorridor(generated.Rooms[i].Center, generated.Rooms[j].Center, generated.WalkableTiles, random.Next(0, 2) == 0);
                }
            }
        }

        private static void PlaceEntranceAndExit(CaveGeneratedLevel generated)
        {
            var firstRoom = generated.Rooms[0];
            var start = firstRoom.Center;
            var exit = start;
            var bestDistance = -1f;

            foreach (var room in generated.Rooms)
            {
                var distance = Vector2Int.Distance(start, room.Center);
                if (distance <= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                exit = room.Center;
            }

            generated.Entrance = start;
            generated.Exit = exit;
            generated.WalkableTiles.Add(generated.Entrance);
            generated.WalkableTiles.Add(generated.Exit);
        }

        private static void PlaceGenerationPoints(CaveGenerationConfigSO config, System.Random random, CaveGeneratedLevel generated)
        {
            var candidates = new List<Vector2Int>(generated.WalkableTiles);
            candidates.Remove(generated.Entrance);
            candidates.Remove(generated.Exit);
            Shuffle(candidates, random);

            AddPoints(candidates, generated.EnemySpawnPoints, CaveGenerationPointType.Enemy, config.EnemyPointCount);
            AddPoints(candidates, generated.ResourceSpawnPoints, CaveGenerationPointType.Resource, config.ResourcePointCount);
        }

        private static void AddPoints(List<Vector2Int> candidates, List<CaveGenerationPoint> points, CaveGenerationPointType pointType, int count)
        {
            for (var i = 0; i < count && candidates.Count > 0; i++)
            {
                var position = candidates[0];
                candidates.RemoveAt(0);
                points.Add(new CaveGenerationPoint(pointType, position));
            }
        }

        private static void BuildWalls(CaveGeneratedLevel generated)
        {
            generated.WallTiles.Clear();
            foreach (var tile in generated.WalkableTiles)
            {
                for (var x = tile.x - 1; x <= tile.x + 1; x++)
                {
                    for (var y = tile.y - 1; y <= tile.y + 1; y++)
                    {
                        var position = new Vector2Int(x, y);
                        if (position.x < 0 || position.x >= generated.Width || position.y < 0 || position.y >= generated.Height)
                        {
                            continue;
                        }

                        if (!generated.WalkableTiles.Contains(position))
                        {
                            generated.WallTiles.Add(position);
                        }
                    }
                }
            }
        }

        private static bool OverlapsAny(CaveRoom room, List<CaveRoom> rooms)
        {
            foreach (var existing in rooms)
            {
                if (room.Overlaps(existing, 1))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CarveRoom(CaveRoom room, HashSet<Vector2Int> walkableTiles)
        {
            for (var x = room.MinX; x <= room.MaxX; x++)
            {
                for (var y = room.MinY; y <= room.MaxY; y++)
                {
                    walkableTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        private static void CarveCorridor(Vector2Int from, Vector2Int to, HashSet<Vector2Int> walkableTiles, bool horizontalFirst)
        {
            if (horizontalFirst)
            {
                CarveHorizontal(from.x, to.x, from.y, walkableTiles);
                CarveVertical(from.y, to.y, to.x, walkableTiles);
            }
            else
            {
                CarveVertical(from.y, to.y, from.x, walkableTiles);
                CarveHorizontal(from.x, to.x, to.y, walkableTiles);
            }
        }

        private static void CarveHorizontal(int startX, int endX, int y, HashSet<Vector2Int> walkableTiles)
        {
            var min = Mathf.Min(startX, endX);
            var max = Mathf.Max(startX, endX);
            for (var x = min; x <= max; x++)
            {
                walkableTiles.Add(new Vector2Int(x, y));
            }
        }

        private static void CarveVertical(int startY, int endY, int x, HashSet<Vector2Int> walkableTiles)
        {
            var min = Mathf.Min(startY, endY);
            var max = Mathf.Max(startY, endY);
            for (var y = min; y <= max; y++)
            {
                walkableTiles.Add(new Vector2Int(x, y));
            }
        }

        private static void Shuffle<T>(IList<T> values, System.Random random)
        {
            for (var i = values.Count - 1; i > 0; i--)
            {
                var j = random.Next(0, i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }

        private static int BuildSeed(string worldSeed, string runSeed, int caveLevel)
        {
            unchecked
            {
                var hash = 2166136261u;
                Append(ref hash, worldSeed);
                Append(ref hash, "|");
                Append(ref hash, runSeed);
                Append(ref hash, "|");
                Append(ref hash, caveLevel.ToString());
                return (int)hash;
            }
        }

        private static void Append(ref uint hash, string value)
        {
            value ??= string.Empty;
            for (var i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }
        }
    }
}
