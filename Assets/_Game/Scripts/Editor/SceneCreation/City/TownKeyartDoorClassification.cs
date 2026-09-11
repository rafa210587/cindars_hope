using System;
using System.Collections.Generic;
using System.Linq;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Canonical Town-only classification for the 24 authored lots. This is editor data, not a
    /// second runtime routing system: scene generation still uses the existing physical shell.
    /// </summary>
    public static class TownKeyartDoorClassification
    {
        public const string WalkInReveal = "walk_in_reveal";
        public const string ShopHoursGated = "shop_hours_gated";
        public const string ExteriorOnly = "exterior_only";

        private static readonly HashSet<string> Shops = new HashSet<string>(StringComparer.Ordinal)
        {
            "House_Bakery", "House_Fishery", "House_AlchemyLab", "House_Blacksmith",
            "House_Inn", "House_Tannery", "House_Workshop", "House_Residential_2",
            "House_Residential_3"
        };

        private static readonly string[] ExpectedIds =
        {
            "House_Temple", "House_Chamber", "House_Prison", "House_Manor", "House_Registry", "House_Archive",
            "House_MarketHall", "House_Bakery", "House_Inn", "House_Fishery", "House_Blacksmith", "House_AlchemyLab",
            "House_Workshop", "House_Tannery", "House_Residential_4", "House_CarvalhoTorto", "House_Residential_1",
            "House_Dagna", "House_Residential_2", "House_Pip", "House_Residential_3", "House_Tovin", "House_GateKeeper",
            "House_AnimalYard"
        };

        public static string For(string houseName)
        {
            if (string.Equals(houseName, "House_AnimalYard", StringComparison.Ordinal))
                return ExteriorOnly;
            return Shops.Contains(houseName) ? ShopHoursGated : WalkInReveal;
        }

        public static int Count(string classification)
        {
            var count = 0;
            foreach (var lot in TownCityLayout.AllBuildings)
                if (For(lot.Name) == classification) count++;
            return count;
        }

        /// <summary>Validates the generator contract: exact lot IDs, category counts and shop gates.</summary>
        public static bool Validate(out string reason)
        {
            var actual = TownCityLayout.AllBuildings.Select(lot => lot.Name).ToArray();
            var expected = new HashSet<string>(ExpectedIds, StringComparer.Ordinal);
            var actualSet = new HashSet<string>(actual, StringComparer.Ordinal);
            if (actual.Length != 24 || actualSet.Count != 24 || !expected.SetEquals(actualSet))
            {
                reason = "Town lot IDs do not match the canonical 24-lot classification set.";
                return false;
            }
            if (Count(WalkInReveal) != 14 || Count(ShopHoursGated) != 9 || Count(ExteriorOnly) != 1)
            {
                reason = $"Unexpected classification counts: walk_in={Count(WalkInReveal)}, shop={Count(ShopHoursGated)}, exterior={Count(ExteriorOnly)}.";
                return false;
            }
            if (TownCityLayout.AllNpcPlaces.Count(place => !string.IsNullOrWhiteSpace(place.BuildingName) && Shops.Contains(place.BuildingName)) == 0)
            {
                reason = "Shop-hours-gated classification has no existing NPC shop schedule anchors.";
                return false;
            }
            reason = "24/24 Town lots classified: 14 walk_in_reveal, 9 shop_hours_gated, 1 exterior_only.";
            return true;
        }
    }
}
