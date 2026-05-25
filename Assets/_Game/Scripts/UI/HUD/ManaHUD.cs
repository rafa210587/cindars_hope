using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public class ManaHUD : MonoBehaviour
    {
        [SerializeField] private ManaManager _manaManager;
        [SerializeField] private Image _manaBar;
        [SerializeField] private Text _manaText;

        private void OnEnable()
        {
            GameEventBus.Subscribe<ManaChangedEvent>(OnManaChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ManaChangedEvent>(OnManaChanged);
        }

        private void Start()
        {
            if (_manaManager != null)
            {
                UpdateManaDisplay(_manaManager.CurrentMana, _manaManager.MaxMana);
            }
        }

        private void OnManaChanged(ManaChangedEvent evt)
        {
            UpdateManaDisplay(evt.CurrentMana, evt.MaxMana);
        }

        private void UpdateManaDisplay(int currentMana, int maxMana)
        {
            if (_manaBar != null && maxMana > 0)
            {
                _manaBar.fillAmount = (float)currentMana / maxMana;
            }

            if (_manaText != null)
            {
                _manaText.text = $"{currentMana}/{maxMana}";
            }
        }
    }
}
