using System.Collections.Generic;

namespace CindarsHope.Quests.Conditions
{
    // Pure C# snapshot — populated from domain providers at evaluation time, no direct Unity refs
    public class QuestConditionContext
    {
        public Dictionary<string, string> ActiveFlags { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> ItemCounts { get; set; } = new Dictionary<string, int>();
        public int CurrentDay { get; set; }
        public int CurrentHour { get; set; }
        public string CurrentSeason { get; set; }
        public string CurrentWeather { get; set; }
        public string CurrentLunarPhase { get; set; }
        public HashSet<string> VisitedLocations { get; set; } = new HashSet<string>();
        public HashSet<string> MetNpcIds { get; set; } = new HashSet<string>();
        public HashSet<string> KnownDialogueIds { get; set; } = new HashSet<string>();
        public int FarmLevel { get; set; }
        public int CaveProgress { get; set; }
        public int PlayerReputation { get; set; }
        public HashSet<string> BestiaryKnownIds { get; set; } = new HashSet<string>();
        public HashSet<string> ActiveQuestIds { get; set; } = new HashSet<string>();
        public HashSet<string> CompletedQuestIds { get; set; } = new HashSet<string>();
    }
}
