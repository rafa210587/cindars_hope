namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando a fome atual do jogador muda.
    /// Delta negativo = fome reduziu; delta positivo = fome restaurada.
    /// </summary>
    public readonly struct HungerChangedEvent
    {
        public int Delta { get; }
        public int CurrentValue { get; }
        public int MaxValue { get; }

        public HungerChangedEvent(int delta, int currentValue, int maxValue)
        {
            Delta = delta;
            CurrentValue = currentValue;
            MaxValue = maxValue;
        }
    }
}
