namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 — the scq_dragon_egg incubator at Nimble's: a SIMPLE day timer (no minigame, cosmetic
    /// in v1; future-pets hook). Pure / deterministic / EditMode-testable.
    ///
    /// State is just two ints (start day + duration) and a bool — no Unity refs — so it persists with
    /// the quest record / flags (save-dto-simple-types-only). Hatching is idempotent: once hatched it
    /// stays hatched regardless of further day ticks.
    /// </summary>
    public sealed class DragonEggIncubator
    {
        /// <summary>Catalog incubation time in in-game days (cosmetic timer; v1).</summary>
        public const int IncubationDays = 7;

        private int _startDay;
        private bool _started;
        private bool _hatched;

        public bool IsIncubating => _started && !_hatched;
        public bool IsHatched => _hatched;
        public int StartDay => _startDay;

        /// <summary>Places the egg in the incubator on <paramref name="currentDay"/>. No-op if already started.</summary>
        public void StartIncubation(int currentDay)
        {
            if (_started) return;
            _startDay = currentDay;
            _started = true;
            _hatched = false;
        }

        /// <summary>Days elapsed since incubation started (0 when not started).</summary>
        public int DaysElapsed(int currentDay)
        {
            if (!_started) return 0;
            int elapsed = currentDay - _startDay;
            return elapsed < 0 ? 0 : elapsed;
        }

        /// <summary>
        /// Advances the timer to <paramref name="currentDay"/>; hatches (once) when the incubation time
        /// has fully elapsed. Returns true only on the transition to hatched.
        /// </summary>
        public bool Tick(int currentDay)
        {
            if (!_started || _hatched) return false;
            if (DaysElapsed(currentDay) >= IncubationDays)
            {
                _hatched = true;
                return true;
            }
            return false;
        }

        /// <summary>Restores incubator state from persisted simple values (after save load).</summary>
        public void Restore(int startDay, bool started, bool hatched)
        {
            _startDay = startDay;
            _started = started;
            _hatched = hatched;
        }
    }
}
