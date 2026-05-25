using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Locations;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Locations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class AnyaFountainInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private AnyaFountain _fountain;
        private AnyaFountainUIController _uiController;
        private AnyaRespawnService _respawnService;

        public string InteractableName => "Anya's Fountain";
        public string InteractionPrompt => "Press E to interact";

        private void OnEnable()
        {
            if (_fountain == null)
            {
                _fountain = GetComponentInParent<AnyaFountain>();
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && bootstrap.AnyaFountain != null)
            {
                _respawnService = new AnyaRespawnService(
                    bootstrap.PlayerManager,
                    bootstrap.StaminaManager,
                    bootstrap.ManaManager,
                    bootstrap.AnyaFountain.RespawnPoint
                );

                _uiController = FindObjectOfType<AnyaFountainUIController>();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_uiController != null && _respawnService != null)
            {
                _uiController.OpenFountainMenu(_respawnService);
            }
            else
            {
                Debug.LogWarning("[AnyaFountainInteractable] UI controller or respawn service not available");
                GameEventBus.Publish(new AnyaFountainOpenedEvent());
            }
        }
    }
}
