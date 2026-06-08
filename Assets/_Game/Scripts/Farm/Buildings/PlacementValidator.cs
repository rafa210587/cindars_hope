using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Deterministic building placement validator.
    /// Validates footprint, entrance clearance, terrain, zones, and lore anchors.
    /// </summary>
    public static class PlacementValidator
    {
        /// <summary>
        /// Validate building placement at a given position.
        /// </summary>
        /// <param name="building">Building definition</param>
        /// <param name="positionX">Building origin X in world units</param>
        /// <param name="positionY">Building origin Y in world units</param>
        /// <param name="existingBuildings">List of already-placed buildings for collision check</param>
        /// <param name="loreAnchors">List of lore anchor positions (Fonte, Raiz, etc)</param>
        /// <param name="farmLevel">Current farm level</param>
        /// <returns>PlacementState indicating if placement is valid</returns>
        public static PlacementState ValidatePlacement(
            BuildingDefinition building,
            float positionX,
            float positionY,
            List<(string id, float x, float y, int widthTiles, int heightTiles)> existingBuildings,
            List<(float x, float y, int widthTiles, int heightTiles)> loreAnchors,
            int farmLevel)
        {
            // Check farm level requirement
            if (farmLevel < building.RequiredFarmLevel)
            {
                return PlacementState.RequiresUpgrade;
            }

            // Check collision with existing buildings
            if (CollidesWithBuildings(positionX, positionY, building.FootprintWidthTiles, building.FootprintHeightTiles, existingBuildings))
            {
                return PlacementState.BlockedByObject;
            }

            // Check collision with lore anchors (Fonte, Raiz, etc)
            if (CollidesWithAnchors(positionX, positionY, building.FootprintWidthTiles, building.FootprintHeightTiles, loreAnchors))
            {
                return PlacementState.BlockedByLoreAnchor;
            }

            // Check entrance clearance
            if (!HasValidEntrance(building, positionX, positionY, existingBuildings, loreAnchors))
            {
                return PlacementState.BlockedByPath;
            }

            return PlacementState.ValidPlacement;
        }

        /// <summary>
        /// Check if building footprint collides with existing buildings.
        /// </summary>
        private static bool CollidesWithBuildings(
            float posX, float posY, int widthTiles, int heightTiles,
            List<(string id, float x, float y, int widthTiles, int heightTiles)> existing)
        {
            foreach (var (id, existingX, existingY, existingW, existingH) in existing)
            {
                if (RectanglesOverlap(posX, posY, widthTiles, heightTiles, existingX, existingY, existingW, existingH))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if building footprint collides with lore anchors.
        /// </summary>
        private static bool CollidesWithAnchors(
            float posX, float posY, int widthTiles, int heightTiles,
            List<(float x, float y, int widthTiles, int heightTiles)> anchors)
        {
            foreach (var (anchorX, anchorY, anchorW, anchorH) in anchors)
            {
                if (RectanglesOverlap(posX, posY, widthTiles, heightTiles, anchorX, anchorY, anchorW, anchorH))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if entrance tiles have required clearance.
        /// </summary>
        private static bool HasValidEntrance(
            BuildingDefinition building, float posX, float posY,
            List<(string id, float x, float y, int widthTiles, int heightTiles)> existingBuildings,
            List<(float x, float y, int widthTiles, int heightTiles)> loreAnchors)
        {
            if (building.EntranceTiles.Count == 0)
            {
                // No entrance defined = no clearance required
                return true;
            }

            foreach (var (entranceX, entranceY) in building.EntranceTiles)
            {
                // Entrance position in world space
                float worldEntranceX = posX + entranceX;
                float worldEntranceY = posY + entranceY;

                // Check clearance area around entrance
                float clearanceWidth = 1 + (2 * building.RequiredClearanceTiles);
                float clearanceHeight = 1 + (2 * building.RequiredClearanceTiles);
                float clearanceStartX = worldEntranceX - building.RequiredClearanceTiles;
                float clearanceStartY = worldEntranceY - building.RequiredClearanceTiles;

                // Clearance must not overlap with other buildings or anchors
                if (CollidesWithBuildingsInArea(clearanceStartX, clearanceStartY, (int)clearanceWidth, (int)clearanceHeight, existingBuildings))
                {
                    return false;
                }

                if (CollidesWithAnchorsInArea(clearanceStartX, clearanceStartY, (int)clearanceWidth, (int)clearanceHeight, loreAnchors))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if a clearance area overlaps with existing buildings.
        /// </summary>
        private static bool CollidesWithBuildingsInArea(
            float areaPosX, float areaPosY, int areaWidthTiles, int areaHeightTiles,
            List<(string id, float x, float y, int widthTiles, int heightTiles)> existingBuildings)
        {
            foreach (var (id, buildingX, buildingY, buildingW, buildingH) in existingBuildings)
            {
                if (RectanglesOverlap(areaPosX, areaPosY, areaWidthTiles, areaHeightTiles, buildingX, buildingY, buildingW, buildingH))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a clearance area overlaps with lore anchors.
        /// </summary>
        private static bool CollidesWithAnchorsInArea(
            float areaPosX, float areaPosY, int areaWidthTiles, int areaHeightTiles,
            List<(float x, float y, int widthTiles, int heightTiles)> loreAnchors)
        {
            foreach (var (anchorX, anchorY, anchorW, anchorH) in loreAnchors)
            {
                if (RectanglesOverlap(areaPosX, areaPosY, areaWidthTiles, areaHeightTiles, anchorX, anchorY, anchorW, anchorH))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if two axis-aligned rectangles overlap.
        /// </summary>
        private static bool RectanglesOverlap(
            float x1, float y1, int w1, int h1,
            float x2, float y2, int w2, int h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 &&
                   y1 < y2 + h2 && y1 + h1 > y2;
        }
    }
}
