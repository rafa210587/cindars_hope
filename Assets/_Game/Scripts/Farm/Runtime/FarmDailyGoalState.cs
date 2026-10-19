namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Estado serializável de uma meta diária de farm.
    /// Apenas tipos simples compatíveis com JsonUtility.
    /// Sem referências Unity.
    /// </summary>
    [System.Serializable]
    public class FarmDailyGoalState
    {
        public string GoalId;
        public int Day;
        public int CurrentProgress;
        public int RequiredProgress;
        public bool Completed;
        public bool Claimed;
    }
}
