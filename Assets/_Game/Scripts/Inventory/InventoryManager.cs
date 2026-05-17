using UnityEngine;

namespace CindarsHope.Inventory
{
    [DisallowMultipleComponent]
    public class InventoryManager : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
        }
    }
}
