using CindarsHope.World.Calendar;
using CindarsHope.World.Lunar;
using CindarsHope.World.Weather;

namespace CindarsHope.UI.Calendar
{
    /// <summary>
    /// fable_20 — tabela pura id→nome de exibição (player-facing) para o widget de relógio e a
    /// aba Calendário, sem reordenar nem mexer no enum <see cref="Season"/> (Rule 4 de
    /// time_rules.md: a ORDEM é canônica, saves/GameDate dependem dela; só adicionamos um mapa de
    /// nome). Resolve a pendência aberta "wire the canonical Vaalaran season names (fable_20)".
    ///
    /// Nomes canônicos (decisão v2 6.1, EMENDA 2026-06-12-D): Semeio / Brasa / Véu / Gelo.
    /// Dias D1-D7 NÃO têm nome próprio (decisão v2 6.1) — exibe-se o índice.
    ///
    /// Sem dependência de UnityEngine: EditMode-testável em isolamento.
    /// </summary>
    public static class CalendarDisplayNames
    {
        /// <summary>Nome Vaalaran canônico da estação (Rule 4), mapeado do enum por ordem.</summary>
        public static string SeasonName(Season season)
        {
            switch (season)
            {
                case Season.Primavera: return "Semeio";
                case Season.Verao: return "Brasa";
                case Season.Outono: return "Véu";
                case Season.Inverno: return "Gelo";
                default: return season.ToString();
            }
        }

        /// <summary>Rótulo curto do dia da semana sem nome próprio (decisão v2 6.1): "D1".."D7".</summary>
        public static string WeekdayLabel(int dayOfWeek)
        {
            if (dayOfWeek < 1) dayOfWeek = 1;
            if (dayOfWeek > GameDate.DaysPerWeek) dayOfWeek = GameDate.DaysPerWeek;
            return "D" + dayOfWeek;
        }

        /// <summary>Descrição player-facing do clima (reusa a do WeatherGenerator existente).</summary>
        public static string WeatherName(WeatherType weather)
        {
            return WeatherGenerator.GetWeatherDescription(weather);
        }

        /// <summary>
        /// Nome em PT da fase lunar genérica de 8 fases (modelo runtime implementado, Rule 6).
        /// </summary>
        public static string LunarPhaseName(LunarPhase phase)
        {
            switch (phase)
            {
                case LunarPhase.NewMoon: return "Lua Nova";
                case LunarPhase.WaxingCrescent: return "Crescente Côncava";
                case LunarPhase.FirstQuarter: return "Quarto Crescente";
                case LunarPhase.WaxingGibbous: return "Crescente Gibosa";
                case LunarPhase.FullMoon: return "Lua Cheia";
                case LunarPhase.WaningGibbous: return "Minguante Gibosa";
                case LunarPhase.LastQuarter: return "Quarto Minguante";
                case LunarPhase.WaningCrescent: return "Minguante Côncava";
                default: return phase.ToString();
            }
        }
    }
}
