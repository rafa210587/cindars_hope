using System;
using System.Collections.Generic;

namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — seção de save do grimório (knownSpellIds + progresso de tomos).
    /// Padrão WI-18 / F13 / F17: DTO [Serializable] só com tipos simples e IDs string estáveis.
    /// SEM refs Unity (rule save-dto-simple-types-only). Seção aditiva sem migration:
    /// saves antigos (sem o campo) carregam com grimório vazio (default neutro).
    /// </summary>
    [Serializable]
    public class SpellbookSaveData
    {
        /// <summary>IDs das magias aprendidas permanentemente (ordem estável de save).</summary>
        public List<string> KnownSpellIds = new List<string>();

        /// <summary>Progresso de estudo de tomos ainda não concluídos.</summary>
        public List<TomeProgressEntry> TomeProgress = new List<TomeProgressEntry>();
    }

    /// <summary>
    /// fable_07 — contador de usos de um tomo por spell, persistido até o aprendizado concluir.
    /// </summary>
    [Serializable]
    public class TomeProgressEntry
    {
        public string SpellId;
        public int Uses;
    }
}
