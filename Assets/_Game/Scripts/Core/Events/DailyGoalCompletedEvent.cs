namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando uma meta diária de farm é completada.
    /// goalId é persistível.
    /// </summary>
    public readonly struct DailyGoalCompletedEvent
    {
        public readonly string GoalId;

        public DailyGoalCompletedEvent(string goalId)
        {
            GoalId = goalId ?? string.Empty;
        }
    }
}
