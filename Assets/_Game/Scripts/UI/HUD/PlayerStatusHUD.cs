using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public class PlayerStatusHUD : MonoBehaviour
    {
        [SerializeField] private Image _hungerBar;
        [SerializeField] private Image _staminaBar;
        [SerializeField] private Text _hungerLabel;
        [SerializeField] private Text _staminaLabel;
        [SerializeField] private Text _statusEffectsLabel;
        [SerializeField] private Color _hungerColor = new Color(1f, 0.8f, 0f, 1f);
        [SerializeField] private Color _staminaColor = new Color(0.2f, 0.8f,0.2f, 1f);

        private void OnEnable()
        {
            GameEventBus.Subscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Subscribe<StaminaChangedEvent>(OnStaminaChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<HungerChangedEvent>(OnHungerChanged);
            GameEventBus.Unsubscribe<StaminaChangedEvent>(OnStaminaChanged);
        }

        private void OnHungerChanged(HungerChangedEvent evt)
        {
            if (_hungerBar != null)
            {
                float hungerPercent = evt.MaxValue > 0 ? (float)evt.CurrentValue / evt.MaxValue : 0f;
                _hungerBar.fillAmount = Mathf.Clamp01(hungerPercent);
            }

            if (_hungerLabel != null)
            {
                _hungerLabel.text = $"Hunger: {evt.CurrentValue}/{evt.MaxValue}";
            }
        }

        private void OnStaminaChanged(StaminaChangedEvent evt)
        {
            if (_staminaBar != null)
            {
                float staminaPercent = evt.MaxStamina > 0 ? (float)evt.CurrentStamina / evt.MaxStamina : 0f;
                _staminaBar.fillAmount = Mathf.Clamp01(staminaPercent);
            }

            if (_staminaLabel != null)
            {
                _staminaLabel.text = $"Stamina: {evt.CurrentStamina}/{evt.MaxStamina}";
            }
        }
    }
}
