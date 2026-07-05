using System.Collections.Generic;

namespace CindarsHope.Farm.Expansion
{
    public class ZonePlacementResult
    {
        public bool Allowed { get; set; }
        public string BlockedReason { get; set; }
        public string ZoneId { get; set; }

        public static ZonePlacementResult Allow(string zoneId) =>
            new ZonePlacementResult { Allowed = true, ZoneId = zoneId };
        public static ZonePlacementResult Block(string reason, string zoneId = null) =>
            new ZonePlacementResult { Allowed = false, BlockedReason = reason, ZoneId = zoneId };
    }

    public class FarmExpansionValidator
    {
        private readonly List<FarmExpansionZone> _zones;

        public FarmExpansionValidator(List<FarmExpansionZone> zones)
        {
            _zones = zones ?? new List<FarmExpansionZone>();
        }

        public ZonePlacementResult CanPlaceBuildingAt(int tileX, int tileY, FarmPropertyLevel currentLevel)
        {
            foreach (var zone in _zones)
            {
                if (!zone.ContainsTile(tileX, tileY)) continue;

                // Lore/endgame zones are permanently blocked
                if (zone.IsLoreReserved)
                    return ZonePlacementResult.Block("LoreOrEndgameZone", zone.ZoneId);

                // Path must remain clear
                if (zone.IsMustClearAccess)
                    return ZonePlacementResult.Block("PathMustRemainClear", zone.ZoneId);

                // Fixed anchors (lake, entrances, etc.)
                if (zone.ZoneType == FarmZoneType.Fixed || zone.ZoneType == FarmZoneType.Water)
                    return ZonePlacementResult.Block("FixedOrWaterZone", zone.ZoneId);

                // Zone not yet unlocked by level
                if (!zone.IsUnlocked)
                    return ZonePlacementResult.Block("ZoneNotUnlocked", zone.ZoneId);

                // Farm level insufficient
                if ((int)currentLevel < (int)zone.RequiredLevel)
                    return ZonePlacementResult.Block("FarmLevelInsufficient", zone.ZoneId);

                if (zone.AllowBuilding)
                    return ZonePlacementResult.Allow(zone.ZoneId);
            }

            // Tile is outside all known zones — not allowed
            return ZonePlacementResult.Block("OutsideKnownZones");
        }

        public bool UnlockZone(string zoneId, FarmPropertyLevel currentLevel)
        {
            var zone = _zones.Find(z => z.ZoneId == zoneId);
            if (zone == null) return false;
            if (zone.IsLoreReserved) return false;
            if ((int)currentLevel < (int)zone.RequiredLevel) return false;

            // Blocked is the locked state of a future free zone, not permanent terrain.
            if (zone.ZoneType == FarmZoneType.Blocked)
                zone.ZoneType = FarmZoneType.Free;
            zone.IsUnlocked = true;
            return true;
        }
    }
}
