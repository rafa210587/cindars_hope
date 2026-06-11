using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.UI.HUD;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class InteractionPromptHudView : MonoBehaviour
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
            // Headless: Text/Image refs wired by human in Unity Editor.
            // ViewModel.ContextPrompt.IsVisible controls show/hide; DescriptionKey is the prompt text.
        }

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt) => gameObject.SetActive(evt.IsVisible);
    }
}
