using System;
using CindarsHope.Combat;

namespace CindarsHope.Enemy
{
    internal interface IEnemyMovementStrategy
    {
        void Move(IEnemyMovementStrategyContext context, float speed);
    }

    internal interface IEnemyMovementStrategyContext
    {
        void MoveOrbit(float speed);
        void MoveFloatingSlow(float speed);
        void MoveChargeLine(float speed);
        void MoveRetreatAndCall(float speed);
        void MoveRetreat(float speed);
        void MoveMimicAmbush(float speed);
        void MoveAnchoredChase(float speed);
        void MovePackFlanker(float speed);
    }

    internal sealed class DelegateEnemyMovementStrategy : IEnemyMovementStrategy
    {
        private readonly Action<IEnemyMovementStrategyContext, float> _move;

        public DelegateEnemyMovementStrategy(Action<IEnemyMovementStrategyContext, float> move)
        {
            _move = move ?? throw new ArgumentNullException(nameof(move));
        }

        public void Move(IEnemyMovementStrategyContext context, float speed) => _move(context, speed);
    }

    internal sealed class EnemyMovementStrategyRegistry
    {
        public static readonly EnemyMovementStrategyRegistry Default = CreateDefault();

        private readonly IEnemyMovementStrategy[] _strategies =
            new IEnemyMovementStrategy[Enum.GetValues(typeof(EnemyMovementType)).Length];

        public void Register(EnemyMovementType movementType, IEnemyMovementStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            _strategies[(int)movementType] = strategy;
        }

        public bool TryMove(IEnemyMovementStrategyContext context, EnemyMovementType movementType, float speed)
        {
            int index = (int)movementType;
            if (context == null || index < 0 || index >= _strategies.Length) return false;
            var strategy = _strategies[index];
            if (strategy == null) return false;
            strategy.Move(context, speed);
            return true;
        }

        private static EnemyMovementStrategyRegistry CreateDefault()
        {
            var registry = new EnemyMovementStrategyRegistry();
            var orbit = new DelegateEnemyMovementStrategy((context, speed) => context.MoveOrbit(speed));
            registry.Register(EnemyMovementType.CircleStrafe, orbit);
            registry.Register(EnemyMovementType.FloatingOrbit, orbit);
            registry.Register(EnemyMovementType.FloatingSlow,
                new DelegateEnemyMovementStrategy((context, speed) => context.MoveFloatingSlow(speed)));
            registry.Register(EnemyMovementType.ChargeLine,
                new DelegateEnemyMovementStrategy((context, speed) => context.MoveChargeLine(speed)));
            registry.Register(EnemyMovementType.RetreatAndCall,
                new DelegateEnemyMovementStrategy((context, speed) => context.MoveRetreatAndCall(speed)));
            registry.Register(EnemyMovementType.HazardLure,
                new DelegateEnemyMovementStrategy((context, speed) => context.MoveRetreat(speed)));
            registry.Register(EnemyMovementType.TreasureIdleAmbush,
                new DelegateEnemyMovementStrategy((context, speed) => context.MoveMimicAmbush(speed)));
            var anchored = new DelegateEnemyMovementStrategy((context, speed) => context.MoveAnchoredChase(speed));
            registry.Register(EnemyMovementType.ProtectAnchor, anchored);
            registry.Register(EnemyMovementType.BossArenaControl, anchored);
            registry.Register(EnemyMovementType.PackFlanker,
                new DelegateEnemyMovementStrategy((context, speed) => context.MovePackFlanker(speed)));
            return registry;
        }
    }
}
