namespace CindarsHope.Core.Events
{
    /// <summary>Publicado quando o valor/threshold de fadiga do player muda de faixa (F16).</summary>
    public readonly struct PlayerFatigueChangedEvent
    {
        public float FatigueValue { get; }
        public int Threshold { get; }

        public PlayerFatigueChangedEvent(float fatigueValue, int threshold)
        {
            FatigueValue = fatigueValue;
            Threshold = threshold;
        }
    }

    /// <summary>Publicado quando o player colapsa de exaustão às 02:00 (F16).</summary>
    public readonly struct PlayerCollapsedEvent
    {
        public int DayNumber { get; }

        public PlayerCollapsedEvent(int dayNumber)
        {
            DayNumber = dayNumber;
        }
    }
}
