using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public class PlayerNeedsHUD : MonoBehaviour
    {
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;

        [SerializeField] private Image _hungerBar;
        [SerializeField] private Image _staminaBar;
        [SerializeField] private Text _statusText;

        private bool _isInitialized = false;

        public void Initialize(HungerManager hungerManager, StaminaManager staminaManager, StatusEffectManager statusEffectManager)
        {
            _hungerManager = hungerManager;
            _staminaManager = staminaManager;
            _statusEffectManager = statusEffectManager;
            _isInitialized = true;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<HungerChangedEvent>(UpdateDisplay);
            GameEventBus.Subscribe<StaminaChangedEvent>(UpdateDisplay);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HungerChangedEvent>(UpdateDisplay);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(UpdateDisplay);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_hungerManager != null && _hungerBar != null)
            {
                _hungerBar.fillAmount = Mathf.Clamp01((float)_hungerManager.CurrentHunger / _hungerManager.MaxHunger);
            }

            if (_staminaManager != null && _staminaBar != null)
            {
                _staminaBar.fillAmount = _staminaManager.StaminaPercent;
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (_statusText == null || _statusEffectManager == null)
                return;

            var statusString = "Status: ";
            var count = 0;

            foreach (var kvp in _statusEffectManager.ActiveEffects)
            {
                if (kvp.Value != null && kvp.Value.IsActive)
                {
                    statusString += kvp.Key + " ";
                    count++;
                    if (count >= 3) break;
                }
            }

            if (count == 0)
            {
                statusString = "Status: Normal";
            }

            _statusText.text = statusString;
        }

        private void UpdateDisplay(HungerChangedEvent evt) => UpdateDisplay();
        private void UpdateDisplay(StaminaChangedEvent evt) => UpdateDisplay();
    }
}
