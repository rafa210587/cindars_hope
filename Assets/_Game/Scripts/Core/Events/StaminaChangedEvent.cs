namespace CindarsHope.Core.Events
{
    public class StaminaChangedEvent
    {
        public int CurrentStamina { get; }
        public int MaxStamina { get; }

        public StaminaChangedEvent(int currentStamina, int maxStamina)
        {
            CurrentStamina = currentStamina;
            MaxStamina = maxStamina;
        }
    }
}
