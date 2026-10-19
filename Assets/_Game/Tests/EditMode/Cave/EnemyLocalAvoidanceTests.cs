using NUnit.Framework;
using UnityEngine;
using CindarsHope.Enemy;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_83 — EditMode tests para a logica pura de desvio local (EnemyLocalAvoidance.Steer).
    /// Sem raycasts reais / sem scene / determinisico.
    /// </summary>
    public class EnemyLocalAvoidanceTests
    {
        // ── GetWhiskerDirections ────────────────────────────────────────────────

        [Test]
        public void WhiskerDirections_AreNormalized()
        {
            EnemyLocalAvoidance.GetWhiskerDirections(
                Vector2.right, out var center, out var left, out var right);

            Assert.That(center.magnitude, Is.EqualTo(1f).Within(0.001f));
            Assert.That(left.magnitude, Is.EqualTo(1f).Within(0.001f));
            Assert.That(right.magnitude, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void WhiskerDirections_CenterMatchesDesiredDirection()
        {
            var desired = new Vector2(1f, 0f);
            EnemyLocalAvoidance.GetWhiskerDirections(desired, out var center, out _, out _);
            Assert.That(center.x, Is.EqualTo(desired.x).Within(0.001f));
            Assert.That(center.y, Is.EqualTo(desired.y).Within(0.001f));
        }

        [Test]
        public void WhiskerDirections_LeftAndRight_AreSymmetric()
        {
            EnemyLocalAvoidance.GetWhiskerDirections(
                Vector2.up, out _, out var left, out var right);
            // Para cima: right deve ter x > 0, left deve ter x < 0 (simetria)
            Assert.Greater(right.x, 0f, "Whisker direito deve ter componente x positivo");
            Assert.Less(left.x, 0f, "Whisker esquerdo deve ter componente x negativo");
        }

        // ── Steer — caminho livre ──────────────────────────────────────────────

        [Test]
        public void Steer_FreePath_NoAvoidance()
        {
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, false, false, false, false);
            Assert.IsFalse(result.IsAvoiding, "Caminho livre nao deve ativar desvio");
        }

        [Test]
        public void Steer_FreePath_DirectionMatchesDesired()
        {
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, false, false, false, false);
            Assert.That(result.Direction.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(result.Direction.y, Is.EqualTo(0f).Within(0.01f));
        }

        // ── Steer — desvio quando centro bloqueado ─────────────────────────────

        [Test]
        public void Steer_CenterBlocked_ActivatesAvoidance()
        {
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, true, false, false, false);
            Assert.IsTrue(result.IsAvoiding, "Centro bloqueado deve ativar desvio");
        }

        [Test]
        public void Steer_CenterAndLeftBlocked_TurnsRight()
        {
            // centro bloqueado + esquerdo bloqueado -> deve desviar para direita
            var desired = new Vector2(0f, 1f); // indo para cima
            var result = EnemyLocalAvoidance.Steer(desired, true, true, false, false);
            Assert.IsTrue(result.IsAvoiding);
            // Desvio para direita de "cima" = componente x > 0
            Assert.Greater(result.Direction.x, 0f, "Desvio deve ter componente x positivo (direita)");
        }

        [Test]
        public void Steer_CenterAndRightBlocked_TurnsLeft()
        {
            // centro bloqueado + direito bloqueado -> deve desviar para esquerda
            var desired = new Vector2(0f, 1f); // indo para cima
            var result = EnemyLocalAvoidance.Steer(desired, true, false, true, false);
            Assert.IsTrue(result.IsAvoiding);
            // Desvio para esquerda de "cima" = componente x < 0
            Assert.Less(result.Direction.x, 0f, "Desvio deve ter componente x negativo (esquerda)");
        }

        [Test]
        public void Steer_AllBlocked_StillReturnsADirection()
        {
            // Todos os whiskers bloqueados: contorno pelo lado direito (determinisico)
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, true, true, true, false);
            Assert.IsTrue(result.IsAvoiding);
            Assert.Greater(result.Direction.magnitude, 0.1f, "Deve retornar uma direcao valida mesmo com tudo bloqueado");
        }

        // ── Histerese ─────────────────────────────────────────────────────────

        [Test]
        public void Steer_Hysteresis_MaintainsAvoidance_WhenCenterStillBlocked()
        {
            // Estava desviando, centro ainda bloqueado -> continua desviando
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, true, false, false, true);
            Assert.IsTrue(result.IsAvoiding, "Histerese: desvio deve continuar se centro ainda bloqueado");
        }

        [Test]
        public void Steer_Hysteresis_ExitsAvoidance_WhenPathClear()
        {
            // Estava desviando, mas agora tudo livre -> pode sair
            var desired = new Vector2(1f, 0f);
            var result = EnemyLocalAvoidance.Steer(desired, false, false, false, true);
            Assert.IsFalse(result.IsAvoiding, "Histerese: desvio deve terminar se caminho livre");
        }

        // ── Constantes ────────────────────────────────────────────────────────

        [Test]
        public void Constants_WhiskerLength_Positive()
        {
            Assert.Greater(EnemyLocalAvoidance.WhiskerLength, 0f);
        }

        [Test]
        public void Constants_WhiskerAngle_InRange()
        {
            Assert.Greater(EnemyLocalAvoidance.WhiskerAngleDeg, 0f);
            Assert.Less(EnemyLocalAvoidance.WhiskerAngleDeg, 90f);
        }

        [Test]
        public void Constants_AvoidanceSpeedMultiplier_InValidRange()
        {
            Assert.Greater(EnemyLocalAvoidance.AvoidanceSpeedMultiplier, 0f);
            Assert.LessOrEqual(EnemyLocalAvoidance.AvoidanceSpeedMultiplier, 1f);
        }
    }
}
