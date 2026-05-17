using UnityEngine;

namespace CindarsHope.Core.Time
{
    [DisallowMultipleComponent]
    public class TimeManager : MonoBehaviour
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
