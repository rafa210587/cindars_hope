using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Locations
{
    public class AnyaFountainMenu : ModalBase
    {
        [SerializeField] private Button _returnToCaveButton;
        [SerializeField] private Button _respecButton;
        [SerializeField] private Button _exitButton;

        private AnyaRespawnService _respawnService;

        private void OnEnable()
        {
            if (_returnToCaveButton != null)
            {
                _returnToCaveButton.onClick.AddListener(OnReturnToCaveClicked);
            }

            if (_respecButton != null)
            {
                _respecButton.onClick.AddListener(OnRespecClicked);
                _respecButton.interactable = false; // Disabled for SPEC 15, enabled in SPEC 16
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        private void OnDisable()
        {
            if (_returnToCaveButton != null)
            {
                _returnToCaveButton.onClick.RemoveListener(OnReturnToCaveClicked);
            }

            if (_respecButton != null)
            {
                _respecButton.onClick.RemoveListener(OnRespecClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }
        }

        public void Initialize(AnyaRespawnService respawnService)
        {
            _respawnService = respawnService;
        }

        private void OnReturnToCaveClicked()
        {
            // TODO: Open checkpoint portal UI or return to cave
            CloseModal();
        }

        private void OnRespecClicked()
        {
            // TODO: Implement respec for SPEC 16
            Debug.Log("[AnyaFountainMenu] Respec not yet implemented (SPEC 16)");
        }

        private void OnExitClicked()
        {
            CloseModal();
        }
    }
}
