using System.Collections.Generic;
using CindarsHope.City.Schedule;

namespace CindarsHope.Dialogue
{
    public class DialogueContext
    {
        public string NpcId { get; set; }
        public string LocationId { get; set; }
        public SchedulePeriod CurrentPeriod { get; set; }
        public int CurrentDay { get; set; }
        public string Season { get; set; }
        public string Weather { get; set; }
        public string LunarState { get; set; }
        public string FestivalId { get; set; }
        public int RelationshipValue { get; set; }
        public int CityReputation { get; set; }
        public List<string> ActiveQuestIds { get; set; } = new List<string>();
        public List<string> CompletedQuestIds { get; set; } = new List<string>();
        public List<string> StoryFlags { get; set; } = new List<string>();
        public int CaveProgress { get; set; }
        public int FarmLevel { get; set; }
        public string CompanionEventState { get; set; }
        public bool IsFarmVisit { get; set; }
        public bool IsShopInteraction { get; set; }
        public bool IsServiceInteraction { get; set; }
    }
}
