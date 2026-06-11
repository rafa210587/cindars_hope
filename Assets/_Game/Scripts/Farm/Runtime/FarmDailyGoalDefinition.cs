namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Definição imutável de uma meta diária de farm.
    /// Apenas tipos simples — sem referências Unity.
    /// </summary>
    public class FarmDailyGoalDefinition
    {
        public string GoalId;
        public string DisplayName;
        public int RequiredProgress;
        // DAILY_GOAL_REWARD_DEFERRED: recompensa de gold/item diferida para fase de economia final
    }
}
