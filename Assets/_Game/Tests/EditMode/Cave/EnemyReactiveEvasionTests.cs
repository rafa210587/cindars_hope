using CindarsHope.Combat;
using CindarsHope.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Cave
{
    /// <summary>
    /// fable_82 — EditMode tests para EnemyEvasionDecision (helpers estaticos puros).
    /// Cobre: ShouldEvade (raio/cooldown/chance/role), gate por role, cooldown, determinismo dado seed/estado.
    /// Sem MonoBehaviour, sem Time.time, sem UnityEngine.Random.
    /// </summary>
    [TestFixture]
    public class EnemyReactiveEvasionTests
    {
        // ── ShouldEvade — gate de profile desabilitado ───────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_WhenProfileDisabled()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: false);

            Assert.IsFalse(result, "Profile disabled => nao deve evadir.");
        }

        // ── ShouldEvade — sem windup ativo ───────────────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_WhenNoWindup()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: false,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Sem windup do player => nao deve evadir.");
        }

        // ── ShouldEvade — cooldown nao expirou ──────────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_WhenCooldownNotReady()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: false,
                role: EnemyRole.Ranged,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Cooldown ativo => nao deve evadir.");
        }

        // ── ShouldEvade — role bloqueado (Tank) ─────────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_ForTankRole()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Tank,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Tank nunca deve evadir.");
        }

        [Test]
        public void ShouldEvade_ReturnsFalse_ForSwarmRole()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Swarm,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Swarm nunca deve evadir.");
        }

        [Test]
        public void ShouldEvade_ReturnsFalse_ForGuardRole()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 1f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Guard,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Guard nunca deve evadir.");
        }

        // ── ShouldEvade — player fora do raio ───────────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_WhenPlayerOutsideRadius()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 10f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                chanceRoll: 0f,
                evadeChance: 1f,
                profileEnabled: true);

            Assert.IsFalse(result, "Player fora do raio => nao deve evadir.");
        }

        // ── ShouldEvade — chance insuficiente ───────────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsFalse_WhenChanceRollTooHigh()
        {
            // chanceRoll 0.9 >= evadeChance 0.5 => nao evade
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 2f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                chanceRoll: 0.9f,
                evadeChance: 0.5f,
                profileEnabled: true);

            Assert.IsFalse(result, "Roll acima da chance => nao deve evadir.");
        }

        // ── ShouldEvade — sucesso (CA-1 completo) ───────────────────────────────────────────────

        [Test]
        public void ShouldEvade_ReturnsTrue_WhenAllConditionsMet_Ranged()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 2f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                chanceRoll: 0.1f,
                evadeChance: 0.5f,
                profileEnabled: true);

            Assert.IsTrue(result, "Ranged com todas condicoes => deve evadir.");
        }

        [Test]
        public void ShouldEvade_ReturnsTrue_WhenAllConditionsMet_Caster()
        {
            bool result = EnemyEvasionDecision.ShouldEvade(
                playerWindupActive: true,
                distanceToPlayer: 3f,
                reactionRadius: 4f,
                cooldownReady: true,
                role: EnemyRole.Caster,
                chanceRoll: 0.05f,
                evadeChance: 0.6f,
                profileEnabled: true);

            Assert.IsTrue(result, "Caster com todas condicoes => deve evadir.");
        }

        // ── Gate por role — abrangente ───────────────────────────────────────────────────────────

        [Test]
        public void RoleCanEvade_OnlyRangedAndCasterReturnTrue()
        {
            Assert.IsTrue(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Ranged), "Ranged deve poder evadir.");
            Assert.IsTrue(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Caster), "Caster deve poder evadir.");

            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Tank), "Tank nao evade.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Swarm), "Swarm nao evade.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Guard), "Guard nao evade.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Chaser), "Chaser nao evade por default.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Burrower), "Burrower nao evade.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Elite), "Elite nao evade por default.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.MiniBoss), "MiniBoss nao evade por default.");
            Assert.IsFalse(EnemyEvasionDecision.RoleCanEvade(EnemyRole.Boss), "Boss nao evade por default.");
        }

        // ── ShouldRepositionDash ─────────────────────────────────────────────────────────────────

        [Test]
        public void ShouldRepositionDash_ReturnsFalse_WhenDisabled()
        {
            bool result = EnemyEvasionDecision.ShouldRepositionDash(
                repositionDashEnabled: false,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                distanceToPlayer: 0.5f,
                preferredDistance: 4f);

            Assert.IsFalse(result, "Dash desabilitado => nao deve reposicionar.");
        }

        [Test]
        public void ShouldRepositionDash_ReturnsFalse_ForTankRole()
        {
            bool result = EnemyEvasionDecision.ShouldRepositionDash(
                repositionDashEnabled: true,
                cooldownReady: true,
                role: EnemyRole.Tank,
                distanceToPlayer: 0.5f,
                preferredDistance: 4f);

            Assert.IsFalse(result, "Tank nao pode dash de reposicionamento.");
        }

        [Test]
        public void ShouldRepositionDash_ReturnsFalse_WhenPlayerFarEnough()
        {
            // Player esta longe (acima de 75% da distancia preferida) — nao precisa recuar.
            bool result = EnemyEvasionDecision.ShouldRepositionDash(
                repositionDashEnabled: true,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                distanceToPlayer: 4f,
                preferredDistance: 4f);

            Assert.IsFalse(result, "Player na distancia adequada => nao deve dashear.");
        }

        [Test]
        public void ShouldRepositionDash_ReturnsTrue_WhenPlayerTooClose_Ranged()
        {
            bool result = EnemyEvasionDecision.ShouldRepositionDash(
                repositionDashEnabled: true,
                cooldownReady: true,
                role: EnemyRole.Ranged,
                distanceToPlayer: 1f,
                preferredDistance: 5f);

            Assert.IsTrue(result, "Ranged com player muito proximo => deve dashear para reposicionar.");
        }

        // ── Determinismo do RNG dado seed/estado ─────────────────────────────────────────────────

        [Test]
        public void EvasionDecision_IsDeterministic_GivenSameSeedAndState()
        {
            // Dois RNGs com o mesmo seed devem gerar a mesma sequencia de rolls.
            var rng1 = new System.Random(42);
            var rng2 = new System.Random(42);

            for (int i = 0; i < 20; i++)
            {
                float roll1 = (float)rng1.NextDouble();
                float roll2 = (float)rng2.NextDouble();
                Assert.AreEqual(roll1, roll2, $"Roll {i}: mesmo seed deve produzir mesmo valor.");

                bool evade1 = EnemyEvasionDecision.ShouldEvade(
                    playerWindupActive: true, distanceToPlayer: 2f, reactionRadius: 4f,
                    cooldownReady: true, role: EnemyRole.Ranged,
                    chanceRoll: roll1, evadeChance: 0.6f, profileEnabled: true);

                bool evade2 = EnemyEvasionDecision.ShouldEvade(
                    playerWindupActive: true, distanceToPlayer: 2f, reactionRadius: 4f,
                    cooldownReady: true, role: EnemyRole.Ranged,
                    chanceRoll: roll2, evadeChance: 0.6f, profileEnabled: true);

                Assert.AreEqual(evade1, evade2, $"Decisao {i}: mesmo seed deve produzir mesma decisao.");
            }
        }

        // ── SidestepDirection — calculo puro ─────────────────────────────────────────────────────

        [Test]
        public void SidestepDirection_IsPerpendicular_ToPlayerDirection()
        {
            Vector2 toPlayer = new Vector2(1f, 0f); // player esta a direita
            Vector2 sidestep = EnemyEvasionDecision.SidestepDirection(toPlayer, 1f);

            float dot = Vector2.Dot(sidestep, toPlayer);
            Assert.AreEqual(0f, dot, 0.001f, "Sidestep deve ser perpendicular ao vetor ao player.");
        }

        [Test]
        public void SidestepDirection_FlankSign_InvertsDirection()
        {
            Vector2 toPlayer = new Vector2(1f, 0f);
            Vector2 sidestepPos = EnemyEvasionDecision.SidestepDirection(toPlayer, 1f);
            Vector2 sidestepNeg = EnemyEvasionDecision.SidestepDirection(toPlayer, -1f);

            Assert.AreEqual(-sidestepPos.x, sidestepNeg.x, 0.001f);
            Assert.AreEqual(-sidestepPos.y, sidestepNeg.y, 0.001f);
        }
    }
}
