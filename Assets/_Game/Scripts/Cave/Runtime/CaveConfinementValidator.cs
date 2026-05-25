using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveConfinementValidator
    {
        private const float MinDistanceFromWall = 0.5f;

        public static bool IsPositionSafe(Vector3 position, CaveGeneratedLevel generatedLevel, float radius = 0.5f)
        {
            if (generatedLevel == null || generatedLevel.Rooms == null || generatedLevel.Rooms.Count == 0)
            {
                return false;
            }

            // Check if position is in a valid room
            bool inValidRoom = false;
            foreach (var room in generatedLevel.Rooms)
            {
                if (IsPositionInRoom(position, room))
                {
                    inValidRoom = true;
                    break;
                }
            }

            if (!inValidRoom)
            {
                return false;
            }

            // Check distance from walls
            bool tooCloseToWall = CheckWallDistance(position, generatedLevel, radius);
            return !tooCloseToWall;
        }

        public static bool IsPositionInRoom(Vector3 position, CaveRoom room)
        {
            // Convert world position to grid coordinates
            int gridX = Mathf.RoundToInt(position.x);
            int gridY = Mathf.RoundToInt(position.y);

            // Check if position is within room bounds
            return gridX >= room.X && gridX < room.X + room.Width
                && gridY >= room.Y && gridY < room.Y + room.Height;
        }

        public static bool CheckWallDistance(Vector3 position, CaveGeneratedLevel generatedLevel, float minDistance = MinDistanceFromWall)
        {
            if (generatedLevel == null)
            {
                return true;
            }

            // Simple check: verify position is not at the edge of a room
            foreach (var room in generatedLevel.Rooms)
            {
                if (IsPositionInRoom(position, room))
                {
                    int gridX = Mathf.RoundToInt(position.x);
                    int gridY = Mathf.RoundToInt(position.y);

                    // Check distance from room edges
                    float distFromLeft = gridX - room.X;
                    float distFromRight = room.X + room.Width - gridX;
                    float distFromBottom = gridY - room.Y;
                    float distFromTop = room.Y + room.Height - gridY;

                    float minDistToEdge = Mathf.Min(distFromLeft, distFromRight, distFromBottom, distFromTop);

                    if (minDistToEdge < minDistance)
                    {
                        return true; // Too close to wall
                    }
                }
            }

            return false; // Safe distance from all walls
        }

        public static Vector3 FindSafeSpawnPosition(
            CaveGeneratedLevel generatedLevel,
            System.Random random,
            float minDistance = MinDistanceFromWall)
        {
            if (generatedLevel == null || generatedLevel.Rooms == null || generatedLevel.Rooms.Count == 0)
            {
                return Vector3.zero;
            }

            // Try up to 10 times to find a safe position
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var room = generatedLevel.Rooms[random.Next(generatedLevel.Rooms.Count)];

                // Get a random position within the room interior
                int insetX = Mathf.RoundToInt(minDistance) + 1;
                int insetY = Mathf.RoundToInt(minDistance) + 1;

                int minX = room.X + insetX;
                int maxX = room.X + room.Width - insetX;
                int minY = room.Y + insetY;
                int maxY = room.Y + room.Height - insetY;

                if (minX >= maxX || minY >= maxY)
                {
                    continue; // Room too small
                }

                int spawnX = random.Next(minX, maxX);
                int spawnY = random.Next(minY, maxY);

                var position = new Vector3(spawnX, spawnY, 0);

                if (IsPositionSafe(position, generatedLevel, minDistance))
                {
                    return position;
                }
            }

            // Fallback: return center of first room
            if (generatedLevel.Rooms.Count > 0)
            {
                var room = generatedLevel.Rooms[0];
                return new Vector3(room.X + room.Width / 2f, room.Y + room.Height / 2f, 0);
            }

            return Vector3.zero;
        }
    }
}
