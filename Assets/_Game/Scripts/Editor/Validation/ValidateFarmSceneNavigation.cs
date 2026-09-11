using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.Farm.Scene;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateFarmSceneNavigation
    {
        private const float CellSize = 1f;
        private static readonly Vector2[] RequiredLandmarks =
        {
            new Vector2(FarmLevel1LayoutContract.HouseStartX, FarmLevel1LayoutContract.HouseMinY - 1f),
            new Vector2(FarmLevel1LayoutContract.FonteAnchorX, FarmLevel1LayoutContract.FonteAnchorY),
            new Vector2(FarmLevel1LayoutContract.CraftingYardMinX + FarmLevel1LayoutContract.CraftingYardWidth / 2f, FarmLevel1LayoutContract.CraftingYardMinY - 1f),
            new Vector2(FarmLevel1LayoutContract.EvolutionBoardX, FarmLevel1LayoutContract.EvolutionBoardY),
            new Vector2(FarmLevel1LayoutContract.SellPointStartX, FarmLevel1LayoutContract.SellPointStartY),
            new Vector2(FarmLevel1LayoutContract.SpawnFromCaveX, FarmLevel1LayoutContract.SpawnFromCaveY),
            new Vector2(FarmLevel1LayoutContract.SpawnFromTownX, FarmLevel1LayoutContract.SpawnFromTownY),
            FarmSceneCompositionContract.BridgePassageProbe,
            new Vector2(FarmLevel1LayoutContract.AnimalBuildingsMinX + 3f, FarmLevel1LayoutContract.AnimalBuildingsMinY - 1f),
            new Vector2(FarmLevel1LayoutContract.AnimalBuildingsMinX + 10f, FarmLevel1LayoutContract.AnimalBuildingsMinY - 1f)
        };

        [MenuItem("CindarsHope/Validar Navegacao FarmScene")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateConfiguration();
            var reachable = CountReachableLandmarks();
            if (reachable != RequiredLandmarks.Length)
            {
                errors++;
                Debug.LogError("ValidateFarmSceneNavigation: Reachable required landmarks: " + reachable + "/" + RequiredLandmarks.Length);
            }
            else
            {
                Debug.Log("ValidateFarmSceneNavigation: Reachable required landmarks: " + reachable + "/" + RequiredLandmarks.Length);
            }

            Debug.Log("ValidateFarmSceneNavigation: " + errors + " orphan collider(s)");
        }

        public static int ValidateConfiguration()
        {
            var errors = 0;
            var collisionRoot = FindCollisionRoot();
            if (collisionRoot == null)
            {
                Debug.LogError("ValidateFarmSceneNavigation: FarmSpatialCollision root is missing");
                return 1;
            }

            var colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider.isTrigger || !collider.transform.IsChildOf(collisionRoot)) continue;
                if (!collider.gameObject.name.StartsWith("Collision_"))
                {
                    errors++;
                    Debug.LogError("ValidateFarmSceneNavigation: orphan collider " + collider.name, collider);
                    continue;
                }

                var id = collider.gameObject.name.Substring("Collision_".Length);
                if (!FarmSceneSpatialContract.TryGet(id, out var footprint) || !FarmSceneNavigationPolicy.RequiresMaterializedTerrainCollider(footprint))
                {
                    errors++;
                    Debug.LogError("ValidateFarmSceneNavigation: orphan collider " + collider.name, collider);
                }
            }

            errors += ValidateExistingBuildingOwners(colliders, collisionRoot);
            errors += ValidateBridgeCorridorPhysics();

            return errors;
        }

        private static int ValidateBridgeCorridorPhysics()
        {
            var errors = 0;
            if (HasSolidColliderAt(FarmSceneCompositionContract.BridgePassageProbe))
            {
                errors++;
                Debug.LogError("ValidateFarmSceneNavigation: Bridge corridor physics: FAIL (center blocked)");
            }

            var bridge = FarmSceneCompositionContract.BridgePassageProbe;
            float bankOffset = FarmLevel1LayoutContract.BridgeHeight / 2f + 1f;
            if (!HasSolidColliderAt(bridge + Vector2.up * bankOffset) || !HasSolidColliderAt(bridge + Vector2.down * bankOffset))
            {
                errors++;
                Debug.LogError("ValidateFarmSceneNavigation: Bridge corridor physics: FAIL (water bank not blocked)");
            }

            if (errors == 0) Debug.Log("ValidateFarmSceneNavigation: Bridge corridor physics: PASS");
            return errors;
        }

        private static bool HasSolidColliderAt(Vector2 point)
        {
            var overlaps = Physics2D.OverlapPointAll(point);
            for (var i = 0; i < overlaps.Length; i++)
            {
                if (!overlaps[i].isTrigger) return true;
            }
            return false;
        }

        private static int ValidateExistingBuildingOwners(Collider2D[] colliders, Transform collisionRoot)
        {
            var errors = 0;
            for (var i = 0; i < FarmSceneSpatialContract.All.Count; i++)
            {
                var footprint = FarmSceneSpatialContract.All[i];
                if (!FarmSceneNavigationPolicy.RequiresExistingSolidCollider(footprint)) continue;
                if (HasExistingSolidCollider(footprint, colliders, collisionRoot)) continue;

                errors++;
                Debug.LogError("ValidateFarmSceneNavigation: missing existing solid collider for " + footprint.Id);
            }

            return errors;
        }

        private static bool HasExistingSolidCollider(FarmSceneFootprint footprint, Collider2D[] colliders, Transform collisionRoot)
        {
            var bounds = footprint.Bounds;
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider.isTrigger || collider.transform.IsChildOf(collisionRoot)) continue;
                if (collider.bounds.Intersects(bounds)) return true;
            }

            return false;
        }

        public static int CountReachableLandmarks()
        {
            var width = Mathf.RoundToInt(FarmLevel1LayoutContract.Level1WidthTiles);
            var height = Mathf.RoundToInt(FarmLevel1LayoutContract.Level1HeightTiles);
            var visited = new bool[width, height];
            var queue = new Queue<Vector2Int>();
            var spawn = ToCell(new Vector2(FarmLevel1LayoutContract.DefaultSpawnX, FarmLevel1LayoutContract.DefaultSpawnY));
            if (IsWalkable(spawn))
            {
                visited[spawn.x, spawn.y] = true;
                queue.Enqueue(spawn);
            }

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                Visit(cell + Vector2Int.right, visited, queue);
                Visit(cell + Vector2Int.left, visited, queue);
                Visit(cell + Vector2Int.up, visited, queue);
                Visit(cell + Vector2Int.down, visited, queue);
            }

            var reachable = 0;
            for (var i = 0; i < RequiredLandmarks.Length; i++)
            {
                if (HasReachableApproach(RequiredLandmarks[i], visited)) reachable++;
            }
            return reachable;
        }

        private static Transform FindCollisionRoot()
        {
            var root = GameObject.Find("FarmSpatialCollision");
            return root == null ? null : root.transform;
        }

        private static void Visit(Vector2Int cell, bool[,] visited, Queue<Vector2Int> queue)
        {
            if (cell.x < 0 || cell.y < 0 || cell.x >= visited.GetLength(0) || cell.y >= visited.GetLength(1)) return;
            if (visited[cell.x, cell.y] || !IsWalkable(cell)) return;
            visited[cell.x, cell.y] = true;
            queue.Enqueue(cell);
        }

        private static bool HasReachableApproach(Vector2 landmark, bool[,] visited)
        {
            var cell = ToCell(landmark);
            return IsVisited(cell + Vector2Int.right, visited) || IsVisited(cell + Vector2Int.left, visited) ||
                   IsVisited(cell + Vector2Int.up, visited) || IsVisited(cell + Vector2Int.down, visited);
        }

        private static bool IsVisited(Vector2Int cell, bool[,] visited)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < visited.GetLength(0) && cell.y < visited.GetLength(1) && visited[cell.x, cell.y];
        }

        private static bool IsWalkable(Vector2Int cell)
        {
            return !FarmSceneNavigationRaster.IsBlocked(ToWorld(cell));
        }

        private static Vector2Int ToCell(Vector2 point)
        {
            return new Vector2Int(Mathf.FloorToInt(point.x - FarmLevel1LayoutContract.MinX), Mathf.FloorToInt(point.y - FarmLevel1LayoutContract.MinY));
        }

        private static Vector2 ToWorld(Vector2Int cell)
        {
            return new Vector2(FarmLevel1LayoutContract.MinX + (cell.x + 0.5f) * CellSize, FarmLevel1LayoutContract.MinY + (cell.y + 0.5f) * CellSize);
        }
    }
}
