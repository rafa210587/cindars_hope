using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Locations
{
    [DisallowMultipleComponent]
    public class AnyaFountainUIController : MonoBehaviour
    {
        [SerializeField] private AnyaFountainMenu _fountainMenuPrefab;
        private ModalManager _modalManager;
        private AnyaRespawnService _respawnService;
        private bool _isInitialized;

        private void Start()
        {
            TryInitialize("Start");
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<AnyaFountainOpenedEvent>(OnFountainOpened);
            TryInitialize("OnEnable");
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<AnyaFountainOpenedEvent>(OnFountainOpened);
        }

        public void Initialize()
        {
            TryInitialize("Initialize");
        }

        private bool TryInitialize(string reason)
        {
            if (_isInitialized)
            {
                return true;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return false;
            }

            // arch: Core|UI (2026-07-15) — GameBootstrap.ModalManager agora e IModalRuntime (porta);
            // cast para o tipo concreto porque este controller usa OpenModal<T>, nao portavel.
            // AnyaFountainUIController ja e do modulo UI, entao isso nao afeta o par Core|UI.
            _modalManager = bootstrap.ModalManager as ModalManager;

            if (_modalManager == null)
            {
                Debug.LogWarning($"[AnyaFountainUIController] Initialization incomplete from {reason}. modalManager=false", this);
                return false;
            }

            _isInitialized = true;
            return true;
        }

        public void OpenFountainMenu(AnyaRespawnService respawnService)
        {
            _respawnService = respawnService;
            GameEventBus.Publish(new AnyaFountainOpenedEvent());
        }

        private void OnFountainOpened(AnyaFountainOpenedEvent evt)
        {
            if (!TryInitialize("OnFountainOpened"))
            {
                Debug.LogError("[AnyaFountainUIController] Cannot open menu: ModalManager not initialized", this);
                return;
            }

            if (_fountainMenuPrefab == null)
            {
                Debug.LogWarning("[AnyaFountainUIController] Fountain menu prefab not assigned", this);
                return;
            }

            var modalInstance = _modalManager.OpenModal(_fountainMenuPrefab);
            if (modalInstance is AnyaFountainMenu fountainMenu)
            {
                fountainMenu.Initialize(_respawnService);
                Debug.Log("[AnyaFountainUIController] Fountain menu opened");
            }
        }
    }
}
