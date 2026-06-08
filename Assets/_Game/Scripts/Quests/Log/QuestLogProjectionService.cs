using System.Collections.Generic;
using CindarsHope.Quests;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.Log
{
    // Anti-spoiler constants — these QuestIds must have spoilerTier > 0 before being shown
    public static class QuestAntiSpoilerGuard
    {
        public static readonly HashSet<string> HighSpoilerQuestIds = new HashSet<string>
        {
            "quest_archivist_silence", "quest_level_101", "quest_final_choice_protect_seal_use",
            "quest_black_stone_truth", "quest_anya_not_restored", "quest_vaelrion_final_boss",
            "quest_sethra_leader_reveal"
        };

        public static bool IsSafeToShow(string questId, int playerDiscoveredSpoilerTier) =>
            !HighSpoilerQuestIds.Contains(questId) || playerDiscoveredSpoilerTier >= 3;
    }

    // Pure C# read-only projection service — never mutates QuestState
    public class QuestLogProjectionService
    {
        public QuestLogEntryViewModel ProjectEntry(
            QuestStateRecord record,
            QuestDefinition def,
            QuestVisibilityPolicy policy,
            int playerDiscoveredSpoilerTier = 0)
        {
            if (record == null || def == null) return null;

            var state = (QuestStateStatus)record.State;
            bool isSpoilerSafe = QuestAntiSpoilerGuard.IsSafeToShow(def.QuestId, playerDiscoveredSpoilerTier);
            bool isHidden = policy != null && policy.VisibilityState == QuestVisibilityState.Hidden && !record.Discovered;

            if (isHidden) return null;

            var title = isSpoilerSafe || (policy?.ShowTitleWhenUnknown == true)
                ? (def.DisplayNameKey ?? def.QuestId)
                : (def.HiddenDisplayNameKey ?? "???");

            var vm = new QuestLogEntryViewModel
            {
                QuestId = def.QuestId,
                Category = def.Category,
                DisplayTitle = title,
                DisplaySummary = isSpoilerSafe ? (def.DescriptionKey ?? "") : "",
                StateDisplay = state.ToString(),
                Tracked = record.Tracked,
                CanTrack = state == QuestStateStatus.Active || state == QuestStateStatus.Waiting,
                SpoilerSafe = isSpoilerSafe,
                SortKey = (int)def.Category * 1000 + (int)state
            };

            // Only show deadline if quest can expire
            if (record.ExpiresAtDay.HasValue)
                vm.KnownDeadline = record.ExpiresAtDay;

            // Waiting state hints
            if (state == QuestStateStatus.Waiting)
                vm.WaitingReasonValue = WaitingReason.HiddenWaitingReason;

            return vm;
        }

        public QuestDetailViewModel ProjectDetail(
            QuestStateRecord record,
            QuestDefinition def,
            QuestVisibilityPolicy policy,
            int playerDiscoveredSpoilerTier = 0)
        {
            if (record == null || def == null) return null;

            bool isSpoilerSafe = QuestAntiSpoilerGuard.IsSafeToShow(def.QuestId, playerDiscoveredSpoilerTier);
            var state = (QuestStateStatus)record.State;

            var vm = new QuestDetailViewModel
            {
                QuestId = def.QuestId,
                Category = def.Category,
                DisplayTitle = isSpoilerSafe ? (def.DisplayNameKey ?? def.QuestId) : (def.HiddenDisplayNameKey ?? "???"),
                KnownSummary = isSpoilerSafe ? (def.DescriptionKey ?? "") : "",
                CanTrack = state == QuestStateStatus.Active || state == QuestStateStatus.Waiting,
                CanUntrack = record.Tracked,
                WaitingReasonValue = state == QuestStateStatus.Waiting ? WaitingReason.HiddenWaitingReason : WaitingReason.None
            };

            // Only expose objectives that are known
            foreach (var obj in record.ObjectiveStates)
            {
                if (!record.KnownObjectiveIds.Contains(obj.ObjectiveId)) continue;
                vm.CurrentObjectiveRows.Add(new QuestObjectiveProjection
                {
                    ObjectiveId = obj.ObjectiveId,
                    DisplayText = obj.ObjectiveId, // key reference; localization deferred
                    CurrentProgress = obj.CurrentProgress,
                    RequiredProgress = obj.RequiredProgress,
                    IsCompleted = obj.IsCompleted
                });
            }

            // Known hints only
            vm.KnownHints.AddRange(record.KnownHints);

            // Completed step history (safe to show labels)
            vm.CompletedStepHistory.AddRange(record.CompletedStepIds);

            return vm;
        }

        public List<QuestLogEntryViewModel> ProjectAll(
            QuestStateSection section,
            Dictionary<string, QuestDefinition> definitions,
            Dictionary<string, QuestVisibilityPolicy> policies,
            int playerDiscoveredSpoilerTier = 0)
        {
            var result = new List<QuestLogEntryViewModel>();
            if (section == null) return result;

            foreach (var record in section.QuestStates)
            {
                var state = (QuestStateStatus)record.State;
                if (state == QuestStateStatus.Unknown) continue;

                definitions.TryGetValue(record.QuestId, out var def);
                if (def == null) continue;

                policies.TryGetValue(def.JournalVisibilityPolicyId ?? "default", out var policy);
                if (policy == null) policy = QuestVisibilityPolicy.Default();

                var vm = ProjectEntry(record, def, policy, playerDiscoveredSpoilerTier);
                if (vm != null) result.Add(vm);
            }

            return result;
        }
    }
}
