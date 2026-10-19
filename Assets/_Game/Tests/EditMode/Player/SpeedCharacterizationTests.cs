using CindarsHope.Player.Movement;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_47 — testes de CARACTERIZAÇÃO do caminho de velocidade. Capturam o valor efetivo
    /// esperado para a matriz de cenários (block on/off, dash, displacement, exhausted on/off,
    /// status slow apply/expire e pares simultâneos) e provam que o composer reproduz o
    /// comportamento esperado SEM a corrida do antigo mutável.
    ///
    /// Constantes espelhadas dos sistemas reais:
    ///   Block slow  = 0.45  (PlayerBlockController._blockSlowMultiplier)
    ///   Exhausted   = 0.85  (PlayerConditionService.ExhaustedSpeedMultiplier)
    ///   Dash/Disp   = 0.0   (durante o deslocamento)
    /// </summary>
    public class SpeedCharacterizationTests
    {
        private const float BlockSlow = 0.45f;
        private const float Exhausted = 0.85f;

        private static PlayerSpeedComposer NewComposer() => new PlayerSpeedComposer();

        // -------------------------------------------------- estados isolados

        [Test]
        public void Baseline_NoFactors_IsOne()
        {
            Assert.AreEqual(1f, NewComposer().Value, 0.0001f);
        }

        [Test]
        public void BlockOnly_AppliesBlockSlow()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Block, BlockSlow);
            Assert.AreEqual(0.45f, c.Value, 0.0001f);
        }

        [Test]
        public void BlockReleased_ReturnsToBaseline()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Block, BlockSlow);
            c.ClearFactor(SpeedFactorKind.Block);
            Assert.AreEqual(1f, c.Value, 0.0001f);
        }

        [Test]
        public void ExhaustedOnly_AppliesExhausted()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            Assert.AreEqual(0.85f, c.Value, 0.0001f);
        }

        [Test]
        public void ExhaustedCleared_ReturnsToBaseline()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            c.ClearFactor(SpeedFactorKind.Exhausted);
            Assert.AreEqual(1f, c.Value, 0.0001f);
        }

        [Test]
        public void DashActive_StopsControllerMovement()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Dash, 0f);
            Assert.AreEqual(0f, c.Value, 0.0001f);
        }

        [Test]
        public void DisplacementActive_StopsControllerMovement()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Displacement, 0f);
            Assert.AreEqual(0f, c.Value, 0.0001f);
        }

        [Test]
        public void StatusSlowApply_ThenExpire()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Status, 0.6f); // chill/slow MoveSpeedMultiplier de asset
            Assert.AreEqual(0.6f, c.Value, 0.0001f, "Slow ativo.");
            c.ClearFactor(SpeedFactorKind.Status);
            Assert.AreEqual(1f, c.Value, 0.0001f, "Slow expira → velocidade normal.");
        }

        // -------------------------------------------------- pares simultâneos (corrida corrigida)

        [Test]
        public void BlockDuringExhausted_ComposesBoth()
        {
            // CORREÇÃO de corrida: no antigo padrão, soltar um corrompia o outro. Aqui é o produto.
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            c.SetFactor(SpeedFactorKind.Block, BlockSlow);
            Assert.AreEqual(0.3825f, c.Value, 0.0001f, "0.85 × 0.45 = 0.3825");
        }

        [Test]
        public void BlockReleasedWhileExhausted_KeepsOnlyExhausted()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            c.SetFactor(SpeedFactorKind.Block, BlockSlow);

            c.ClearFactor(SpeedFactorKind.Block);
            Assert.AreEqual(0.85f, c.Value, 0.0001f, "Sem resíduo do block; exhausted intacto.");
        }

        [Test]
        public void StatusExpiringDuringDash_DashStillStops()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Dash, 0f);
            c.SetFactor(SpeedFactorKind.Status, 0.6f);
            Assert.AreEqual(0f, c.Value, 0.0001f, "Dash (0) domina o produto.");

            // Status expira no meio do dash — não corrompe o fator dash.
            c.ClearFactor(SpeedFactorKind.Status);
            Assert.AreEqual(0f, c.Value, 0.0001f, "Dash continua parando o movimento.");

            // Dash termina: velocidade normal restaurada limpa.
            c.ClearFactor(SpeedFactorKind.Dash);
            Assert.AreEqual(1f, c.Value, 0.0001f);
        }

        [Test]
        public void ExhaustedPlusStatusFloor_RespectsStatusFloorOnly()
        {
            // Status pisado em 0.5 (floor F01) compõe com exhausted 0.85.
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            c.SetFactor(SpeedFactorKind.Status, 0.1f); // será pisado em 0.5
            Assert.AreEqual(0.425f, c.Value, 0.0001f, "0.85 × 0.5 = 0.425 (floor de status aplicado).");
        }

        [Test]
        public void DerivedMoveSpeedWithEverything_IsFullProduct()
        {
            var c = NewComposer();
            c.SetFactor(SpeedFactorKind.DerivedMoveSpeed, 1.2f);
            c.SetFactor(SpeedFactorKind.Exhausted, Exhausted);
            c.SetFactor(SpeedFactorKind.Block, BlockSlow);
            Assert.AreEqual(0.459f, c.Value, 0.0001f, "1.2 × 0.85 × 0.45 = 0.459");
        }
    }
}
