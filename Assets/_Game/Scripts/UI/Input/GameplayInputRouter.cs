using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Routing
{
    /// <summary>
    /// Central keyboard input router. Handles I/K/U/C/Esc and publishes UI events
    /// instead of directly invoking panels. Legacy OnGUI panels check IsActive to
    /// yield their own key handling when this router is present.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayInputRouter : MonoBehaviour
    {
        public static GameplayInputRouter Instance { get; private set; }

        // When true, legacy OnGUI singleton panels should skip their own key handling.
        public static bool IsActive => Instance != null && Instance.enabled;

        private ModalManager _modalManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            ResolveModalManager();

            var hasModal = _modalManager != null && _modalManager.HasActiveModal;
            var currentModal = _modalManager != null ? _modalManager.CurrentModal : ModalType.None;

            // Esc: close current modal OR open pause
            if (UnityEngine.global::UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (hasModal)
                {
                    GameEventBus.Publish(new ModalCloseRequestedEvent());
                }
                else
                {
                    GameEventBus.Publish(new PauseOpenedEvent());
                }
                return;
            }

            // Block all gameplay shortcuts while any modal is open
            if (hasModal) return;

            if (UnityEngine.global::UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                GameEventBus.Publish(new InventoryPanelOpenedEvent());
            }
            else if (UnityEngine.global::UnityEngine.Input.GetKeyDown(KeyCode.K))
            {
                GameEventBus.Publish(new EquipmentPanelOpenedEvent());
            }
            else if (UnityEngine.global::UnityEngine.Input.GetKeyDown(KeyCode.U))
            {
                // Skill tree panel is already Canvas-based via SkillTreeInputHandler;
                // publishing the event lets it respond without duplicate key handling.
                GameEventBus.Publish(new SkillTreeOpenedEvent());
            }
        }

        private void ResolveModalManager()
        {
            if (_modalManager != null) return;
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null) _modalManager = bootstrap.ModalManager;
        }
    }
}
