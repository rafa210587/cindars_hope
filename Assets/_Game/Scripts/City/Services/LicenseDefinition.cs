using System.Collections.Generic;

namespace CindarsHope.City.Services
{
    public class LicenseReputationImpact
    {
        public string FactionId { get; set; }
        public int Delta { get; set; }
    }

    public class LicenseDefinition
    {
        public string LicenseId { get; set; }
        public string DisplayName { get; set; }
        public string IssuerServiceId { get; set; }
        public int RequiredGold { get; set; }
        public List<string> RequiredItemIds { get; set; } = new List<string>();
        public int RequiredReputation { get; set; } = 0;
        public string RequiredQuestFlag { get; set; }
        // GrantedFlag is stored in player flags on purchase
        public string GrantedFlag { get; set; }
        public string GrantedPermission { get; set; }
        public int? ExpiresAfterDays { get; set; }
        public List<LicenseReputationImpact> ReputationImpacts { get; set; } = new List<LicenseReputationImpact>();

        public bool IsIssuedBy(string serviceId) => IssuerServiceId == serviceId;
    }
}
