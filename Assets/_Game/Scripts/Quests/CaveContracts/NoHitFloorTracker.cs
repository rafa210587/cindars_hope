namespace CindarsHope.Quests.CaveContracts
{
    /// <summary>
    /// fable_51 (CA-4) — pure, deterministic "no damage taken on this level" state machine.
    ///
    /// Contract: a no-hit floor is honest. Entering a level zeroes the flag; ANY damage taken on
    /// that level dirties it (direct hit, DoT tick, hazard — anything routed through the single
    /// player-damage point); clearing the level evaluates the flag. No Unity dependency — fully
    /// EditMode-testable. <see cref="CaveContractService"/> drives it from the existing
    /// PlayerDamagedEvent and cave level events on the GameEventBus.
    /// </summary>
    public sealed class NoHitFloorTracker
    {
        /// <summary>The level currently being tracked. 0 = not inside a tracked level.</summary>
        public int CurrentLevel { get; private set; }

        /// <summary>True once any damage has been taken on <see cref="CurrentLevel"/>.</summary>
        public bool DamagedThisLevel { get; private set; }

        /// <summary>
        /// Begin tracking a level (called on entering a cave level). Resets the damage flag.
        /// Re-entering the same level via back-exit also resets — a clean run from re-entry counts;
        /// the no-hit contract is about clearing a level without damage, evaluated at clear time.
        /// </summary>
        public void EnterLevel(int level)
        {
            CurrentLevel = level;
            DamagedThisLevel = false;
        }

        /// <summary>Mark that the player took damage on the current level (idempotent once dirty).</summary>
        public void MarkDamaged()
        {
            if (CurrentLevel > 0) DamagedThisLevel = true;
        }

        /// <summary>
        /// Evaluate a clear of <paramref name="level"/>. Returns true only when this is the level we
        /// were tracking AND no damage was taken. Always stops tracking afterwards (flag consumed).
        /// </summary>
        public bool EvaluateClear(int level)
        {
            bool clean = CurrentLevel == level && CurrentLevel > 0 && !DamagedThisLevel;
            // Clearing/leaving a level always ends tracking so a stale flag never leaks forward.
            CurrentLevel = 0;
            DamagedThisLevel = false;
            return clean;
        }

        /// <summary>Stop tracking without evaluating (e.g. leaving the cave / death).</summary>
        public void Reset()
        {
            CurrentLevel = 0;
            DamagedThisLevel = false;
        }
    }
}
