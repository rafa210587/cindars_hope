using CindarsHope.UI.Calendar;
using CindarsHope.World.Calendar;
using CindarsHope.World.Lunar;
using CindarsHope.World.Weather;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_20 — projeção pura (sem UnityEngine) do widget permanente de relógio/calendário no
    /// canto superior-direito do GameplayHudCanvas (sup-dir, sob ele o minimapa F38 — Rule 3 de
    /// ui_rules.md / HUD_LAYOUT_SCENES §2). Deriva o texto/estado a partir dos VALORES já expostos
    /// pelos serviços existentes (GameCalendarService.CurrentDate, LunarCycleService.CurrentCycle,
    /// WorldWeatherService.CurrentWeather, GamePhaseChangedEvent.GamePhase) — não toca os serviços
    /// nem cria fonte de tempo paralela.
    ///
    /// EMENDA 2026-06-12-C: carrega também Nível/XP (barra fina junto ao relógio) a partir do
    /// PlayerLevelChangedEvent existente. EditMode-testável (CA-1).
    /// </summary>
    public sealed class ClockCalendarHudWidgetModel
    {
        // ---- Tempo / fase ----
        public bool IsNight { get; private set; }
        public string PhaseLabel => IsNight ? "Noite" : "Dia";

        // ---- Data ----
        public int AbsoluteDay { get; private set; } = 1;
        public int Year { get; private set; } = 1;
        public int DayInSeason { get; private set; } = 1;
        public int DayOfWeek { get; private set; } = 1;
        public Season Season { get; private set; } = Season.Primavera;

        public string SeasonName => CalendarDisplayNames.SeasonName(Season);
        public string WeekdayLabel => CalendarDisplayNames.WeekdayLabel(DayOfWeek);

        /// <summary>Linha compacta de data: "Semeio D3 · Dia 3/28 · Ano 1".</summary>
        public string DateLabel =>
            $"{SeasonName} {WeekdayLabel} · Dia {DayInSeason}/{GameDate.DaysPerSeason} · Ano {Year}";

        // ---- Clima ----
        public WeatherType Weather { get; private set; } = WeatherType.Clear;
        public string WeatherName => CalendarDisplayNames.WeatherName(Weather);

        // ---- Lua ----
        public LunarPhase LunarPhase { get; private set; } = LunarPhase.NewMoon;
        public int LunarDayInPhase { get; private set; } = 1;
        public string LunarPhaseName => CalendarDisplayNames.LunarPhaseName(LunarPhase);

        // ---- Progressão (EMENDA-C) ----
        public bool HasLevel { get; private set; }
        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public int XpForNextLevel { get; private set; }

        /// <summary>Fração 0..1 da barra fina de XP (0 quando o próximo nível é desconhecido).</summary>
        public float XpPercent
        {
            get
            {
                if (XpForNextLevel <= 0) return 0f;
                float p = (float)CurrentXp / XpForNextLevel;
                if (p < 0f) return 0f;
                if (p > 1f) return 1f;
                return p;
            }
        }

        public string LevelLabel => HasLevel ? $"Nv {Level}" : string.Empty;

        /// <summary>Atualiza a porção de data a partir de um <see cref="GameDate"/> do calendário.</summary>
        public void SetDate(GameDate date)
        {
            AbsoluteDay = date.AbsoluteDay;
            Year = date.Year;
            DayInSeason = date.DayInSeason;
            DayOfWeek = date.DayOfWeek;
            Season = date.CurrentSeason;
        }

        public void SetPhase(bool isNight) => IsNight = isNight;

        public void SetWeather(WeatherType weather) => Weather = weather;

        /// <summary>Atualiza a porção lunar a partir de um <see cref="LunarCycle"/>.</summary>
        public void SetLunar(LunarCycle cycle)
        {
            LunarPhase = cycle.CurrentPhase;
            LunarDayInPhase = cycle.DayInPhase;
        }

        /// <summary>Atualiza Nível/XP (EMENDA-C). xpForNext &lt;= 0 = próximo nível desconhecido.</summary>
        public void SetLevel(int level, int currentXp, int xpForNext)
        {
            HasLevel = true;
            Level = level < 1 ? 1 : level;
            CurrentXp = currentXp < 0 ? 0 : currentXp;
            XpForNextLevel = xpForNext < 0 ? 0 : xpForNext;
        }
    }
}
