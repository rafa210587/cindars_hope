using System.Collections.Generic;

namespace CindarsHope.Quests.FestivalQuests
{
    /// <summary>
    /// fable_53 (CA-3) — pure, deterministic "3 echoes, one under each moon, SAME night" state machine
    /// for fq_luas. No Unity dependency — fully EditMode-testable. <see cref="FestivalQuestService"/>
    /// drives it from existing GameEventBus events (echo collection + DayStartedEvent for the dawn reset).
    ///
    /// Contract: the player must collect one echo under EACH of the named moons (peaks) within a single
    /// night. Collecting two echoes under the same moon does not advance toward distinctness; dawn
    /// (a new day) resets the gathered set. Completion = the set of distinct moons reaches the required
    /// count (3).
    /// </summary>
    public sealed class NightEchoTracker
    {
        private readonly int _requiredMoons;
        private readonly HashSet<string> _collectedMoons = new HashSet<string>();

        public NightEchoTracker(int requiredMoons = 3)
        {
            _requiredMoons = requiredMoons < 1 ? 1 : requiredMoons;
        }

        /// <summary>Distinct moons under which an echo has been collected this night.</summary>
        public int DistinctMoonsCollected => _collectedMoons.Count;

        /// <summary>True once an echo has been collected under <paramref name="moonId"/> distinct moons.</summary>
        public bool IsComplete => _collectedMoons.Count >= _requiredMoons;

        /// <summary>
        /// Register an echo collected under <paramref name="moonId"/>. Returns true when this collection
        /// added a NEW distinct moon (i.e. progressed). A repeated moon or an empty id is ignored.
        /// </summary>
        public bool CollectEcho(string moonId)
        {
            if (string.IsNullOrEmpty(moonId)) return false;
            return _collectedMoons.Add(moonId);
        }

        /// <summary>True when echo <paramref name="moonId"/> was already collected this night.</summary>
        public bool HasMoon(string moonId) => !string.IsNullOrEmpty(moonId) && _collectedMoons.Contains(moonId);

        /// <summary>Dawn / a new night resets the gathered set (echoes must be the same night).</summary>
        public void ResetNight()
        {
            _collectedMoons.Clear();
        }
    }
}
