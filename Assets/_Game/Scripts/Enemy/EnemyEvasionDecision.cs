using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_82 — Helpers estaticos puros para decisao de evasao reativa do inimigo.
    /// Todos os metodos sao deterministas: sem UnityEngine.Random, sem Time.time, sem refs de Unity.
    /// Testavel em EditMode sem MonoBehaviour.
    /// </summary>
    public static class EnemyEvasionDecision
    {
        // ── Gate por role (CA-2): define quais roles PODEM evadir ──────────────────────────────

        /// <summary>
        /// Retorna true se o role do inimigo e elegivel para evasao reativa.
        /// Tanks, brutes, swarm e guard nao evadem (gate por design).
        /// </summary>
        public static bool RoleCanEvade(EnemyRole role)
        {
            switch (role)
            {
                // Roles ageis/a distancia: evasao habilitada por default (ainda precisa de CanReactiveEvade=true no profile).
                case EnemyRole.Ranged:
                case EnemyRole.Caster:
                    return true;

                // Tanks, swarm, guard, burrower nao evadem.
                // Chaser, Elite, MiniBoss, Boss: gate por profile (CanReactiveEvade deve ser true explicitamente).
                case EnemyRole.Guard:
                case EnemyRole.Swarm:
                case EnemyRole.Tank:
                case EnemyRole.Burrower:
                case EnemyRole.Chaser:
                case EnemyRole.Elite:
                case EnemyRole.MiniBoss:
                case EnemyRole.Boss:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Retorna true se o role permite dash de reposicionamento.
        /// Mesmo gate de RoleCanEvade para ranged/caster.
        /// </summary>
        public static bool RoleCanRepositionDash(EnemyRole role) => RoleCanEvade(role);

        // ── Decisao principal de evasao ─────────────────────────────────────────────────────────

        /// <summary>
        /// Decide se o inimigo deve executar um sidestep reativo.
        /// Puro: todos os inputs sao passados como parametros.
        /// </summary>
        /// <param name="playerWindupActive">True quando ha windup ativo do player na janela de reacao.</param>
        /// <param name="distanceToPlayer">Distancia atual ao player (tiles).</param>
        /// <param name="reactionRadius">Raio maximo de reacao (do profile).</param>
        /// <param name="cooldownReady">True quando o cooldown de evasao expirou.</param>
        /// <param name="role">Role deste inimigo.</param>
        /// <param name="chanceRoll">Valor aleatorio pre-computado no intervalo [0, 1).</param>
        /// <param name="evadeChance">Probabilidade do profile (0-1).</param>
        /// <param name="profileEnabled">CanReactiveEvade do profile.</param>
        /// <returns>True se a esquiva deve ser executada.</returns>
        public static bool ShouldEvade(
            bool playerWindupActive,
            float distanceToPlayer,
            float reactionRadius,
            bool cooldownReady,
            EnemyRole role,
            float chanceRoll,
            float evadeChance,
            bool profileEnabled)
        {
            if (!profileEnabled) return false;
            if (!playerWindupActive) return false;
            if (!cooldownReady) return false;
            if (!RoleCanEvade(role)) return false;
            if (distanceToPlayer > reactionRadius) return false;
            return chanceRoll < evadeChance;
        }

        /// <summary>
        /// Decide se o inimigo deve executar um dash de reposicionamento.
        /// </summary>
        /// <param name="repositionDashEnabled">RepositionDashEnabled do profile.</param>
        /// <param name="cooldownReady">True quando o cooldown de dash expirou.</param>
        /// <param name="role">Role deste inimigo.</param>
        /// <param name="distanceToPlayer">Distancia ao player.</param>
        /// <param name="preferredDistance">Distancia preferida do profile.</param>
        /// <returns>True se o dash de reposicionamento deve ser executado.</returns>
        public static bool ShouldRepositionDash(
            bool repositionDashEnabled,
            bool cooldownReady,
            EnemyRole role,
            float distanceToPlayer,
            float preferredDistance)
        {
            if (!repositionDashEnabled) return false;
            if (!cooldownReady) return false;
            if (!RoleCanRepositionDash(role)) return false;
            // Dash so faz sentido quando o inimigo esta muito perto do player (abaixo da distancia preferida).
            return distanceToPlayer < preferredDistance * 0.75f;
        }

        // ── Calculo de direcao do sidestep ─────────────────────────────────────────────────────

        /// <summary>
        /// Calcula a direcao lateral do sidestep (perpendicular ao vetor player→inimigo).
        /// flankSign deve ser +1 ou -1 (determinado por seed em runtime; nao por Random).
        /// </summary>
        public static Vector2 SidestepDirection(Vector2 toPlayer, float flankSign)
        {
            if (toPlayer.sqrMagnitude < 0.0001f)
                return Vector2.right * flankSign;

            return Vector2.Perpendicular(toPlayer.normalized) * flankSign;
        }

        /// <summary>
        /// Calcula a direcao do dash de reposicionamento (afastar do player, levar em conta flanco).
        /// </summary>
        public static Vector2 RepositionDashDirection(Vector2 toPlayer, float flankSign)
        {
            if (toPlayer.sqrMagnitude < 0.0001f)
                return Vector2.right * flankSign;

            // Recua levemente e deflecte para o flanco (nao foge reto para tras).
            Vector2 back = -toPlayer.normalized;
            Vector2 flank = Vector2.Perpendicular(toPlayer.normalized) * flankSign;
            return (back * 0.7f + flank * 0.5f).normalized;
        }
    }
}
