using System.Collections.Generic;

namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — wrapper PURO sobre a familia de flags ja persistida do QuestStateSection
    /// (a lista <c>GlobalKnownHints</c>, que ja faz round-trip via QuestRuntimeBootstrap
    /// Capture/Restore). NAO cria secao nova nem migracao: apenas le/grava strings simples
    /// nessa lista existente.
    ///
    /// Mantido em C# puro (recebe a List&lt;string&gt;) para ser testavel em EditMode e para o
    /// MonoBehaviour adapter so passar a referencia da secao real em runtime.
    /// </summary>
    public sealed class NarrativeFlagStore
    {
        private readonly List<string> _flags;

        public NarrativeFlagStore(List<string> backingFlagList)
        {
            _flags = backingFlagList ?? new List<string>();
        }

        public bool IsSet(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return false;
            return _flags.Contains(flagId);
        }

        /// <summary>Grava a flag (idempotente). Retorna true se foi setada agora (era ausente).</summary>
        public bool Set(string flagId)
        {
            if (string.IsNullOrEmpty(flagId) || _flags.Contains(flagId)) return false;
            _flags.Add(flagId);
            return true;
        }
    }
}
