using System.Collections.Generic;
using CindarsHope.Combat;
using UnityEngine;

namespace CindarsHope.Cave.Ecosystem
{
    /// <summary>
    /// fable_78 (SLICE 4) — marca um inimigo como participante de um conflito inter-monstro (seção 14.6).
    /// Preenchido pelo CaveRuntimeMaterializer NO SPAWN (injeção explícita; sem GameObject.Find): carrega o
    /// enemyId do próprio lado, o enemyId do lado rival e as referências de EnemyHealth dos rivais para o
    /// EnemyBrain resolver alvo. Conflito é COMPORTAMENTO por visita (ADR-0018) — este componente só existe
    /// na visita em que o conflito está ativo e é destruído com a materialização.
    ///
    /// Quando ESTE componente está ausente, o EnemyBrain segue o caminho player-only intacto (byte-for-byte).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CaveConflictCombatant : MonoBehaviour
    {
        private readonly List<EnemyHealth> _rivals = new List<EnemyHealth>();

        /// <summary>enemyId do próprio lado (Faction A ou B). Rivais têm enemyId DIFERENTE (garantido pelo planner).</summary>
        public string OwnEnemyId { get; private set; } = string.Empty;

        /// <summary>enemyId do lado rival.</summary>
        public string RivalEnemyId { get; private set; } = string.Empty;

        public int CaveLevel { get; private set; }

        public IReadOnlyList<EnemyHealth> Rivals => _rivals;

        public void Configure(string ownEnemyId, string rivalEnemyId, int caveLevel)
        {
            OwnEnemyId = ownEnemyId ?? string.Empty;
            RivalEnemyId = rivalEnemyId ?? string.Empty;
            CaveLevel = caveLevel;
        }

        public void AddRival(EnemyHealth rival)
        {
            if (rival == null || rival == GetComponent<EnemyHealth>())
            {
                return;
            }

            if (!_rivals.Contains(rival))
            {
                _rivals.Add(rival);
            }
        }

        /// <summary>
        /// Alvo rival vivo mais próximo da posição dada, dentro do raio. Same-type nunca é alvo: o invariante
        /// é garantido porque a lista de rivais só contém o lado oposto (enemyId != OwnEnemyId), mas
        /// re-checamos aqui por segurança (defesa em profundidade contra wiring incorreto).
        /// </summary>
        public EnemyHealth FindNearestLivingRival(Vector2 fromPosition, float radius)
        {
            EnemyHealth nearest = null;
            var bestSqr = radius > 0f ? radius * radius : float.MaxValue;

            for (var i = 0; i < _rivals.Count; i++)
            {
                var rival = _rivals[i];
                if (rival == null || rival.IsDead)
                {
                    continue;
                }

                // Same-type nunca se ataca (rivais são sempre de enemyId diferente).
                if (!string.IsNullOrEmpty(OwnEnemyId) && rival.EnemyId == OwnEnemyId)
                {
                    continue;
                }

                var sqr = ((Vector2)rival.transform.position - fromPosition).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    nearest = rival;
                }
            }

            return nearest;
        }
    }
}
