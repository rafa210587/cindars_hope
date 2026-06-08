using CindarsHope.Core;
using CindarsHope.Core.Time;
using CindarsHope.Save;
using CindarsHope.World.Calendar;
using CindarsHope.World.Lunar;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.World
{
    public class WorldTimeProvider : MonoBehaviour
    {
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private GameCalendarService _calendarService;
        [SerializeField] private LunarCycleService _lunarService;

        private WorldTimeSaveData _saveData;

        public void Initialize(TimeManager timeManager, GameCalendarService calendarService, LunarCycleService lunarService)
        {
            _timeManager = timeManager;
            _calendarService = calendarService;
            _lunarService = lunarService;
            _saveData = new WorldTimeSaveData();
        }

        public WorldTimeSaveData GetSaveData()
        {
            if (_timeManager != null && _timeManager.IsInitialized)
            {
                _saveData.CurrentDay = _timeManager.CurrentDay;
                if (_calendarService != null && _calendarService.IsInitialized)
                {
                    _saveData.CurrentYear = _calendarService.CurrentDate.Year;
                }
            }

            if (_lunarService != null && _lunarService.IsInitialized)
            {
                _saveData.CurrentLunarPhaseType = (int)_lunarService.CurrentCycle.CurrentPhase;
            }

            return _saveData;
        }

        public void RestoreFromSaveData(WorldTimeSaveData saveData)
        {
            if (saveData == null)
            {
                _saveData = new WorldTimeSaveData();
                return;
            }

            _saveData = saveData;

            if (_timeManager != null)
            {
                _timeManager.SetCurrentDay(_saveData.CurrentDay);
            }

            if (_calendarService != null)
            {
                _calendarService.SetCurrentDate(GameDate.FromAbsoluteDay(_saveData.CurrentDay));
            }

            if (_lunarService != null)
            {
                _lunarService.RestoreFromSaveData(_saveData.CurrentDay);
            }
        }
    }
}
