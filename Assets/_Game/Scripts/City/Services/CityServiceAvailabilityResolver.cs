using System.Collections.Generic;

namespace CindarsHope.City.Services
{
    public class ServiceAccessContext
    {
        public string NpcId { get; set; }
        public int CurrentHour { get; set; }
        public int PlayerReputation { get; set; }
        public int FarmLevel { get; set; }
        public int CaveProgress { get; set; }
        public bool IsNight { get; set; }
        public string ActiveLunarPhase { get; set; }
        public bool IsNpcAvailable { get; set; } = true;
        public List<string> PlayerQuestFlags { get; set; } = new List<string>();
        public List<string> PlayerStoryFlags { get; set; } = new List<string>();
    }

    public class CityServiceAvailabilityResolver
    {
        public ServiceAvailabilityResult Resolve(CityServiceDefinition service, ServiceAccessContext ctx)
        {
            if (service == null) return ServiceAvailabilityResult.Unavailable("service is null");
            if (ctx == null) return ServiceAvailabilityResult.Unavailable("context is null");

            // Anya city service is always blocked
            if (service.ServiceId != null &&
                (service.ServiceId.Contains("anya_temple") || service.ServiceId.Contains("altar_anya")))
                return ServiceAvailabilityResult.Unavailable("Anya city temple/altar is not an active urban service per canon");

            // NPC availability
            if (service.RequiredNpcAvailability && !ctx.IsNpcAvailable)
                return ServiceAvailabilityResult.Unavailable("Provider NPC unavailable");

            // Open hours
            if (service.RequiredOpenHoursRule != null && !service.RequiredOpenHoursRule.IsOpenAt(ctx.CurrentHour))
            {
                var msg = service.RequiredOpenHoursRule.ClosedMessage ?? "Service closed at this hour";
                return new ServiceAvailabilityResult
                {
                    Available = false,
                    UnavailableReason = msg,
                    OpenHoursStatus = "CLOSED",
                    BuildingId = service.ProviderBuildingId
                };
            }

            // Night shop conditions
            if (service.IsNightShop)
            {
                bool nightCondMet = ctx.IsNight ||
                    ctx.ActiveLunarPhase == "Nyx" ||
                    (!string.IsNullOrEmpty(service.NightShopConditionFlag) && ctx.PlayerStoryFlags.Contains(service.NightShopConditionFlag));
                if (!nightCondMet)
                    return ServiceAvailabilityResult.Unavailable("Night shop: requires night, Nyx lunar phase, or story flag");
            }

            // Reputation gate
            if (service.RequiredReputation.HasValue && ctx.PlayerReputation < service.RequiredReputation.Value)
                return ServiceAvailabilityResult.Unavailable($"Insufficient reputation: need {service.RequiredReputation.Value}");

            // Quest flag
            if (!string.IsNullOrEmpty(service.RequiredQuestFlag) && !ctx.PlayerQuestFlags.Contains(service.RequiredQuestFlag))
                return new ServiceAvailabilityResult { Available = false, UnavailableReason = $"Quest flag '{service.RequiredQuestFlag}' required", RequiredFlagsMissing = new System.Collections.Generic.List<string> { service.RequiredQuestFlag } };

            // Story flag
            if (!string.IsNullOrEmpty(service.RequiredStoryFlag) && !ctx.PlayerStoryFlags.Contains(service.RequiredStoryFlag))
                return ServiceAvailabilityResult.Unavailable($"Story flag '{service.RequiredStoryFlag}' required");

            // Forbidden flags
            foreach (var flag in service.ForbiddenIfFlags)
                if (ctx.PlayerStoryFlags.Contains(flag))
                    return ServiceAvailabilityResult.Unavailable($"Service blocked by flag '{flag}'");

            // Farm level
            if (service.RequiredFarmLevel.HasValue && ctx.FarmLevel < service.RequiredFarmLevel.Value)
                return ServiceAvailabilityResult.Unavailable($"Farm level {ctx.FarmLevel} < required {service.RequiredFarmLevel.Value}");

            // Cave progress
            if (service.RequiredCaveProgress.HasValue && ctx.CaveProgress < service.RequiredCaveProgress.Value)
                return ServiceAvailabilityResult.Unavailable($"Cave progress insufficient");

            var providerId = service.ProviderNpcIds.Count > 0 ? service.ProviderNpcIds[0] : null;
            return new ServiceAvailabilityResult
            {
                Available = true,
                ProviderNpcId = providerId,
                BuildingId = service.ProviderBuildingId,
                OpenHoursStatus = "OPEN"
            };
        }
    }
}
