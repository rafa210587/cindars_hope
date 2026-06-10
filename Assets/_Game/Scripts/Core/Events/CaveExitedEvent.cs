namespace CindarsHope.Core.Events
{
    /// <summary>
    /// WAVE_INTEGRATION_16 — Published by CaveRuntimeBridge when the player
    /// exits the cave and returns to a surface scene (FarmScene / TownScene).
    ///
    /// Note: within-cave level transitions (ForwardExit / BackExit between levels)
    /// do NOT publish this event — those use CaveLevelEnteredEvent.
    /// This event fires only when the player leaves the cave entirely.
    ///
    /// Consumers: HUD, quest bridge (WAVE15), analytics stubs.
    /// </summary>
    public readonly struct CaveExitedEvent
    {
        /// <summary>CaveRunSeed of the run that just ended (surface exit).</summary>
        public readonly string CaveRunSeed;

        /// <summary>Cave level the player was on when they exited to surface.</summary>
        public readonly int ExitedFromLevel;

        /// <summary>Name of the surface scene the player is returning to.</summary>
        public readonly string ReturnScene;

        /// <summary>Spawn anchor ID in the return scene.</summary>
        public readonly string ReturnSpawnId;

        public CaveExitedEvent(
            string caveRunSeed,
            int exitedFromLevel,
            string returnScene,
            string returnSpawnId)
        {
            CaveRunSeed = caveRunSeed ?? string.Empty;
            ExitedFromLevel = exitedFromLevel;
            ReturnScene = returnScene ?? string.Empty;
            ReturnSpawnId = returnSpawnId ?? string.Empty;
        }
    }
}
