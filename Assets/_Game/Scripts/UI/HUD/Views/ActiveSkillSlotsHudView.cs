using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class ActiveSkillSlotsHudView : MonoBehaviour
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
            // Headless: slot icons wired by human in Unity Editor.
            // ViewModel.ActiveSkillSlots (capped at 4) — slots R/T/Y/G.
            var errors = FinalHudGuardValidator.Validate(_viewModel);
            if (errors.Count > 0)
                Debug.LogWarning($"[ActiveSkillSlotsHudView] Guard violation: {string.Join(", ", errors)}");
        }

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);
    }
}
