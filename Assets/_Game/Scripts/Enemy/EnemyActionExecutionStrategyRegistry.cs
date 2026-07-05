using System;
using CindarsHope.Combat;

namespace CindarsHope.Enemy
{
    internal interface IEnemyActionExecutionStrategy
    {
        void Execute(IEnemyActionExecutionContext context, EnemyActionSO action);
    }

    internal interface IEnemyActionExecutionContext
    {
        void ExecuteSelfBuff(EnemyActionSO action);
        void ExecuteComboStrike(EnemyActionSO action);
        void ExecuteTelegraphedAoE(EnemyActionSO action);
        void ExecuteSummonAdds(EnemyActionSO action);
        void ExecuteMultiHitCharge(EnemyActionSO action);
        void ExecuteDebuffStrike(EnemyActionSO action);
    }

    internal sealed class DelegateEnemyActionExecutionStrategy : IEnemyActionExecutionStrategy
    {
        private readonly Action<IEnemyActionExecutionContext, EnemyActionSO> _execute;

        public DelegateEnemyActionExecutionStrategy(Action<IEnemyActionExecutionContext, EnemyActionSO> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public void Execute(IEnemyActionExecutionContext context, EnemyActionSO action)
        {
            _execute(context, action);
        }
    }

    /// <summary>Dispatch table for action families that fully own their resolution.</summary>
    internal sealed class EnemyActionExecutionStrategyRegistry
    {
        public static readonly EnemyActionExecutionStrategyRegistry Default = CreateDefault();

        private readonly IEnemyActionExecutionStrategy[] _strategies =
            new IEnemyActionExecutionStrategy[Enum.GetValues(typeof(EnemyActionType)).Length];

        public void Register(EnemyActionType actionType, IEnemyActionExecutionStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            _strategies[(int)actionType] = strategy;
        }

        public bool TryExecute(IEnemyActionExecutionContext context, EnemyActionSO action)
        {
            int index = action == null ? -1 : (int)action.ActionType;
            if (context == null || index < 0 || index >= _strategies.Length)
                return false;
            var strategy = _strategies[index];
            if (strategy == null) return false;

            strategy.Execute(context, action);
            return true;
        }

        private static EnemyActionExecutionStrategyRegistry CreateDefault()
        {
            var registry = new EnemyActionExecutionStrategyRegistry();
            registry.Register(EnemyActionType.SelfBuff,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteSelfBuff(action)));
            registry.Register(EnemyActionType.ComboStrike,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteComboStrike(action)));
            registry.Register(EnemyActionType.TelegraphedAoE,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteTelegraphedAoE(action)));
            registry.Register(EnemyActionType.SummonAdds,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteSummonAdds(action)));
            registry.Register(EnemyActionType.MultiHitCharge,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteMultiHitCharge(action)));
            registry.Register(EnemyActionType.DebuffStrike,
                new DelegateEnemyActionExecutionStrategy((context, action) => context.ExecuteDebuffStrike(action)));
            return registry;
        }
    }
}
