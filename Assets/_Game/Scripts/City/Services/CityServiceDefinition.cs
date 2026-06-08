using System.Collections.Generic;
using CindarsHope.City.Layout;

namespace CindarsHope.City.Services
{
    public class CityServiceDefinition
    {
        public string ServiceId { get; set; }
        public CityServiceType ServiceType { get; set; }
        public string DisplayName { get; set; }
        public List<string> ProviderNpcIds { get; set; } = new List<string>();
        public string ProviderBuildingId { get; set; }
        public OpenHoursRule RequiredOpenHoursRule { get; set; }
        public bool RequiredNpcAvailability { get; set; } = true;
        public int? RequiredReputation { get; set; }
        public string RequiredQuestFlag { get; set; }
        public int? RequiredFarmLevel { get; set; }
        public int? RequiredCaveProgress { get; set; }
        public string RequiredStoryFlag { get; set; }
        // References to economy contracts (from WAVE 06)
        public string PriceChannel { get; set; }
        public string PricingProfileId { get; set; }
        public string ShopInventoryId { get; set; }
        public string QuestObjectiveAdapterId { get; set; }
        public string LicenseDefinitionId { get; set; }
        public List<string> ForbiddenIfFlags { get; set; } = new List<string>();
        public List<string> DebugTags { get; set; } = new List<string>();
        // Night shop — only available under night/Nyx/story condition
        public bool IsNightShop { get; set; } = false;
        public string NightShopConditionFlag { get; set; }
    }
}
