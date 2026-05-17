namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o total de ouro do jogador muda.
    /// Delta positivo = ganhou ouro; delta negativo = gastou/perdeu ouro.
    /// </summary>
    public readonly struct GoldChangedEvent
    {
        public int Delta { get; }
        public int NewTotal { get; }

        public GoldChangedEvent(int delta, int newTotal)
        {
            Delta = delta;
            NewTotal = newTotal;
        }
    }
}
