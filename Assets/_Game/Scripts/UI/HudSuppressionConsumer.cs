using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.UI
{
    /// <summary>
    /// fable_43 — the HUD side of the Archivist information fight. A pure-C# consumer of
    /// <see cref="HudSuppressionChangedEvent"/> (provided by F43 today; fable_71 later — EMENDA V3):
    /// it tracks WHICH existing widgets are currently hidden so a thin HUD view can show/hide them.
    /// It never creates widgets and never hides anything outside a published suppression event.
    ///
    /// Restoration is guaranteed (CA-3): phase 0 / an empty hidden set clears everything visible, and
    /// <see cref="Subscribe"/>/<see cref="Unsubscribe"/> follow the project's unsubscribe discipline
    /// so a teardown can never leave the HUD blind. Pure (no Unity reference) for EditMode testing.
    /// </summary>
    public sealed class HudSuppressionConsumer
    {
        private readonly HashSet<string> _hidden = new HashSet<string>();
        private bool _subscribed;

        public int CurrentPhase { get; private set; }

        /// <summary>True when nothing is hidden (HUD fully visible).</summary>
        public bool AllVisible => _hidden.Count == 0;

        /// <summary>True if the named widget is currently suppressed.</summary>
        public bool IsHidden(string widgetId) => !string.IsNullOrEmpty(widgetId) && _hidden.Contains(widgetId);

        /// <summary>Snapshot of the currently hidden widget ids.</summary>
        public IReadOnlyCollection<string> HiddenWidgets => _hidden;

        public void Subscribe()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<HudSuppressionChangedEvent>(OnSuppressionChanged);
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<HudSuppressionChangedEvent>(OnSuppressionChanged);
            _subscribed = false;
            // Defensive restoration on teardown: never leave the HUD hidden.
            _hidden.Clear();
            CurrentPhase = 0;
        }

        /// <summary>Directly apply an event (exposed for tests / non-bus callers).</summary>
        public void Apply(HudSuppressionChangedEvent evt)
        {
            CurrentPhase = evt.Phase;
            _hidden.Clear();
            if (evt.Phase <= 0 || evt.HiddenWidgets == null) return;
            foreach (var id in evt.HiddenWidgets)
            {
                if (!string.IsNullOrEmpty(id)) _hidden.Add(id);
            }
        }

        private void OnSuppressionChanged(HudSuppressionChangedEvent evt) => Apply(evt);
    }
}
