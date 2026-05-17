using UnityEngine;

namespace CindarsHope.Player
{
    [DisallowMultipleComponent]
    public class PlayerManager : MonoBehaviour
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
