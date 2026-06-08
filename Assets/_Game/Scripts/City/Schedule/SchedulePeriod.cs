namespace CindarsHope.City.Schedule
{
    // Day periods as defined in City Layout Direction
    public enum SchedulePeriod
    {
        Morning = 0,        // 06:00-09:00
        WorkStart,          // 09:00-12:00
        Midday,             // 12:00-14:00
        WorkAfternoon,      // 14:00-18:00
        Evening,            // 18:00-21:00
        Night,              // 21:00-00:00
        SleepLateNight      // 00:00-06:00
    }

    public enum ScheduleModifierType
    {
        None = 0,
        Rain,
        Alihana,    // Lunar phase
        Senya,      // Lunar phase
        Nyx,        // Lunar phase
        Festival,
        Quest,
        Relationship
    }

    public static class SchedulePeriodHelper
    {
        public static SchedulePeriod FromHour(int hour)
        {
            if (hour >= 0 && hour < 6)   return SchedulePeriod.SleepLateNight;
            if (hour >= 6 && hour < 9)   return SchedulePeriod.Morning;
            if (hour >= 9 && hour < 12)  return SchedulePeriod.WorkStart;
            if (hour >= 12 && hour < 14) return SchedulePeriod.Midday;
            if (hour >= 14 && hour < 18) return SchedulePeriod.WorkAfternoon;
            if (hour >= 18 && hour < 21) return SchedulePeriod.Evening;
            return SchedulePeriod.Night;
        }

        public static int PeriodStartHour(SchedulePeriod period)
        {
            switch (period)
            {
                case SchedulePeriod.Morning:       return 6;
                case SchedulePeriod.WorkStart:     return 9;
                case SchedulePeriod.Midday:        return 12;
                case SchedulePeriod.WorkAfternoon: return 14;
                case SchedulePeriod.Evening:       return 18;
                case SchedulePeriod.Night:         return 21;
                default:                           return 0;
            }
        }
    }
}
