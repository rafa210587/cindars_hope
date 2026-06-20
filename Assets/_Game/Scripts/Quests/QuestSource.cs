namespace CindarsHope.Quests
{
    /// <summary>
    /// fable_34 — the channel through which a quest reaches the player.
    ///
    /// This is the "source" axis from QUEST_CATALOG_DIRECTION §fontes / quest_rules.md Rule 2.
    /// It is ORTHOGONAL to <see cref="QuestCategory"/> (the authored classification used by the
    /// existing flow): a quest authored as <c>QuestCategory.Side</c> may be delivered by the
    /// <see cref="Npc"/> source, while a procedural board contract is authored as
    /// <c>QuestCategory.FarmOrder</c> but delivered by the <see cref="Board"/> source.
    ///
    /// The Quest Log (F14) groups by this value into tabs Main / Side / Contracts / Secrets.
    /// Adding this enum does NOT create a second registry/manager — quests still flow through the
    /// existing QuestRegistry/QuestService; this only labels the channel.
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

    /// <summary>
    /// fable_34 — maps the authored <see cref="QuestCategory"/> to the delivery
    /// <see cref="QuestSource"/> used by the Quest Log tabs, when a dynamic instance has not
    /// already pinned an explicit source. Pure/deterministic — EditMode-testable.
    /// </summary>
    public static class QuestSourceMapper
    {
        public static QuestSource FromCategory(QuestCategory category)
        {
            switch (category)
            {
                case QuestCategory.Main:
                    return QuestSource.Main;
                case QuestCategory.FarmOrder:
                    return QuestSource.Board;
                case QuestCategory.CaveContract:
                    return QuestSource.CaveContract;
                case QuestCategory.Hidden:
                    return QuestSource.CaveSecret;
                case QuestCategory.Side:
                case QuestCategory.Festival:
                case QuestCategory.Tutorial:
                case QuestCategory.System:
                default:
                    return QuestSource.Npc;
            }
        }

        /// <summary>The Quest Log tab a source belongs to (Secrets only lists discovered).</summary>
        public static QuestLogTab TabFor(QuestSource source)
        {
            switch (source)
            {
                case QuestSource.Main:
                    return QuestLogTab.Main;
                case QuestSource.Board:
                case QuestSource.CaveContract:
                    return QuestLogTab.Contracts;
                case QuestSource.CaveSecret:
                    return QuestLogTab.Secrets;
                case QuestSource.Npc:
                case QuestSource.Mural:
                default:
                    return QuestLogTab.Side;
            }
        }
    }

    /// <summary>fable_34 — Quest Log tabs (QUEST_CATALOG §queue: Main/Side/Contracts/Secrets).</summary>
    public enum QuestLogTab
    {
        Main = 0,
        Side = 1,
        Contracts = 2,
        Secrets = 3
    }
}
