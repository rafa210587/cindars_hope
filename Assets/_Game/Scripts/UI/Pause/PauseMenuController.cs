using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Save;
using CindarsHope.UI.Menu;
using UnityEngine;

namespace CindarsHope.UI.Pause
{
    /// <summary>
    /// Handles pause/resume lifecycle. Subscribes to PauseOpenedEvent (published by
    /// GameplayInputRouter on Esc with no modal open). Shows an OnGUI overlay that is
    /// replaced by a Canvas prefab when one is wired in the scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private bool _showDebugPauseOverlay = true;

        private MenuManager _menuManager;
        private SaveManager _saveManager;
        private bool _isPaused;
        private string _feedback = string.Empty;

        private static readonly Rect WindowRect = new Rect(
            Screen.width * 0.5f - 100f, Screen.height * 0.5f - 120f, 200f, 240f);

        private void OnEnable()
        {
            GameEventBus.Subscribe<PauseOpenedEvent>(OnPauseOpened);
            GameEventBus.Subscribe<PauseClosedEvent>(OnPauseClosed);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PauseOpenedEvent>(OnPauseOpened);
            GameEventBus.Unsubscribe<PauseClosedEvent>(OnPauseClosed);
        }

        private void Update()
        {
            if (!_isPaused) return;

            if (UnityEngine.global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                Resume();
            }
        }

        private void OnGUI()
        {
            if (!_isPaused || !_showDebugPauseOverlay) return;

            var rect = new Rect(Screen.width * 0.5f - 100f, Screen.height * 0.5f - 120f, 200f, 240f);
            GUI.Window(900, rect, DrawPauseWindow, "PAUSED");
        }

        private void DrawPauseWindow(int windowId)
        {
            GUILayout.Space(8);

            if (GUILayout.Button("Resume (Esc)"))
            {
                Resume();
            }

            GUILayout.Space(4);

            if (GUILayout.Button("Save Game"))
            {
                TrySave();
            }

            if (GUILayout.Button("Load Game"))
            {
                TryLoad();
            }

            GUILayout.Space(4);

            if (GUILayout.Button("Options"))
            {
                _feedback = "Options: not yet implemented.";
            }

            GUILayout.Space(8);

            if (!string.IsNullOrEmpty(_feedback))
            {
                GUILayout.Label(_feedback);
            }
        }

        public void Open()
        {
            if (_isPaused) return;

            ResolveReferences();
            _isPaused = true;
            _feedback = string.Empty;

            if (_menuManager != null)
            {
                _menuManager.OpenMenu(MenuType.Pause);
            }
            else
            {
                Time.timeScale = 0f;
            }

            GameEventBus.Publish(new PauseOpenedEvent());
            Debug.Log("[PauseMenuController] Game paused.", this);
        }

        public void Resume()
        {
            if (!_isPaused) return;

            _isPaused = false;

            if (_menuManager != null)
            {
                _menuManager.CloseMenu();
            }
            else
            {
                Time.timeScale = 1f;
            }

            GameEventBus.Publish(new PauseClosedEvent());
            Debug.Log("[PauseMenuController] Game resumed.", this);
        }

        private void TrySave()
        {
            ResolveReferences();
            if (_saveManager == null)
            {
                _feedback = "SaveManager not available.";
                return;
            }

            _saveManager.SaveGame();
            _feedback = "Game saved.";
        }

        private void TryLoad()
        {
            ResolveReferences();
            if (_saveManager == null)
            {
                _feedback = "SaveManager not available.";
                return;
            }

            _saveManager.LoadGame();
            _feedback = "Game loaded.";
            Resume();
        }

        private void OnPauseOpened(PauseOpenedEvent evt)
        {
            // Avoid re-opening if this controller itself published the event
            if (_isPaused) return;
            Open();
        }

        private void OnPauseClosed(PauseClosedEvent evt)
        {
            if (!_isPaused) return;
            Resume();
        }

        private void ResolveReferences()
        {
            if (_menuManager != null && _saveManager != null) return;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null) return;

            if (_menuManager == null) _menuManager = bootstrap.GetComponent<MenuManager>();
            if (_saveManager == null) _saveManager = bootstrap.SaveManager;
        }
    }
}
