using System.Collections.Generic;

namespace CindarsHope.UI.Onboarding
{
    /// <summary>
    /// fable_62 — logica pura de primeira-ocorrencia dos hints de onboarding (sem MonoBehaviour,
    /// sem UnityEngine). Mantem o conjunto de hints ja vistos NESTE save e responde se um hint
    /// deve disparar agora. Idempotente: o segundo gatilho do mesmo hint nao re-dispara.
    ///
    /// Persistencia: este tracker e a fonte de verdade em memoria; o provider de save serializa
    /// <see cref="GetSeenHintIds"/> e reidrata via <see cref="RestoreSeen"/>. Saves legados sem a
    /// secao reidratam com lista vazia (todos os hints elegiveis de novo — comportamento seguro).
    /// </summary>
    public sealed class OnboardingHintTracker
    {
        private readonly HashSet<string> _seen = new HashSet<string>();

        /// <summary>True se o hint ja foi visto neste save.</summary>
        public bool HasSeen(string hintId) => _seen.Contains(hintId);

        /// <summary>
        /// Ponto unico de decisao de primeira-ocorrencia. Retorna true SOMENTE na primeira vez
        /// para um hint valido do catalogo, marcando-o como visto. Chamadas subsequentes (mesmo
        /// id) retornam false. Ids fora do catalogo retornam false e nao sao marcados.
        /// </summary>
        public bool TryMarkSeen(string hintId)
        {
            if (string.IsNullOrEmpty(hintId))
            {
                return false;
            }

            if (!OnboardingHintCatalog.TryGet(hintId, out _))
            {
                return false;
            }

            // Add retorna false se ja existia => nao e primeira ocorrencia.
            return _seen.Add(hintId);
        }

        /// <summary>Snapshot estavel (ordenado) dos hints vistos, para o provider de save.</summary>
        public List<string> GetSeenHintIds()
        {
            var list = new List<string>(_seen);
            list.Sort(System.StringComparer.Ordinal);
            return list;
        }

        /// <summary>
        /// Reidrata a partir do save. Null/vazio = nada visto (save legado seguro). Ids
        /// desconhecidos (ex.: hint removido em versao futura) sao ignorados silenciosamente.
        /// </summary>
        public void RestoreSeen(IReadOnlyList<string> seenHintIds)
        {
            _seen.Clear();
            if (seenHintIds == null)
            {
                return;
            }

            for (int i = 0; i < seenHintIds.Count; i++)
            {
                var id = seenHintIds[i];
                if (!string.IsNullOrEmpty(id) && OnboardingHintCatalog.TryGet(id, out _))
                {
                    _seen.Add(id);
                }
            }
        }

        /// <summary>Reset para new game (limpa o estado em memoria).</summary>
        public void Clear() => _seen.Clear();
    }
}
