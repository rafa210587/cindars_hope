namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o HP atual do jogador muda.
    /// Delta negativo = dano; delta positivo = cura.
    /// </summary>
    public readonly struct HPChangedEvent
    {
        public int Delta { get; }
        public int CurrentHP { get; }
        public int MaxHP { get; }

        public HPChangedEvent(int delta, int currentHP, int maxHP)
        {
            Delta = delta;
            CurrentHP = currentHP;
            MaxHP = maxHP;
        }
    }
}
