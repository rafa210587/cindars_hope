using System.Collections.Generic;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// DTO de save para metas diárias de farm.
    /// Apenas tipos simples — sem referências Unity.
    /// Compatível com JsonUtility.
    /// </summary>
    [System.Serializable]
    public class FarmDailyGoalsSaveData
    {
        public List<FarmDailyGoalState> Goals = new List<FarmDailyGoalState>();
    }
}
