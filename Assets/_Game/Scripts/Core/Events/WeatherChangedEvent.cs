using CindarsHope.World.Weather;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o clima do dia é fixado (no DayStartedEvent).
    /// Fonte única: WorldWeatherService (determinístico por dia via WeatherGenerator).
    /// </summary>
    public readonly struct WeatherChangedEvent
    {
        public int DayNumber { get; }
        public WeatherType Weather { get; }

        public WeatherChangedEvent(int dayNumber, WeatherType weather)
        {
            DayNumber = dayNumber;
            Weather = weather;
        }
    }
}
