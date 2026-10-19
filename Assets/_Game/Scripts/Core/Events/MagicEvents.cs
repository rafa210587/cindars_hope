namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_07 — publicado quando o jogador aprende uma magia (entra em knownSpellIds).
    /// Source identifica a fonte canônica do aprendizado (LearnableScroll, Tome, FonteStory, ...).
    /// Não dispara para magia provida por item equipado (EquippedItem nunca adiciona conhecimento).
    /// </summary>
    public readonly struct SpellLearnedEvent
    {
        public string SpellId { get; }
        public string Source { get; }

        public SpellLearnedEvent(string spellId, string source)
        {
            SpellId = spellId;
            Source = source;
        }
    }

    /// <summary>
    /// fable_08 — publicado quando uma magia com cast time é CANCELADA antes de resolver (o caster
    /// tomou dano durante a janela). O reembolso de mana já foi aplicado pelo SpellCastRoutine.
    /// O início do cast reutiliza o existente <see cref="SpellCastStartedEvent"/> (não duplicar).
    /// </summary>
    public readonly struct SpellCastInterruptedEvent
    {
        public string SpellId { get; }
        public int RefundedMana { get; }

        public SpellCastInterruptedEvent(string spellId, int refundedMana)
        {
            SpellId = spellId ?? string.Empty;
            RefundedMana = refundedMana;
        }
    }
}
