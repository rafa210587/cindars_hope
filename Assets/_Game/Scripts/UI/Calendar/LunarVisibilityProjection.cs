using System.Collections.Generic;
using CindarsHope.World.Calendar;

namespace CindarsHope.UI.Calendar
{
    /// <summary>As três luas de Vaalara (Rule 5 de time_rules.md, lore §27-29).</summary>
    public enum VaalaraMoon
    {
        Alihana = 0, // branca — memória, sonhos, profecia
        Senya = 1,   // escarlate — caos, magia, festivais
        Nyx = 2      // oculta — noite, segredos, cave, night shop
    }

    /// <summary>Projeção pura de um pico lunar conhecido (D7/D14/D21/D28, Rule 6 design model).</summary>
    public readonly struct LunarPeakProjection
    {
        public VaalaraMoon Moon { get; }
        public int DayInSeason { get; }
        public string MoonName { get; }
        public bool IsTonight { get; }

        public LunarPeakProjection(VaalaraMoon moon, int dayInSeason, string moonName, bool isTonight)
        {
            Moon = moon;
            DayInSeason = dayInSeason;
            MoonName = moonName;
            IsTonight = isTonight;
        }
    }

    /// <summary>
    /// fable_20 — projeta o estado das TRÊS luas nomeadas (decisão v2 6.3 / EMENDA 2026-06-12-D:
    /// as 3 luas aparecem no calendário DESDE O INÍCIO) e a cadência de picos por estação
    /// (Rule 6 design model: D7 Alihana, D14 Senya, D21 Nyx, D28 evento sazonal/maior).
    ///
    /// Os picos continuam SPOILER-GATED: só são marcados como conhecidos quando a descoberta
    /// (uma flag por pico) já ocorreu — usa o <see cref="CalendarEventVisibilityPolicy.CanShowLunarEvent"/>
    /// já existente (SPEC 04), não cria política nova. Puro/determinístico (re-derivável de
    /// AbsoluteDay, Rule 6 constraint), sem UnityEngine, sem RNG.
    /// </summary>
    public static class LunarVisibilityProjection
    {
        /// <summary>Dia-da-estação do pico de cada lua (Rule 6 design table).</summary>
        public const int AlihanaPeakDay = 7;
        public const int SenyaPeakDay = 14;
        public const int NyxPeakDay = 21;
        public const int SeasonalPeakDay = 28;

        public static string MoonName(VaalaraMoon moon)
        {
            switch (moon)
            {
                case VaalaraMoon.Alihana: return "Alihana";
                case VaalaraMoon.Senya: return "Senya";
                case VaalaraMoon.Nyx: return "Nyx";
                default: return moon.ToString();
            }
        }

        /// <summary>A lua cujo pico cai neste dia-da-estação, se houver (D7/D14/D21).</summary>
        public static bool TryGetMoonForPeakDay(int dayInSeason, out VaalaraMoon moon)
        {
            switch (dayInSeason)
            {
                case AlihanaPeakDay: moon = VaalaraMoon.Alihana; return true;
                case SenyaPeakDay: moon = VaalaraMoon.Senya; return true;
                case NyxPeakDay: moon = VaalaraMoon.Nyx; return true;
                default: moon = VaalaraMoon.Alihana; return false;
            }
        }

        /// <summary>
        /// Picos CONHECIDOS visíveis na data (spoiler-gated). <paramref name="isPeakKnown"/> recebe
        /// a flag de descoberta de cada (moon,day) — um pico não descoberto fica oculto (Rule 8 /
        /// "Calendar não revela segredo antes da descoberta").
        /// </summary>
        public static List<LunarPeakProjection> BuildKnownPeaksForDay(
            GameDate date,
            System.Func<VaalaraMoon, int, bool> isPeakKnown)
        {
            var peaks = new List<LunarPeakProjection>();
            int dayInSeason = date.DayInSeason;

            if (TryGetMoonForPeakDay(dayInSeason, out var moon))
            {
                bool known = isPeakKnown != null && isPeakKnown(moon, dayInSeason);
                // Spoiler gate: reusa a política existente (não duplica regra).
                if (CalendarEventVisibilityPolicy.CanShowLunarEvent(known))
                {
                    peaks.Add(new LunarPeakProjection(moon, dayInSeason, MoonName(moon), isTonight: true));
                }
            }

            return peaks;
        }
    }
}
