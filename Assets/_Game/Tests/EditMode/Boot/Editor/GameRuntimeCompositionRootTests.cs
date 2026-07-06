using CindarsHope.Combat;
using CindarsHope.Composition;
using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Boot
{
    public class GameRuntimeCompositionRootTests
    {
        [TearDown]
        public void TearDown()
        {
            if (GameRuntimeCompositionRoot.Instance != null)
            {
                Object.DestroyImmediate(GameRuntimeCompositionRoot.Instance.gameObject);
            }

            if (CombatStateTracker.ActiveInstance != null)
            {
                Object.DestroyImmediate(CombatStateTracker.ActiveInstance.gameObject);
            }

            if (ActiveSkillExecutionController.Instance != null)
            {
                Object.DestroyImmediate(ActiveSkillExecutionController.Instance.gameObject);
            }
        }

        [Test]
        public void EnsureExists_InstallsOneReadyRootAndTracker()
        {
            GameRuntimeCompositionRoot first = GameRuntimeCompositionRoot.EnsureExists();
            GameRuntimeCompositionRoot second = GameRuntimeCompositionRoot.EnsureExists();

            Assert.That(second, Is.SameAs(first));
            Assert.That(GameRuntimeCompositionRoot.Instance, Is.SameAs(first));
            Assert.That(GameRuntimeCompositionRoot.State, Is.EqualTo(RuntimeCompositionState.Ready));
            Assert.That(CombatStateTracker.ActiveInstance, Is.Not.Null);
            Assert.That(ActiveSkillExecutionController.Instance, Is.Not.Null);
        }

        [Test]
        public void DestroyingRoot_ResetsReadinessAndAllowsCleanRecreation()
        {
            GameRuntimeCompositionRoot first = GameRuntimeCompositionRoot.EnsureExists();
            Object.DestroyImmediate(first.gameObject);

            Assert.That(GameRuntimeCompositionRoot.Instance, Is.Null);
            Assert.That(GameRuntimeCompositionRoot.State, Is.EqualTo(RuntimeCompositionState.NotInstalled));

            GameRuntimeCompositionRoot replacement = GameRuntimeCompositionRoot.EnsureExists();
            Assert.That(replacement, Is.Not.SameAs(first));
            Assert.That(GameRuntimeCompositionRoot.IsReady, Is.True);
            Assert.That(CombatStateTrackerBootstrap.Install(), Is.SameAs(CombatStateTracker.ActiveInstance));
        }
    }
}
