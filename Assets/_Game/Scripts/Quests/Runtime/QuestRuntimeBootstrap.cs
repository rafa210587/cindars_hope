using System.Collections;
using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
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

                Initialize(inventoryManager, playerManager);
                yield break;
            }

            // GameBootstrap not found — initialize with null adapters (graceful degradation)
            Debug.LogWarning("[QuestRuntimeBootstrap] GameBootstrap not found after max attempts. Initializing QuestService without inventory/gold adapters.");
            Initialize(null, null);
        }

        private void Initialize(
            CindarsHope.Inventory.InventoryManager inventoryManager,
            CindarsHope.Player.PlayerManager playerManager)
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

            // QuestFlagService — minimal (no SO registry needed for smoke test)
            var flagRegistry = new QuestFlagRegistry();
            var flagService = new QuestFlagService(flagRegistry);

            // Build QuestService
            QuestService = new QuestService(QuestRegistry, saveSection, inventoryAccess, goldAccess, flagService);

            // Apply pending save data from LoadGame (if LoadGame ran before Initialize)
            if (_pendingSaveData != null)
            {
                QuestService.RestoreFromSaveData(_pendingSaveData);
                _pendingSaveData = null;
            }

            // Wire event bridge
            var bridge = new QuestProgressEventBridge(QuestService);
            bridge.Subscribe();

            // UI controllers
            EnsureUiControllers();

            Debug.Log($"[QuestRuntimeBootstrap] Quest runtime initialized. Registry quests: {QuestRegistry.GetAllQuests().Count}. Inventory adapter: {(inventoryAccess != null ? "WIRED" : "NULL")}. Gold adapter: {(goldAccess != null ? "WIRED" : "NULL")}.");
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
