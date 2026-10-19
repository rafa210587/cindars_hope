using System.Collections.Generic;

namespace CindarsHope.Farm.Forage
{
    public class ForageCollectResult
    {
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public ForageRarity Rarity { get; set; }
        public ForageNodeState NodeStateAfter { get; set; }

        public static ForageCollectResult Fail(string reason) =>
            new ForageCollectResult { Success = false, FailureReason = reason };
    }

    public class FarmForageSpawnService
    {
        private readonly Dictionary<string, ForageDefinition> _definitions;

        // Zones where forage must never spawn
        private static readonly HashSet<string> ForbiddenZones = new HashSet<string>
        {
            "zone_fonte", "zone_cave_entrance", "zone_city_exit", "zone_lore_reserved"
        };

        public FarmForageSpawnService(Dictionary<string, ForageDefinition> definitions)
        {
            _definitions = definitions ?? new Dictionary<string, ForageDefinition>();
        }

        public ForageCollectResult Collect(ForageSpawnState spawn, int currentDay, string currentSeason = null)
        {
            if (spawn == null)
                return ForageCollectResult.Fail("SpawnNotFound");

            if (!_definitions.TryGetValue(spawn.ForageId, out var def))
                return ForageCollectResult.Fail("DefinitionNotFound");

            if (!def.IsCollectable)
                return ForageCollectResult.Fail("ForageNotCollectable");

            if (!spawn.IsAvailable)
                return ForageCollectResult.Fail("ForageNotAvailable");

            if (ForbiddenZones.Contains(spawn.ZoneId))
                return ForageCollectResult.Fail("ForbiddenZone");

            if (def.AllowedSeasons.Count > 0 && !string.IsNullOrEmpty(currentSeason))
            {
                if (!def.AllowedSeasons.Contains(currentSeason))
                    return ForageCollectResult.Fail("WrongSeason");
            }

            // Idempotency: mark collected — second call returns ForageNotAvailable
            spawn.CurrentState = ForageNodeState.Collected;
            spawn.CollectedDay = currentDay;
            if (def.RespawnAfterDays > 0)
                spawn.NextEligibleSpawnDay = currentDay + def.RespawnAfterDays;

            return new ForageCollectResult
            {
                Success = true,
                ItemId = def.ItemId,
                Quantity = 1,
                Rarity = def.Rarity,
                NodeStateAfter = spawn.CurrentState
            };
        }

        public bool TryRespawn(ForageSpawnState spawn, int currentDay, string currentSeason = null)
        {
            if (spawn == null || spawn.IsAvailable) return false;
            if (!_definitions.TryGetValue(spawn.ForageId, out var def)) return false;
            if (!def.CanRespawnSameSeason) return false;
            if (spawn.NextEligibleSpawnDay < 0) return false;
            if (currentDay < spawn.NextEligibleSpawnDay) return false;
            if (ForbiddenZones.Contains(spawn.ZoneId)) return false;
            if (def.AllowedSeasons.Count > 0 && !string.IsNullOrEmpty(currentSeason))
            {
                if (!def.AllowedSeasons.Contains(currentSeason)) return false;
            }

            spawn.CurrentState = ForageNodeState.Available;
            spawn.NextEligibleSpawnDay = -1;
            spawn.SpawnedDay = currentDay;
            return true;
        }
    }
}
