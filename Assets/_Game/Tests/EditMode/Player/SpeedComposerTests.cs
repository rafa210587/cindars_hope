using CindarsHope.Player.Movement;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_47 — PlayerSpeedComposer puro: produto de fatores nomeados, clear restaura,
    /// fatores simultâneos compõem sem corromper, floor F01 no fator de status.
    /// </summary>
    public class SpeedComposerTests
    {
        [Test]
        public void Value_DefaultsToOne_WhenNoFactors()
        {
            var composer = new PlayerSpeedComposer();
            Assert.AreEqual(1f, composer.Value, 0.0001f);
        }

        [Test]
        public void SetFactor_MultipliesIntoProduct()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Block, 0.45f);
            Assert.AreEqual(0.45f, composer.Value, 0.0001f);
        }

        [Test]
        public void Factors_ComposeMultiplicatively()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Block, 0.5f);
            composer.SetFactor(SpeedFactorKind.Exhausted, 0.85f);
            Assert.AreEqual(0.425f, composer.Value, 0.0001f, "0.5 × 0.85 = 0.425");
        }

        [Test]
        public void ClearFactor_RestoresOtherFactorsExactly()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Block, 0.5f);
            composer.SetFactor(SpeedFactorKind.Exhausted, 0.85f);

            // Solta o block: deve sobrar EXATAMENTE o fator exhausted (sem corrida de restore).
            composer.ClearFactor(SpeedFactorKind.Block);
            Assert.AreEqual(0.85f, composer.Value, 0.0001f);

            composer.ClearFactor(SpeedFactorKind.Exhausted);
            Assert.AreEqual(1f, composer.Value, 0.0001f, "Sem fatores: volta a 1.0.");
        }

        [Test]
        public void RaceScenario_RemovingOneFactorDoesNotCorruptOther()
        {
            // Cenário de corrida do antigo padrão: block + exhausted simultâneos, soltos fora de ordem.
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Exhausted, 0.85f);
            composer.SetFactor(SpeedFactorKind.Block, 0.45f);

            // Solta exhausted ANTES do block (ordem inversa da aplicação).
            composer.ClearFactor(SpeedFactorKind.Exhausted);
            Assert.AreEqual(0.45f, composer.Value, 0.0001f, "Só o block permanece — sem valor preso.");

            composer.ClearFactor(SpeedFactorKind.Block);
            Assert.AreEqual(1f, composer.Value, 0.0001f, "Velocidade normal restaurada sem resíduo.");
        }

        [Test]
        public void DisplacementFactorZero_StopsMovement()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Exhausted, 0.85f);
            composer.SetFactor(SpeedFactorKind.Displacement, 0f);
            Assert.AreEqual(0f, composer.Value, 0.0001f, "Displacement/dash zera o produto.");

            composer.ClearFactor(SpeedFactorKind.Displacement);
            Assert.AreEqual(0.85f, composer.Value, 0.0001f, "Ao terminar, exhausted continua intacto.");
        }

        [Test]
        public void StatusFactor_AppliesMinSpeedFloor()
        {
            var composer = new PlayerSpeedComposer();
            // Slow não-letal abaixo do floor é pisado em MinSpeedFloor (regra F01).
            composer.SetFactor(SpeedFactorKind.Status, 0.2f);
            Assert.AreEqual(PlayerSpeedComposer.MinSpeedFloor, composer.Value, 0.0001f);
        }

        [Test]
        public void StatusFactor_RootStunZeroBypassesFloor()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Status, 0f); // Root/Stun
            Assert.AreEqual(0f, composer.Value, 0.0001f, "Root/Stun zeram, sem floor.");
        }

        [Test]
        public void StatusFloor_OnlyAppliesToStatusKind()
        {
            // O floor é exclusivo do fator de status; block pode ir abaixo de 0.5.
            Assert.AreEqual(0.2f, PlayerSpeedComposer.NormalizeFactor(SpeedFactorKind.Block, 0.2f), 0.0001f);
            Assert.AreEqual(0.5f, PlayerSpeedComposer.NormalizeFactor(SpeedFactorKind.Status, 0.2f), 0.0001f);
        }

        [Test]
        public void NegativeFactor_ClampedToZero()
        {
            Assert.AreEqual(0f, PlayerSpeedComposer.NormalizeFactor(SpeedFactorKind.Dash, -3f), 0.0001f);
        }

        [Test]
        public void ClearAll_RemovesEveryFactor()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Block, 0.5f);
            composer.SetFactor(SpeedFactorKind.Status, 0.6f);
            composer.SetFactor(SpeedFactorKind.DerivedMoveSpeed, 1.2f);

            composer.ClearAll();
            Assert.AreEqual(1f, composer.Value, 0.0001f, "Teardown/respawn não deixa fator órfão.");
            Assert.IsFalse(composer.HasFactor(SpeedFactorKind.Block));
        }

        [Test]
        public void DerivedMoveSpeedAbove1_CompoundsWithSlow()
        {
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.DerivedMoveSpeed, 1.1f);
            composer.SetFactor(SpeedFactorKind.Block, 0.5f);
            Assert.AreEqual(0.55f, composer.Value, 0.0001f, "1.1 × 0.5 = 0.55");
        }
    }
}
