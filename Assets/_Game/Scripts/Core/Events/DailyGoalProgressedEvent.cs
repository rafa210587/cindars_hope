namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando o progresso de uma meta diária de farm avança.
    /// goalId é persistível; current e required são numéricos simples.
    /// </summary>
    public readonly struct DailyGoalProgressedEvent
    {
        public readonly string GoalId;
        public readonly int Current;
        public readonly int Required;

        public DailyGoalProgressedEvent(string goalId, int current, int required)
        {
            GoalId = goalId ?? string.Empty;
            Current = current;
            Required = required;
        }
    }
}
