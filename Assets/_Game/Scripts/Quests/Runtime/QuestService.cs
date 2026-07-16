using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Rewards;
using CindarsHope.Quests.Save;
using CindarsHope.Save;
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
    public class QuestService : IQuestInteractionQuery
    {
        private readonly QuestRegistry _registry;
        private readonly QuestStateSection _saveSection;
        private readonly IQuestInventoryAccess _inventoryAccess;
        private readonly IQuestGoldAccess _goldAccess;
        private readonly QuestFlagService _flagService;
        private readonly IQuestProgressionAccess _progressionAccess; // fable_34 — scaled XP + act skill point
        private readonly QuestObjectiveProgressDispatcher _progressDispatcher;
        private readonly QuestTurnInRewardGranter _rewardGranter;
        private readonly QuestDynamicInstanceRegistrar _dynamicRegistrar;

        public QuestService(
            QuestRegistry registry,
            QuestStateSection saveSection,
            IQuestInventoryAccess inventoryAccess,
            IQuestGoldAccess goldAccess,
            QuestFlagService flagService,
            IQuestProgressionAccess progressionAccess = null)
        {
            _registry = registry;
            _saveSection = saveSection;
            _inventoryAccess = inventoryAccess;
            _goldAccess = goldAccess;
            _progressionAccess = progressionAccess;
            _flagService = flagService;
            _rewardGranter = new QuestTurnInRewardGranter(
                new QuestRewardApplicator(flagService),
                inventoryAccess,
                goldAccess,
                progressionAccess);
            _dynamicRegistrar = new QuestDynamicInstanceRegistrar(registry, flagService);

            foreach (var quest in _registry.GetAllQuests())
                _dynamicRegistrar.EnsureRewardFlagsRegistered(_registry.GetRewards(quest.QuestId));

            _progressDispatcher = new QuestObjectiveProgressDispatcher(
                _registry,
                _saveSection,
                CheckObjectiveProgress,
                MarkObjectiveComplete,
                ProgressObjectiveCount);
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

            // Catalogs register offers before the player accepts them by id. Keep the complete
            // dynamic metadata on that path, not only through AcceptDynamicInstance.
            if (_dynamicRegistrar.TryGetInstance(questId, out var dynamicInstance))
                QuestDynamicInstancePersistence.HydrateRecord(record, dynamicInstance);

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

        /// <inheritdoc />
        public bool HasQuestState(string questId) => GetQuestState(questId) != null;

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

            // Apply rewards idempotently (delegated — see QuestTurnInRewardGranter)
            var rewards = _registry.GetRewards(questId);
            var grantResult = _rewardGranter.Grant(questId, rewards, record);

            // Mark quest complete
            record.State = (int)QuestStateStatus.Completed;
            record.CompletedAtDay = 0;

            GameEventBus.Publish(new QuestCompletedEvent(questId));
            GameEventBus.Publish(new QuestRewardClaimedEvent(questId, grantResult.GoldGiven, grantResult.ItemsGiven));

            Debug.Log($"[QuestService] Quest completed: {questId}. Gold: {grantResult.GoldGiven}. Xp: {grantResult.XpGiven}. Items: {grantResult.ItemsGiven.Count}. Flags: {grantResult.FlagsGranted.Count}.");
            return QuestTurnInResult.Success(grantResult.GoldGiven, grantResult.ItemsGiven, grantResult.FlagsGranted);
        }

        public QuestStateSection GetSaveSection() => _saveSection;

        // ─── fable_34 — quest source channels (board / secret / main act) ───────────────

        /// <summary>
        /// fable_34 (CA-1/CA-2) — registers a dynamic board/procedural instance into the EXISTING
        /// registry/flow (no second registry). Builds the objective from the template kind and a
        /// scaled Gold reward; XP is applied on turn-in via the progression hook. Returns the
        /// concrete quest id, or null if the instance is invalid.
        /// </summary>
        public string RegisterDynamicInstance(QuestInstance instance) => _dynamicRegistrar.Register(instance);

        /// <summary>
        /// fable_34 — accepts a dynamic instance: registers it (if needed) and creates the active
        /// QuestStateRecord carrying the instance params (so it survives save/load standalone).
        /// </summary>
        public bool AcceptDynamicInstance(QuestInstance instance)
        {
            if (instance == null) return false;
            var questId = RegisterDynamicInstance(instance);
            if (questId == null) return false;
            if (!AcceptQuest(questId)) return false;

            var record = _saveSection.GetQuestState(questId);
            if (record != null)
                QuestDynamicInstancePersistence.HydrateRecord(record, instance);
            return true;
        }

        /// <summary>
        /// fable_34 (CA-4) — grants +1 skill point for a completed main-quest act, EXACTLY once.
        /// Idempotent across reloads: the act id is recorded in the persisted RewardedMainActIds.
        /// Returns true only on the first grant for that act.
        /// </summary>
        public bool TryAwardActSkillPoint(string actId)
        {
            if (string.IsNullOrEmpty(actId)) return false;
            if (_saveSection.RewardedMainActIds.Contains(actId)) return false;

            _saveSection.RewardedMainActIds.Add(actId);
            _progressionAccess?.GrantSkillPoints(1);
            Debug.Log($"[QuestService] Main act '{actId}' completed: +1 skill point granted (idempotent).");
            return true;
        }

        /// <summary>
        /// fable_34 (CA-3) — single entry point to offer a cave-secret quest. Marks the quest as
        /// discovered (so it appears in the Quest Log) and publishes SecretQuestDiscoveredEvent.
        /// Consumed by the wandering merchant and peaceful-monster interactables. The concrete
        /// scq_* content is authored by fable_52; this only opens the channel. Idempotent.
        /// </summary>
        public bool OfferSecretQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return false;
            bool newlyDiscovered = _saveSection.MarkSecretDiscovered(questId);
            if (newlyDiscovered)
            {
                GameEventBus.Publish(new SecretQuestDiscoveredEvent(questId));
                Debug.Log($"[QuestService] Secret quest discovered: {questId}.");
            }
            return newlyDiscovered;
        }

        public bool IsSecretDiscovered(string questId) => _saveSection.IsSecretDiscovered(questId);

        /// <summary>
        /// fable_34 (EMENDA 2026-06-12-C, PREREQUISITE_UI_DEBT) — true when every
        /// PrerequisiteQuestId of <paramref name="questId"/> is Completed. Used by
        /// QuestGiverInteractable to gate offers. Unknown definitions / no prerequisites => true.
        /// </summary>
        public bool ArePrerequisitesComplete(string questId)
        {
            if (!_registry.TryGetQuest(questId, out var definition)) return true;
            if (definition.PrerequisiteQuestIds == null || definition.PrerequisiteQuestIds.Count == 0) return true;

            foreach (var prereqId in definition.PrerequisiteQuestIds)
            {
                if (string.IsNullOrWhiteSpace(prereqId)) continue;
                var prereqState = _saveSection.GetQuestState(prereqId);
                if (prereqState == null) return false;
                if ((QuestStateStatus)prereqState.State != QuestStateStatus.Completed) return false;
            }
            return true;
        }

        /// <summary>fable_34 — the delivery source of a quest record (instance pin or category map).</summary>
        public QuestSource GetQuestSource(string questId)
        {
            var record = _saveSection.GetQuestState(questId);
            if (record != null && record.IsDynamicInstance)
                return (QuestSource)record.Source;
            if (_registry.TryGetQuest(questId, out var def))
                return QuestSourceMapper.FromCategory(def.Category);
            return record != null ? (QuestSource)record.Source : QuestSource.Npc;
        }

        /// <summary>
        /// Restores quest state from the serializable save DTO.
        /// Clears current QuestStates and repopulates from save data.
        /// GrantedRewardIds preserved to guarantee reward idempotency after load.
        /// </summary>
        public void RestoreFromSaveData(QuestStateSectionSaveData saveData)
        {
            if (saveData == null) return;

            _saveSection.QuestStates.Clear();
            _saveSection.GlobalKnownHints.Clear();
            // fable_34 — restore discovered secrets + rewarded acts (idempotency carried by save).
            _saveSection.DiscoveredSecretQuestIds.Clear();
            _saveSection.RewardedMainActIds.Clear();

            if (saveData.GlobalKnownHints != null)
            {
                _saveSection.GlobalKnownHints.AddRange(saveData.GlobalKnownHints);
            }

            if (saveData.DiscoveredSecretQuestIds != null)
            {
                _saveSection.DiscoveredSecretQuestIds.AddRange(saveData.DiscoveredSecretQuestIds);
            }

            if (saveData.RewardedMainActIds != null)
            {
                _saveSection.RewardedMainActIds.AddRange(saveData.RewardedMainActIds);
            }

            foreach (var dto in saveData.QuestStates ?? new List<QuestStateSaveData>())
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.QuestId)) continue;

                var record = new QuestStateRecord
                {
                    QuestId = dto.QuestId,
                    State = dto.State,
                    CurrentStepId = dto.CurrentStepId,
                    CompletedStepIds = new List<string>(dto.CompletedStepIds ?? new List<string>()),
                    FailedStepIds = new List<string>(dto.FailedStepIds ?? new List<string>()),
                    KnownObjectiveIds = new List<string>(dto.KnownObjectiveIds ?? new List<string>()),
                    KnownHints = new List<string>(dto.KnownHints ?? new List<string>()),
                    StartedAtDay = dto.StartedAtDay,
                    StartedAtTime = dto.StartedAtTime,
                    CompletedAtDay = dto.CompletedAtDay > 0 ? (int?)dto.CompletedAtDay : null,
                    Tracked = dto.Tracked,
                    Discovered = dto.Discovered,
                    FailureReason = dto.FailureReason,
                    GrantedRewardIds = new List<string>(dto.GrantedRewardIds ?? new List<string>()),
                    GrantedFlagIds = new List<string>(dto.GrantedFlagIds ?? new List<string>()),
                    RepeatInstanceId = dto.RepeatInstanceId,
                    // fable_34 — dynamic instance + source channel (additive simple types).
                    Source = dto.Source,
                    IsDynamicInstance = dto.IsDynamicInstance,
                    TemplateId = dto.TemplateId,
                    InstanceTargetId = dto.InstanceTargetId,
                    InstanceQuantity = dto.InstanceQuantity,
                    QuestLevel = dto.QuestLevel,
                    InstanceRewardGold = dto.InstanceRewardGold,
                    InstanceRewardXp = dto.InstanceRewardXp,
                    GeneratedForDay = dto.GeneratedForDay,
                    DynamicRewards = (dto.DynamicRewards ?? new List<QuestDynamicRewardSaveData>())
                        .Where(r => r != null)
                        .Select(r => new QuestDynamicRewardRecord
                        {
                            RewardId = r.RewardId,
                            RewardType = r.RewardType,
                            TargetId = r.TargetId,
                            Quantity = r.Quantity,
                            GrantedFlagId = r.GrantedFlagId,
                            IdempotencyPolicy = r.IdempotencyPolicy
                        }).ToList(),
                    ObjectiveStates = new List<QuestObjectiveStateRecord>()
                };

                foreach (var objDto in dto.ObjectiveStates ?? new List<QuestObjectiveStateSaveData>())
                {
                    if (objDto == null) continue;
                    record.ObjectiveStates.Add(new QuestObjectiveStateRecord
                    {
                        ObjectiveId = objDto.ObjectiveId,
                        CurrentProgress = objDto.CurrentProgress,
                        RequiredProgress = objDto.RequiredProgress,
                        IsCompleted = objDto.IsCompleted,
                        IsFailed = objDto.IsFailed,
                        IsKnown = objDto.IsKnown
                    });
                }

                _saveSection.QuestStates.Add(record);

                // fable_34 — a dynamic instance must rejoin the live flow after load: re-register
                // its definition/objectives/rewards into the registry so progress/turn-in resolve.
                foreach (var flagId in record.GrantedFlagIds)
                {
                    _flagService?.EnsureRewardFlagRegistered(flagId, "QuestSaveRestore");
                    _flagService?.GrantFlag(flagId, "QuestSaveRestore");
                }

                if (record.IsDynamicInstance && !_registry.TryGetQuest(record.QuestId, out _))
                {
                    _dynamicRegistrar.ReRegisterFromRecord(record);
                }
            }

            Debug.Log($"[QuestService] Restored {_saveSection.QuestStates.Count} quest records from save data.");
        }

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

        /// <summary>
        /// Called by QuestProgressEventBridge on InventoryChangedEvent.
        /// </summary>
        public void OnInventoryChanged(string itemId)
        {
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.CollectItem,
                QuestProgressApplication.RecheckInventory,
                itemId));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on ItemCraftedEvent.
        /// </summary>
        public void OnItemCrafted(string itemId)
        {
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.CraftItem,
                QuestProgressApplication.Complete,
                itemId));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on CropHarvestedEvent.
        /// WAVE_INTEGRATION_26: handles HarvestCrop objectives.
        /// </summary>
        public void OnCropHarvested(string seedId, string itemId)
        {
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.HarvestCrop,
                QuestProgressApplication.Complete,
                seedId,
                itemId));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on EconomyTransactionCompletedEvent.
        /// WAVE_INTEGRATION_26: handles SellItem objectives.
        /// TransactionType "sell" and GoldDelta > 0 means a successful sale.
        /// </summary>
        public void OnItemSold(string itemId, string transactionType, int goldDelta)
        {
            // Only care about sell transactions that generated gold
            if (transactionType != "sell" || goldDelta <= 0) return;
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.SellItem,
                QuestProgressApplication.Complete,
                itemId));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on NpcInteractionStartedEvent.
        /// WAVE_INTEGRATION_26: handles TalkToNpc objectives.
        /// </summary>
        public void OnNpcTalkedTo(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return;
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.TalkToNpc,
                QuestProgressApplication.Complete,
                npcId));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on CaveLevelEnteredEvent.
        /// WAVE_INTEGRATION_26: handles ReachCaveDepth objectives.
        /// </summary>
        public void OnCaveLevelEntered(int caveLevel)
        {
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.ReachCaveDepth,
                QuestProgressApplication.Complete,
                $"cave_level_{caveLevel}",
                numericValue: caveLevel));
        }

        /// <summary>
        /// Called by QuestProgressEventBridge on EnemyKilledEvent.
        /// WAVE_INTEGRATION_26: handles DefeatEnemy objectives.
        /// </summary>
        public void OnEnemyKilled(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            _progressDispatcher.Dispatch(new QuestProgressSignal(
                QuestObjectiveType.DefeatEnemy,
                QuestProgressApplication.Increment,
                enemyId));
        }

        // ─── Private helpers (continued) ──────────────────────────────────────────

        /// <summary>
        /// Increments numeric progress for count-based objectives (DefeatEnemy, etc.).
        /// </summary>
        private void ProgressObjectiveCount(string questId, string objectiveId, int amount)
        {
            if (!_registry.TryGetQuest(questId, out var definition)) return;
            var record = _saveSection.GetQuestState(questId);
            if (record == null) return;

            var obj = record.ObjectiveStates.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (obj == null || obj.IsCompleted) return;

            var objectives = _registry.GetObjectives(questId);
            var def = objectives.FirstOrDefault(o => o.ObjectiveId == objectiveId);
            if (def == null) return;

            obj.CurrentProgress = System.Math.Min(obj.CurrentProgress + amount, def.RequiredAmount);
            GameEventBus.Publish(new QuestObjectiveProgressedEvent(questId, objectiveId, obj.CurrentProgress, obj.RequiredProgress));

            if (obj.CurrentProgress >= def.RequiredAmount)
            {
                obj.IsCompleted = true;
                EvaluateReadyToComplete(questId, record, definition);
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
