using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.MainProgression.Runtime
{
    /// <summary>
    /// fable_43 — pure-C# state machine orchestrating the four level-101 final encounters in
    /// canonical order with a controlled recovery window between them, re-entrable after a defeat.
    ///
    /// It does NOT spawn bosses or touch scenes (that is the runtime adapter's job in Play Mode) and
    /// it does NOT create a second boss AI — each encounter consumes the F05 boss-phase AI plus its
    /// special mechanic. This class owns only the ORDER, the recovery gate and the HUD-suppression
    /// publishing contract (Archivist). Communication is event-only via the GameEventBus.
    ///
    /// States per encounter: NotStarted -> InEncounter -> Recovery -> (next) InEncounter ... ->
    /// Complete. A defeat keeps the current encounter (re-entrable: <see cref="RetryCurrent"/>) and
    /// always restores the HUD, so the sequence can never softlock the player into a blind state.
    /// </summary>
    public sealed class EndgameSequenceController
    {
        public enum Phase
        {
            NotStarted = 0,
            InEncounter = 1,
            Recovery = 2,
            Complete = 3
        }

        private int _currentIndex = -1;
        private Phase _phase = Phase.NotStarted;
        private bool _hudSuppressed;

        public Phase CurrentPhase => _phase;
        public int CurrentIndex => _currentIndex;

        /// <summary>The active encounter definition, or null when not started / complete.</summary>
        public EndgameEncounterDefinition CurrentEncounter =>
            _phase == Phase.InEncounter ? EndgameEncounterCatalog.At(_currentIndex) : null;

        public bool IsComplete => _phase == Phase.Complete;

        /// <summary>Starts the sequence at encounter 0 (Vel-Karaum). Idempotent while in progress.</summary>
        public void Begin()
        {
            if (_phase != Phase.NotStarted && _phase != Phase.Complete) return;
            _currentIndex = 0;
            EnterEncounter();
        }

        /// <summary>
        /// Marks the current encounter as defeated by the player and opens the recovery window
        /// (no add respawn, consumables allowed — enforced by the adapter). Always restores the HUD.
        /// Returns false if there is no active encounter.
        /// </summary>
        public bool DefeatCurrentEncounter()
        {
            if (_phase != Phase.InEncounter) return false;
            RestoreHud();

            if (_currentIndex >= EndgameEncounterCatalog.Count - 1)
            {
                CompleteSequence();
                return true;
            }

            _phase = Phase.Recovery;
            return true;
        }

        /// <summary>Advances from the recovery window into the next encounter.</summary>
        public bool AdvanceEncounter()
        {
            if (_phase != Phase.Recovery) return false;
            _currentIndex++;
            EnterEncounter();
            return true;
        }

        /// <summary>
        /// Player was defeated mid-encounter. Re-entrable: the current encounter is restarted from
        /// the top (HUD restored first). Never advances and never softlocks. Returns false if no
        /// active encounter.
        /// </summary>
        public bool RetryCurrent()
        {
            if (_phase != Phase.InEncounter) return false;
            RestoreHud();
            EnterEncounter(); // re-publish encounter-changed + reset mechanic
            return true;
        }

        /// <summary>
        /// Archivist mechanic: publishes a HUD-suppression event for the given phase. Phase 0 means
        /// "restore all". Only valid while the Archivist (HudSuppression mechanic) is active; for
        /// any other encounter this is a no-op so the HUD can never be hidden outside its fight.
        /// </summary>
        public bool PublishHudSuppressionPhase(int phase)
        {
            var encounter = CurrentEncounter;
            if (encounter == null || encounter.Mechanic != EndgameSpecialMechanic.HudSuppression)
                return false;

            if (phase <= 0)
            {
                RestoreHud();
                return true;
            }

            var hidden = HiddenWidgetsForPhase(phase);
            _hudSuppressed = hidden.Count > 0;
            GameEventBus.Publish(new HudSuppressionChangedEvent(phase, hidden));
            return true;
        }

        /// <summary>
        /// Centralized restoration point (risk mitigation): publishes phase 0 = everything visible.
        /// Safe to call any number of times; only emits when something was actually suppressed, but
        /// always safe to force on victory / death / exit.
        /// </summary>
        public void RestoreHud()
        {
            if (!_hudSuppressed) return;
            _hudSuppressed = false;
            GameEventBus.Publish(HudSuppressionChangedEvent.RestoreAll());
        }

        /// <summary>Aborts the sequence (player left level 101). Restores the HUD; resets to NotStarted.</summary>
        public void Abort()
        {
            RestoreHud();
            _phase = Phase.NotStarted;
            _currentIndex = -1;
        }

        private void EnterEncounter()
        {
            var encounter = EndgameEncounterCatalog.At(_currentIndex);
            if (encounter == null)
            {
                CompleteSequence();
                return;
            }

            _phase = Phase.InEncounter;
            _hudSuppressed = false; // each encounter starts with the HUD fully visible
            GameEventBus.Publish(new EndgameEncounterChangedEvent(encounter.BossId, encounter.Index));
        }

        private void CompleteSequence()
        {
            _phase = Phase.Complete;
            GameEventBus.Publish(new EndgameEncounterChangedEvent(string.Empty, -1));
        }

        /// <summary>
        /// Canonical widget set hidden per Archivist phase (information fight). Phase 1 hides the
        /// minimap, phase 2 also the boss HP bar, phase 3 also the damage numbers. Phase 0 = none.
        /// All ids reference EXISTING widgets — this method only chooses which to suppress.
        /// </summary>
        public static IReadOnlyList<string> HiddenWidgetsForPhase(int phase)
        {
            var hidden = new List<string>();
            if (phase >= 1) hidden.Add(EndgameEncounterCatalog.WidgetMinimap);
            if (phase >= 2) hidden.Add(EndgameEncounterCatalog.WidgetBossHpBar);
            if (phase >= 3) hidden.Add(EndgameEncounterCatalog.WidgetDamageNumbers);
            return hidden;
        }
    }
}
