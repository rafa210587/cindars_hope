using System.Collections.Generic;

namespace CindarsHope.Companions
{
    public class AllowedArea
    {
        public string AreaId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
    }

    public class AllowedResources
    {
        public List<string> AllowedItemIds { get; set; } = new List<string>();
        public List<string> ForbiddenItemIds { get; set; } = new List<string>();
        public int MaxDailyConsumption { get; set; }
    }

    public class OutputRules
    {
        public string DestinationStorageId { get; set; }
        public List<string> AllowedOutputItemIds { get; set; } = new List<string>();
        public bool AutoSell { get; set; } = false;
    }

    public class CompanionFarmJobDefinition
    {
        public string JobId { get; set; }
        public CompanionFarmJobType JobType { get; set; }
        public string RequiredToolOrStation { get; set; }
        public int StartTimeHour { get; set; }
        public int EndTimeHour { get; set; }
        public int StaminaBudgetPerDay { get; set; }
        public int RelationshipGainPerJob { get; set; }
        public int FatigueGainPerJob { get; set; }
        public AllowedArea Area { get; set; }
        public AllowedResources Resources { get; set; }
        public OutputRules OutputRules { get; set; }
        public int DailyLimit { get; set; } = 3;
        public int Priority { get; set; } = 1;

        public CompanionFarmJobDefinition()
        {
            Area = new AllowedArea();
            Resources = new AllowedResources();
            OutputRules = new OutputRules();
        }

        public bool CanExecuteAtTime(int currentHour)
        {
            return currentHour >= StartTimeHour && currentHour < EndTimeHour;
        }

        public bool IsItemAllowed(string itemId)
        {
            if (Resources.ForbiddenItemIds.Contains(itemId))
                return false;

            if (Resources.AllowedItemIds.Count == 0)
                return true;

            return Resources.AllowedItemIds.Contains(itemId);
        }

        public bool CanOutputItem(string itemId)
        {
            if (OutputRules.AllowedOutputItemIds.Count == 0)
                return true;

            return OutputRules.AllowedOutputItemIds.Contains(itemId);
        }
    }
}
