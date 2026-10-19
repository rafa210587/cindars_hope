using System.Collections.Generic;
using CindarsHope.Combat;

namespace CindarsHope.Enemy
{
    internal readonly struct EnemyActionSelectionContext
    {
        public EnemyActionSelectionContext(
            string[] actionIds,
            EnemyActionDatabaseSO actionDatabase,
            IReadOnlyDictionary<string, EnemyActionRuntime> cooldowns,
            float distance,
            float currentTime)
        {
            ActionIds = actionIds;
            ActionDatabase = actionDatabase;
            Cooldowns = cooldowns;
            Distance = distance;
            CurrentTime = currentTime;
        }

        public string[] ActionIds { get; }
        public EnemyActionDatabaseSO ActionDatabase { get; }
        public IReadOnlyDictionary<string, EnemyActionRuntime> Cooldowns { get; }
        public float Distance { get; }
        public float CurrentTime { get; }
    }

    internal interface IEnemyActionSelectionStrategy
    {
        EnemyActionSO Select(in EnemyActionSelectionContext context);
    }

    /// <summary>Preserva a seleção legada: primeira ação pronta e válida na ordem do action set.</summary>
    internal sealed class OrderedReadyEnemyActionSelectionStrategy : IEnemyActionSelectionStrategy
    {
        public EnemyActionSO Select(in EnemyActionSelectionContext context)
        {
            if (context.ActionIds == null || context.ActionDatabase == null || context.Cooldowns == null)
            {
                return null;
            }

            foreach (string actionId in context.ActionIds)
            {
                if (!context.ActionDatabase.TryGetById(actionId, out EnemyActionSO action))
                {
                    continue;
                }

                if (!context.Cooldowns.TryGetValue(actionId, out EnemyActionRuntime runtime)
                    || !runtime.IsReady(context.CurrentTime))
                {
                    continue;
                }

                if (action.ActionType == EnemyActionType.SelfBuff
                    || context.Distance >= action.MinRange && context.Distance <= action.Range)
                {
                    return action;
                }
            }

            return null;
        }
    }
}
