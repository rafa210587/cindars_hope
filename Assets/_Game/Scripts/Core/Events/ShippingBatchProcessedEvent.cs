namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_54 — publicado na manhã seguinte quando o batch de envio da caixa de shipping
    /// e processado (pagamento overnight). Resumo para HUD/toast.
    /// Tipos simples apenas (event_rules / ADR-0007).
    /// </summary>
    public readonly struct ShippingBatchProcessedEvent
    {
        public readonly int DayNumber;
        public readonly int TotalGold;
        public readonly int ItemCount;
        public readonly int EntryCount;

        public ShippingBatchProcessedEvent(int dayNumber, int totalGold, int itemCount, int entryCount)
        {
            DayNumber = dayNumber;
            TotalGold = totalGold;
            ItemCount = itemCount;
            EntryCount = entryCount;
        }
    }
}
