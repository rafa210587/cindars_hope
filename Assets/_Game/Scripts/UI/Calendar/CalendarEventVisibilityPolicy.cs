namespace CindarsHope.UI.Calendar
{
    /// <summary>
    /// SPEC 04: Spoiler-safe visibility policy for calendar events.
    /// Prevents revealing hidden, future, or secret states in day detail.
    /// </summary>
    public static class CalendarEventVisibilityPolicy
    {
        /// <summary>
        /// Never show these event types in calendar, even if technically data exists.
        /// </summary>
        public static bool CanShowEvent(string eventType)
        {
            return eventType switch
            {
                // Level 101 final event
                "level_101_final_event" => false,
                // Mana exact unlock condition
                "mana_unlock_condition" => false,
                // Nyx hidden character
                "nyx_hidden_character" => false,
                // Future expansion events
                "future_pets" => false,
                "future_romance" => false,
                // Valid events
                _ => true
            };
        }

        /// <summary>
        /// Only show festival if discovered or direction allows public listing.
        /// </summary>
        public static bool CanShowFestival(bool isHidden, bool isDiscovered)
        {
            if (!isHidden)
                return true; // Public festival always shown
            if (isDiscovered)
                return true; // Hidden but discovered
            return false; // Hidden and not discovered = never shown
        }

        /// <summary>
        /// Only show lunar event if already discovered/known.
        /// </summary>
        public static bool CanShowLunarEvent(bool isKnown)
        {
            return isKnown;
        }

        /// <summary>
        /// Show order deadline if order is active/tracked.
        /// </summary>
        public static bool CanShowOrderDeadline(bool isActive)
        {
            return isActive;
        }

        /// <summary>
        /// Show quest waiting condition if quest is discovered.
        /// For conditions not yet discovered, show generic hint only.
        /// </summary>
        public static string GetQuestConditionDisplay(string exactCondition, bool isConditionDiscovered)
        {
            if (isConditionDiscovered)
                return exactCondition; // Exact condition can be shown
            return "Waiting for something..."; // Generic hint without spoiler
        }

        /// <summary>
        /// Only show shop closed/open state if known from reliable source (event, NPC schedule, etc).
        /// </summary>
        public static bool CanShowShopState(bool stateIsKnown)
        {
            return stateIsKnown;
        }
    }
}
