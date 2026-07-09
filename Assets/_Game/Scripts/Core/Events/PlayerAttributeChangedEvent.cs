// arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) — PlayerAttributeType
// agora vive em CindarsHope.Foundation (enum puro), sem using CindarsHope.Player.Progression.
using CindarsHope.Foundation;

namespace CindarsHope.Core.Events
{
    public readonly struct PlayerAttributeChangedEvent
    {
        public PlayerAttributeChangedEvent(PlayerAttributeType attributeType, int value, int remainingPoints)
        {
            AttributeType = attributeType;
            Value = value;
            RemainingPoints = remainingPoints;
        }

        public PlayerAttributeType AttributeType { get; }
        public int Value { get; }
        public int RemainingPoints { get; }
    }
}
