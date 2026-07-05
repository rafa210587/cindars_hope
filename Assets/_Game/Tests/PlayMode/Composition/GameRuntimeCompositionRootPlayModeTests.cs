using System.Collections;
using CindarsHope.Combat;
using CindarsHope.Composition;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public class GameRuntimeCompositionRootPlayModeTests
    {
        [UnityTest]
        public IEnumerator RootAndTracker_RemainSingleAcrossPlayModeFrames()
        {
            GameRuntimeCompositionRoot root = GameRuntimeCompositionRoot.EnsureExists();
            CombatStateTracker tracker = CombatStateTracker.ActiveInstance;

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.EnsureExists(), Is.SameAs(root));
            Assert.That(CombatStateTrackerBootstrap.Install(), Is.SameAs(tracker));
            Assert.That(GameRuntimeCompositionRoot.IsReady, Is.True);

            yield return null;

            Assert.That(GameRuntimeCompositionRoot.Instance, Is.SameAs(root));
            Assert.That(CombatStateTracker.ActiveInstance, Is.SameAs(tracker));
        }
    }
}
