using System.Collections.Generic;

namespace CindarsHope.UI.Calendar
{
    /// <summary>
    /// SPEC 04: Calendar Day Detail projection model.
    /// Contains only known/visible data to avoid spoilers.
    /// </summary>
    public class CalendarDayDetailModel
    {
        public int AbsoluteDay { get; set; }
        public int DayOfSeason { get; set; }
        public int DayOfWeek { get; set; }
        public string SeasonName { get; set; }
        public int Year { get; set; }

        public string CurrentWeather { get; set; }
        public string ForecastWeather { get; set; }
        public bool HasForecast { get; set; }

        public string KnownLunarEvent { get; set; }
        public bool HasLunarEvent { get; set; }

        public List<FestivalProjection> Festivals { get; set; } = new List<FestivalProjection>();
        public List<OrderDeadlineProjection> ExpiringOrders { get; set; } = new List<OrderDeadlineProjection>();
        public List<QuestWaitingConditionProjection> QuestWaitingConditions { get; set; } = new List<QuestWaitingConditionProjection>();

        // fable_57: aniversários de NPC do dia (entrada aditiva do mesmo tipo de projection de evento;
        // não é sistema novo). Vazio quando ninguém faz aniversário no dia.
        public List<NpcBirthdayProjection> Birthdays { get; set; } = new List<NpcBirthdayProjection>();

        public bool ShopIsOpen { get; set; }
        public bool ShopOpenKnown { get; set; }

        public List<string> KnownEventsText { get; set; } = new List<string>();
    }

    public class FestivalProjection
    {
        public string FestivalId { get; set; }
        public string DisplayName { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDiscovered { get; set; }
    }

    public class OrderDeadlineProjection
    {
        public string OrderId { get; set; }
        public string DisplayName { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
    }

    public class QuestWaitingConditionProjection
    {
        public string QuestId { get; set; }
        public string DisplayName { get; set; }
        public string ConditionHint { get; set; }
        public bool IsPartiallyKnown { get; set; }
    }

    /// <summary>fable_57 — aniversário de um NPC numa data (entrada de calendário, spoiler-safe).</summary>
    public class NpcBirthdayProjection
    {
        public string NpcId { get; set; }
        public string DisplayName { get; set; }
    }
}
