using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public sealed class EnemyBrainTuningResolverTests
    {
        [Test]
        public void ProfileOverridesEnemyDataForAllMovementTuning()
        {
            var profile = ScriptableObject.CreateInstance<EnemyMovementProfileSO>();
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            profile.DetectionRange = 12f;
            profile.LeashRange = 29f;
            profile.MoveSpeed = 4f;
            data.detectionRadius = 5f;
            data.moveSpeed = 2f;

            Assert.That(EnemyBrainTuningResolver.DetectionRange(profile, data), Is.EqualTo(12f));
            Assert.That(EnemyBrainTuningResolver.LeashRange(profile, data), Is.EqualTo(29f));
            Assert.That(EnemyBrainTuningResolver.MoveSpeed(profile, data, 0.5f, 1.5f), Is.EqualTo(3f));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(data);
        }

        [Test]
        public void DataAndDefaultsPreserveExistingFallbacks()
        {
            var data = ScriptableObject.CreateInstance<EnemyDataSO>();
            data.detectionRadius = 7f;
            data.moveSpeed = 3f;

            Assert.That(EnemyBrainTuningResolver.DetectionRange(null, data), Is.EqualTo(7f));
            Assert.That(EnemyBrainTuningResolver.LeashRange(null, data), Is.EqualTo(21f));
            Assert.That(EnemyBrainTuningResolver.MoveSpeed(null, data, 1f, 2f), Is.EqualTo(6f));
            Assert.That(EnemyBrainTuningResolver.DetectionRange(null, null), Is.EqualTo(10f));
            Assert.That(EnemyBrainTuningResolver.LeashRange(null, null), Is.EqualTo(30f));
            Assert.That(EnemyBrainTuningResolver.MoveSpeed(null, null, 1f, 1f), Is.EqualTo(2f));

            Object.DestroyImmediate(data);
        }
    }
}
