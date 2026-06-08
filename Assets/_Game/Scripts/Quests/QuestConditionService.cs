using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.World.Calendar;
using UnityEngine;

namespace CindarsHope.Quests
{
    public interface IQuestInventoryProvider
    {
        int GetItemCount(string itemId);
    }

    public class QuestConditionService : MonoBehaviour
    {
        public bool EvaluateCondition(ConditionDefinition condition, QuestContext context)
        {
            if (condition == null)
                return false;

            return condition.Type switch
            {
                ConditionType.ItemCount => context?.InventoryManager != null && context.InventoryManager.GetItemCount(condition.TargetId) >= condition.TargetValue,
                ConditionType.DaysPassed => context?.TimeManager != null && context.TimeManager.CurrentDay >= condition.TargetValue,
                ConditionType.SeasonReached => context?.CalendarService != null && (int)context.CalendarService.CurrentDate.CurrentSeason >= condition.TargetValue,
                ConditionType.LocationVisited => context?.VisitedLocations != null && context.VisitedLocations.Contains(condition.TargetId),
                ConditionType.NpcMet => context?.MetNpcs != null && context.MetNpcs.Contains(condition.TargetId),
                ConditionType.EventTriggered => context?.TriggeredEvents != null && context.TriggeredEvents.Contains(condition.TargetId),
                _ => false
            };
        }

        public void OnGameEventFired(string eventId, QuestState questState, ConditionDefinition condition)
        {
            if (condition.Type == ConditionType.EventTriggered && condition.TargetId == eventId)
            {
                questState.ConditionStates[condition.ConditionId] = true;
            }
        }
    }

    public class QuestContext
    {
        public CindarsHope.Core.Time.TimeManager TimeManager { get; set; }
        public GameCalendarService CalendarService { get; set; }
        public IQuestInventoryProvider InventoryManager { get; set; }
        public System.Collections.Generic.HashSet<string> VisitedLocations { get; set; } = new System.Collections.Generic.HashSet<string>();
        public System.Collections.Generic.HashSet<string> MetNpcs { get; set; } = new System.Collections.Generic.HashSet<string>();
        public System.Collections.Generic.HashSet<string> TriggeredEvents { get; set; } = new System.Collections.Generic.HashSet<string>();
    }
}
