using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.Death
{
    /// <summary>
    /// Shows an informational death overlay when the player is defeated.
    /// Subscribes to PlayerDiedEvent (overworld) and CavePlayerDefeatedEvent (cave).
    /// Actual respawn logic lives in DeathSystemBootstrap / AnyaRespawnService.
    /// Uses OnGUI as functional fallback; replace with Canvas prefab for polish.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeathScreenController : MonoBehaviour
    {
        private bool _isShowing;
        private string _deathContext = string.Empty;

        private void OnEnable()
        {
            GameEventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Subscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Subscribe<DeathScreenClosedEvent>(OnDeathScreenClosed);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            GameEventBus.Unsubscribe<CavePlayerDefeatedEvent>(OnCavePlayerDefeated);
            GameEventBus.Unsubscribe<DeathScreenClosedEvent>(OnDeathScreenClosed);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            Show(!string.IsNullOrEmpty(evt.SceneName)
                ? $"You died in {evt.SceneName}."
                : "You died.");
        }

        private void OnCavePlayerDefeated(CavePlayerDefeatedEvent evt)
        {
            Show($"Defeated at cave level {evt.CaveLevel}.");
        }

        private void OnDeathScreenClosed(DeathScreenClosedEvent evt)
        {
            _isShowing = false;
        }

        private void Show(string context)
        {
            if (_isShowing) return;
            _deathContext = context;
            _isShowing = true;
            GameEventBus.Publish(new DeathScreenOpenedEvent());
        }

        private void OnGUI()
        {
            if (!_isShowing) return;

            var rect = new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.5f - 90f, 320f, 180f);
            GUI.Window(901, rect, DrawDeathWindow, "YOU DIED");
        }

        private void DrawDeathWindow(int windowId)
        {
            GUILayout.Space(8);
            GUILayout.Label(_deathContext);
            GUILayout.Space(12);

            if (GUILayout.Button("Respawn at Anya's Fountain"))
            {
                Dismiss();
            }

            GUILayout.Space(4);

            if (GUILayout.Button("Return to Town"))
            {
                Dismiss();
            }
        }

        private void Dismiss()
        {
            _isShowing = false;
            GameEventBus.Publish(new DeathScreenClosedEvent());
        }
    }
}
