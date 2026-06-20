using System.Collections.Generic;

namespace CindarsHope.Farm.Shipping
{
    /// <summary>
    /// fable_54 — DTO de save de UMA entrada de envio pendente. Tipos simples apenas (ADR-0006 /
    /// save-dto-simple-types-only): ids/ints/floats. Sem referencias Unity.
    /// Espelha PendingShippingEntry (modulo orfao, INTOCADO) em forma serializavel por JsonUtility.
    /// </summary>
    [System.Serializable]
    public class PendingShippingEntrySaveData
    {
        public string EntryId;
        public string SellPointId;
        public string ItemId;
        public int Quantity;
        public int QualityTier;
        public float BaseValueSnapshot;
        public int DepositedDay;
        public int ProcessOnDay;
        public int State; // ShippingEntryState (int)
    }

    /// <summary>
    /// fable_54 — lista de envios pendentes da caixa de shipping. Campo ADITIVO na secao farm.
    /// Ausente em save legado => lista vazia => nenhum batch pendente (regenera no proximo DayStarted).
    /// </summary>
    [System.Serializable]
    public class PendingShippingSaveData
    {
        public List<PendingShippingEntrySaveData> Entries = new List<PendingShippingEntrySaveData>();
    }
}
