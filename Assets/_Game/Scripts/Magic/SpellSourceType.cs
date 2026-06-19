namespace CindarsHope.Magic
{
    /// <summary>
    /// fable_07 — fonte canônica de uma magia disponível ao jogador.
    /// Direction MAGIC_LEARNING_UNLOCKS_SOURCES: magia não vem de level/skill point;
    /// a skill tree libera o domínio, a FONTE libera a spell.
    /// Valores explícitos e estáveis (serialização por inteiro).
    /// </summary>
    public enum SpellSourceType
    {
        /// <summary>Default neutro: item não é fonte de magia.</summary>
        None = 0,

        /// <summary>Pergaminho que ensina permanente, consome no uso, exige pré-requisito de domínio.</summary>
        LearnableScroll = 1,

        /// <summary>Pergaminho que casta e consome, sem adicionar à knownSpellIds.</summary>
        CastScroll = 2,

        /// <summary>Tomo de estudo: aprende após N usos (contador no spellbook).</summary>
        Tome = 3,

        /// <summary>Wand/Staff/arma/foco: concede a magia apenas enquanto equipado.</summary>
        EquippedItem = 4,

        /// <summary>Ensino por NPC (hook para spec futura de social/diálogo).</summary>
        NpcTeaching = 5,

        /// <summary>Fonte de Anya (story unlock — WAVE 10 hooks).</summary>
        FonteStory = 6
    }
}
