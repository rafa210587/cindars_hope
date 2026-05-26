using CindarsHope.Player.Progression;

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
