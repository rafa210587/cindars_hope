using System.Collections.Generic;

namespace CindarsHope.City.Layout
{
    public class CityBuildingDefinition
    {
        public string BuildingId { get; set; }
        public string DisplayName { get; set; }
        public string ZoneId { get; set; }
        // Tile and pixel sizes (32px per tile; NPCs are 32x48px)
        public int ExternalWidthTiles { get; set; }
        public int ExternalHeightTiles { get; set; }
        public int InteriorWidthTiles { get; set; }
        public int InteriorHeightTiles { get; set; }
        public List<string> OwnerNpcIds { get; set; } = new List<string>();
        public List<string> ResidentNpcIds { get; set; } = new List<string>();
        public List<string> ServiceTags { get; set; } = new List<string>();
        public List<string> DoorTriggerIds { get; set; } = new List<string>();
        public List<string> BedIds { get; set; } = new List<string>();
        public OpenHoursRule OpenHoursRule { get; set; }
        public List<string> LoreFlags { get; set; } = new List<string>();
        public bool IsHidden { get; set; } = false;
    }

    public class OpenHoursRule
    {
        public int OpenHour { get; set; } = 0;
        public int CloseHour { get; set; } = 24;
        public string ClosedMessage { get; set; }
        public bool NightShopConditional { get; set; } = false;
        public string NightShopConditionFlag { get; set; }

        public bool IsOpenAt(int hour)
        {
            if (OpenHour <= CloseHour)
                return hour >= OpenHour && hour < CloseHour;
            // Wraps midnight: e.g. open 21:00, close 03:00
            return hour >= OpenHour || hour < CloseHour;
        }
    }
}
