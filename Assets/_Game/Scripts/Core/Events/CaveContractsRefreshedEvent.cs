namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_51 — published by CaveContractService when the weekly cave contracts (Zrix's board)
    /// are regenerated for a new in-game week. The Quest Log / Zrix board projection (F34) listens
    /// to refresh the offered list. Carries only the week index (simple type).
    /// </summary>
    public readonly struct CaveContractsRefreshedEvent
    {
        public readonly int Week;

        public CaveContractsRefreshedEvent(int week)
        {
            Week = week;
        }
    }
}
