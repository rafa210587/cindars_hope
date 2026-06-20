namespace CindarsHope.City.Schedule
{
    // fable_19 (CA-1): vocabulário SEMÂNTICO de períodos absorvido para o sistema canônico em
    // NPC/Schedule/NpcSchedulePeriod (crosswalk 1:1 de janelas horárias + ToTimeBlock). Este enum
    // permanece NÃO-obsoleto porque ainda é consumido por Dialogue/* (DialogueContext/DialogueCondition)
    // e City/FarmVisits/FarmVisitRule — fora do escopo de edição desta spec. Código NOVO deve usar
    // CindarsHope.NPC.Schedule.NpcSchedulePeriod. As classes-MOTOR (NpcScheduleDefinition/Resolver)
    // estão [Obsolete]; só o vocabulário sobrevive até a migração dos consumidores de Dialogue.

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
