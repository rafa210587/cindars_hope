using System.Collections.Generic;

namespace CindarsHope.City.Services
{
    public class ServiceAvailabilityResult
    {
        public bool Available { get; set; }
        public string UnavailableReason { get; set; }
        public string ProviderNpcId { get; set; }
        public string BuildingId { get; set; }
        public string OpenHoursStatus { get; set; }
        public List<string> RequiredFlagsMissing { get; set; } = new List<string>();
        public string DialogueHookId { get; set; }

        public static ServiceAvailabilityResult Unavailable(string reason) =>
            new ServiceAvailabilityResult { Available = false, UnavailableReason = reason };
    }
}
