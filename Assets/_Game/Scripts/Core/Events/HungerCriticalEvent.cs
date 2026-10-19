namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando a fome entra na faixa critica.
    /// </summary>
    public readonly struct HungerCriticalEvent
    {
        public int CurrentHunger { get; }
        public int MaxHunger { get; }

        public HungerCriticalEvent(int currentHunger, int maxHunger)
        {
            CurrentHunger = currentHunger;
            MaxHunger = maxHunger;
        }
    }
}
