using System.Collections.Generic;

namespace CindarsHope.Farm.Forage
{
    /// <summary>
    /// fable_54 — DTO de save de UM ponto de forrageio do dia. Tipos simples apenas (ADR-0006 /
    /// save-dto-simple-types-only). Espelha ForageSpawnState (modulo orfao, INTOCADO).
    /// </summary>
    [System.Serializable]
    public class ForageSpawnSaveData
    {
        public string SpawnId;
        public string ForageId;
        public int State; // ForageNodeState (int)
        public int SpawnedDay;
        public int CollectedDay;
        public int NextEligibleSpawnDay;
    }

    /// <summary>
    /// fable_54 — estado dos pontos de forrageio do dia. Campo ADITIVO na secao farm. Ausente em
    /// save legado => lista vazia => os spawns sao regenerados no proximo DayStarted.
    /// </summary>
    [System.Serializable]
    public class ForageSpawnsSaveData
    {
        public int Day = -1;
        public List<ForageSpawnSaveData> Spawns = new List<ForageSpawnSaveData>();
    }
}
