using CindarsHope.Player.Death;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Player
{
    /// <summary>
    /// Bugfix: o jogador nao morria (HP zerava e nada acontecia) e, apos morrer 1x, nunca mais morria.
    /// Estes testes travam a regra pura de decisao de morte (HP&lt;=0 dispara morte 1x; revive rearma).
    /// </summary>
    public class PlayerDeathDecisionTests
    {
        [Test]
        public void HpZero_NotYetDead_FiresDeathOnce()
        {
            var d = PlayerDeathDecision.Evaluate(currentHp: 0, isDead: false);
            Assert.IsTrue(d.ShouldDie, "HP<=0 pela primeira vez deve disparar morte.");
            Assert.IsTrue(d.NextIsDead, "Apos disparar morte o estado deve ficar morto.");
        }

        [Test]
        public void HpNegative_NotYetDead_FiresDeath()
        {
            var d = PlayerDeathDecision.Evaluate(currentHp: -5, isDead: false);
            Assert.IsTrue(d.ShouldDie, "HP negativo tambem conta como morte.");
            Assert.IsTrue(d.NextIsDead);
        }

        [Test]
        public void HpZero_AlreadyDead_DoesNotRefire()
        {
            var d = PlayerDeathDecision.Evaluate(currentHp: 0, isDead: true);
            Assert.IsFalse(d.ShouldDie, "Ja morto: HPChangedEvent repetido nao redispara morte.");
            Assert.IsTrue(d.NextIsDead, "Continua morto.");
        }

        [Test]
        public void HpPositive_WhileDead_Rearms()
        {
            // Revive/cura: o bug original nunca resetava _isDead -> nunca morria de novo.
            var d = PlayerDeathDecision.Evaluate(currentHp: 30, isDead: true);
            Assert.IsFalse(d.ShouldDie, "HP>0 nunca dispara morte.");
            Assert.IsFalse(d.NextIsDead, "Revive deve rearmar para a proxima morte.");
        }

        [Test]
        public void HpPositive_WhileAlive_StaysAlive()
        {
            var d = PlayerDeathDecision.Evaluate(currentHp: 100, isDead: false);
            Assert.IsFalse(d.ShouldDie);
            Assert.IsFalse(d.NextIsDead);
        }

        [Test]
        public void DeathReviveDeath_Sequence_FiresTwice()
        {
            // Sequencia completa: morre, revive (rearma), morre de novo.
            var dead1 = PlayerDeathDecision.Evaluate(0, isDead: false);
            Assert.IsTrue(dead1.ShouldDie, "1a morte dispara.");

            var revive = PlayerDeathDecision.Evaluate(50, isDead: dead1.NextIsDead);
            Assert.IsFalse(revive.ShouldDie);
            Assert.IsFalse(revive.NextIsDead, "Revive rearma.");

            var dead2 = PlayerDeathDecision.Evaluate(0, isDead: revive.NextIsDead);
            Assert.IsTrue(dead2.ShouldDie, "2a morte tambem dispara (bug corrigido).");
        }
    }
}
