using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.UI.HUD.Views
{
    [DisallowMultipleComponent]
    public sealed class ModalBlockerHudView : MonoBehaviour
    {
        private bool _hudVisible = true;

        private void OnEnable() => GameEventBus.Subscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);
        private void OnDisable() => GameEventBus.Unsubscribe<HudVisibilityChangedEvent>(OnVisibilityChanged);

        private void OnVisibilityChanged(HudVisibilityChangedEvent evt)
        {
            _hudVisible = evt.IsVisible;
            // Headless: canvas group alpha wired by human in Unity Editor.
            // When _hudVisible == false, HUD should be transparent/inactive.
        }

        public bool IsBlocking => !_hudVisible;
    }
}
