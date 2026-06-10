// Stable scene ID constants for WAVE_INTEGRATION_13.
// Maps to SceneNames constants for backward compatibility.
// Do not use these as display labels — they are internal IDs.
using CindarsHope.SceneManagement;

namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// Stable string IDs for all gameplay scenes.
    /// These IDs drive transition logic and must never change once wired.
    /// Use SceneNames constants for the actual Unity scene name strings.
    /// </summary>
    public static class SceneId
    {
        /// <summary>Farm scene — primary gameplay hub. Maps to "FarmScene".</summary>
        public const string Farm = SceneNames.Farm;

        /// <summary>Town/City scene. Maps to "TownScene".</summary>
        public const string Town = SceneNames.Town;

        /// <summary>Cave runtime scene. Maps to "CaveScene".</summary>
        public const string Cave = SceneNames.CaveRuntime;

        // -------------------------------------------------------------------
        // Gate IDs (stable, never use GameObject.name)
        // -------------------------------------------------------------------

        /// <summary>Gate on FarmScene side leading to Town.</summary>
        public const string GateFarmTownExit = "gate_farm_town_exit";

        /// <summary>Gate on FarmScene side leading to Cave.</summary>
        public const string GateFarmCaveEntrance = "gate_farm_cave_entrance";

        /// <summary>Gate on TownScene side returning to Farm.</summary>
        public const string GateTownFarmExit = "gate_town_farm_exit";

        /// <summary>Gate on CaveScene side returning to Farm.</summary>
        public const string GateCaveFarmExit = "gate_cave_farm_exit";

        // -------------------------------------------------------------------
        // Spawn Anchor IDs (stable, must match SceneSpawnPoint._spawnId values)
        // -------------------------------------------------------------------

        /// <summary>FarmScene spawn used when arriving from Town.</summary>
        public const string SpawnFarmFromTown = "spawn_farm_from_town";

        /// <summary>FarmScene spawn used when returning from Cave.</summary>
        public const string SpawnFarmFromCave = "spawn_farm_from_cave";

        /// <summary>TownScene spawn used when arriving from Farm.</summary>
        public const string SpawnTownFromFarm = "spawn_town_from_farm";

        /// <summary>CaveScene spawn used when arriving from Farm.</summary>
        public const string SpawnCaveFromFarm = "spawn_cave_from_farm";

        /// <summary>FarmScene default spawn (no incoming transition context).</summary>
        public const string SpawnFarmDefault = "spawn_farm_default";
    }
}
