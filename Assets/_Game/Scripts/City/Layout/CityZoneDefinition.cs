using System.Collections.Generic;

namespace CindarsHope.City.Layout
{
    public class CityZoneDefinition
    {
        public string ZoneId { get; set; }
        public string DisplayName { get; set; }
        // Approximate tile origin (top-left)
        public int ApproxTileOriginX { get; set; }
        public int ApproxTileOriginY { get; set; }
        // Approximate size in tiles
        public int ApproxTileWidth { get; set; }
        public int ApproxTileHeight { get; set; }
        public List<string> FunctionTags { get; set; } = new List<string>();
        public List<string> AllowedBuildingIds { get; set; } = new List<string>();
        public int NavigationPriority { get; set; } = 0;
        public bool FestivalEligible { get; set; } = false;
        public bool NightActivityEligible { get; set; } = false;
        public bool LoreSensitive { get; set; } = false;
    }
}
