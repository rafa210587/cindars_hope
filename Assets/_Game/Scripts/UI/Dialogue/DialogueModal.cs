using System;
using CindarsHope.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Dialogue
{
    [DisallowMultipleComponent]
    public class DialogueModal : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _dialogueText;
        [SerializeField] private Button _continueButton;

        private Modal.ModalManager _modalManager;
        private bool _isShowing = false;
        public event Action OnClose;

        private void OnEnable()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void OnDisable()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            }
        }

        public void Initialize(Modal.ModalManager modalManager)
        {
            _modalManager = modalManager;
            Hide();
        }

        public void Show(string dialogueText)
        {
            if (_dialogueText != null)
            {
                _dialogueText.text = dialogueText;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            _isShowing = true;
            _modalManager?.PushModal(Modal.ModalType.Dialogue);
        }

        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
            _isShowing = false;
            _modalManager?.TryPopModal(Modal.ModalType.Dialogue, out _);
            OnClose?.Invoke();
        }

        private void OnContinueClicked()
        {
            Hide();
        }
    }
}
