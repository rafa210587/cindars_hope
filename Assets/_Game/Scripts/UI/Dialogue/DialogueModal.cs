using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Dialogue;
using CindarsHope.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.Dialogue
{
    // arch: quebra do par mutuo NPC|UI (2026-07-15) — implementa CindarsHope.NPC.INpcDialoguePresenter
    // para que NpcController/NpcManager/NpcShopController consumam via porta, sem nomear este tipo
    // concreto. `using CindarsHope.NPC;` removido (não era usado no corpo do arquivo).
    [DisallowMultipleComponent]
    public class DialogueModal : MonoBehaviour, CindarsHope.NPC.INpcDialoguePresenter
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _dialogueText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;

        private IModalRuntime _modalManager;
        private bool _isShowing = false;
        private List<Button> _choiceButtons = new();
        private int _selectedChoiceIndex = -1;
        private DialogueChoice _selectedChoice = null;
        public event Action OnClose;
        public event Action<DialogueChoice> OnChoiceSelected;

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

        private void Update()
        {
            if (!_isShowing)
                return;

            if (_choiceButtons.Count > 0 && (global::UnityEngine.Input.GetKeyDown(KeyCode.W) || global::UnityEngine.Input.GetKeyDown(KeyCode.UpArrow)))
            {
                SelectPreviousChoice();
            }
            else if (_choiceButtons.Count > 0 && (global::UnityEngine.Input.GetKeyDown(KeyCode.S) || global::UnityEngine.Input.GetKeyDown(KeyCode.DownArrow)))
            {
                SelectNextChoice();
            }
            else if (_choiceButtons.Count > 0 && (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E)))
            {
                ConfirmChoice();
            }
            else if (_choiceButtons.Count == 0
                && (global::UnityEngine.Input.GetKeyDown(KeyCode.Return) || global::UnityEngine.Input.GetKeyDown(KeyCode.Space) || global::UnityEngine.Input.GetKeyDown(KeyCode.E) || global::UnityEngine.Input.GetKeyDown(KeyCode.Escape)))
            {
                Hide();
            }
        }

        public void Initialize(IModalRuntime modalManager)
        {
            _modalManager = modalManager;
            Hide();
        }

        public void Show(string dialogueText)
        {
            ShowWithChoices(dialogueText, null);
        }

        public void ShowWithChoices(string dialogueText, List<DialogueChoice> choices)
        {
            var mm = _modalManager ?? GameBootstrap.Instance?.ModalManager;
            if (mm != null && !mm.PushModal(CindarsHope.Foundation.ModalType.Dialogue))
            {
                Debug.LogWarning("DialogueModal rejected because another interactive modal is active.", this);
                return;
            }

            if (_dialogueText != null)
            {
                _dialogueText.text = dialogueText;
            }

            ClearChoices();
            if (choices != null && choices.Count > 0)
            {
                ShowChoices(choices);
            }

            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(choices == null || choices.Count == 0);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            gameObject.SetActive(true);
            _isShowing = true;
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
            ClearChoices();
            var mm = _modalManager ?? GameBootstrap.Instance?.ModalManager;
            mm?.TryPopModal(CindarsHope.Foundation.ModalType.Dialogue, out _);
            OnClose?.Invoke();
        }

        private void ShowChoices(List<DialogueChoice> choices)
        {
            if (_choicesContainer == null || _choiceButtonPrefab == null)
                return;

            foreach (var choice in choices)
            {
                var buttonObj = Instantiate(_choiceButtonPrefab, _choicesContainer);
                buttonObj.SetActive(true);
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.GetComponentInChildren<Text>().text = choice.Label;
                    _choiceButtons.Add(button);

                    var localChoice = choice;
                    button.onClick.AddListener(() => SelectChoiceByButton(localChoice));
                }
            }

            if (_choiceButtons.Count > 0)
            {
                _selectedChoiceIndex = 0;
                UpdateChoiceHighlight();
            }
        }

        private void ClearChoices()
        {
            foreach (var button in _choiceButtons)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
            _choiceButtons.Clear();
            _selectedChoiceIndex = -1;
            _selectedChoice = null;
        }

        private void SelectPreviousChoice()
        {
            if (_choiceButtons.Count == 0)
                return;
            _selectedChoiceIndex = (_selectedChoiceIndex - 1 + _choiceButtons.Count) % _choiceButtons.Count;
            UpdateChoiceHighlight();
        }

        private void SelectNextChoice()
        {
            if (_choiceButtons.Count == 0)
                return;
            _selectedChoiceIndex = (_selectedChoiceIndex + 1) % _choiceButtons.Count;
            UpdateChoiceHighlight();
        }

        private void ConfirmChoice()
        {
            if (_selectedChoiceIndex >= 0 && _selectedChoiceIndex < _choiceButtons.Count)
            {
                _choiceButtons[_selectedChoiceIndex].onClick.Invoke();
            }
        }

        private void SelectChoiceByButton(DialogueChoice choice)
        {
            _selectedChoice = choice;
            OnChoiceSelected?.Invoke(choice);
        }

        private void UpdateChoiceHighlight()
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                var colors = _choiceButtons[i].colors;
                if (i == _selectedChoiceIndex)
                {
                    colors.normalColor = Color.yellow;
                }
                else
                {
                    colors.normalColor = Color.white;
                }
                _choiceButtons[i].colors = colors;
            }
        }

        private void OnContinueClicked()
        {
            Hide();
        }
    }
}
