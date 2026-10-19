namespace CindarsHope.Foundation
{
    /// <summary>
    /// fable_34 — the channel through which a quest reaches the player.
    ///
    /// This is the "source" axis from QUEST_CATALOG_DIRECTION §fontes / quest_rules.md Rule 2.
    /// It is ORTHOGONAL to <c>QuestCategory</c> (the authored classification used by the
    /// existing flow): a quest authored as <c>QuestCategory.Side</c> may be delivered by the
    /// <see cref="Npc"/> source, while a procedural board contract is authored as
    /// <c>QuestCategory.FarmOrder</c> but delivered by the <see cref="Board"/> source.
    ///
    /// The Quest Log (F14) groups by this value into tabs Main / Side / Contracts / Secrets.
    /// Adding this enum does NOT create a second registry/manager — quests still flow through the
    /// existing QuestRegistry/QuestService; this only labels the channel.
    ///
    /// arch: quebra do ciclo Quests|Save (spec_arch_quests_save_cycle_reduction_v24) — enum puro
    /// (sem dependência de engine) movido de CindarsHope.Quests para Foundation porque
    /// Save/SaveData.cs referenciava este tipo apenas para um valor default; decisão explícita de
    /// arquitetura.
    /// </summary>
    public enum QuestSource
    {
        /// <summary>Main quest in 4 acts — golden "!", never expires.</summary>
        Main = 0,

        /// <summary>NPC direct personal chains — silver "!".</summary>
        Npc = 1,

        /// <summary>Hund's notice board in the square — 3 rotating procedural contracts/day.</summary>
        Board = 2,

        /// <summary>Town hall mural — read-only announcements (festivals, main milestones). No accept.</summary>
        Mural = 3,

        /// <summary>
        /// Cave secrets — no marker; offered by non-aggressive creatures and wandering merchants;
        /// listed in the log only after discovery.
        /// </summary>
        CaveSecret = 4,

        /// <summary>
        /// Zrix's cave contracts board (4th of the 5 sources — EMENDA 2026-06-12-B).
        /// The channel is implemented by fable_51; this spec only reserves the enum value and
        /// guarantees the Quest Log groups by it.
        /// </summary>
        CaveContract = 5
    }
}
