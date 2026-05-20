using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.UI.Hotbar
{
    [DisallowMultipleComponent]
    public sealed class HotbarDebugInput : MonoBehaviour
    {
        [SerializeField] private SaveManager _saveManager;

        private void Update()
        {
            if (_saveManager == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) _saveManager.HotbarState.SelectSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _saveManager.HotbarState.SelectSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _saveManager.HotbarState.SelectSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) _saveManager.HotbarState.SelectSlot(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) _saveManager.HotbarState.SelectSlot(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) _saveManager.HotbarState.SelectSlot(5);
        }

        public void Configure(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }
    }
}