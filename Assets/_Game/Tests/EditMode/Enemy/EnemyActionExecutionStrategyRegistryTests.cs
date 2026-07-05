using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public sealed class EnemyActionExecutionStrategyRegistryTests
    {
        private EnemyActionSO _action;

        [SetUp]
        public void SetUp() => _action = ScriptableObject.CreateInstance<EnemyActionSO>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_action);

        [Test]
        public void TryExecute_RoutesRegisteredActionType()
        {
            var registry = new EnemyActionExecutionStrategyRegistry();
            var context = new FakeExecutionContext();
            EnemyActionSO received = null;
            registry.Register(EnemyActionType.ComboStrike,
                new DelegateEnemyActionExecutionStrategy((_, action) => received = action));
            _action.ActionType = EnemyActionType.ComboStrike;

            Assert.That(registry.TryExecute(context, _action), Is.True);
            Assert.That(received, Is.SameAs(_action));
        }

        [Test]
        public void TryExecute_LeavesSharedPipelineActionsUnhandled()
        {
            var registry = new EnemyActionExecutionStrategyRegistry();
            var context = new FakeExecutionContext();
            _action.ActionType = EnemyActionType.MeleeAttack;

            Assert.That(registry.TryExecute(context, _action), Is.False);
        }

        [Test]
        public void Register_ReplacesStrategyForSameActionType()
        {
            var registry = new EnemyActionExecutionStrategyRegistry();
            var context = new FakeExecutionContext();
            int firstCalls = 0;
            int replacementCalls = 0;
            registry.Register(EnemyActionType.SummonAdds,
                new DelegateEnemyActionExecutionStrategy((_, __) => firstCalls++));
            registry.Register(EnemyActionType.SummonAdds,
                new DelegateEnemyActionExecutionStrategy((_, __) => replacementCalls++));
            _action.ActionType = EnemyActionType.SummonAdds;

            registry.TryExecute(context, _action);

            Assert.That(firstCalls, Is.Zero);
            Assert.That(replacementCalls, Is.EqualTo(1));
        }

        private sealed class FakeExecutionContext : IEnemyActionExecutionContext
        {
            public void ExecuteSelfBuff(EnemyActionSO action) { }
            public void ExecuteComboStrike(EnemyActionSO action) { }
            public void ExecuteTelegraphedAoE(EnemyActionSO action) { }
            public void ExecuteSummonAdds(EnemyActionSO action) { }
            public void ExecuteMultiHitCharge(EnemyActionSO action) { }
            public void ExecuteDebuffStrike(EnemyActionSO action) { }
        }
    }
}
