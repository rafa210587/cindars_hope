using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Core.Time
{
    [DisallowMultipleComponent]
    public class TimeManager : MonoBehaviour
    {
        public bool IsInitialized { get; private set; }
        public int CurrentDay { get; private set; } = 1;

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;
        }

        public void AdvanceDay()
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            CurrentDay++;
            GameEventBus.Publish(new DayStartedEvent(CurrentDay));
            Debug.Log($"Day advanced to {CurrentDay}.", this);
        }

        public void SetCurrentDay(int day)
        {
            CurrentDay = Mathf.Max(1, day);
            Debug.Log($"Day restored to {CurrentDay}.", this);
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
