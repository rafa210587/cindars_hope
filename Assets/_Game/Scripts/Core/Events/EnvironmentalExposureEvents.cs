using CindarsHope.Player;

namespace CindarsHope.Core.Events
{
    public class EnvironmentalExposureStartedEvent
    {
        public HazardType HazardType { get; }
        public string StatusEffectId { get; }

        public EnvironmentalExposureStartedEvent(HazardType hazardType, string statusEffectId)
        {
            HazardType = hazardType;
            StatusEffectId = statusEffectId;
        }
    }

    public class EnvironmentalExposureEndedEvent
    {
        public HazardType HazardType { get; }

        public EnvironmentalExposureEndedEvent(HazardType hazardType)
        {
            HazardType = hazardType;
        }
    }
}
