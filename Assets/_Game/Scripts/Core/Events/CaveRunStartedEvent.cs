namespace CindarsHope.Core.Events
{
    /// <summary>
    /// WAVE_INTEGRATION_16 — Published by CaveEntranceInteractable when the player
    /// initiates entry into the cave. The CaveRunSeed is not known at publish time
    /// (CaveRunManager initializes it in CaveScene); leave it empty if unknown.
    ///
    /// Consumers: HUD, quest system (WAVE15 bridge), analytics stubs.
    /// </summary>
    public readonly struct CaveRunStartedEvent
    {
        /// <summary>
        /// CaveRunSeed at the time of entering. May be empty if published before
        /// CaveScene loads (CaveRunManager will generate/restore the seed on Awake).
        /// </summary>
        public readonly string CaveRunSeed;

        /// <summary>Scene name the player is transitioning from (e.g., "FarmScene").</summary>
        public readonly string EnteredFromScene;

        /// <summary>Target spawn anchor ID in CaveScene (e.g., "spawn_cave_from_farm").</summary>
        public readonly string TargetSpawnId;

        public CaveRunStartedEvent(string caveRunSeed, string enteredFromScene, string targetSpawnId)
        {
            CaveRunSeed = caveRunSeed ?? string.Empty;
            EnteredFromScene = enteredFromScene ?? string.Empty;
            TargetSpawnId = targetSpawnId ?? string.Empty;
        }
    }
}
