using System.Collections.Generic;
using CindarsHope.UI.Quest;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14: spoiler-aware read model for the Quest Log screen tab.
    ///
    /// Consumes the existing <see cref="QuestLogViewModel"/> (WAVE 09 projection) and yields
    /// only the rows that may be shown plus a masked label for hidden objectives. The
    /// Anti-regression rule "quest log não revela oculto" is enforced here, in pure C#, so it
    /// can be covered by EditMode tests instead of relying on the Canvas view.
    /// </summary>
    public static class QuestLogSpoilerProjection
    {
        public const string HiddenObjectiveLabel = "???";

        public sealed class Row
        {
            public string QuestId { get; set; }
            public string DisplayName { get; set; }
            public string Status { get; set; }
            public bool IsMainProgression { get; set; }
            /// <summary>Player-visible objective line (masked when the quest is hidden).</summary>
            public string ObjectiveLabel { get; set; }
            public bool IsComplete { get; set; }
        }

        /// <summary>
        /// Build the visible rows. Hidden quests are still listed (so the player knows a
        /// secret exists) but their objective detail is masked; non-hidden quests show real
        /// progress. Returns an empty list when there are no active quests (empty state).
        /// </summary>
        public static List<Row> BuildVisibleRows(QuestLogViewModel viewModel)
        {
            var rows = new List<Row>();
            if (viewModel?.ActiveQuests == null)
            {
                return rows;
            }

            foreach (var entry in viewModel.ActiveQuests)
            {
                if (entry == null)
                {
                    continue;
                }

                var objective = entry.IsHidden
                    ? HiddenObjectiveLabel
                    : $"{entry.ObjectiveProgress}/{entry.ObjectiveTarget}";

                rows.Add(new Row
                {
                    QuestId = entry.QuestId,
                    DisplayName = entry.IsHidden ? HiddenObjectiveLabel : entry.DisplayName,
                    Status = entry.Status,
                    IsMainProgression = entry.IsMainProgression,
                    ObjectiveLabel = objective,
                    IsComplete = !entry.IsHidden && entry.IsComplete
                });
            }

            return rows;
        }

        public static bool HasActiveQuests(QuestLogViewModel viewModel)
        {
            return viewModel?.ActiveQuests != null && viewModel.ActiveQuests.Count > 0;
        }
    }
}
