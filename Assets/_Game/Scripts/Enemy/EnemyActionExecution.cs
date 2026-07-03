using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>
    /// fable_83 — Logica pura (sem UnityEngine.Time, sem MonoBehaviour) para os ataques-assinatura
    /// de inimigos: ComboStrike, TelegraphedAoE, SummonAdds, MultiHitCharge, DebuffStrike.
    /// Testavel em EditMode sem scene; EnemyBrain orquestra o timing/telegraph e chama aqui.
    /// </summary>
    public static class EnemyActionExecution
    {
        // ── Limites de segurança (no-magic-balance-values) ──────────────────────────────────────

        /// <summary>Limite maximo de hits por combo (previne combo de dano arbitrario).</summary>
        public const int MaxComboHits = 8;

        /// <summary>Limite maximo de adds por invocacao (previne enxame infinito).</summary>
        public const int MaxSummonCount = 4;

        /// <summary>Cap de adds por sala (F81 — stable-run).</summary>
        public const int MaxAddsPerRoom = 8;

        /// <summary>Raio minimo aceitavel de AoE.</summary>
        public const float MinAoeRadius = 0.5f;

        // ── ComboStrike ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calcula quantos hits de combo serao aplicados neste ciclo (clamped para seguranca).
        /// Retorna o valor canonico; EnemyBrain aplica cada hit no player via DamageRequest.
        /// </summary>
        public static int ResolveComboHitCount(int comboHits)
        {
            return Mathf.Clamp(comboHits, 1, MaxComboHits);
        }

        /// <summary>
        /// Calcula o dano de cada hit individual do combo.
        /// Dano por hit = baseDamage / comboHits (1 hit de 30 vira 3 de 10), arredondado para
        /// cima no ultimo hit para nao perder fracao por int division.
        /// </summary>
        public static int ComboHitDamage(int totalBaseDamage, int hitIndex, int totalHits)
        {
            if (totalHits <= 0) return totalBaseDamage;
            int perHit = totalBaseDamage / totalHits;
            // Ultimo hit absorve o restante para total ser exato
            if (hitIndex == totalHits - 1)
                return totalBaseDamage - perHit * (totalHits - 1);
            return perHit;
        }

        // ── TelegraphedAoE ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retorna a lista de posicoes de alvos dentro do raio da AoE centrado em <paramref name="origin"/>.
        /// Aceita targets como Vector2 para ser testavel sem MonoBehaviours.
        /// </summary>
        public static List<int> ResolveAoETargetIndices(
            Vector2 origin,
            float aoeRadius,
            IReadOnlyList<Vector2> targetPositions)
        {
            float clampedRadius = Mathf.Max(aoeRadius, MinAoeRadius);
            float sqrRadius = clampedRadius * clampedRadius;
            var hits = new List<int>();
            for (int i = 0; i < targetPositions.Count; i++)
            {
                if ((targetPositions[i] - origin).sqrMagnitude <= sqrRadius)
                    hits.Add(i);
            }
            return hits;
        }

        /// <summary>
        /// Verifica se o atraso de telegraph ja expirou e o dano pode ser resolvido.
        /// Telegraph DEVE preceder o dano; dano nunca antes de aoeDelay apos BeginAction.
        /// </summary>
        public static bool IsTelegraphDelayComplete(float windupElapsed, float aoeDelay)
        {
            return windupElapsed >= aoeDelay;
        }

        // ── SummonAdds ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve quantos adds podem ser spawnados respeitando o cap por sala.
        /// Cap por sala (F81 / ADR-0005) impede enxame infinito; seed garante determinismo.
        /// </summary>
        /// <param name="requestedCount">Quantidade pedida pelo EnemyActionSO.SummonCount.</param>
        /// <param name="currentAddsInRoom">Adds vivos na sala agora.</param>
        /// <returns>Quantidade efetiva a spawnar (pode ser 0 se sala lotada).</returns>
        public static int ResolveSummonCount(int requestedCount, int currentAddsInRoom)
        {
            int capped = Mathf.Clamp(requestedCount, 1, MaxSummonCount);
            int available = Mathf.Max(0, MaxAddsPerRoom - currentAddsInRoom);
            return Mathf.Min(capped, available);
        }

        /// <summary>
        /// Gera posicoes de spawn deterministas para os adds ao redor da posicao do invocador.
        /// Usa System.Random seeded (sem GUID/timestamp; stable-run ADR-0005).
        /// </summary>
        /// <param name="summonerPosition">Posicao do inimigo invocador.</param>
        /// <param name="count">Quantidade de adds a spawnar.</param>
        /// <param name="seed">Seed derivada do contexto (CaveRunSeed + caveLevel + summonerId).</param>
        /// <param name="spawnRadius">Raio ao redor do invocador para distribuir os adds.</param>
        public static List<Vector2> GenerateSummonPositions(
            Vector2 summonerPosition,
            int count,
            int seed,
            float spawnRadius = 1.5f)
        {
            var rng = new System.Random(seed);
            var positions = new List<Vector2>(count);
            float radiusClamped = Mathf.Max(spawnRadius, 0.5f);
            for (int i = 0; i < count; i++)
            {
                // Distribui em angulo equidistante mais offset aleatorio pequeno
                float angle = (i * (360f / Mathf.Max(count, 1)) + (float)(rng.NextDouble() * 30.0)) * Mathf.Deg2Rad;
                float r = radiusClamped * (0.7f + (float)rng.NextDouble() * 0.6f);
                positions.Add(summonerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r);
            }
            return positions;
        }

        /// <summary>
        /// Gera um seed inteiro determinisico para summon a partir de strings de contexto.
        /// Derivado de CaveRunSeed + caveLevel + summonerId (estavel por run, sem GUID/timestamp).
        /// spec_codex_09 / cave-stable-run: string.GetHashCode() nao e garantido estavel entre
        /// processos/runtimes; usa o FNV-1a determinístico ja existente no projeto para cada string
        /// individual, mantendo intacta a formula de combinacao h * 31 + x.
        /// </summary>
        public static int DeriveSummonSeed(string caveRunSeed, int caveLevel, string summonerId)
        {
            int h = 17;
            h = h * 31 + CaveLayoutStableHash.Compute(caveRunSeed ?? string.Empty);
            h = h * 31 + caveLevel;
            h = h * 31 + CaveLayoutStableHash.Compute(summonerId ?? string.Empty);
            return h;
        }

        // ── MultiHitCharge ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve quantas posicoes ao longo da linha de charge recebem hits.
        /// Retorna indices de amostras que estao dentro do alcance de cada hit.
        /// <paramref name="chargeOrigin"/> e <paramref name="chargeEnd"/> definem a linha.
        /// <paramref name="targetPositions"/> sao posicoes de alvos potenciais.
        /// </summary>
        public static List<int> ResolveMultiHitChargeTargets(
            Vector2 chargeOrigin,
            Vector2 chargeEnd,
            IReadOnlyList<Vector2> targetPositions,
            float hitRadius = 0.8f)
        {
            Vector2 line = chargeEnd - chargeOrigin;
            float lineLen = line.magnitude;
            var hits = new List<int>();
            if (lineLen < 0.01f) return hits;

            Vector2 lineDir = line / lineLen;
            float sqrHitRadius = hitRadius * hitRadius;

            for (int i = 0; i < targetPositions.Count; i++)
            {
                Vector2 toTarget = targetPositions[i] - chargeOrigin;
                float proj = Vector2.Dot(toTarget, lineDir);
                // Target deve estar ao longo da linha (nao antes do inicio nem alem do fim)
                if (proj < 0f || proj > lineLen) continue;
                Vector2 closestOnLine = chargeOrigin + lineDir * proj;
                if ((targetPositions[i] - closestOnLine).sqrMagnitude <= sqrHitRadius)
                    hits.Add(i);
            }
            return hits;
        }

        // ── DebuffStrike ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica se o DebuffStrike deve aplicar o status com base na chance.
        /// Retorna true se deve aplicar; usa valor pre-calculado de [0,1) para testabilidade.
        /// </summary>
        public static bool ShouldApplyDebuff(float applyChance, float randomValue)
        {
            return applyChance > 0f && randomValue < applyChance;
        }

        /// <summary>
        /// Retorna o status ID a aplicar; prefere debuffStatusId do action, depois o primeiro
        /// StatusApplicationId (compatibilidade com F01); string.Empty se nenhum disponivel.
        /// </summary>
        public static string ResolveDebuffStatusId(string debuffStatusId, string[] statusApplicationIds)
        {
            if (!string.IsNullOrWhiteSpace(debuffStatusId)) return debuffStatusId;
            if (statusApplicationIds != null && statusApplicationIds.Length > 0) return statusApplicationIds[0];
            return string.Empty;
        }
    }
}
