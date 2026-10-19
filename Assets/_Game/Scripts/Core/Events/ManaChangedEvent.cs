namespace CindarsHope.Core.Events
{
    public class ManaChangedEvent
    {
        public int CurrentMana { get; }
        public int MaxMana { get; }

        public ManaChangedEvent(int currentMana, int maxMana)
        {
            CurrentMana = currentMana;
            MaxMana = maxMana;
        }
    }
}
