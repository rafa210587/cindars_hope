using UnityEngine;

namespace CindarsHope.Save
{
    [DisallowMultipleComponent]
    public class SaveManager : MonoBehaviour
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
