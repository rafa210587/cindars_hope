using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_83 — EditMode tests para a logica pura de ataques-assinatura.
    /// Sem MonoBehaviours / sem scene / sem UnityEngine.Time.
    /// </summary>
    public class EnemySignatureActionsTests
    {
        // ── ComboStrike ──────────────────────────────────────────────────────────

        [Test]
        public void ComboHitCount_Clamped_ToMax()
        {
            int result = EnemyActionExecution.ResolveComboHitCount(999);
            Assert.AreEqual(EnemyActionExecution.MaxComboHits, result);
        }

        [Test]
        public void ComboHitCount_Minimum_IsOne()
        {
            int result = EnemyActionExecution.ResolveComboHitCount(-5);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void ComboHitCount_ReturnsAsIs_WhenInRange()
        {
            int result = EnemyActionExecution.ResolveComboHitCount(3);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void ComboHitDamage_SumsToTotalBaseDamage()
        {
            int totalDamage = 30;
            int hits = 3;
            int sum = 0;
            for (int i = 0; i < hits; i++)
                sum += EnemyActionExecution.ComboHitDamage(totalDamage, i, hits);
            Assert.AreEqual(totalDamage, sum, "Soma dos hits do combo deve igualar o dano base total");
        }

        [Test]
        public void ComboHitDamage_LastHitAbsorbsRemainder()
        {
            // 31 / 3 = 10 (int div); resto = 1; ultimo hit = 31 - 10*2 = 11
            int total = 31;
            int hits = 3;
            int lastHit = EnemyActionExecution.ComboHitDamage(total, hits - 1, hits);
            Assert.AreEqual(11, lastHit);
        }

        [Test]
        public void ComboHitDamage_SingleHit_EqualsTotalDamage()
        {
            int total = 15;
            int result = EnemyActionExecution.ComboHitDamage(total, 0, 1);
            Assert.AreEqual(total, result);
        }

        // ── TelegraphedAoE ───────────────────────────────────────────────────────

        [Test]
        public void AoE_ResolveTargets_HitsPlayerInsideRadius()
        {
            var origin = new Vector2(0, 0);
            float radius = 2f;
            var positions = new List<Vector2> { new Vector2(1f, 1f) }; // dentro
            var hits = EnemyActionExecution.ResolveAoETargetIndices(origin, radius, positions);
            Assert.AreEqual(1, hits.Count, "Player dentro do raio deve ser atingido");
        }

        [Test]
        public void AoE_ResolveTargets_MissesPlayerOutsideRadius()
        {
            var origin = new Vector2(0, 0);
            float radius = 1f;
            var positions = new List<Vector2> { new Vector2(5f, 0f) }; // fora
            var hits = EnemyActionExecution.ResolveAoETargetIndices(origin, radius, positions);
            Assert.AreEqual(0, hits.Count, "Player fora do raio nao deve ser atingido");
        }

        [Test]
        public void AoE_TelegraphDelay_NotComplete_BeforeDelay()
        {
            Assert.IsFalse(EnemyActionExecution.IsTelegraphDelayComplete(0.3f, 0.8f),
                "Dano nao deve resolver antes do atraso do telegraph");
        }

        [Test]
        public void AoE_TelegraphDelay_Complete_AfterDelay()
        {
            Assert.IsTrue(EnemyActionExecution.IsTelegraphDelayComplete(1.0f, 0.8f),
                "Dano deve resolver apos o atraso do telegraph");
        }

        // ── SummonAdds ───────────────────────────────────────────────────────────

        [Test]
        public void SummonAdds_Deterministic_SameSeedSamePositions()
        {
            var origin = new Vector2(3f, 4f);
            int seed = EnemyActionExecution.DeriveSummonSeed("runA", 2, "enemy_invoker");
            var posA = EnemyActionExecution.GenerateSummonPositions(origin, 2, seed);
            var posB = EnemyActionExecution.GenerateSummonPositions(origin, 2, seed);
            Assert.AreEqual(posA.Count, posB.Count);
            for (int i = 0; i < posA.Count; i++)
                Assert.AreEqual(posA[i], posB[i], $"Posicao {i} deve ser identica (determinismo)");
        }

        [Test]
        public void SummonAdds_DifferentSeed_DifferentPositions()
        {
            var origin = new Vector2(0, 0);
            int seedA = EnemyActionExecution.DeriveSummonSeed("runA", 1, "enemy");
            int seedB = EnemyActionExecution.DeriveSummonSeed("runB", 2, "enemy");
            // Seeds diferentes devem ser diferentes (nao devem colidir no mesmo caso trivial)
            Assert.AreNotEqual(seedA, seedB);
        }

        [Test]
        public void SummonAdds_Count_CappedByRoom()
        {
            // Sala com 7 adds -> so pode adicionar 1 (cap = 8)
            int count = EnemyActionExecution.ResolveSummonCount(4, 7);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SummonAdds_Count_ZeroWhenRoomFull()
        {
            int count = EnemyActionExecution.ResolveSummonCount(2, 8);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void SummonAdds_Count_CappedToMaxSummonCount()
        {
            // Pede 99 adds mas MaxSummonCount = 4
            int count = EnemyActionExecution.ResolveSummonCount(99, 0);
            Assert.AreEqual(EnemyActionExecution.MaxSummonCount, count);
        }

        // ── MultiHitCharge ───────────────────────────────────────────────────────

        [Test]
        public void MultiHitCharge_HitsTargetOnLine()
        {
            var origin = new Vector2(0, 0);
            var end = new Vector2(4, 0);
            var targets = new List<Vector2> { new Vector2(2f, 0f) }; // diretamente na linha
            var hits = EnemyActionExecution.ResolveMultiHitChargeTargets(origin, end, targets, 0.8f);
            Assert.AreEqual(1, hits.Count, "Alvo na linha do charge deve ser atingido");
        }

        [Test]
        public void MultiHitCharge_MissesTargetOffLine()
        {
            var origin = new Vector2(0, 0);
            var end = new Vector2(4, 0);
            var targets = new List<Vector2> { new Vector2(2f, 3f) }; // muito longe da linha
            var hits = EnemyActionExecution.ResolveMultiHitChargeTargets(origin, end, targets, 0.8f);
            Assert.AreEqual(0, hits.Count, "Alvo longe da linha do charge nao deve ser atingido");
        }

        [Test]
        public void MultiHitCharge_MissesTargetBehindOrigin()
        {
            var origin = new Vector2(0, 0);
            var end = new Vector2(4, 0);
            var targets = new List<Vector2> { new Vector2(-1f, 0f) }; // antes do inicio
            var hits = EnemyActionExecution.ResolveMultiHitChargeTargets(origin, end, targets, 0.8f);
            Assert.AreEqual(0, hits.Count, "Alvo atras da origem nao deve ser atingido");
        }

        // ── DebuffStrike ─────────────────────────────────────────────────────────

        [Test]
        public void DebuffStrike_ShouldApply_WhenChanceIsOne()
        {
            Assert.IsTrue(EnemyActionExecution.ShouldApplyDebuff(1f, 0f));
            Assert.IsTrue(EnemyActionExecution.ShouldApplyDebuff(1f, 0.99f));
        }

        [Test]
        public void DebuffStrike_ShouldNotApply_WhenChanceIsZero()
        {
            Assert.IsFalse(EnemyActionExecution.ShouldApplyDebuff(0f, 0f));
        }

        [Test]
        public void DebuffStrike_ResolveStatusId_UsesDebuffStatusIdFirst()
        {
            string result = EnemyActionExecution.ResolveDebuffStatusId("status_poison", new[] { "status_bleed" });
            Assert.AreEqual("status_poison", result);
        }

        [Test]
        public void DebuffStrike_ResolveStatusId_FallsBackToStatusApplicationIds()
        {
            string result = EnemyActionExecution.ResolveDebuffStatusId(string.Empty, new[] { "status_chill" });
            Assert.AreEqual("status_chill", result);
        }

        [Test]
        public void DebuffStrike_ResolveStatusId_ReturnsEmpty_WhenNoneAvailable()
        {
            string result = EnemyActionExecution.ResolveDebuffStatusId(null, null);
            Assert.AreEqual(string.Empty, result);
        }
    }
}
