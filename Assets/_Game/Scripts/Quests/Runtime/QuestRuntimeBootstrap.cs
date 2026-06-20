using System.Collections;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Save;
using CindarsHope.Save;
using CindarsHope.UI.Quests.Runtime;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// Bootstraps the WAVE_INTEGRATION_15 quest runtime singleton.
    ///
    /// WAVE_INTEGRATION_15 — Quest Giver + Quest Log Real
    ///
    /// Creates QuestRegistry, QuestStateSection, QuestFlagService,
    /// QuestInventoryAdapter, QuestGoldAdapter, QuestService,
    /// QuestProgressEventBridge, and attaches UI controllers.
    ///
    /// Pattern: identical to CraftingStationRuntimeBootstrap (WAVE14).
    /// DontDestroyOnLoad singleton.
    ///
    /// Does NOT use GameObject.Find at runtime except in the one-time bootstrap
    /// coroutine waiting for GameBootstrap.Instance.
    /// </summary>
    public sealed class QuestRuntimeBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 120;
        private static QuestRuntimeBootstrap _instance;
        private static QuestStateSectionSaveData _pendingSaveData;

        // Static accessors for other components (avoids Find/FindObjectOfType)
        public static QuestService QuestService { get; private set; }
        public static QuestRegistry QuestRegistry { get; private set; }

        // ─── Save/Load Bridge ─────────────────────────────────────────────────────

        public static QuestStateSectionSaveData CaptureSaveData()
        {
            if (QuestService == null)
            {
                return null;
            }

            var section = QuestService.GetSaveSection();
            var dto = new QuestStateSectionSaveData
            {
                Version = section.Version,
                GlobalKnownHints = new List<string>(section.GlobalKnownHints ?? new List<string>()),
                // fable_34 — persist discovered cave secrets + rewarded main acts (idempotency).
                DiscoveredSecretQuestIds = new List<string>(section.DiscoveredSecretQuestIds ?? new List<string>()),
                RewardedMainActIds = new List<string>(section.RewardedMainActIds ?? new List<string>()),
                QuestStates = new List<QuestStateSaveData>()
            };

            foreach (var record in section.QuestStates ?? new List<QuestStateRecord>())
            {
                if (record == null) continue;
                var qDto = new QuestStateSaveData
                {
                    QuestId = record.QuestId,
                    State = record.State,
                    CurrentStepId = record.CurrentStepId,
                    CompletedStepIds = new List<string>(record.CompletedStepIds ?? new List<string>()),
                    FailedStepIds = new List<string>(record.FailedStepIds ?? new List<string>()),
                    KnownObjectiveIds = new List<string>(record.KnownObjectiveIds ?? new List<string>()),
                    KnownHints = new List<string>(record.KnownHints ?? new List<string>()),
                    StartedAtDay = record.StartedAtDay,
                    StartedAtTime = record.StartedAtTime,
                    CompletedAtDay = record.CompletedAtDay ?? 0,
                    Tracked = record.Tracked,
                    Discovered = record.Discovered,
                    FailureReason = record.FailureReason,
                    GrantedRewardIds = new List<string>(record.GrantedRewardIds ?? new List<string>()),
                    GrantedFlagIds = new List<string>(record.GrantedFlagIds ?? new List<string>()),
                    RepeatInstanceId = record.RepeatInstanceId,
                    // fable_34 — dynamic instance + source channel (simple types only).
                    Source = record.Source,
                    IsDynamicInstance = record.IsDynamicInstance,
                    TemplateId = record.TemplateId,
                    InstanceTargetId = record.InstanceTargetId,
                    InstanceQuantity = record.InstanceQuantity,
                    QuestLevel = record.QuestLevel,
                    InstanceRewardGold = record.InstanceRewardGold,
                    InstanceRewardXp = record.InstanceRewardXp,
                    GeneratedForDay = record.GeneratedForDay,
                    ObjectiveStates = new List<QuestObjectiveStateSaveData>()
                };
                foreach (var obj in record.ObjectiveStates ?? new List<QuestObjectiveStateRecord>())
                {
                    if (obj == null) continue;
                    qDto.ObjectiveStates.Add(new QuestObjectiveStateSaveData
                    {
                        ObjectiveId = obj.ObjectiveId,
                        CurrentProgress = obj.CurrentProgress,
                        RequiredProgress = obj.RequiredProgress,
                        IsCompleted = obj.IsCompleted,
                        IsFailed = obj.IsFailed,
                        IsKnown = obj.IsKnown
                    });
                }
                dto.QuestStates.Add(qDto);
            }

            return dto;
        }

        public static void RestoreFromSaveData(QuestStateSectionSaveData saveData)
        {
            if (saveData == null) return;

            if (QuestService != null)
            {
                QuestService.RestoreFromSaveData(saveData);
                return;
            }

            // QuestService not yet initialized — store pending for when Initialize() runs
            _pendingSaveData = saveData;
        }

        public static void SetPendingSaveData(QuestStateSectionSaveData saveData)
        {
            _pendingSaveData = saveData;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null) return;

            var go = new GameObject("QuestRuntimeBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<QuestRuntimeBootstrap>();
        }

        private void OnEnable()
        {
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            // If already initialized, just ensure UI is present
            if (QuestService != null)
            {
                EnsureUiControllers();
                yield break;
            }

            for (var attempt = 0; attempt < MaxBindAttempts; attempt++)
            {
                var bootstrap = GameBootstrap.Instance;
                if (bootstrap == null)
                {
                    yield return null;
                    continue;
                }

                var inventoryManager = bootstrap.InventoryManager;
                var playerManager = bootstrap.PlayerManager;

                if (inventoryManager == null)
                {
                    yield return null;
                    continue;
                }

                Initialize(inventoryManager, playerManager, bootstrap.PlayerProgressionManager);
                yield break;
            }

            // GameBootstrap not found — initialize with null adapters (graceful degradation)
            Debug.LogWarning("[QuestRuntimeBootstrap] GameBootstrap not found after max attempts. Initializing QuestService without inventory/gold adapters.");
            Initialize(null, null, null);
        }

        private void Initialize(
            CindarsHope.Inventory.InventoryManager inventoryManager,
            CindarsHope.Player.PlayerManager playerManager,
            CindarsHope.Player.Progression.PlayerProgressionManager progressionManager)
        {
            // Build registry (smoke test quests registered in constructor)
            QuestRegistry = new QuestRegistry();

            // Build save section (in-memory for this session)
            var saveSection = new QuestStateSection();

            // Build adapters
            IQuestInventoryAccess inventoryAccess = inventoryManager != null
                ? new QuestInventoryAdapter(inventoryManager)
                : null;

            IQuestGoldAccess goldAccess = playerManager != null
                ? new QuestGoldAdapter(playerManager)
                : null;

            // fable_34 — progression adapter for scaled XP + main-act skill points.
            IQuestProgressionAccess progressionAccess = progressionManager != null
                ? new QuestProgressionAdapter(progressionManager)
                : null;

            // QuestFlagService — minimal (no SO registry needed for smoke test)
            var flagRegistry = new QuestFlagRegistry();
            var flagService = new QuestFlagService(flagRegistry);

            // Build QuestService
            QuestService = new QuestService(QuestRegistry, saveSection, inventoryAccess, goldAccess, flagService, progressionAccess);
            _progressionManager = progressionManager;

            // Apply pending save data from LoadGame (if LoadGame ran before Initialize)
            if (_pendingSaveData != null)
            {
                QuestService.RestoreFromSaveData(_pendingSaveData);
                _pendingSaveData = null;
            }

            // Wire event bridge
            var bridge = new QuestProgressEventBridge(QuestService);
            bridge.Subscribe();

            // fable_34 — notice board: rotate 3 contracts/day on DayStartedEvent (deterministic).
            _boardService = new QuestBoardService();
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            _boardWired = true;

            // UI controllers
            EnsureUiControllers();

            Debug.Log($"[QuestRuntimeBootstrap] Quest runtime initialized. Registry quests: {QuestRegistry.GetAllQuests().Count}. Inventory adapter: {(inventoryAccess != null ? "WIRED" : "NULL")}. Gold adapter: {(goldAccess != null ? "WIRED" : "NULL")}. Progression adapter: {(progressionAccess != null ? "WIRED" : "NULL")}.");
        }

        // ─── fable_34 — notice board daily rotation ─────────────────────────────────

        private QuestBoardService _boardService;
        private CindarsHope.Player.Progression.PlayerProgressionManager _progressionManager;
        private bool _boardWired;
        private int _lastBoardDay = -1;

        /// <summary>fable_34 — the board seed: a stable salt so rotation depends only on the day.</summary>
        private const string BoardSeed = "cindars_hope_notice_board";

        /// <summary>fable_34 — current daily contracts (exposed for UI/board interactable + tests).</summary>
        public static System.Collections.Generic.List<QuestInstance> CurrentBoardContracts { get; private set; }
            = new System.Collections.Generic.List<QuestInstance>();

        private void OnDayStarted(DayStartedEvent evt)
        {
            RefreshBoard(evt.DayNumber);
        }

        /// <summary>
        /// fable_34 — regenerates the day's contracts and publishes QuestBoardRefreshedEvent.
        /// Idempotent for the same day (skips if already generated). The generated instances are
        /// offered (registered) so they can be accepted at the board; acceptance creates the
        /// active record via QuestService.AcceptDynamicInstance.
        /// </summary>
        public void RefreshBoard(int day)
        {
            if (_boardService == null || QuestService == null) return;
            if (day == _lastBoardDay) return;
            _lastBoardDay = day;

            int playerLevel = _progressionManager != null ? _progressionManager.Level : 1;
            CurrentBoardContracts = _boardService.GenerateDailyContracts(BoardSeed, day, playerLevel);

            var ids = new List<string>();
            foreach (var instance in CurrentBoardContracts)
            {
                // Register the offered instance so the board interactable can accept it.
                QuestService.RegisterDynamicInstance(instance);
                ids.Add(instance.QuestId);
            }

            GameEventBus.Publish(new QuestBoardRefreshedEvent(day, ids));
            Debug.Log($"[QuestRuntimeBootstrap] Notice board refreshed for day {day}: {ids.Count} contracts.");
        }

        private void OnDisable()
        {
            if (_boardWired)
            {
                GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
                _boardWired = false;
            }
        }

        private static void EnsureUiControllers()
        {
            // QuestOfferPanelController — use static Instance to avoid FindObjectOfType (CS0618)
            if (QuestOfferPanelController.Instance == null)
            {
                var offerGo = new GameObject("QuestOfferPanelController");
                DontDestroyOnLoad(offerGo);
                offerGo.AddComponent<QuestOfferPanelController>();
            }

            // QuestLogPanelController + QuestLogRuntimeBinder — use static Instance to avoid FindObjectOfType (CS0618)
            if (QuestLogPanelController.Instance == null)
            {
                var logGo = new GameObject("QuestLogPanelController");
                DontDestroyOnLoad(logGo);
                logGo.AddComponent<QuestLogPanelController>();
                logGo.AddComponent<QuestLogRuntimeBinder>();
            }
        }
    }
}
