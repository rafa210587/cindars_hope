using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using CindarsHope.Enemy;
using CindarsHope.Foundation;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// spec_enemy_attack_kits_v1 — EditMode tests para as 4 primitivas P2 do catalogo
    /// (Rise-once, AllyHeal/AllyBuff, HazardZone, Pull). Logica pura, sem MonoBehaviour/scene,
    /// seguindo o padrao de EnemySignatureActionsTests.cs (fable_83).
    /// </summary>
    public class EnemyAttackKitPrimitivesTests
    {
        // ── Rise-Once ────────────────────────────────────────────────────────────

        [Test]
        public void RiseOnce_BlockedByDamageType_MatchCaseInsensitive()
        {
            bool blocked = EnemyRiseOnceRules.ShouldBlockRise("Fire", new[] { "fire", "radiant" });
            Assert.IsTrue(blocked, "Elemento bloqueador deve casar case-insensitive");
        }

        [Test]
        public void RiseOnce_NotBlocked_WhenDamageTypeNotInList()
        {
            bool blocked = EnemyRiseOnceRules.ShouldBlockRise("Physical", new[] { "fire", "radiant" });
            Assert.IsFalse(blocked);
        }

        [Test]
        public void RiseOnce_NotBlocked_WhenListEmpty()
        {
            bool blocked = EnemyRiseOnceRules.ShouldBlockRise("Fire", new string[0]);
            Assert.IsFalse(blocked);
        }

        [Test]
        public void RiseOnce_ShouldRise_WhenEnabledNotConsumedNotBlocked()
        {
            bool shouldRise = EnemyRiseOnceRules.ShouldRiseOnce(
                riseOnceEnabled: true, alreadyConsumed: false, lastDamageType: "Physical", blockedTypes: new[] { "fire" });
            Assert.IsTrue(shouldRise);
        }

        [Test]
        public void RiseOnce_ShouldNotRise_WhenAlreadyConsumed()
        {
            bool shouldRise = EnemyRiseOnceRules.ShouldRiseOnce(
                riseOnceEnabled: true, alreadyConsumed: true, lastDamageType: "Physical", blockedTypes: new[] { "fire" });
            Assert.IsFalse(shouldRise, "Rise-once so pode consumir 1x por vida");
        }

        [Test]
        public void RiseOnce_ShouldNotRise_WhenBlockedByDamageType()
        {
            bool shouldRise = EnemyRiseOnceRules.ShouldRiseOnce(
                riseOnceEnabled: true, alreadyConsumed: false, lastDamageType: "fire", blockedTypes: new[] { "fire", "radiant" });
            Assert.IsFalse(shouldRise, "Elemento bloqueador impede o reerguimento");
        }

        [Test]
        public void RiseOnce_ShouldNotRise_WhenDisabled()
        {
            bool shouldRise = EnemyRiseOnceRules.ShouldRiseOnce(
                riseOnceEnabled: false, alreadyConsumed: false, lastDamageType: "Physical", blockedTypes: new string[0]);
            Assert.IsFalse(shouldRise);
        }

        [Test]
        public void RiseOnce_ResolveHp_UsesConfiguredPercent()
        {
            int hp = EnemyRiseOnceRules.ResolveRiseHp(maxHp: 100, riseOnceHpPercent: 0.25f);
            Assert.AreEqual(25, hp);
        }

        [Test]
        public void RiseOnce_ResolveHp_NeverReturnsZero()
        {
            int hp = EnemyRiseOnceRules.ResolveRiseHp(maxHp: 10, riseOnceHpPercent: 0f);
            Assert.AreEqual(1, hp, "Reerguer com 0 HP nao faz sentido — minimo 1");
        }

        // ── Ally Heal/Buff ───────────────────────────────────────────────────────

        [Test]
        public void AllyHeal_SelectsMostWounded_WithinRadius()
        {
            var origin = new Vector2(0, 0);
            var candidates = new List<EnemyActionExecution.AllyCandidate>
            {
                new EnemyActionExecution.AllyCandidate(0, new Vector2(1f, 0f), 0.8f), // levemente ferido
                new EnemyActionExecution.AllyCandidate(1, new Vector2(2f, 0f), 0.2f), // mais ferido
            };
            int result = EnemyActionExecution.ResolveAllyHealTarget(origin, radius: 4f, candidates);
            Assert.AreEqual(1, result, "Deve escolher o aliado mais ferido dentro do raio");
        }

        [Test]
        public void AllyHeal_SelectsClosest_WhenNoneWounded()
        {
            var origin = new Vector2(0, 0);
            var candidates = new List<EnemyActionExecution.AllyCandidate>
            {
                new EnemyActionExecution.AllyCandidate(0, new Vector2(3f, 0f), 1f),
                new EnemyActionExecution.AllyCandidate(1, new Vector2(1f, 0f), 1f), // mais proximo
            };
            int result = EnemyActionExecution.ResolveAllyHealTarget(origin, radius: 4f, candidates);
            Assert.AreEqual(1, result, "Sem ferido, deve escolher o mais proximo");
        }

        [Test]
        public void AllyHeal_ReturnsNegativeOne_WhenNoCandidateInRadius()
        {
            var origin = new Vector2(0, 0);
            var candidates = new List<EnemyActionExecution.AllyCandidate>
            {
                new EnemyActionExecution.AllyCandidate(0, new Vector2(10f, 0f), 0.1f),
            };
            int result = EnemyActionExecution.ResolveAllyHealTarget(origin, radius: 4f, candidates);
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void AllyHeal_ReturnsNegativeOne_WhenNoCandidates()
        {
            int result = EnemyActionExecution.ResolveAllyHealTarget(Vector2.zero, 4f, new List<EnemyActionExecution.AllyCandidate>());
            Assert.AreEqual(-1, result);
        }

        // ── Hazard Zone ──────────────────────────────────────────────────────────

        [Test]
        public void HazardZone_NotExpired_BeforeDuration()
        {
            Assert.IsFalse(EnemyActionExecution.IsHazardExpired(elapsedSeconds: 2f, durationSeconds: 4f));
        }

        [Test]
        public void HazardZone_Expired_AfterDuration()
        {
            Assert.IsTrue(EnemyActionExecution.IsHazardExpired(elapsedSeconds: 4f, durationSeconds: 4f));
        }

        [Test]
        public void HazardZone_TickCount_ResolvesDeterministically()
        {
            int ticks = EnemyActionExecution.ResolveHazardTickCount(elapsedSinceLastTick: 2.5f, tickIntervalSeconds: 1f);
            Assert.AreEqual(2, ticks, "2.5s / 1s de intervalo = 2 ticks completos");
        }

        [Test]
        public void HazardZone_TickCount_ZeroWhenBelowInterval()
        {
            int ticks = EnemyActionExecution.ResolveHazardTickCount(elapsedSinceLastTick: 0.5f, tickIntervalSeconds: 1f);
            Assert.AreEqual(0, ticks);
        }

        [Test]
        public void HazardZone_IsInsideHazard_TrueWithinRadius()
        {
            bool inside = EnemyActionExecution.IsInsideHazard(new Vector2(0, 0), 2f, new Vector2(1f, 1f));
            Assert.IsTrue(inside);
        }

        [Test]
        public void HazardZone_IsInsideHazard_FalseOutsideRadius()
        {
            bool inside = EnemyActionExecution.IsInsideHazard(new Vector2(0, 0), 1f, new Vector2(5f, 0f));
            Assert.IsFalse(inside);
        }

        // ── Pull ─────────────────────────────────────────────────────────────────

        [Test]
        public void Pull_MovesPlayerTowardOrigin_ByConfiguredDistance()
        {
            var origin = new Vector2(5f, 0f);
            var player = new Vector2(0f, 0f);
            var result = EnemyActionExecution.ResolvePullTargetPosition(origin, player, pullDistanceTiles: 2f);
            Assert.AreEqual(new Vector2(2f, 0f), result);
        }

        [Test]
        public void Pull_ClampsToOrigin_WhenDistanceExceedsGap()
        {
            var origin = new Vector2(1f, 0f);
            var player = new Vector2(0f, 0f);
            var result = EnemyActionExecution.ResolvePullTargetPosition(origin, player, pullDistanceTiles: 10f);
            Assert.AreEqual(origin, result, "Nao deve puxar o player alem da posicao do atacante/hazard");
        }

        [Test]
        public void Pull_NoOp_WhenAlreadyAtOrigin()
        {
            var origin = new Vector2(3f, 3f);
            var result = EnemyActionExecution.ResolvePullTargetPosition(origin, origin, pullDistanceTiles: 2f);
            Assert.AreEqual(origin, result);
        }

        // ── Salvo Multiplo (follow-up salvas de projeteis em leque) ─────────────────────────────

        [Test]
        public void Salvo_Count1_ReturnsOnlyBaseDirection_IgnoringSpread()
        {
            var baseDir = new Vector2(1f, 0f);
            var directions = EnemyActionExecution.ResolveSalvoDirections(baseDir, count: 1, spreadAngleDegrees: 45f);
            Assert.AreEqual(1, directions.Length);
            Assert.AreEqual(baseDir, directions[0], "count=1 deve ignorar o spread e disparar so na direcao base");
        }

        [Test]
        public void Salvo_Count3_Symmetric_WithCenterOnBase()
        {
            var baseDir = new Vector2(1f, 0f);
            var directions = EnemyActionExecution.ResolveSalvoDirections(baseDir, count: 3, spreadAngleDegrees: 30f);
            Assert.AreEqual(3, directions.Length);

            // Direcao central (indice do meio) deve ser exatamente a base (count impar tem centro).
            Assert.AreEqual(baseDir.x, directions[1].x, 0.0001f, "Direcao central deve casar com a base em X");
            Assert.AreEqual(baseDir.y, directions[1].y, 0.0001f, "Direcao central deve casar com a base em Y");

            // As duas direcoes extremas devem ser simetricas em torno da base (mesmo Y absoluto, sinais opostos).
            Assert.AreEqual(-directions[0].y, directions[2].y, 0.0001f, "Extremos do leque devem ser simetricos");
        }

        [Test]
        public void Salvo_Count4_HasNoCenterDirection()
        {
            var baseDir = new Vector2(1f, 0f);
            var directions = EnemyActionExecution.ResolveSalvoDirections(baseDir, count: 4, spreadAngleDegrees: 40f);
            Assert.AreEqual(4, directions.Length);

            // Nenhuma das 4 direcoes deve ser exatamente igual a base (count par nao tem centro).
            foreach (var dir in directions)
            {
                bool isExactlyBase = Mathf.Abs(dir.x - baseDir.x) < 0.0001f && Mathf.Abs(dir.y - baseDir.y) < 0.0001f;
                Assert.IsFalse(isExactlyBase, "count par nao deve ter nenhuma direcao exatamente na base (sem centro)");
            }
        }

        [Test]
        public void Salvo_ZeroSpread_AllDirectionsCollapseToBase()
        {
            var baseDir = new Vector2(0f, 1f);
            var directions = EnemyActionExecution.ResolveSalvoDirections(baseDir, count: 3, spreadAngleDegrees: 0f);
            foreach (var dir in directions)
            {
                Assert.AreEqual(baseDir.x, dir.x, 0.0001f);
                Assert.AreEqual(baseDir.y, dir.y, 0.0001f);
            }
        }

        [Test]
        public void Salvo_CountClamped_ToMaxProjectileCount()
        {
            var directions = EnemyActionExecution.ResolveSalvoDirections(Vector2.right, count: 99, spreadAngleDegrees: 30f);
            Assert.AreEqual(EnemyActionExecution.MaxProjectileCount, directions.Length);
        }

        [Test]
        public void Salvo_CountClamped_ToMinimumOne()
        {
            var directions = EnemyActionExecution.ResolveSalvoDirections(Vector2.right, count: 0, spreadAngleDegrees: 30f);
            Assert.AreEqual(1, directions.Length);
        }
    }
}
