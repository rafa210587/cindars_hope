using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class QuestTrackerHudView : MonoBehaviour
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
            // Headless: Text ref wired by human in Unity Editor.
            // ViewModel.QuestPrompt contains the active quest tracker string.
        }

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);
    }
}
