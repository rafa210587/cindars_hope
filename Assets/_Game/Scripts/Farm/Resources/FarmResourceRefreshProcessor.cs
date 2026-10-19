using System.Collections.Generic;

namespace CindarsHope.Farm.Resources
{
    // Forbidden zones where no farm resource node may ever refresh
    public static class ResourceNodeForbiddenZones
    {
        public static readonly HashSet<string> Zones = new HashSet<string>
        {
            "zone_fonte", "zone_cave_entrance", "zone_city_exit", "zone_lore_reserved",
            "zone_mana_root", "zone_endgame_quarry"
        };

        public static bool IsForbidden(string zoneId) =>
            !string.IsNullOrEmpty(zoneId) && Zones.Contains(zoneId);
    }

    public class ResourceNodeRefreshContext
    {
        public int CurrentDay { get; set; }
        public string CurrentSeason { get; set; }
        public string CurrentWeather { get; set; }
        public int FarmLevel { get; set; }
        public HashSet<string> UnlockedZones { get; set; } = new HashSet<string>();
        public int RandomSeed { get; set; }
    }

    public class FarmResourceRefreshProcessor
    {
        private readonly Dictionary<string, ResourceNodeDefinition> _definitions;

        public FarmResourceRefreshProcessor(Dictionary<string, ResourceNodeDefinition> definitions)
        {
            _definitions = definitions ?? new Dictionary<string, ResourceNodeDefinition>();
        }

        public bool TryRefresh(ResourceNodeInstanceState node, ResourceNodeRefreshContext ctx)
        {
            if (node == null || ctx == null) return false;
            if (!node.IsDepleted) return false; // already available — no-op

            if (!_definitions.TryGetValue(node.NodeId, out var def)) return false;
            if (!def.CanRegrow) return false;
            if (def.IsEndgameReserved) return false;

            // Zone guardrails
            if (ResourceNodeForbiddenZones.IsForbidden(node.ZoneId)) return false;

            // Zone unlock gate
            if (def.AllowedZones.Count > 0 && !string.IsNullOrEmpty(node.ZoneId))
            {
                if (!def.AllowedZones.Contains(node.ZoneId)) return false;
                if (!ctx.UnlockedZones.Contains(node.ZoneId)) return false;
            }

            // Farm level gate
            if (def.RequiredFarmLevel > ctx.FarmLevel) return false;

            switch (def.RefreshPolicy)
            {
                case ResourceNodeRefreshPolicy.None:
                    return false;

                case ResourceNodeRefreshPolicy.FixedDays:
                    if (node.NextEligibleRefreshDay < 0) return false;
                    if (ctx.CurrentDay < node.NextEligibleRefreshDay) return false;
                    break;

                case ResourceNodeRefreshPolicy.NextDayChance:
                    if (node.LastHarvestedDay >= ctx.CurrentDay) return false; // same-day guard
                    var deterministicRoll = ((node.RandomSeed + ctx.CurrentDay * 31) % 100) / 100f;
                    if (deterministicRoll >= def.NextDayRefreshChance) return false;
                    break;

                case ResourceNodeRefreshPolicy.SeasonStart:
                    // Only refreshes when season changes — caller must supply season transition flag
                    // For simplicity, this processor checks if the policy is SeasonStart and current day matches
                    // Season transition is modeled as NextEligibleRefreshDay set by caller
                    if (node.NextEligibleRefreshDay < 0) return false;
                    if (ctx.CurrentDay < node.NextEligibleRefreshDay) return false;
                    break;

                case ResourceNodeRefreshPolicy.WeatherTriggered:
                    if (string.IsNullOrEmpty(def.RequiredWeather)) return false;
                    if (ctx.CurrentWeather != def.RequiredWeather) return false;
                    if (node.LastHarvestedDay >= ctx.CurrentDay) return false;
                    break;

                case ResourceNodeRefreshPolicy.EndgameOnly:
                    return false;

                default:
                    return false;
            }

            // Restore node
            node.CurrentState = ResourceNodeCurrentState.Available;
            node.RemainingHits = def.MaxHitsEnabled ? def.MaxHits : 1;
            node.NextEligibleRefreshDay = -1;
            return true;
        }

        public int ProcessBatch(IEnumerable<ResourceNodeInstanceState> nodes, ResourceNodeRefreshContext ctx)
        {
            int refreshed = 0;
            foreach (var node in nodes)
            {
                if (TryRefresh(node, ctx))
                    refreshed++;
            }
            return refreshed;
        }
    }
}
