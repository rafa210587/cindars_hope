using System.Collections.Generic;

namespace CindarsHope.Farm.Lots
{
    /// <summary>
    /// fable_41 — DTO de save dos lotes de expansão da fazenda.
    /// Campo ADITIVO na seção farm (GameSaveData.FarmLots): apenas IDs string dos lotes Owned.
    /// Sem referências Unity (ADR-0006). Sem migração: ausente em save legado = lista vazia
    /// = todos os lotes Locked (CA-4).
    /// Compatível com JsonUtility.
    /// </summary>
    [System.Serializable]
    public class FarmLotsSaveData
    {
        /// <summary>IDs dos lotes destravados (Owned). Lotes ausentes = Locked.</summary>
        public List<string> OwnedLots = new List<string>();
    }
}
