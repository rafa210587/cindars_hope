using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Death
{
    public class CorpseRecoveryModal : ModalBase
    {
        [SerializeField] private Button _recoverButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Text _corpseInfoText;

        private CorpseRecoveryManager _recoveryManager;
        private Corpse _corpse;

        public override ModalType ModalType => ModalType.CorpseRecovery;

        private void OnEnable()
        {
            if (_recoverButton != null)
            {
                _recoverButton.onClick.AddListener(OnRecoverButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_recoverButton != null)
            {
                _recoverButton.onClick.RemoveListener(OnRecoverButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }
        }

        public void Initialize(Corpse corpse, CorpseRecoveryManager recoveryManager)
        {
            _corpse = corpse;
            _recoveryManager = recoveryManager;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_corpse == null)
            {
                return;
            }

            var itemCount = _corpse.GetTotalRecoverableItems();
            var goldAmount = _corpse.GoldAmount;

            if (_corpseInfoText != null)
            {
                _corpseInfoText.text = $"Gold: {goldAmount}\nItems: {itemCount}";
            }
        }

        private void OnRecoverButtonClicked()
        {
            if (_recoveryManager != null)
            {
                _recoveryManager.RecoverCorpse();
            }

            CloseModal();
        }

        private void OnCancelButtonClicked()
        {
            CloseModal();
        }
    }
}
