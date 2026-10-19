using System.Collections.Generic;

namespace CindarsHope.Combat
{
    /// <summary>
    /// fable_48 — seleção PURA e determinística da próxima munição compatível, sem Random e sem
    /// Unity scene API. Chamada pelo <see cref="BowArrowAttackService"/> quando a pilha equipada
    /// zera, para auto-equipar a próxima flecha do inventário na ordem canônica do catálogo
    /// (wood→iron→steel→silver→fire→frost — barata→cara).
    ///
    /// Testável com um inventário sintético (lista de <see cref="AmmoCandidate"/>). Devolve null
    /// quando não há nenhuma munição compatível com estoque > 0 (o serviço mantém o bloqueio
    /// NoArrowsInInventory existente).
    /// </summary>
    public static class ArrowAmmoSelector
    {
        /// <summary>
        /// Escolhe a próxima munição compatível na ordem canônica. Ignora o slot já esgotado
        /// (<paramref name="excludeAmmoId"/>), candidatos com estoque &lt;= 0 e AmmoType incompatível.
        /// Empate impossível: a ordem canônica fixa garante determinismo.
        /// </summary>
        /// <param name="excludeAmmoId">Munição recém-esgotada a ignorar (a equipada que zerou).</param>
        /// <param name="requiredAmmoType">AmmoType aceito pelo arco (ex.: "arrow").</param>
        /// <param name="candidates">Inventário disponível (itemId, ammoType, count).</param>
        /// <param name="canonicalOrder">Ordem canônica de prioridade (default: catálogo §8).</param>
        public static string SelectNextCompatible(
            string excludeAmmoId,
            string requiredAmmoType,
            IEnumerable<AmmoCandidate> candidates,
            IReadOnlyList<string> canonicalOrder = null)
        {
            if (candidates == null)
            {
                return null;
            }

            canonicalOrder ??= ArrowBallisticsResolver.CanonicalOrder;

            // Indexa o inventário por id (somando estoques de eventuais entradas repetidas).
            var stockById = new Dictionary<string, AmmoCandidate>();
            foreach (var candidate in candidates)
            {
                if (candidate == null || string.IsNullOrEmpty(candidate.ItemId) || candidate.Count <= 0)
                {
                    continue;
                }

                if (stockById.TryGetValue(candidate.ItemId, out var existing))
                {
                    stockById[candidate.ItemId] = new AmmoCandidate(candidate.ItemId, candidate.AmmoType, existing.Count + candidate.Count);
                }
                else
                {
                    stockById[candidate.ItemId] = candidate;
                }
            }

            // 1) Varre na ordem canônica (prioridade barata→cara), respeitando AmmoType e exclusão.
            for (var i = 0; i < canonicalOrder.Count; i++)
            {
                var id = canonicalOrder[i];
                if (IsSelectable(id, excludeAmmoId, requiredAmmoType, stockById))
                {
                    return id;
                }
            }

            // 2) Fallback: munição compatível fora da tabela canônica (mod/itens futuros) na ordem
            // de iteração do inventário — mantém determinismo dado um inventário ordenado.
            foreach (var candidate in candidates)
            {
                if (candidate == null || string.IsNullOrEmpty(candidate.ItemId))
                {
                    continue;
                }

                if (!IsCanonical(candidate.ItemId, canonicalOrder)
                    && IsSelectable(candidate.ItemId, excludeAmmoId, requiredAmmoType, stockById))
                {
                    return candidate.ItemId;
                }
            }

            return null;
        }

        private static bool IsSelectable(
            string id,
            string excludeAmmoId,
            string requiredAmmoType,
            Dictionary<string, AmmoCandidate> stockById)
        {
            if (string.IsNullOrEmpty(id) || id == excludeAmmoId)
            {
                return false;
            }

            if (!stockById.TryGetValue(id, out var candidate) || candidate.Count <= 0)
            {
                return false;
            }

            return AmmoTypeMatches(requiredAmmoType, candidate.AmmoType);
        }

        private static bool IsCanonical(string id, IReadOnlyList<string> canonicalOrder)
        {
            for (var i = 0; i < canonicalOrder.Count; i++)
            {
                if (canonicalOrder[i] == id)
                {
                    return true;
                }
            }
            return false;
        }

        // AmmoType requerido vazio => arco aceita qualquer munição (compat com armas sem restrição).
        private static bool AmmoTypeMatches(string requiredAmmoType, string candidateAmmoType)
        {
            if (string.IsNullOrEmpty(requiredAmmoType))
            {
                return true;
            }

            return string.Equals(requiredAmmoType, candidateAmmoType, System.StringComparison.Ordinal);
        }
    }

    /// <summary>fable_48 — candidato de munição para a seleção pura (snapshot do inventário).</summary>
    public sealed class AmmoCandidate
    {
        public string ItemId { get; }
        public string AmmoType { get; }
        public int Count { get; }

        public AmmoCandidate(string itemId, string ammoType, int count)
        {
            ItemId = itemId ?? string.Empty;
            AmmoType = ammoType ?? string.Empty;
            Count = count;
        }
    }
}
