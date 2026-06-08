using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Building metadata contract for placement validation.
    /// Defines footprint, entrance, terrain requirements, and move/rotate constraints.
    /// </summary>
    public class BuildingDefinition
    {
        public string BuildingId { get; set; }
        public string DisplayName { get; set; }

        // Footprint in tiles (width, height)
        public int FootprintWidthTiles { get; set; }
        public int FootprintHeightTiles { get; set; }

        // Footprint in pixels for visual alignment
        public int FootprintWidthPixels => FootprintWidthTiles * 32;
        public int FootprintHeightPixels => FootprintHeightTiles * 32;

        // Entrance tiles (relative to building origin)
        public List<(int x, int y)> EntranceTiles { get; set; } = new();

        // Required clearance around entrance in tiles
        public int RequiredClearanceTiles { get; set; } = 1;

        // Movement and rotation constraints
        public bool CanMove { get; set; } = true;
        public bool CanRotate { get; set; } = false;

        // Does this building block player movement paths?
        public bool BlocksPath { get; set; } = true;

        // Valid terrain tags for placement (e.g., "grass", "soil", "farm")
        public List<string> ValidTerrainTags { get; set; } = new();

        // Minimum farm level required for placement
        public int RequiredFarmLevel { get; set; } = 1;

        // Is this a lore anchor (Fonte, Raiz, etc) - cannot be moved
        public bool IsLoreAnchor { get; set; } = false;

        public BuildingDefinition(string buildingId, string displayName, int widthTiles, int heightTiles)
        {
            BuildingId = buildingId;
            DisplayName = displayName;
            FootprintWidthTiles = widthTiles;
            FootprintHeightTiles = heightTiles;
        }
    }
}
