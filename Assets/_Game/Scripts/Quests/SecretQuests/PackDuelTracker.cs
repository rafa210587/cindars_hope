using System.Collections.Generic;

namespace CindarsHope.Quests.SecretQuests
{
    /// <summary>
    /// fable_52 (CA-4) — tracks the Goblin Warchief duel: the truce only completes if the Warchief is
    /// defeated with 0 deaths in his pack DURING the encounter.
    ///
    /// Pure / deterministic / EditMode-testable — no Unity, no event bus. A thin adapter feeds it the
    /// real death / victory events scoped to the active encounter. Scope is by encounterId + an explicit
    /// begin/end window so deaths OUTSIDE the duel (other levels, other runs) never invalidate it
    /// (risk mitigation in the spec). Any pack death inside the window dirties the duel permanently for
    /// that window; the duel is simply re-offerable on the next run (no softlock).
    /// </summary>
    public sealed class PackDuelTracker
    {
        private string _encounterId;
        private bool _active;
        private bool _packMemberDied;
        private bool _warchiefDefeated;
        private readonly HashSet<string> _packIds = new HashSet<string>();

        public bool IsActive => _active;
        public string EncounterId => _encounterId;
        public bool PackMemberDied => _packMemberDied;
        public bool WarchiefDefeated => _warchiefDefeated;

        /// <summary>Opens the duel window for an encounter and registers the pack member ids to watch.</summary>
        public void Begin(string encounterId, IEnumerable<string> packIds)
        {
            _encounterId = encounterId;
            _active = true;
            _packMemberDied = false;
            _warchiefDefeated = false;
            _packIds.Clear();
            if (packIds != null)
            {
                foreach (var id in packIds)
                {
                    if (!string.IsNullOrEmpty(id)) _packIds.Add(id);
                }
            }
        }

        /// <summary>An enemy died. Only deaths inside the active window AND in the watched pack dirty the duel.</summary>
        public void OnEnemyDied(string enemyId)
        {
            if (!_active || string.IsNullOrEmpty(enemyId)) return;
            // Empty pack list = watch every death in the window (any death is a pack death).
            if (_packIds.Count == 0 || _packIds.Contains(enemyId))
            {
                _packMemberDied = true;
            }
        }

        /// <summary>The Warchief himself was defeated (the duel objective). Killing him is not a pack death.</summary>
        public void OnWarchiefDefeated(string warchiefId)
        {
            if (!_active) return;
            _warchiefDefeated = true;
        }

        /// <summary>
        /// True only when the duel is won honestly: Warchief defeated AND no pack member died in the
        /// window. Evaluating does not close the window (the caller closes via <see cref="End"/>).
        /// </summary>
        public bool IsTruceEarned() => _active && _warchiefDefeated && !_packMemberDied;

        /// <summary>Closes the duel window (encounter left / cave exited / run ended). Resets tracking.</summary>
        public void End()
        {
            _active = false;
            _encounterId = null;
            _packMemberDied = false;
            _warchiefDefeated = false;
            _packIds.Clear();
        }
    }
}
