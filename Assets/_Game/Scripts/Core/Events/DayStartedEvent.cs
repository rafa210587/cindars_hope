namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um novo dia começa.
    /// MVP: disparado ao dormir/pressionar TAB.
    /// Pós-MVP: disparado pelo ciclo completo de tempo.
    /// </summary>
    public readonly struct DayStartedEvent
    {
        public int DayNumber { get; }

        public DayStartedEvent(int dayNumber)
        {
            DayNumber = dayNumber;
        }
    }
}
