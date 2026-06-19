using CindarsHope.Combat;
using CindarsHope.Player.Movement;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// fable_69 — sprint em combate (lógica pura). Cobre os critérios de aceite:
    /// CA-1 (fator aplicado + drenagem 8/s + soltar restaura penalidade) e
    /// CA-2 (sem stamina cai sozinho; stun/modal cancelam; fora de combate é no-op).
    /// Espelha o estilo de SpeedComposerTests/SpeedCharacterizationTests (NUnit, sem cena).
    /// </summary>
    public class SprintTests
    {
        // -------------------------------------------------- janela de combate (CombatStateTracker)

        [Test]
        public void CombatWindow_NoActivity_IsNotInCombat()
        {
            Assert.IsFalse(CombatWindowRules.IsInCombat(-1000f, 100f), "Sentinela negativa = nunca em combate.");
        }

        [Test]
        public void CombatWindow_WithinFourSeconds_IsInCombat()
        {
            // Dano às 10s; agora 13.9s → ainda dentro da janela de 4s.
            Assert.IsTrue(CombatWindowRules.IsInCombat(10f, 13.9f));
        }

        [Test]
        public void CombatWindow_AfterFourSeconds_LeavesCombat()
        {
            // Dano às 10s; agora 14.01s → janela de 4s expirou.
            Assert.IsFalse(CombatWindowRules.IsInCombat(10f, 14.01f));
            Assert.AreEqual(4f, CombatWindowRules.CombatWindowSeconds, 0.0001f);
        }

        // -------------------------------------------------- decisão de sprintar (ShouldSprint)

        [Test]
        public void ShouldSprint_InCombatHeldWithStamina_IsTrue()
        {
            Assert.IsTrue(SprintRules.ShouldSprint(inCombat: true, sprintHeld: true, stunned: false, modalOpen: false, hasStamina: true));
        }

        [Test]
        public void ShouldSprint_OutOfCombat_IsFalse_NoOp()
        {
            // CA-2: fora de combate é no-op mesmo com tecla segurada e stamina cheia.
            Assert.IsFalse(SprintRules.ShouldSprint(inCombat: false, sprintHeld: true, stunned: false, modalOpen: false, hasStamina: true));
        }

        [Test]
        public void ShouldSprint_NoStamina_IsFalse()
        {
            // CA-2: sem stamina o sprint não pode ativar.
            Assert.IsFalse(SprintRules.ShouldSprint(inCombat: true, sprintHeld: true, stunned: false, modalOpen: false, hasStamina: false));
        }

        [Test]
        public void ShouldSprint_Stunned_IsFalse()
        {
            // CA-2: Stun (F01) cancela o sprint.
            Assert.IsFalse(SprintRules.ShouldSprint(inCombat: true, sprintHeld: true, stunned: true, modalOpen: false, hasStamina: true));
        }

        [Test]
        public void ShouldSprint_ModalOpen_IsFalse()
        {
            // CA-2: modal aberto cancela o sprint.
            Assert.IsFalse(SprintRules.ShouldSprint(inCombat: true, sprintHeld: true, stunned: false, modalOpen: true, hasStamina: true));
        }

        [Test]
        public void ShouldSprint_KeyReleased_IsFalse()
        {
            Assert.IsFalse(SprintRules.ShouldSprint(inCombat: true, sprintHeld: false, stunned: false, modalOpen: false, hasStamina: true));
        }

        // -------------------------------------------------- fator de mobilidade (CA-1)

        [Test]
        public void Mobility_OutOfCombat_IsFull_NoOp()
        {
            // Fora de combate o fator é neutro (1.0) — o controller limpa o fator (no-op).
            Assert.AreEqual(1f, SprintRules.ResolveMobilityFactor(inCombat: false, sprinting: false), 0.0001f);
            Assert.AreEqual(1f, SprintRules.ResolveMobilityFactor(inCombat: false, sprinting: true), 0.0001f);
        }

        [Test]
        public void Mobility_InCombatNotSprinting_AppliesPenalty()
        {
            // CA-1: em combate sem sprint a penalidade canônica (~0.9 → 3.4-3.8 tiles/s) está ativa.
            Assert.AreEqual(SprintRules.CombatReadyMultiplier, SprintRules.ResolveMobilityFactor(inCombat: true, sprinting: false), 0.0001f);
            Assert.AreEqual(0.9f, SprintRules.CombatReadyMultiplier, 0.0001f);
        }

        [Test]
        public void Mobility_InCombatSprinting_RestoresFullSpeed()
        {
            // CA-1: sprintando em combate restaura a velocidade plena de fora-de-combate (fator 1.0).
            Assert.AreEqual(1f, SprintRules.ResolveMobilityFactor(inCombat: true, sprinting: true), 0.0001f);
        }

        [Test]
        public void Mobility_ReleaseSprint_RestoresCombatPenalty()
        {
            // CA-1: soltar o sprint (ainda em combate) volta a penalidade — não a velocidade de fora.
            var sprinting = SprintRules.ResolveMobilityFactor(inCombat: true, sprinting: true);
            var released = SprintRules.ResolveMobilityFactor(inCombat: true, sprinting: false);
            Assert.AreEqual(1f, sprinting, 0.0001f);
            Assert.AreEqual(0.9f, released, 0.0001f);
        }

        // -------------------------------------------------- fator no composer (integração F47)

        [Test]
        public void Composer_SprintFactorRestoresFullSpeed_OverCombatPenalty()
        {
            // Aplica a penalidade de combate e depois o sprint substitui o MESMO fator nomeado por 1.0.
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.CombatMobility, SprintRules.CombatReadyMultiplier);
            Assert.AreEqual(0.9f, composer.Value, 0.0001f, "Penalidade de combate ativa.");

            composer.SetFactor(SpeedFactorKind.CombatMobility, SprintRules.FullMobilityMultiplier);
            Assert.AreEqual(1f, composer.Value, 0.0001f, "Sprint restaura a velocidade plena.");

            // Sai de combate: fator removido → velocidade base (no-op fora de combate).
            composer.ClearFactor(SpeedFactorKind.CombatMobility);
            Assert.AreEqual(1f, composer.Value, 0.0001f);
        }

        [Test]
        public void Composer_SprintComposesWithBlock_WithoutCorruption()
        {
            // Mutuamente exclusivos na prática, mas o composer garante que soltar um não corrompe o outro.
            var composer = new PlayerSpeedComposer();
            composer.SetFactor(SpeedFactorKind.Block, 0.45f);
            composer.SetFactor(SpeedFactorKind.CombatMobility, SprintRules.CombatReadyMultiplier);
            Assert.AreEqual(0.405f, composer.Value, 0.0001f, "0.45 × 0.9 = 0.405");

            composer.ClearFactor(SpeedFactorKind.CombatMobility);
            Assert.AreEqual(0.45f, composer.Value, 0.0001f, "Só o block permanece, sem resíduo.");
        }

        // -------------------------------------------------- drenagem 8/s por acumulador (CA-1)

        [Test]
        public void Drain_RatePerSecond_IsEight()
        {
            Assert.AreEqual(8f, SprintRules.StaminaDrainPerSecond, 0.0001f);
        }

        [Test]
        public void Drain_AccumulatesAcrossSubSecondFrames()
        {
            // 8/s: dois frames de 0.25s = 0.5s = 4 stamina (2 + 2 inteiros, ou acumula até inteiro).
            float acc = 0f;
            var spendA = SprintRules.AdvanceDrain(ref acc, 0.25f); // +2.0 → gasta 2
            var spendB = SprintRules.AdvanceDrain(ref acc, 0.25f); // +2.0 → gasta 2
            Assert.AreEqual(2, spendA);
            Assert.AreEqual(2, spendB);
            Assert.AreEqual(4, spendA + spendB, "0.5s a 8/s = 4 stamina.");
        }

        [Test]
        public void Drain_FractionalAccumulates_NoLoss()
        {
            // Frames pequenos (0.1s = 0.8 stamina) acumulam até virar inteiro, sem perder fração.
            float acc = 0f;
            int total = 0;
            for (int i = 0; i < 10; i++) // 10 × 0.1s = 1.0s
            {
                total += SprintRules.AdvanceDrain(ref acc, 0.1f);
            }

            Assert.AreEqual(8, total, "1.0s a 8/s = 8 stamina, sem perda de fração.");
        }

        [Test]
        public void Drain_OneFullSecond_SpendsEight()
        {
            float acc = 0f;
            var spend = SprintRules.AdvanceDrain(ref acc, 1f);
            Assert.AreEqual(8, spend);
            Assert.AreEqual(0f, acc, 0.0001f, "Sem resto após 1s exato.");
        }
    }
}
