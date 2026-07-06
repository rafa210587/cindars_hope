using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public sealed class EnemyBrainConfigurationPolicyTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NormalizePackId_MapsBlankValuesToNull(string packId)
        {
            Assert.That(EnemyBrainConfigurationPolicy.NormalizePackId(packId), Is.Null);
        }

        [Test]
        public void ResolveDecisionTick_UsesOnlyPositiveProfileOverride()
        {
            var profile = ScriptableObject.CreateInstance<EnemyMovementProfileSO>();
            profile.DecisionTickSeconds = 0.15f;
            Assert.That(EnemyBrainConfigurationPolicy.ResolveDecisionTick(profile, 0.3f), Is.EqualTo(0.15f));

            profile.DecisionTickSeconds = 0f;
            Assert.That(EnemyBrainConfigurationPolicy.ResolveDecisionTick(profile, 0.3f), Is.EqualTo(0.3f));
            Assert.That(EnemyBrainConfigurationPolicy.ResolveDecisionTick(null, 0.3f), Is.EqualTo(0.3f));
            Object.DestroyImmediate(profile);
        }
    }
}
