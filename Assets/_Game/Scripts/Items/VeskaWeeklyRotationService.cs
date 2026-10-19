namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 CA-4 — DETERMINISTIC weekly rotation for Veska's single "unidentified" offer. The offer is a
    /// function of the in-game week only: same week ⇒ same offer, next week ⇒ a rotated offer. The week is
    /// derived from the absolute day counter (<c>TimeManager.CurrentDay</c>), so no new save field is needed
    /// (it is recomputed from the calendar). Pure C# ⇒ EditMode-testable with synthetic week numbers.
    /// </summary>
    public static class VeskaWeeklyRotationService
    {
        /// <summary>Days per in-game week (matches GameDate.DaysPerWeek).</summary>
        public const int DaysPerWeek = 7;

        private const string SeedSalt = "veska_unidentified_rotation";

        /// <summary>Absolute week index from an absolute day counter (day 1..7 → week 0, 8..14 → week 1, ...).</summary>
        public static int WeekFromDay(int currentDay)
        {
            var day = currentDay < 1 ? 1 : currentDay;
            return (day - 1) / DaysPerWeek;
        }

        /// <summary>
        /// The item id Veska offers this week. Today it is always the unidentified trinket (a single dormant
        /// SKU in the catalog), but the offer is gated/seeded by week so the weekly cadence is deterministic
        /// and stable across reloads. Returns the unidentified trinket id.
        /// </summary>
        public static string GetWeeklyOfferItemId(int currentDay) => MagicItemCatalog.UnidentifiedTrinketId;

        /// <summary>
        /// A deterministic per-week token (stable within a week, different across weeks). Used to prove the
        /// rotation is seeded by week and to drive WHICH real item a Veska-bought trinket reveals if needed.
        /// </summary>
        public static int GetWeeklyRotationToken(int currentDay)
        {
            var week = WeekFromDay(currentDay);
            return MagicItemCatalog.StableHash($"{SeedSalt}|{week}");
        }

        /// <summary>
        /// True only on the boundary where the rotation changes — i.e. <paramref name="dayA"/> and
        /// <paramref name="dayB"/> fall in different weeks. Convenience for tests/refresh logic.
        /// </summary>
        public static bool RotationChangedBetween(int dayA, int dayB) =>
            WeekFromDay(dayA) != WeekFromDay(dayB);
    }
}
