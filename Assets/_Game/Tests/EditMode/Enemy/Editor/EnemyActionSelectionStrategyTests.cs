using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public class EnemyActionSelectionStrategyTests
    {
        private sealed class FixedStrategy : IEnemyActionSelectionStrategy
        {
            private readonly EnemyActionSO _result;

            public FixedStrategy(EnemyActionSO result)
            {
                _result = result;
            }

            public int Calls { get; private set; }

            public EnemyActionSO Select(in EnemyActionSelectionContext context)
            {
                Calls++;
                return _result;
            }
        }

        [Test]
        public void Runner_DelegatesSelectionToInjectedStrategy()
        {
            var expected = ScriptableObject.CreateInstance<EnemyActionSO>();
            var actionSet = ScriptableObject.CreateInstance<EnemyActionSetSO>();
            var database = ScriptableObject.CreateInstance<EnemyActionDatabaseSO>();
            actionSet.ActionIds = new[] { "action_test" };

            var strategy = new FixedStrategy(expected);
            var runner = new EnemyActionRunner
            {
                ActiveActionSet = actionSet
            };
            runner.UpdateRefs(null, null, database, null, null, null, null, null);
            runner.SetActionSelectionStrategy(strategy);

            Assert.That(runner.SelectBestAction(3f), Is.SameAs(expected));
            Assert.That(strategy.Calls, Is.EqualTo(1));

            Object.DestroyImmediate(expected);
            Object.DestroyImmediate(actionSet);
            Object.DestroyImmediate(database);
        }
    }
}
