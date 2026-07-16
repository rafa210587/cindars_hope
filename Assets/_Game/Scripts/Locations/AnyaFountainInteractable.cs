using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Core.Respawn;
using CindarsHope.Interaction;
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
        [SerializeField] private AnyaFountainUIController _uiController;
        private AnyaRespawnService _respawnService;

        public string InteractionPrompt => "Press E to interact";

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

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
                    bootstrap.PlayerManager as CindarsHope.Player.PlayerManager,
                    bootstrap.StaminaManager as CindarsHope.Player.StaminaManager,
                    bootstrap.ManaManager as CindarsHope.Player.ManaManager,
                    bootstrap.AnyaFountain.RespawnPoint
                );
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
