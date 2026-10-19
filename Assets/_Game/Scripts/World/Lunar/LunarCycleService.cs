using CindarsHope.Core;
using UnityEngine;

namespace CindarsHope.World.Lunar
{
    [DisallowMultipleComponent]
    public class LunarCycleService : MonoBehaviour
    {
        private CindarsHope.Core.Time.TimeManager _timeManager;
        private LunarCycle _currentCycle;
        private bool _isInitialized = false;

        public LunarCycle CurrentCycle => _currentCycle;
        public bool IsInitialized => _isInitialized;

        public void Initialize(CindarsHope.Core.Time.TimeManager timeManager)
        {
            if (_isInitialized)
                return;

            _timeManager = timeManager;
            if (_timeManager != null && _timeManager.IsInitialized)
            {
                _currentCycle = LunarCycle.FromAbsoluteDay(_timeManager.CurrentDay);
            }
            else
            {
                _currentCycle = LunarCycle.FromAbsoluteDay(1);
            }

            _isInitialized = true;
            Debug.Log("LunarCycleService initialized.");
        }

        public void RestoreFromSaveData(int absoluteDay)
        {
            _currentCycle = LunarCycle.FromAbsoluteDay(absoluteDay);
        }

        private void Update()
        {
            if (!_isInitialized || _timeManager == null)
                return;

            LunarCycle newCycle = LunarCycle.FromAbsoluteDay(_timeManager.CurrentDay);
            if (newCycle.AbsoluteDay != _currentCycle.AbsoluteDay)
            {
                _currentCycle = newCycle;
            }
        }
    }
}
