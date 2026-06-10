using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Save;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Runtime orchestrator for quest accept/progress/turn-in lifecycle.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Responsibilities:
    /// - Accept quests (creates QuestStateRecord in QuestStateSection)
    /// - Track objective progress (CollectItem via inventory, CraftItem via events)
    /// - Evaluate ReadyToComplete
    /// - TurnIn with idempotent reward application (RewardClaimed guard via GrantedRewardIds)
    /// - Persist via QuestStateSection (WAVE09 save DTO — no Unity refs)
    ///
    /// Rules:
    /// - No GameObject.Find / FindObjectOfType
    /// - All external communication via GameEventBus
    /// - Reward applied at most once per rewardId (GrantedRewardIds guard)
    /// - No Unity refs in QuestStateRecord (save-dto-simple-types-only rule)
    /// </summary>
    public class QuestService
    {
        private readonly QuestRegistry _registry;
        private readonly QuestStateSection _saveSection;
        private readonly QuestRewardApplicator _rewardApplicator;
        private readonly IQuestInventoryAccess _inventoryAccess;
        private readonly IQuestGoldAccess _goldAccess;

        public QuestService(
            QuestRegistry registry,
            QuestStateSection saveSection,
            IQuestInventoryAccess inventoryAccess,
            IQuestGoldAccess goldAccess,
            QuestFlagService flagService)
        {
            _registry = registry;
            _saveSection = saveSection;
            _inventoryAccess = inventoryAccess;
            _goldAccess = goldAccess;
            _rewardApplicator = new QuestRewardApplicator(flagService);
        }

        // ─── Public API ────────────────────────────────────────────────────────────

        public bool AcceptQuest(string questId)
        {
            if (!_registry.TryGetQuest(questId, out var definition))
            {
                Debug.LogWarning($"[QuestService] AcceptQuest: quest '{questId}' not found in registry.");
                return false;
            }

            var existing = _saveSection.GetQuestState(questId);
            if (existing != null && (QuestStateStatus)existing.State == QuestStateStatus.Active)
            {
                Debug.LogWarning($"[QuestService] AcceptQuest: quest '{questId}' already active.");
                return false;
            }

            // Use the richer QuestObjective list from the registry (has ObjectiveType + TargetId)
            var objectives = _registry.GetObjectives(questId);

            var record = new QuestStateRecord
            {
                QuestId = questId,
                State = (int)QuestStateStatus.Active,
                Tracked = definition.Trackable,
                Discovered = true,
                StartedAtDay = 0,
                KnownObjectiveIds = objectives.Select(o => o.ObjectiveId).ToList(),
                ObjectiveStates = objectives.Select(o => new QuestObjectiveStateRecord
                {
                    ObjectiveId = o.ObjectiveId,
                    CurrentProgress = 0,
                    RequiredProgress = o.RequiredAmount,
                    IsCompleted = false,
                    IsKnown = true
                }).ToList()
            };

            _saveSection.QuestStates.RemoveAll(q => q.QuestId == questId);
            _saveSection.QuestStates.Add(record);

            CheckObjectiveProgress(questId);

            GameEventBus.Publish(new QuestAcceptedEvent(questId));
            Debug.Log($"[QuestService] Quest accepted: {questId}");
            return true;
        }

        public QuestStateRecord GetQuestState(string questId)
        {
            return _saveSection.GetQuestState(questId);
        }

        public IReadOnlyList<QuestStateRecord> GetActiveQuests()
        {
            return _saveSection.QuestStates
                .Where(q => (QuestStateStatus)q.State == QuestStateStatus.Active ||
                            (QuestStateStatus)q.State == QuestStateStatus.ReadyToComplete)
                .ToList();
        }

        public IReadOnlyList<QuestStateRecord> GetCompletedQuests()
        {
            return _saveSection.QuestStates
                .Where(q => (QuestStateStatus)q.State == QuestStateStatus.Completed)
                .ToList();
        }

        public bool CanTurnIn(string questId)
        {
            var record = _saveSection.GetQuestState(questId);
            if (record == null) return false;
            var state = (QuestStateStatus)record.State;
            return state == QuestStateStatus.ReadyToComplete || state == QuestStateStatus.Active && AllObjectivesComplete(record);
        }

        /// <summary>
        /// Re-evaluates progress for CollectItem objectives for a specific quest.
        /// Called on InventoryChangedEvent.
        /// </summary>
        public void CheckObjectiveProgress(string questId)
        {
            if (!_registry.TryGetQuest(questId, out var definition)) return;
            var record = _saveSection.GetQuestState(questId);
            if (record == null) return;
            var state = (QuestStateStatus)record.State;
            if (state != QuestStateStatus.Active && state != QuestStateStatus.ReadyToComplete) return;

            // Use the richer QuestObjective list from registry (has ObjectiveType + TargetId)
            var objectives = _registry.GetObjectives(questId);

            bool anyChanged = false;
            foreach (var obj in record.ObjectiveStates)
            {
                if (obj.IsCompleted) continue;
                var def = objectives.FirstOrDefault(o => o.ObjectiveId == obj.ObjectiveId);
                if (def == null) continue;

                if (def.ObjectiveType == QuestObjectiveType.CollectItem)
                {
                    int count = _inventoryAccess?.GetItemCount(def.TargetId) ?? 0;
                    int clampedCount = System.Math.Min(count, def.RequiredAmount);
                    if (clampedCount != obj.CurrentProgress)
                    {
                        obj.CurrentProgress = clampedCount;
                        anyChanged = true;
                        if (clampedCount >= def.RequiredAmount)
                        {
                            obj.IsCompleted = true;
                        }
                        GameEventBus.Publish(new QuestObjectiveProgressedEvent(questId, obj.ObjectiveId, obj.CurrentProgress, obj.RequiredProgress));
                    }
                }
            }

            if (anyChanged) EvaluateReadyToComplete(questId, record, definition);
        }

        /// <summary>
        /// Marks a specific objective as complete (for CraftItem/TalkToNpc/DeliverItem triggers).
        /// </summary>
        public void MarkObjectiveComplete(string questId, string objectiveId)
        {
            if (!_registry.TryGetQuest(questId, out var definition)) return;
            var record = _saveSection.GetQuestState(questId);
            if (record == null) return;

            var obj = record.ObjectiveStates.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (obj == null || obj.IsCompleted) return;

            // Use richer objective list from registry
            var objectives = _registry.GetObjectives(questId);
            var def = objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (def == null) return;

            obj.CurrentProgress = def.RequiredAmount;
            obj.IsCompleted = true;
            GameEventBus.Publish(new QuestObjectiveProgressedEvent(questId, obj.ObjectiveId, obj.CurrentProgress, obj.RequiredProgress));
            EvaluateReadyToComplete(questId, record, definition);
        }

        /// <summary>
        /// Turn in quest and apply rewards once. Idempotent via GrantedRewardIds.
        /// </summary>
        public QuestTurnInResult TurnIn(string questId)
        {
            if (!_registry.TryGetQuest(questId, out var definition))
                return QuestTurnInResult.Fail("Quest not found in registry.");

            var record = _saveSection.GetQuestState(questId);
            if (record == null)
                return QuestTurnInResult.Fail("Quest not started.");

            var state = (QuestStateStatus)record.State;
            if (state == QuestStateStatus.Completed)
                return QuestTurnInResult.AlreadyCompleted();

            if (!CanTurnIn(questId))
                return QuestTurnInResult.Fail("Quest objectives not complete.");

            // Apply rewards idempotently
            var rewards = _registry.GetRewards(questId);
            var rewardResults = new List<QuestRewardApplicationResult>();
            var alreadyGranted = new System.Collections.Generic.HashSet<string>(record.GrantedRewardIds);
            var alreadyGrantedFlags = new System.Collections.Generic.HashSet<string>(record.GrantedFlagIds);

            int goldGiven = 0;
            var itemsGiven = new List<string>();
            var flagsGranted = new List<string>();

            foreach (var reward in rewards)
            {
                var ctx = new QuestRewardApplicationContext
                {
                    QuestId = questId,
                    RewardId = reward.RewardId,
                    AlreadyGrantedRewardIds = alreadyGranted,
                    AlreadyGrantedFlagIds = alreadyGrantedFlags
                };
                var result = _rewardApplicator.Apply(reward, ctx);
                rewardResults.Add(result);

                if (result.Success && !result.SkippedAlreadyGranted)
                {
                    // Apply Gold
                    if (result.GrantedGold > 0 && _goldAccess != null)
                    {
                        _goldAccess.AddGold(result.GrantedGold);
                        goldGiven += result.GrantedGold;
                    }

                    // Apply Items
                    foreach (var itemId in result.GrantedItems)
                    {
                        if (_inventoryAccess != null && _inventoryAccess.TryAddItem(itemId, 1))
                            itemsGiven.Add(itemId);
                    }

                    // Record reward as granted (idempotency)
                    if (!string.IsNullOrEmpty(result.GrantedRewardId) && !record.GrantedRewardIds.Contains(result.GrantedRewardId))
                        record.GrantedRewardIds.Add(result.GrantedRewardId);

                    // Record flags
                    foreach (var flagId in result.GrantedFlagIds)
                    {
                        if (!record.GrantedFlagIds.Contains(flagId))
                            record.GrantedFlagIds.Add(flagId);
                        flagsGranted.Add(flagId);
                    }
                }
            }

            // Mark quest complete
            record.State = (int)QuestStateStatus.Completed;
            record.CompletedAtDay = 0;

            GameEventBus.Publish(new QuestCompletedEvent(questId));
            GameEventBus.Publish(new QuestRewardClaimedEvent(questId, goldGiven, itemsGiven));

            Debug.Log($"[QuestService] Quest completed: {questId}. Gold: {goldGiven}. Items: {itemsGiven.Count}. Flags: {flagsGranted.Count}.");
            return QuestTurnInResult.Success(goldGiven, itemsGiven, flagsGranted);
        }

        public QuestStateSection GetSaveSection() => _saveSection;

        // ─── Private helpers ───────────────────────────────────────────────────────

        private bool AllObjectivesComplete(QuestStateRecord record)
        {
            return record.ObjectiveStates.All(o => o.IsCompleted || !o.IsKnown);
        }

        private void EvaluateReadyToComplete(string questId, QuestStateRecord record, QuestDefinition definition)
        {
            if ((QuestStateStatus)record.State != QuestStateStatus.Active) return;

            if (AllObjectivesComplete(record))
            {
                record.State = (int)QuestStateStatus.ReadyToComplete;
                GameEventBus.Publish(new QuestReadyToCompleteEvent(questId));
                Debug.Log($"[QuestService] Quest ready to complete: {questId}");
            }
        }

        private void CheckAllActiveQuestsForItem(string itemId)
        {
            var activeIds = _saveSection.QuestStates
                .Where(q => (QuestStateStatus)q.State == QuestStateStatus.Active ||
                            (QuestStateStatus)q.State == QuestStateStatus.ReadyToComplete)
                .Select(q => q.QuestId)
                .ToList();

            foreach (var id in activeIds)
            {
                if (!_registry.TryGetQuest(id, out _)) continue;
                var objectives = _registry.GetObjectives(id);
                if (objectives.Any(o => o.ObjectiveType == QuestObjectiveType.CollectItem && o.TargetId == itemId))
                    CheckObjectiveProgress(id);
            }
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on InventoryChangedEvent.
        /// </summary>
        public void OnInventoryChanged(string itemId)
        {
            CheckAllActiveQuestsForItem(itemId);
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on ItemCraftedEvent.
        /// </summary>
        public void OnItemCrafted(string itemId)
        {
            var activeIds = _saveSection.QuestStates
                .Where(q => (QuestStateStatus)q.State == QuestStateStatus.Active)
                .Select(q => q.QuestId)
                .ToList();

            foreach (var id in activeIds)
            {
                if (!_registry.TryGetQuest(id, out _)) continue;
                var objectives = _registry.GetObjectives(id);
                foreach (var obj in objectives)
                {
                    if (obj.ObjectiveType == QuestObjectiveType.CraftItem && obj.TargetId == itemId)
                        MarkObjectiveComplete(id, obj.ObjectiveId);
                }
            }
        }
    }

    /// <summary>
    /// Result of a quest turn-in attempt.
    /// </summary>
    public class QuestTurnInResult
    {
        public bool Succeeded { get; set; }
        public bool WasAlreadyCompleted { get; set; }
        public string FailureReason { get; set; }
        public int GoldGiven { get; set; }
        public List<string> ItemsGiven { get; set; } = new List<string>();
        public List<string> FlagsGranted { get; set; } = new List<string>();

        public static QuestTurnInResult Success(int gold, List<string> items, List<string> flags) =>
            new QuestTurnInResult { Succeeded = true, GoldGiven = gold, ItemsGiven = items, FlagsGranted = flags };

        public static QuestTurnInResult Fail(string reason) =>
            new QuestTurnInResult { Succeeded = false, FailureReason = reason };

        public static QuestTurnInResult AlreadyCompleted() =>
            new QuestTurnInResult { Succeeded = true, WasAlreadyCompleted = true };
    }
}
