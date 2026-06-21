namespace CindarsHope.Player.Death
{
    /// <summary>
    /// fable_66 — regra PURA do replace de corpse (extraída para caracterização EditMode).
    /// Documenta o contrato que CorpseRecoveryManager.SetActiveCorpse já aplica: um corpse ativo
    /// anterior só é substituído (publicando CorpseReplacedEvent) quando o novo tem id DIFERENTE.
    /// Não altera o manager — apenas torna a decisão testável e auditável.
    /// </summary>
    public static class CorpseReplaceDecision
    {
        /// <summary>True quando o corpse anterior deve ser marcado Replaced e CorpseReplacedEvent emitido.</summary>
        public static bool ShouldReplace(string activeCorpseId, string newCorpseId)
        {
            return !string.IsNullOrEmpty(activeCorpseId)
                   && !string.IsNullOrEmpty(newCorpseId)
                   && activeCorpseId != newCorpseId;
        }
    }
}
