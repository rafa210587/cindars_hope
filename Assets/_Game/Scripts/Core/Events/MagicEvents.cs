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
}
