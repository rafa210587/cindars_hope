namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando a fome chega ao minimo e efeitos de exaustao podem iniciar.
    /// </summary>
    public readonly struct HungerEmptyEvent
    {
        public int CurrentHunger { get; }
        public int MaxHunger { get; }

        public HungerEmptyEvent(int currentHunger, int maxHunger)
        {
            CurrentHunger = currentHunger;
            MaxHunger = maxHunger;
        }
    }
}
