using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Combat.Feel
{
    /// <summary>
    /// fable_71 — the canonical PUBLISHER of <see cref="HudSuppressionChangedEvent"/> (defined in
    /// CindarsHope.Core.Events; fable_71 is now its canonical owner, F43 the consumer — see EMENDA V3).
    /// There is NO second event type: this only publishes the existing struct.
    ///
    /// CA-3 / guaranteed restoration: this broadcaster publishes <b>phase 0 = everything visible</b>
    /// (<see cref="HudSuppressionChangedEvent.RestoreAll"/>) on enable and on disable, so the HUD — and
    /// the pre-existing floating damage numbers, never recreated here — is always returned to a fully
    /// visible state at the boundaries of the feel layer's lifetime. The per-phase suppression during
    /// the final boss fight is published by F43 (the consumer side owns the boss HUD).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudSuppressionBroadcaster : MonoBehaviour
    {
        private void OnEnable() => RestoreAllHud();

        private void OnDisable() => RestoreAllHud();

        /// <summary>Publish the canonical "restore all HUD" event (phase 0, nothing hidden).</summary>
        public void RestoreAllHud()
        {
            GameEventBus.Publish(HudSuppressionChangedEvent.RestoreAll());
        }
    }
}
