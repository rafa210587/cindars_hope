using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World.Calendar;
using UnityEngine;

namespace CindarsHope.World.Weather
{
    /// <summary>
    /// Autoridade runtime de clima. Fixa o clima do dia no DayStartedEvent usando o
    /// WeatherGenerator determinístico existente (WAVE 02) e publica WeatherChangedEvent.
    /// Clima é re-derivável por dia — não entra no save.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldWeatherService : MonoBehaviour
    {
        private static WorldWeatherService _instance;

        public static WorldWeatherService Instance => _instance;

        public int CurrentDay { get; private set; } = 1;
        public WeatherType CurrentWeather { get; private set; }
        public WeatherType TomorrowWeather { get; private set; }

        public static WeatherType ResolveWeatherForDay(int dayNumber)
        {
            return WeatherGenerator.GenerateWeather(GameDate.FromAbsoluteDay(dayNumber));
        }

        public static bool IsWetWeather(WeatherType weather)
        {
            return weather == WeatherType.Rainy || weather == WeatherType.Stormy;
        }

        public void SetDay(int dayNumber)
        {
            CurrentDay = Mathf.Max(1, dayNumber);
            CurrentWeather = ResolveWeatherForDay(CurrentDay);
            TomorrowWeather = ResolveWeatherForDay(CurrentDay + 1);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetDay(CurrentDay);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            SetDay(evt.DayNumber);
            GameEventBus.Publish(new WeatherChangedEvent(CurrentDay, CurrentWeather));
            Debug.Log($"WorldWeatherService: dia {CurrentDay} — {WeatherGenerator.GetWeatherDescription(CurrentWeather)}.");
        }
    }

    /// <summary>
    /// Garante WorldWeatherService em runtime (mesmo padrão do FarmDailyGoalRuntimeBootstrap).
    /// FindAnyObjectByType é permitido aqui: wiring de bootstrap, não comunicação de gameplay.
    /// </summary>
    public static class WorldWeatherRuntimeBootstrap
    {
        public static WorldWeatherService Install(Transform owner)
        {
            var existing = Object.FindAnyObjectByType<WorldWeatherService>();
            if (existing != null) return existing;

            var go = new GameObject("WorldWeatherService");
            if (owner != null) go.transform.SetParent(owner, false);
            else Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<WorldWeatherService>();

            var timeManager = Object.FindAnyObjectByType<CindarsHope.Core.Time.TimeManager>();
            if (timeManager != null && timeManager.IsInitialized)
            {
                service.SetDay(timeManager.CurrentDay);
            }

            Debug.Log("[WorldWeatherRuntimeBootstrap] WorldWeatherService instanciado via bootstrap.");
            return service;
        }
    }
}
