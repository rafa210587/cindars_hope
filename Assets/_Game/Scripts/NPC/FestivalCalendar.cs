using CindarsHope.World.Calendar;

namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_28 — pure, deterministic check for the canonical (always-visible) festival days, mirroring
    /// the defaults seeded by <c>FestivalRegistry</c> (Planting day 14, Market day 56, Harvest day 98 of
    /// the year). Kept pure and Unity-free so <see cref="DialogueConditionContext"/> can decide the
    /// "festival day" axis without a scene reference or a global search (no GameObject.Find).
    ///
    /// Hidden/discoverable festivals are intentionally out of scope (the spec asks only for a generic
    /// festival line); this covers the three canonical public festivals used for dialogue flavour.
    /// </summary>
    public static class FestivalCalendar
    {
        // Day-in-year (1-112) of the three canonical public festivals, matching FestivalRegistry.
        public const int PlantingFestivalDayInYear = 14;
        public const int MarketFestivalDayInYear = 56;
        public const int HarvestFestivalDayInYear = 98;

        /// <summary>Day-in-year (1-112) for the given date.</summary>
        public static int DayInYear(GameDate date)
        {
            return date.DayInSeason + ((int)date.CurrentSeason * GameDate.DaysPerSeason);
        }

        /// <summary>True on one of the three canonical public festival days.</summary>
        public static bool IsCanonicalFestivalDay(GameDate date)
        {
            int dayInYear = DayInYear(date);
            return dayInYear == PlantingFestivalDayInYear
                || dayInYear == MarketFestivalDayInYear
                || dayInYear == HarvestFestivalDayInYear;
        }
    }
}
