using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Routing
{
    /// <summary>
    /// Central keyboard input router. Handles I/K/U/Esc and publishes UI events
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

        public static GameplayInputRouter Install(Transform host)
        {
            if (Instance != null) return Instance;
            if (host == null) return null;
            return host.GetComponent<GameplayInputRouter>() ?? host.gameObject.AddComponent<GameplayInputRouter>();
        }

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
            var frame = new GameplayShortcutFrame(
                Input.GetKeyDown(KeyCode.Escape),
                Input.GetKeyDown(KeyCode.I),
                Input.GetKeyDown(KeyCode.K),
                Input.GetKeyDown(KeyCode.U));

            Publish(GameplayShortcutDecision.Resolve(frame, hasModal));
        }

        private static void Publish(GameplayInputCommand command)
        {
            switch (command)
            {
                case GameplayInputCommand.CloseModal:
                    GameEventBus.Publish(new ModalCloseRequestedEvent());
                    break;
                case GameplayInputCommand.OpenPause:
                    GameEventBus.Publish(new PauseOpenedEvent());
                    break;
                case GameplayInputCommand.OpenInventory:
                    GameEventBus.Publish(new InventoryPanelOpenedEvent());
                    break;
                case GameplayInputCommand.OpenEquipment:
                    GameEventBus.Publish(new EquipmentPanelOpenedEvent());
                    break;
                case GameplayInputCommand.OpenSkillTree:
                    GameEventBus.Publish(new SkillTreeOpenedEvent());
                    break;
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
