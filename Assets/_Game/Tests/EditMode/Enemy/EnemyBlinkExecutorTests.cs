using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Enemy
{
    public class EnemyBlinkExecutorTests
    {
        [Test]
        public void BlinkRange_Zero_ReturnsFail()
        {
            var result = EnemyBlinkExecutor.CalculateDestination(
                enemyPos: Vector2.zero, playerPos: Vector2.right * 5f,
                blinkRange: 0f, flankSide: 1f);

            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.FailReason);
        }

        [Test]
        public void BlinkRange_Negative_ReturnsFail()
        {
            var result = EnemyBlinkExecutor.CalculateDestination(
                enemyPos: Vector2.zero, playerPos: Vector2.right * 5f,
                blinkRange: -1f, flankSide: 1f);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void EnemyAndPlayerOverlap_ReturnsFail()
        {
            var result = EnemyBlinkExecutor.CalculateDestination(
                enemyPos: Vector2.zero, playerPos: Vector2.zero,
                blinkRange: 1.5f, flankSide: 1f);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ValidInputs_ReturnsSuccess()
        {
            var result = EnemyBlinkExecutor.CalculateDestination(
                enemyPos: Vector2.zero, playerPos: new Vector2(5f, 0f),
                blinkRange: 1.5f, flankSide: 1f);

            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Destination_IsNearPlayer_WithinBlinkRange()
        {
            Vector2 playerPos = new Vector2(5f, 0f);
            float blinkRange = 1.5f;

            var result = EnemyBlinkExecutor.CalculateDestination(
                enemyPos: Vector2.zero, playerPos: playerPos,
                blinkRange: blinkRange, flankSide: 1f);

            Assert.IsTrue(result.Success);
            float distToPlayer = Vector2.Distance(result.Destination, playerPos);
            // Should land roughly blinkRange away — allow for flank offset
            Assert.Less(distToPlayer, blinkRange * 2f, "Destination should be near the player");
        }

        [Test]
        public void FlankSides_ProduceDifferentDestinations()
        {
            Vector2 enemyPos = Vector2.zero;
            Vector2 playerPos = new Vector2(5f, 0f);

            var left  = EnemyBlinkExecutor.CalculateDestination(enemyPos, playerPos, 1.5f, flankSide:  1f);
            var right = EnemyBlinkExecutor.CalculateDestination(enemyPos, playerPos, 1.5f, flankSide: -1f);

            Assert.IsTrue(left.Success);
            Assert.IsTrue(right.Success);
            Assert.AreNotEqual(left.Destination, right.Destination,
                "Opposite flank sides must produce different destinations");
        }

        [Test]
        public void SameInputs_ProduceSameDestination_Deterministic()
        {
            Vector2 enemy  = new Vector2(1f, 2f);
            Vector2 player = new Vector2(6f, 2f);

            var r1 = EnemyBlinkExecutor.CalculateDestination(enemy, player, 1.5f, 1f);
            var r2 = EnemyBlinkExecutor.CalculateDestination(enemy, player, 1.5f, 1f);

            Assert.AreEqual(r1.Destination, r2.Destination, "Same inputs must yield same result (deterministic)");
        }
    }
}
