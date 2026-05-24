namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o mana atual do jogador muda.
    /// </summary>
    public readonly struct ManaChangedEvent
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
