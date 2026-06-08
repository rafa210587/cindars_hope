using CindarsHope.World.Calendar;
using UnityEngine;

namespace CindarsHope.World.Weather
{
    public static class WeatherGenerator
    {
        public static WeatherType GenerateWeather(GameDate date)
        {
            int seed = date.AbsoluteDay % 4;

            // Deterministic weather based on day mod 4
            return (WeatherType)seed;
        }

        public static WeatherType GenerateWeatherForSeason(Season season)
        {
            int baseIndex = (int)season % 4;
            return (WeatherType)baseIndex;
        }

        public static string GetWeatherDescription(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Clear => "Céu limpo",
                WeatherType.Cloudy => "Nublado",
                WeatherType.Rainy => "Chovendo",
                WeatherType.Stormy => "Tempestade",
                _ => "Desconhecido"
            };
        }
    }
}
