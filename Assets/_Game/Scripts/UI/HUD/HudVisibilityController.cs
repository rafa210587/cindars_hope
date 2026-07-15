using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public sealed class HudVisibilityController : MonoBehaviour
    {
        private bool _wasVisible = true;
        private IModalRuntime _modalManager;

        private void Update()
        {
            if (_modalManager == null)
                _modalManager = GameBootstrap.Instance?.ModalManager;

            var nowVisible = _modalManager == null || !_modalManager.HasActiveModal;
            if (nowVisible != _wasVisible)
            {
                _wasVisible = nowVisible;
                GameEventBus.Publish(new HudVisibilityChangedEvent(nowVisible));
            }
        }

        public bool IsHudVisible => _wasVisible;
    }
}
