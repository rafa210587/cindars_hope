using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.World.Calendar
{
    [DisallowMultipleComponent]
    public class GameCalendarService : MonoBehaviour
    {
        private CindarsHope.Core.Time.TimeManager _timeManager;
        private GameDate _currentDate;
        private bool _isInitialized = false;

        public GameDate CurrentDate => _currentDate;
        public bool IsInitialized => _isInitialized;

        public void Initialize(CindarsHope.Core.Time.TimeManager timeManager)
        {
            if (_isInitialized)
                return;

            _timeManager = timeManager;
            if (_timeManager != null && _timeManager.IsInitialized)
            {
                _currentDate = GameDate.FromAbsoluteDay(_timeManager.CurrentDay);
            }
            else
            {
                _currentDate = GameDate.FromAbsoluteDay(1);
            }

            _isInitialized = true;
            Debug.Log("GameCalendarService initialized.");
        }

        public void SetCurrentDate(GameDate date)
        {
            _currentDate = date;
        }

        public void RestoreFromSaveData(CalendarSaveData saveData)
        {
            if (saveData == null)
            {
                _currentDate = GameDate.FromAbsoluteDay(1);
                return;
            }

            _currentDate = GameDate.FromAbsoluteDay(saveData.AbsoluteDayIndex);
        }

        private void Update()
        {
            if (!_isInitialized || _timeManager == null)
                return;

            GameDate newDate = GameDate.FromAbsoluteDay(_timeManager.CurrentDay);
            if (newDate.AbsoluteDay != _currentDate.AbsoluteDay)
            {
                _currentDate = newDate;
            }
        }
    }
}
