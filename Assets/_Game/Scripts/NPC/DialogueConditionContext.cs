using System;
using System.Collections.Generic;
using CindarsHope.City.Services;
using CindarsHope.NPC.Friendship;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;

namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_28 — immutable snapshot of the world used to evaluate <see cref="DialogueLineCondition"/>.
    /// Built at a SINGLE point (<see cref="FromWorld"/>) from values the caller already holds plus the
    /// real, already-owned singleton services (WorldWeatherService, FriendshipService) and the live
    /// QuestFlagService accessor. The selector and conditions read only this snapshot, so EditMode
    /// tests inject a synthetic context without touching Unity. No state is persisted here —
    /// season/weather/friendship/flags all have their own save owners (this spec adds no save section).
    ///
    /// Pure C#: no MonoBehaviour, no Unity reference, no GameObject.Find/FindObjectOfType, no event bus.
    /// </summary>
    public sealed class DialogueConditionContext
    {
        public string NpcId { get; }
        public int Day { get; }
        public Season Season { get; }
        public WeatherType Weather { get; }
        public DialogueTimeBand TimeBand { get; }
        public int FriendshipLevel { get; }
        public bool IsFestivalDay { get; }

        /// <summary>
        /// fable_39 — the player's currently INFERRED class title id (e.g. "ui.class.warrior"), or
        /// empty when none/Colono. Additive axis read by <see cref="DialogueLineCondition"/>'s
        /// optional <c>RequiredInferredTitleId</c>; defaults to empty so existing contexts and lines
        /// are unaffected.
        /// </summary>
        public string InferredTitleId { get; }

        /// <summary>
        /// fable_46 — the player's current romance stage with this NPC (0 None .. 3 Compromisso). Read
        /// by <see cref="DialogueLineCondition"/>'s RequiresPartner / MinRomanceStage. Defaults to 0 so
        /// existing contexts/lines are unaffected (zero behavior change).
        /// </summary>
        public int RomanceStage { get; }

        private readonly HashSet<string> _flags;

        public DialogueConditionContext(
            string npcId,
            int day,
            Season season,
            WeatherType weather,
            DialogueTimeBand timeBand,
            int friendshipLevel,
            bool isFestivalDay,
            IEnumerable<string> activeFlags = null,
            string inferredTitleId = null,
            int romanceStage = 0)
        {
            NpcId = npcId ?? string.Empty;
            Day = day;
            Season = season;
            Weather = weather;
            TimeBand = timeBand;
            FriendshipLevel = friendshipLevel;
            IsFestivalDay = isFestivalDay;
            InferredTitleId = inferredTitleId ?? string.Empty;
            RomanceStage = romanceStage;
            _flags = new HashSet<string>(StringComparer.Ordinal);
            if (activeFlags != null)
            {
                foreach (var f in activeFlags)
                {
                    if (!string.IsNullOrEmpty(f)) _flags.Add(f);
                }
            }
        }

        public bool IsFlagSet(string flagId) =>
            !string.IsNullOrEmpty(flagId) && _flags.Contains(flagId);

        /// <summary>
        /// Maps an in-game hour (0-23) to a coarse time band. Manha &lt; 12, Tarde &lt; 18, else Noite.
        /// Pure and deterministic so tests can pin the band directly via the ctor instead.
        /// </summary>
        public static DialogueTimeBand BandFromHour(int hour)
        {
            if (hour < 12) return DialogueTimeBand.Manha;
            if (hour < 18) return DialogueTimeBand.Tarde;
            return DialogueTimeBand.Noite;
        }

        /// <summary>
        /// Builds the runtime context from the cached calendar primitives the caller already holds
        /// (<paramref name="absoluteDay"/>, <paramref name="hour"/>) plus the live singleton services
        /// for weather and friendship and the live QuestFlagService. This is the ONLY place that
        /// touches world services; everything downstream is pure. Missing services degrade gracefully
        /// to safe defaults (Clear / level 0 / no flags) so dialogue never throws and the fallback
        /// line always remains selectable. Season and festival-day are derived purely from the day,
        /// so no global scene search is needed (no GameObject.Find / FindObjectOfType).
        /// </summary>
        /// <param name="npcId">NPC whose lines are being selected.</param>
        /// <param name="absoluteDay">Cached absolute in-game day (>= 1).</param>
        /// <param name="hour">Current in-game hour (0-23) for the time band.</param>
        public static DialogueConditionContext FromWorld(string npcId, int absoluteDay, int hour)
        {
            var date = GameDate.FromAbsoluteDay(absoluteDay);
            var season = date.CurrentSeason;
            bool festival = FestivalCalendar.IsCanonicalFestivalDay(date);

            var weather = WeatherType.Clear;
            var weatherService = WorldWeatherService.Instance;
            if (weatherService != null)
            {
                weather = weatherService.CurrentWeather;
            }

            int friendship = 0;
            var friendshipService = FriendshipService.Instance;
            if (friendshipService != null && !string.IsNullOrEmpty(npcId))
            {
                friendship = friendshipService.GetLevel(npcId);
            }

            IEnumerable<string> flags = null;
            var flagService = CityServiceRuntimeBootstrap.FlagService;
            if (flagService != null)
            {
                flags = flagService.GetAllActive().Keys;
            }

            // fable_39: read the current inferred-class title id (degrades to empty when the runtime
            // is absent — dialogue never throws and the fallback line stays selectable).
            var inferredTitleId = CindarsHope.Player.InferredClassRuntime.CurrentProfile.TitleId;

            // fable_46: read the current romance stage with this NPC (degrades to 0/None when the
            // romance service is absent — dialogue never throws and partner lines simply do not gate in).
            int romanceStage = 0;
            var romanceService = Friendship.RomanceService.Instance;
            if (romanceService != null && !string.IsNullOrEmpty(npcId))
            {
                romanceStage = (int)romanceService.GetStage(npcId);
            }

            return new DialogueConditionContext(
                npcId, absoluteDay, season, weather, BandFromHour(hour), friendship, festival, flags,
                inferredTitleId, romanceStage);
        }
    }
}
