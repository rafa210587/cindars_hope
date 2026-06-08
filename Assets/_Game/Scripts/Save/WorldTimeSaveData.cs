using System.Collections.Generic;

namespace CindarsHope.Save
{
    public class WorldTimeSaveData
    {
        // Time / Calendar state
        public int CurrentDay { get; set; } = 1;
        public int CurrentYear { get; set; } = 1;

        // Weather state
        public int CurrentWeatherType { get; set; } = 0; // Clear
        public int TomorrowWeatherType { get; set; } = 0; // Clear
        public int WeatherSeed { get; set; } = 1;

        // Lunar state
        public int CurrentLunarPhaseType { get; set; } = 0; // NewMoon
        public List<int> KnownLunarEvents { get; set; } = new List<int>();

        // Festival state
        public List<string> CompletedFestivals { get; set; } = new List<string>();

        // Internal state
        public int LastProcessedDay { get; set; } = 1;
        public int DayTransitionVersion { get; set; } = 1;

        public WorldTimeSaveData() { }

        public void ResetToDefaults()
        {
            CurrentDay = 1;
            CurrentYear = 1;
            CurrentWeatherType = 0;
            TomorrowWeatherType = 0;
            WeatherSeed = 1;
            CurrentLunarPhaseType = 0;
            KnownLunarEvents.Clear();
            CompletedFestivals.Clear();
            LastProcessedDay = 1;
            DayTransitionVersion = 1;
        }
    }
}
