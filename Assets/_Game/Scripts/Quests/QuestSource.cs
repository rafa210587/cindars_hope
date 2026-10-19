using CindarsHope.Foundation;

namespace CindarsHope.Quests
{
    // arch: quebra do ciclo Quests|Save (spec_arch_quests_save_cycle_reduction_v24) — o enum
    // QuestSource mudou para CindarsHope.Foundation.QuestSource (arquivo
    // Foundation/QuestSource.cs). Este arquivo mantém QuestSourceMapper e QuestLogTab, que
    // continuam pertencendo ao domínio Quests.

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
