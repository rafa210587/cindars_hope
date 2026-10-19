using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.Farm
{
    public class RainIrrigationIntegration : MonoBehaviour
    {
        [SerializeField] private GameCalendarService _calendarService;

        public bool IsRainingToday()
        {
            if (_calendarService == null || !_calendarService.IsInitialized)
                return false;

            GameDate today = _calendarService.CurrentDate;
            WeatherType weather = WeatherGenerator.GenerateWeather(today);

            return weather == WeatherType.Rainy || weather == WeatherType.Stormy;
        }

        public bool IsStormy()
        {
            if (_calendarService == null || !_calendarService.IsInitialized)
                return false;

            GameDate today = _calendarService.CurrentDate;
            WeatherType weather = WeatherGenerator.GenerateWeather(today);

            return weather == WeatherType.Stormy;
        }

        public void WaterExternalCropsIfRaining()
        {
            if (!IsRainingToday())
                return;

            // Farm system can hook here to water external tiles only
            // Does not affect greenhouse (interior) tiles
            // Does not activate magical irrigation
            // Called from farm day transition
        }

        public int GetWaterAmountTodayAsPercent()
        {
            if (!IsRainingToday())
                return 0;

            if (IsStormy())
                return 100; // Full watering from storm

            return 50; // Partial watering from rain
        }
    }
}
