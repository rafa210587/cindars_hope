using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class StatusBarsHudView : MonoBehaviour
    {
        private GameplayHudViewModel _viewModel;

        public void Initialize(GameplayHudViewModel viewModel) => _viewModel = viewModel;

        private void OnEnable() => GameEventBus.Subscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);
        private void OnDisable() => GameEventBus.Unsubscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);

        private void Update()
        {
            if (_viewModel == null) return;
            Refresh();
        }

        private void Refresh()
        {
            // Headless: Canvas Image/Slider refs wired by human in Unity Editor.
            // ViewModel state is authoritative: Hp/MaxHp, Stamina/MaxStamina, Mp/MaxMp, HungerCompact.
        }

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);
    }
}
