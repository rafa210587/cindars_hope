// arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) — HazardType agora
// vive em CindarsHope.Foundation (enum puro), sem using CindarsHope.Player.
using CindarsHope.Foundation;

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
