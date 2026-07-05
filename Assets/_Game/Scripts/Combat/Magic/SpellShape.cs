using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Combat.Magic
{
    /// <summary>
    /// fable_08 — forma de execução de uma magia. Default <see cref="Bolt"/> mantém TODAS as
    /// spells existentes (sem campo novo) como o projétil linear instantâneo de hoje (caracterização
    /// na SpellShapeTests). Cada shape tem semântica distinta despachada pelo SpellCastService.
    /// </summary>
    public enum SpellShape
    {
        /// <summary>Projétil linear único (comportamento legado; default neutro).</summary>
        Bolt = 0,

        /// <summary>Leque de projéteis num arco curto à frente do caster.</summary>
        Cone = 1,

        /// <summary>Explosão radial 360° em torno do caster (query Physics2D com buffer reutilizável).</summary>
        Nova = 2,

        /// <summary>Suporte ao próprio caster: HP/Stamina/Mana (sem alvo).</summary>
        SelfRestore = 3,

        /// <summary>Barreira temporária que absorve dano antes da defesa (PlayerBarrierState).</summary>
        Barrier = 4
    }

    /// <summary>
    /// fable_08 — seleção de alvo determinística para magias com auto-target (ex.: Projétil Arcano,
    /// EMENDA 2026-06-12-D 6.6-A). Lógica PURA (sem cena, sem Physics): recebe posições candidatas e
    /// devolve o índice escolhido, para permitir EditMode tests. O runtime alimenta os candidatos via
    /// query de Physics2D com buffer reutilizável — NUNCA via GameObject.Find/FindObjectsByType.
    ///
    /// Regra de desempate canônica (estável e testável):
    ///   1) preferir o candidato dentro do cone de mira (ângulo &lt;= aimHalfAngleDeg em relação a
    ///      <paramref name="facing"/>), se houver;
    ///   2) entre os elegíveis, o de MENOR distância ao caster;
    ///   3) empate de distância → menor índice (ordem de entrada estável).
    /// </summary>
    public static class SpellTargeting
    {
        /// <summary>Meia-abertura padrão do cone de mira para auto-target (graus).</summary>
        public const float DefaultAimHalfAngleDeg = 60f;

        /// <summary>
        /// Escolhe o melhor alvo. Retorna -1 se não houver candidato dentro do alcance.
        /// </summary>
        /// <param name="casterPosition">Posição do caster.</param>
        /// <param name="facing">Direção de mira do caster (normalizada internamente).</param>
        /// <param name="candidates">Posições de mundo dos inimigos candidatos.</param>
        /// <param name="range">Alcance máximo do auto-target.</param>
        /// <param name="aimHalfAngleDeg">Meia-abertura do cone de mira preferencial.</param>
        public static int SelectAutoTarget(
            Vector2 casterPosition,
            Vector2 facing,
            IReadOnlyList<Vector2> candidates,
            float range,
            float aimHalfAngleDeg = DefaultAimHalfAngleDeg)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return -1;
            }

            float rangeSqr = range * range;
            Vector2 facingNorm = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector2.right;
            float cosHalf = Mathf.Cos(Mathf.Clamp(aimHalfAngleDeg, 0f, 180f) * Mathf.Deg2Rad);

            int bestInCone = -1;
            float bestInConeSqr = float.MaxValue;
            int bestAny = -1;
            float bestAnySqr = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Vector2 toTarget = candidates[i] - casterPosition;
                float sqr = toTarget.sqrMagnitude;
                if (sqr > rangeSqr)
                {
                    continue;
                }

                // Mais próximo em geral (fallback quando nada está na mira).
                if (sqr < bestAnySqr)
                {
                    bestAnySqr = sqr;
                    bestAny = i;
                }

                // Dentro do cone de mira?
                if (sqr > 0.0001f)
                {
                    float dot = Vector2.Dot(toTarget.normalized, facingNorm);
                    if (dot >= cosHalf && sqr < bestInConeSqr)
                    {
                        bestInConeSqr = sqr;
                        bestInCone = i;
                    }
                }
                else if (bestInCone < 0)
                {
                    // Em cima do caster: trata como na mira (distância 0).
                    bestInConeSqr = 0f;
                    bestInCone = i;
                }
            }

            return bestInCone >= 0 ? bestInCone : bestAny;
        }

        /// <summary>
        /// Direção de disparo resultante do auto-target: vetor normalizado caster→alvo escolhido.
        /// Se nenhum alvo elegível, devolve <paramref name="facing"/> (cai para tiro à frente).
        /// </summary>
        public static Vector2 ResolveAimDirection(
            Vector2 casterPosition,
            Vector2 facing,
            IReadOnlyList<Vector2> candidates,
            float range,
            float aimHalfAngleDeg = DefaultAimHalfAngleDeg)
        {
            int index = SelectAutoTarget(casterPosition, facing, candidates, range, aimHalfAngleDeg);
            Vector2 facingNorm = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector2.right;
            if (index < 0)
            {
                return facingNorm;
            }

            Vector2 toTarget = candidates[index] - casterPosition;
            return toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : facingNorm;
        }

        /// <summary>
        /// True se a posição alvo está dentro do cone de raio <paramref name="halfAngleDeg"/> à frente
        /// do caster e dentro do alcance — usado pelo executor de Cone (filtro angular do leque).
        /// </summary>
        public static bool IsInsideCone(
            Vector2 casterPosition,
            Vector2 facing,
            Vector2 targetPosition,
            float range,
            float halfAngleDeg)
        {
            Vector2 toTarget = targetPosition - casterPosition;
            float sqr = toTarget.sqrMagnitude;
            if (sqr > range * range)
            {
                return false;
            }

            if (sqr <= 0.0001f)
            {
                return true;
            }

            Vector2 facingNorm = facing.sqrMagnitude > 0.0001f ? facing.normalized : Vector2.right;
            float dot = Vector2.Dot(toTarget.normalized, facingNorm);
            return dot >= Mathf.Cos(Mathf.Clamp(halfAngleDeg, 0f, 180f) * Mathf.Deg2Rad);
        }
    }
}
