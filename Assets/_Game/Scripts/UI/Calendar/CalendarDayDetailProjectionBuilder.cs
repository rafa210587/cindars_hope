using System.Collections.Generic;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;

namespace CindarsHope.UI.Calendar
{
    /// <summary>
    /// fable_20 — monta o <see cref="CalendarDayDetailModel"/> (CONTRACT_ONLY de SPEC 04) para a
    /// tela de Detalhe do Dia / aba Calendário, aplicando o spoiler gate canônico:
    /// "Calendar não revela segredo antes da descoberta" (Rule 8 de time_rules.md).
    ///
    /// REUSA, sem duplicar:
    ///  - <see cref="CalendarEventVisibilityPolicy"/> (SPEC 04) para a regra de visibilidade lunar;
    ///  - <see cref="CalendarDisplayNames"/> (fable_20) para nomes Vaalaran de estação/clima;
    ///  - <see cref="LunarVisibilityProjection"/> para os picos conhecidos das 3 luas;
    ///  - <see cref="CalendarBirthdayProjectionBuilder"/> (fable_57) para aniversários do dia.
    ///
    /// Puro/determinístico, sem UnityEngine. Os dados de hoje/amanhã (data, clima) entram como
    /// VALORES vindos dos serviços (WorldWeatherService / GameCalendarService); este builder não
    /// faz lookup de serviço por conta própria — recebe o que mostrar.
    /// </summary>
    public static class CalendarDayDetailProjectionBuilder
    {
        /// <summary>
        /// Constrói o modelo de detalhe do dia <paramref name="date"/>.
        /// </summary>
        /// <param name="date">Data exibida (normalmente hoje).</param>
        /// <param name="currentWeather">Clima de hoje (de WorldWeatherService).</param>
        /// <param name="tomorrowWeather">Clima de amanhã (de WorldWeatherService).</param>
        /// <param name="hasForecast">Se a previsão de amanhã é conhecida pelo jogador.</param>
        /// <param name="isLunarPeakKnown">Flag de descoberta por (lua, dia-da-estação) — spoiler gate.</param>
        public static CalendarDayDetailModel Build(
            GameDate date,
            WeatherType currentWeather,
            WeatherType tomorrowWeather,
            bool hasForecast,
            System.Func<VaalaraMoon, int, bool> isLunarPeakKnown)
        {
            var model = new CalendarDayDetailModel
            {
                AbsoluteDay = date.AbsoluteDay,
                DayOfSeason = date.DayInSeason,
                DayOfWeek = date.DayOfWeek,
                SeasonName = CalendarDisplayNames.SeasonName(date.CurrentSeason),
                Year = date.Year,
                CurrentWeather = CalendarDisplayNames.WeatherName(currentWeather),
                HasForecast = hasForecast,
                ForecastWeather = hasForecast ? CalendarDisplayNames.WeatherName(tomorrowWeather) : null
            };

            // Picos lunares CONHECIDOS (spoiler-gated). Picos não descobertos não aparecem.
            List<LunarPeakProjection> knownPeaks =
                LunarVisibilityProjection.BuildKnownPeaksForDay(date, isLunarPeakKnown);

            if (knownPeaks.Count > 0)
            {
                model.HasLunarEvent = true;
                model.KnownLunarEvent = $"Pico de {knownPeaks[0].MoonName}";
                foreach (var peak in knownPeaks)
                {
                    model.KnownEventsText.Add($"Pico de {peak.MoonName}");
                }
            }
            else
            {
                model.HasLunarEvent = false;
                model.KnownLunarEvent = null;
            }

            // Aniversários do dia (fable_57): informação pública, sempre visível.
            CalendarBirthdayProjectionBuilder.ApplyTo(model, date);

            return model;
        }
    }
}
