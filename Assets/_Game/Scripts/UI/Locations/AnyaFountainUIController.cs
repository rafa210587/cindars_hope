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

        private void OnEnable()
        {
            GameEventBus.Subscribe<AnyaFountainOpenedEvent>(OnFountainOpened);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<AnyaFountainOpenedEvent>(OnFountainOpened);
        }

        public void Initialize()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[AnyaFountainUIController] GameBootstrap not found");
                return;
            }

            _modalManager = bootstrap.ModalManager;

            if (_modalManager == null)
            {
                Debug.LogWarning("[AnyaFountainUIController] ModalManager not found");
            }
        }

        public void OpenFountainMenu(AnyaRespawnService respawnService)
        {
            _respawnService = respawnService;
            GameEventBus.Publish(new AnyaFountainOpenedEvent());
        }

        private void OnFountainOpened(AnyaFountainOpenedEvent evt)
        {
            if (_modalManager == null)
            {
                Debug.LogError("[AnyaFountainUIController] Cannot open menu: ModalManager missing");
                return;
            }

            if (_fountainMenuPrefab == null)
            {
                Debug.LogWarning("[AnyaFountainUIController] Fountain menu prefab not assigned");
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
