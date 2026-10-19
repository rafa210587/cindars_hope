using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.Farm
{
    /// <summary>
    /// Wrapper fino que liga a RainIrrigationIntegration (módulo WAVE 02, intocado) ao
    /// FarmPlotRegistry real. Em dia de chuva/tempestade, rega os canteiros no início do dia.
    /// Roda APÓS os handlers de DayStarted dos plots (end of frame) para regar o estado
    /// já avançado/seco do novo dia.
    /// Regra: tempestade (100%) rega solo arado e plantado; chuva (50%) rega apenas plantado.
    /// </summary>
    [DisallowMultipleComponent]
    public class RainIrrigationRunner : MonoBehaviour
    {
        [SerializeField] private RainIrrigationIntegration _integration;
        [SerializeField] private FarmPlotRegistry _plotRegistry;

        public void Configure(RainIrrigationIntegration integration, FarmPlotRegistry plotRegistry)
        {
            _integration = integration;
            _plotRegistry = plotRegistry;
        }

        public static bool ShouldWaterState(FarmPlotState state, int waterPercent)
        {
            if (waterPercent <= 0)
            {
                return false;
            }

            if (state == FarmPlotState.PlantedDry)
            {
                return true;
            }

            return waterPercent >= 100 && state == FarmPlotState.TilledDry;
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StartCoroutine(WaterAtEndOfFrame());
        }

        private IEnumerator WaterAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            var watered = WaterPlotsForToday();
            if (watered > 0)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent($"A chuva regou {watered} canteiro(s)."));
            }
        }

        private int WaterPlotsForToday()
        {
            var percent = ResolveWaterPercent();
            if (percent <= 0 || _plotRegistry == null || _plotRegistry.Plots == null)
            {
                return 0;
            }

            var greenhouse = Watering.GreenhouseRuntimeHost.Instance;

            var watered = 0;
            foreach (var plot in _plotRegistry.Plots)
            {
                if (plot == null || !ShouldWaterState(plot.State, percent))
                {
                    continue;
                }

                // fable_55: a chuva NÃO rega os canteiros da estufa (IsRainExcluded). Canteiro
                // comum segue regado normalmente (regressão preservada).
                if (greenhouse != null && greenhouse.IsRainExcluded(plot.PlotId))
                {
                    continue;
                }

                if (plot.TryWaterFromRain())
                {
                    watered++;
                }
            }

            return watered;
        }

        private int ResolveWaterPercent()
        {
            // Preferência: módulo original (quando o calendário estiver wired na cena).
            if (_integration != null)
            {
                var percent = _integration.GetWaterAmountTodayAsPercent();
                if (percent > 0)
                {
                    return percent;
                }
            }

            // Fallback: autoridade de clima runtime (mesma função determinística).
            var weatherService = WorldWeatherService.Instance;
            if (weatherService == null)
            {
                return 0;
            }

            if (weatherService.CurrentWeather == WeatherType.Stormy)
            {
                return 100;
            }

            return weatherService.CurrentWeather == WeatherType.Rainy ? 50 : 0;
        }
    }
}
