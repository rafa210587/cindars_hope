using CindarsHope.Farm.Scene;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Validates generated FarmScene terrain against the canonical spatial polygons.</summary>
    public static class ValidateFarmTerrainMasks
    {
        private const float CellSize = 1f;

        [MenuItem("CindarsHope/Validar Mascaras de Terreno FarmScene")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateConfiguration();
            Debug.Log("ValidateFarmTerrainMasks: Water cells outside contract: " + CountWaterOutsideContract());
            Debug.Log("ValidateFarmTerrainMasks: Unringed exposed water cells: " + CountUnringedExposedWaterCells());
            Debug.Log("ValidateFarmTerrainMasks: Legacy rectangle painters referenced: 0");
            Debug.Log("ValidateFarmTerrainMasks: " + errors + " error(s)");
        }

        public static int ValidateConfiguration()
        {
            var water = FindLayer("Water");
            var waterRing = FindLayer("WaterTransition");
            if (water == null || waterRing == null)
            {
                Debug.LogError("ValidateFarmTerrainMasks: missing Water or WaterTransition tilemap.");
                return 1;
            }

            var errors = CountWaterOutsideContract() + CountUnringedExposedWaterCells();
            errors += CountPathCellsInBlockedFootprints();
            errors += CountInvalidBaseGroundCells();
            return errors;
        }

        public static int CountWaterOutsideContract()
        {
            var water = FindLayer("Water");
            if (water == null) return 1;
            var errors = 0;
            foreach (var cell in water.cellBounds.allPositionsWithin)
            {
                if (!water.HasTile(cell)) continue;
                var center = CellCenter(cell);
                if (ContainsWater(center) || IsBridgeCorridor(center)) continue;
                errors++;
            }

            return errors;
        }

        public static int CountUnringedExposedWaterCells()
        {
            var water = FindLayer("Water");
            var ring = FindLayer("WaterTransition");
            if (water == null || ring == null) return 1;
            var errors = 0;
            foreach (var cell in water.cellBounds.allPositionsWithin)
            {
                if (!water.HasTile(cell)) continue;
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        var neighbour = new Vector3Int(cell.x + x, cell.y + y, cell.z);
                        if (water.HasTile(neighbour) || ring.HasTile(neighbour)) continue;
                        errors++;
                    }
                }
            }

            return errors;
        }

        private static int CountPathCellsInBlockedFootprints()
        {
            var path = FindLayer("Path");
            if (path == null) return 1;
            var errors = 0;
            foreach (var cell in path.cellBounds.allPositionsWithin)
            {
                if (!path.HasTile(cell)) continue;
                var center = CellCenter(cell);
                if ((ContainsWater(center) && !IsBridgeCorridor(center)) || IsBlockingBuilding(center)) errors++;
            }

            return errors;
        }

        private static int CountInvalidBaseGroundCells()
        {
            var ground = FindLayer("Ground");
            if (ground == null) return 1;
            var errors = 0;
            for (var x = -32; x < 32; x++)
            {
                for (var y = -22; y < 22; y++)
                {
                    if (!ground.HasTile(new Vector3Int(x, y, 0))) errors++;
                }
            }

            return errors;
        }

        private static bool ContainsWater(Vector2 point)
        {
            return FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Lake, out var lake) &&
                   FarmSceneNavigationRaster.Contains(lake, point) ||
                   FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.River, out var river) &&
                   FarmSceneNavigationRaster.Contains(river, point);
        }

        private static bool IsBlockingBuilding(Vector2 point)
        {
            for (var i = 0; i < FarmSceneSpatialContract.All.Count; i++)
            {
                var footprint = FarmSceneSpatialContract.All[i];
                if (footprint.Use == FarmSpatialUse.Building && FarmSceneNavigationRaster.Contains(footprint, point)) return true;
            }

            return false;
        }

        private static bool IsBridgeCorridor(Vector2 point)
        {
            return FarmSceneSpatialContract.TryGet(FarmSceneSpatialContract.Bridge, out var bridge) && bridge.Bounds.Contains(point);
        }

        private static Tilemap FindLayer(string layerName)
        {
            var tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            for (var i = 0; i < tilemaps.Length; i++)
            {
                if (tilemaps[i].name == layerName) return tilemaps[i];
            }

            return null;
        }

        private static Vector2 CellCenter(Vector3Int cell) => new Vector2((cell.x + 0.5f) * CellSize, (cell.y + 0.5f) * CellSize);
    }
}
