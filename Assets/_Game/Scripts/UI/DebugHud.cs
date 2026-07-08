using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Interaction;
using CindarsHope.Player;
using CindarsHope.Player.Progression;
using CindarsHope.Core.Time;
using CindarsHope.Save;
using CindarsHope.Skills;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.UI
{
    [DisallowMultipleComponent]
    public class DebugHud : MonoBehaviour
    {
        private const int DebugXpGrantAmount = 99;

        private static DebugHud _instance;

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private HungerManager _hungerManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private StatusEffectManager _statusEffectManager;
        [SerializeField] private InteractionSystem _interactionSystem;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _caveLevelRuntimeController;
        [SerializeField] private CaveDebugLevelSkipController _caveDebugLevelSkipController;

        private bool _hasInteractionCandidate;
        private string _currentInteractionPrompt = string.Empty;
        private string _lastEconomyTransaction = "nenhuma transacao";
        private string _lastProgressionMessage = string.Empty;
        private string _currentActionFeedback = string.Empty;
        private float _actionFeedbackUntil;
        private bool _isPrimaryInstance;
        private Vector2 _actionsScrollPosition;
        private Vector2 _infoScrollPosition;

        // Both debug overlays start hidden for a clean play screen; N toggles the Actions panel and
        // V toggles the Debug Info panel (B is taken by the equipped-tool cycle, M by the minimap).
        private bool _showActionsPanel;
        private bool _showInfoPanel;

        public static DebugHud Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _isPrimaryInstance = false;
                enabled = false;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _isPrimaryInstance = true;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DebugHud: initialized as primary instance.");
        }

        private void OnEnable()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            GameEventBus.Subscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Subscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
            GameEventBus.Subscribe<PlayerXpChangedEvent>(OnPlayerXpChanged);
            GameEventBus.Subscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            GameEventBus.Subscribe<PlayerActionFeedbackEvent>(OnPlayerActionFeedback);
            GameEventBus.Subscribe<GameSavedEvent>(OnGameSaved);
            GameEventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            GameEventBus.Subscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Subscribe<QuestObjectiveProgressedEvent>(OnQuestObjectiveProgressed);
            GameEventBus.Subscribe<QuestReadyToCompleteEvent>(OnQuestReadyToComplete);
            GameEventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Subscribe<QuestRewardClaimedEvent>(OnQuestRewardClaimed);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Subscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            GameEventBus.Unsubscribe<InteractionPromptChangedEvent>(OnInteractionPromptChanged);
            GameEventBus.Unsubscribe<EconomyTransactionCompletedEvent>(OnEconomyTransactionCompleted);
            GameEventBus.Unsubscribe<PlayerXpChangedEvent>(OnPlayerXpChanged);
            GameEventBus.Unsubscribe<PlayerLevelChangedEvent>(OnPlayerLevelChanged);
            GameEventBus.Unsubscribe<PlayerActionFeedbackEvent>(OnPlayerActionFeedback);
            GameEventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
            GameEventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            GameEventBus.Unsubscribe<QuestAcceptedEvent>(OnQuestAccepted);
            GameEventBus.Unsubscribe<QuestObjectiveProgressedEvent>(OnQuestObjectiveProgressed);
            GameEventBus.Unsubscribe<QuestReadyToCompleteEvent>(OnQuestReadyToComplete);
            GameEventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            GameEventBus.Unsubscribe<QuestRewardClaimedEvent>(OnQuestRewardClaimed);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
            GameEventBus.Unsubscribe<CaveExitedEvent>(OnCaveExited);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isPrimaryInstance = false;
            }
        }

        private void Update()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.O))
            {
                GrantDebugXp();
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                _showActionsPanel = !_showActionsPanel;
            }

            if (global::UnityEngine.Input.GetKeyDown(KeyCode.V))
            {
                _showInfoPanel = !_showInfoPanel;
            }
        }

        // SPEC 14A-FIX13: responsive HUD layout. Font size and panel widths scale with screen size
        // so the HUD stays legible on 1280x720, 1920x1080 and editor windows of any size.
        private float GetPanelWidth(float maxPx, float screenRatio)
        {
            return Mathf.Min(maxPx, Screen.width * screenRatio);
        }

        private void ApplyResponsiveStyles()
        {
            int fs = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 55f), 11, 20);
            GUI.skin.label.fontSize = fs;
            GUI.skin.label.wordWrap = true;
            GUI.skin.box.fontSize = fs;
            GUI.skin.button.fontSize = fs;
        }

        private void OnGUI()
        {
            if (!_isPrimaryInstance || _instance != this)
            {
                return;
            }

            ApplyResponsiveStyles();
            if (_showActionsPanel)
            {
                DrawActionsPanel();
            }

            if (_showInfoPanel)
            {
                DrawInfoPanel();
            }

            DrawToggleHint();
        }

        // Always-visible one-liner so the hidden debug panels stay discoverable. Lists only the panels that
        // are currently hidden, so it vanishes entirely once both are shown.
        private void DrawToggleHint()
        {
            var hint = string.Empty;
            if (!_showActionsPanel)
            {
                hint += "[N] Actions   ";
            }

            if (!_showInfoPanel)
            {
                hint += "[V] Debug Info";
            }

            if (string.IsNullOrWhiteSpace(hint))
            {
                return;
            }

            GUI.Label(new Rect(12f, Screen.height - 22f, 360f, 20f), hint.Trim());
        }

        private void DrawActionsPanel()
        {
            var width = GetPanelWidth(380f, 0.28f);
            var height = Screen.height - 24f;

            GUILayout.BeginArea(new Rect(12f, 12f, width, height), GUI.skin.box);
            _actionsScrollPosition = GUILayout.BeginScrollView(_actionsScrollPosition);

            GUILayout.Label("Actions");
            DrawInteractionState();
            DrawActionFeedback();
            DrawCommands();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawInfoPanel()
        {
            var width = GetPanelWidth(450f, 0.32f);
            var height = Screen.height - 24f;

            GUILayout.BeginArea(new Rect(Screen.width - width - 12f, 12f, width, height), GUI.skin.box);
            _infoScrollPosition = GUILayout.BeginScrollView(_infoScrollPosition);

            GUILayout.Label("Cindar's Hope - Debug Info");

            DrawWorldState();
            DrawPlayerState();
            DrawHungerState();
            DrawStaminaAndStatusState();

            GUILayout.Space(6f);
            DrawEquipmentState();

            GUILayout.Space(6f);
            DrawInventory();

            GUILayout.Space(6f);
            DrawProgressionState();

            GUILayout.Space(6f);
            DrawActiveSkillSlots();

            GUILayout.Space(6f);
            DrawCaveSummary();

            GUILayout.Space(6f);
            DrawCombatTelemetryStatus();

            GUILayout.Space(6f);
            DrawEconomyState();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawWorldState()
        {
            if (_timeManager == null)
            {
                GUILayout.Label("Day: not assigned");
            }
            else
            {
                GUILayout.Label($"Day: {_timeManager.CurrentDay}");
            }

            var weatherService = CindarsHope.World.Weather.WorldWeatherService.Instance;
            if (weatherService != null)
            {
                GUILayout.Label($"Clima: {CindarsHope.World.Weather.WeatherGenerator.GetWeatherDescription(weatherService.CurrentWeather)}");
            }

            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
        }

        private void DrawInteractionState()
        {
            if (_hasInteractionCandidate)
            {
                GUILayout.Label($"Interacao: {_currentInteractionPrompt}");
                return;
            }

            if (_interactionSystem != null && _interactionSystem.HasCandidate)
            {
                GUILayout.Label($"Interacao: {_interactionSystem.CurrentPrompt}");
                return;
            }

            GUILayout.Label("Interacao: nenhum alvo");
        }

        private void DrawEconomyState()
        {
            GUILayout.Label($"Economia: {_lastEconomyTransaction}");
        }

        private void DrawProgressionState()
        {
            var progressionManager = GetProgressionManager();
            if (progressionManager != null)
            {
                GUILayout.Label($"Level: {progressionManager.Level}");
                GUILayout.Label($"XP: {progressionManager.CurrentXp} / {progressionManager.XpToNextLevel}");
                GUILayout.Label($"Attr points: {progressionManager.UnspentAttributePoints}");
                GUILayout.Label($"Skill points: {progressionManager.UnspentSkillPoints}");
                GUILayout.Label($"STR/DEX/INT/WIL/CON/BRE: {progressionManager.Strength}/{progressionManager.Dexterity}/{progressionManager.Intelligence}/{progressionManager.Willpower}/{progressionManager.Constitution}/{progressionManager.Breath}");

                if (!string.IsNullOrWhiteSpace(_lastProgressionMessage))
                {
                    GUILayout.Label($"Progressao: {_lastProgressionMessage}");
                }

                return;
            }

            GUILayout.Label(string.IsNullOrWhiteSpace(_lastProgressionMessage)
                ? "Progressao: not assigned"
                : $"Progressao: {_lastProgressionMessage}");
        }

        // SPEC 14A-FIX13: equipment HUD shows DisplayName for each slot (RightHand/LeftHand)
        // resolved via ItemDatabase + InventoryManager.TryGetItemData. Updates automatically when
        // EquipmentSlotChangedEvent fires (no manual refresh needed). Falls back to legacy
        // EquippedToolId only if slots are empty (covers items equipped via legacy EquipTool).
        private void DrawEquipmentState()
        {
            var equipmentManager = _equipmentManager;
            if (equipmentManager == null && GameBootstrap.Instance != null)
            {
                equipmentManager = GameBootstrap.Instance.EquipmentManager;
            }

            if (equipmentManager == null)
            {
                GUILayout.Label("Equipment: not assigned");
                return;
            }

            GUILayout.Label("Equipment:");
            GUILayout.Label($"  Right hand (E): {ResolveSlotDisplayLabel(equipmentManager, EquipmentSlot.RightHand)}");
            GUILayout.Label($"  Left hand (Q): {ResolveSlotDisplayLabel(equipmentManager, EquipmentSlot.LeftHand)}");

            // Legacy fallback row only when slot-based system has nothing.
            var leftHand = equipmentManager.GetEquippedItem(EquipmentSlot.LeftHand);
            var rightHand = equipmentManager.GetEquippedItem(EquipmentSlot.RightHand);
            if (string.IsNullOrEmpty(leftHand) && string.IsNullOrEmpty(rightHand) && !string.IsNullOrEmpty(equipmentManager.EquippedToolId))
            {
                GUILayout.Label($"  Legacy tool: {equipmentManager.EquippedToolId} ({equipmentManager.EquippedToolType}/{equipmentManager.EquippedToolTier})");
            }

            if (_saveManager != null && _saveManager.HotbarState != null)
            {
                var selectedItem = _saveManager.HotbarState.SelectedItemId;
                if (string.IsNullOrWhiteSpace(selectedItem)) selectedItem = "empty";
                GUILayout.Label($"Hotbar: slot {_saveManager.HotbarState.SelectedSlotIndex + 1} [{selectedItem}]");
            }
        }

        private string ResolveSlotDisplayLabel(EquipmentManager equipmentManager, EquipmentSlot slot)
        {
            var instanceId = equipmentManager.GetEquippedItem(slot);
            if (string.IsNullOrEmpty(instanceId)) return "empty";
            if (_inventoryManager != null && _inventoryManager.TryGetItemData(instanceId, out var itemData) && itemData != null)
            {
                return !string.IsNullOrWhiteSpace(itemData.DisplayName) ? $"{itemData.DisplayName} ({instanceId})" : instanceId;
            }
            return instanceId;
        }

        private void DrawPlayerState()
        {
            if (_playerManager == null)
            {
                GUILayout.Label("PlayerManager: not assigned");
                return;
            }

            GUILayout.Label($"Gold: {_playerManager.CurrentGold}");
            GUILayout.Label($"HP: {_playerManager.CurrentHP}/{_playerManager.MaxHP}");
        }

        private void DrawHungerState()
        {
            if (_hungerManager == null)
            {
                GUILayout.Label("HungerManager: not assigned");
                return;
            }

            GUILayout.Label($"Hunger: {_hungerManager.CurrentHunger}/{_hungerManager.MaxHunger}");
        }

        private void DrawStaminaAndStatusState()
        {
            var bootstrap = GameBootstrap.Instance;
            var staminaManager = _staminaManager != null ? _staminaManager : bootstrap?.StaminaManager;
            var statusEffectManager = _statusEffectManager != null ? _statusEffectManager : bootstrap?.StatusEffectManager;

            if (staminaManager != null)
            {
                GUILayout.Label($"Stamina: {staminaManager.CurrentStamina}/{staminaManager.MaxStamina}");
            }
            else
            {
                GUILayout.Label("StaminaManager: not assigned");
            }

            if (statusEffectManager == null || statusEffectManager.ActiveEffects.Count == 0)
            {
                GUILayout.Label("Status: Normal");
                return;
            }

            GUILayout.Label("Status:");
            foreach (var effect in statusEffectManager.ActiveEffects)
            {
                if (effect.Value != null && effect.Value.IsActive)
                {
                    GUILayout.Label($"- {effect.Key}: {Mathf.CeilToInt(effect.Value.RemainingSeconds)}s");
                }
            }
        }

        private void DrawInventory()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Inventory:");

            if (_inventoryManager == null)
            {
                GUILayout.Label("- not assigned");
                return;
            }

            if (_inventoryManager.Items.Count == 0)
            {
                GUILayout.Label("- empty");
                return;
            }

            foreach (var item in _inventoryManager.Items)
            {
                GUILayout.Label($"- {item.Key}: {item.Value}");
            }
        }

        // WAVE_INTEGRATION_10: active skill slot display. Shows what is assigned to each of the
        // 4 active slots (R/T/Y/G). Data comes from SkillTreeManager.State, which is authoritative.
        private void DrawActiveSkillSlots()
        {
            var bootstrap = GameBootstrap.Instance;
            var skillTreeManager = bootstrap?.SkillTreeManager;
            if (skillTreeManager == null)
            {
                GUILayout.Label("Active Skills: SkillTreeManager not assigned");
                return;
            }

            GUILayout.Label("Active Skills:");
            var state = skillTreeManager.State;
            string[] slotKeys = { "R", "T", "Y", "G" };
            for (int i = 0; i < slotKeys.Length; i++)
            {
                var actionId = state.GetActiveSlotSkillActionId(i);
                GUILayout.Label($"  [{slotKeys[i]}]: {(string.IsNullOrWhiteSpace(actionId) ? "vazio" : actionId)}");
            }
        }

        // SPEC 14A-FIX13: Gameplay commands separated from Debug commands.
        // J was removed - attack uses Q (left hand) and E (right hand / interact).
        private void DrawCommands()
        {
            GUILayout.Space(8f);
            GUILayout.Label("== Gameplay ==");
            GUILayout.Label("WASD / Arrows: move");
            GUILayout.Label("E: interact / right-hand attack");
            GUILayout.Label("Q: left-hand / tool action");
            GUILayout.Label("Space: dodge");
            GUILayout.Label("I: inventory");
            GUILayout.Label("L: equipment");
            GUILayout.Label("K: attributes / progression");
            GUILayout.Label("U: skill trees");
            GUILayout.Label("1-6: hotbar slot");
            GUILayout.Label("H: consume food");
            GUILayout.Label("B: cycle equipped tool");
            GUILayout.Label("Esc: close modal / back");

            GUILayout.Space(6f);
            GUILayout.Label("== Debug ==");
            GUILayout.Label("N / V: toggle Actions / Debug Info HUD");
            GUILayout.Label("O: +99 XP");
            GUILayout.Label("P: next gate / level skip");
            GUILayout.Label("F2: alt debug skip");
            GUILayout.Label("Tab: advance day");
            GUILayout.Label("Shift+R: regenerate cave run");
            GUILayout.Label("F5: save");
            GUILayout.Label("F9: load");

            if (_saveManager != null)
            {
                GUILayout.Space(4f);
                GUILayout.Label($"Save: {ShortenMiddle(_saveManager.SaveFilePath, 48)}");
            }
        }

        private void DrawActionFeedback()
        {
            if (string.IsNullOrWhiteSpace(_currentActionFeedback) || Time.time > _actionFeedbackUntil)
            {
                return;
            }

            GUILayout.Space(6f);
            GUILayout.Label($"Feedback: {_currentActionFeedback}");
        }

        private void DrawCaveSummary()
        {
            GUILayout.Space(8f);
            if (_caveRunManager == null || _caveLevelRuntimeController == null)
            {
                GUILayout.Label("Cave: fixed/unavailable");
                GUILayout.Label("Seed: unavailable");
                return;
            }

            GUILayout.Label("Cave: procedural MVP");
            GUILayout.Label($"CaveLevel: {_caveRunManager.CurrentCaveLevel}");
            GUILayout.Label($"Deepest: {_caveRunManager.DeepestLayerReached}");
            GUILayout.Label($"WorldSeed: {ShortenMiddle(_caveRunManager.CaveWorldSeed, 44)}");
            GUILayout.Label($"RunSeed: {ShortenMiddle(_caveRunManager.CaveRunSeed, 44)}");
            GUILayout.Label($"SpawnAnchor: {_caveLevelRuntimeController.CurrentSpawnAnchor}");
            GUILayout.Label($"Rooms: {_caveLevelRuntimeController.RoomCount}");
            GUILayout.Label($"EnemyPoints: {_caveLevelRuntimeController.EnemyPointCount}");
            GUILayout.Label($"ResourcePoints: {_caveLevelRuntimeController.ResourcePointCount}");

            var currentLevel = _caveLevelRuntimeController.CurrentGeneratedLevel;
            if (currentLevel != null)
            {
                GUILayout.Label($"LayoutHash: {ShortenMiddle(currentLevel.LayoutHash, 20)}");
                GUILayout.Label($"Entrance: ({currentLevel.Entrance.x}, {currentLevel.Entrance.y})");
                GUILayout.Label($"Exit: ({currentLevel.Exit.x}, {currentLevel.Exit.y})");
            }

            var materializationResult = _caveLevelRuntimeController.Materializer.LastMaterializationResult;
            if (materializationResult != null)
            {
                GUILayout.Space(4f);
                GUILayout.Label("Materialized:");
                GUILayout.Label($"  Floors: {materializationResult.CreatedFloorTiles}");
                GUILayout.Label($"  Walls: {materializationResult.CreatedWallTiles}");
                GUILayout.Label($"  ResourceCandidates: {materializationResult.ResourceCandidateCount}");
                GUILayout.Label($"  Resources: {materializationResult.CreatedResourceNodes}");
                GUILayout.Label($"  Enemies: {materializationResult.CreatedEnemies}");
                GUILayout.Label($"  BackExit: {materializationResult.BackExitPosition}");
                GUILayout.Label($"  ForwardExit: {materializationResult.ForwardExitPosition}");
            }

            GUILayout.Space(4f);
            GUILayout.Label("Snapshots:");
            if (_caveRunManager.State.VisitedLevelSnapshots.Count == 0)
            {
                GUILayout.Label("  None");
            }
            else
            {
                foreach (var kvp in _caveRunManager.State.VisitedLevelSnapshots)
                {
                    GUILayout.Label($"  Level {kvp.Key}: hash={ShortenMiddle(kvp.Value.LayoutHash, 12)}");
                }
            }

            GUILayout.Label($"Checkpoints: {string.Join(",", _caveRunManager.State.UnlockedCheckpoints)}");

            GUILayout.Space(4f);
            GUILayout.Label("Current Level Boss Gate:");
            if (_caveRunManager.IsCurrentLevelBossGate())
            {
                var gateId = _caveRunManager.GetCurrentBossGateId();
                var isDefeated = _caveRunManager.IsCurrentBossGateDefeated();
                var canAdvance = _caveRunManager.CanAdvancePastCurrentBossGate();
                var status = isDefeated ? "defeated / open" : "active / blocked";
                GUILayout.Label($"  {gateId}: {status}");
                GUILayout.Label($"  Can advance: {canAdvance}");
            }
            else
            {
                GUILayout.Label("  No gate at this level");
            }

            GUILayout.Space(4f);
            GUILayout.Label("Boss Gates History:");
            if (_caveRunManager.State.BossDefeatStates.Count == 0)
            {
                GUILayout.Label("  None");
            }
            else
            {
                foreach (var kvp in _caveRunManager.State.BossDefeatStates)
                {
                    var defeated = kvp.Value.IsDefeated ? "defeated" : "active";
                    GUILayout.Label($"  {kvp.Key} (level {kvp.Value.CaveLevel}): {defeated}");
                }
            }

            GUILayout.Space(4f);
            DrawDebugLevelSkip();
        }

        private void DrawDebugLevelSkip()
        {
            if (_caveDebugLevelSkipController == null)
            {
                GUILayout.Label("Debug Level Skip: not assigned");
                return;
            }

            var skipStatus = _caveDebugLevelSkipController.IsDebugSkipEnabled ? "enabled" : "disabled";
            GUILayout.Label($"Debug Level Skip: {skipStatus} (Press P)");

            if (!string.IsNullOrWhiteSpace(_caveDebugLevelSkipController.LastDebugAction) &&
                _caveDebugLevelSkipController.LastDebugAction != "none")
            {
                GUILayout.Label($"Last: {_caveDebugLevelSkipController.LastDebugAction}");
            }
        }

        // fable_59 — linha aditiva de status da telemetria de combate (sem tela nova).
        private void DrawCombatTelemetryStatus()
        {
            var service = CindarsHope.Combat.Telemetry.CombatTelemetryService.ActiveInstance;
            if (service == null)
            {
                GUILayout.Label("Telemetry: bootstrap pending");
                return;
            }

            if (service.IsCollecting)
            {
                GUILayout.Label($"Telemetry: ON - level {service.CurrentLevel}, {service.CurrentKillEntries} kill ids");
            }
            else
            {
                GUILayout.Label("Telemetry: OFF (debug toggle)");
            }
        }

        private void GrantDebugXp()
        {
            var progressionManager = GetProgressionManager();
            if (progressionManager == null)
            {
                _currentActionFeedback = "XP debug failed: progression manager not assigned";
                _actionFeedbackUntil = Time.time + 2f;
                Debug.LogWarning("DebugHud: cannot grant debug XP because PlayerProgressionManager is not assigned.", this);
                return;
            }

            var oldLevel = progressionManager.Level;
            var oldXp = progressionManager.CurrentXp;
            progressionManager.AddXp(DebugXpGrantAmount);
            _currentActionFeedback = $"+{DebugXpGrantAmount} XP debug";
            _actionFeedbackUntil = Time.time + 2f;
            Debug.Log($"DebugHud: granted +{DebugXpGrantAmount} XP via O key. Level {oldLevel}->{progressionManager.Level}, XP {oldXp}->{progressionManager.CurrentXp}/{progressionManager.XpToNextLevel}.", this);
        }

        private void OnInteractionPromptChanged(InteractionPromptChangedEvent evt)
        {
            _hasInteractionCandidate = evt.HasCandidate;
            _currentInteractionPrompt = evt.Prompt ?? string.Empty;
        }

        private void OnEconomyTransactionCompleted(EconomyTransactionCompletedEvent evt)
        {
            _lastEconomyTransaction = evt.Message;
        }

        private void OnPlayerXpChanged(PlayerXpChangedEvent evt)
        {
            _lastProgressionMessage = $"XP {evt.CurrentXp}/{evt.XpToNextLevel} (Lv {evt.Level})";
        }

        private void OnPlayerLevelChanged(PlayerLevelChangedEvent evt)
        {
            _lastProgressionMessage = $"Level {evt.OldLevel} -> {evt.NewLevel}";
        }

        private void OnPlayerActionFeedback(PlayerActionFeedbackEvent evt)
        {
            _currentActionFeedback = evt.Message ?? string.Empty;
            _actionFeedbackUntil = Time.time + evt.DurationSeconds;
        }

        private void SetFeedback(string message, float duration = 4f)
        {
            _currentActionFeedback = message ?? string.Empty;
            _actionFeedbackUntil = Time.time + duration;
        }

        private void OnGameSaved(GameSavedEvent evt) =>
            SetFeedback(evt.WasSuccessful ? "Jogo salvo." : $"Falha ao salvar: {evt.Message}");

        private void OnGameLoaded(GameLoadedEvent evt) =>
            SetFeedback(evt.WasSuccessful ? "Jogo carregado." : $"Falha ao carregar: {evt.Message}");

        private void OnQuestAccepted(QuestAcceptedEvent evt) =>
            SetFeedback($"Quest aceita: {evt.QuestId}");

        private void OnQuestObjectiveProgressed(QuestObjectiveProgressedEvent evt) =>
            SetFeedback($"Objetivo: {evt.ObjectiveId} {evt.CurrentProgress}/{evt.RequiredProgress}");

        private void OnQuestReadyToComplete(QuestReadyToCompleteEvent evt) =>
            SetFeedback($"Pronto para entregar: {evt.QuestId}");

        private void OnQuestCompleted(QuestCompletedEvent evt) =>
            SetFeedback($"Quest concluída: {evt.QuestId}");

        private void OnQuestRewardClaimed(QuestRewardClaimedEvent evt) =>
            SetFeedback(evt.GoldGiven > 0
                ? $"Recompensa: {evt.GoldGiven}g"
                : $"Recompensa recebida: {evt.QuestId}");

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt) =>
            SetFeedback($"Entrando na caverna (nível {evt.CaveLevel})");

        private void OnCaveExited(CaveExitedEvent evt) =>
            SetFeedback($"Retornando à superfície → {evt.ReturnScene}");

        private void OnEnemyKilled(EnemyKilledEvent evt) =>
            SetFeedback(string.IsNullOrEmpty(evt.DropItemId)
                ? $"Inimigo derrotado: {evt.EnemyId}"
                : $"Inimigo derrotado: {evt.EnemyId} | Loot: {evt.DropItemId} x{evt.DropAmount}");

        private static PlayerProgressionManager GetProgressionManager()
        {
            return GameBootstrap.Instance != null ? GameBootstrap.Instance.PlayerProgressionManager : null;
        }

        public void RebindRuntimeReferences(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            StaminaManager staminaManager,
            StatusEffectManager statusEffectManager,
            InteractionSystem interactionSystem,
            TimeManager timeManager,
            SaveManager saveManager)
        {
            if (playerManager != null)
            {
                _playerManager = playerManager;
            }

            if (inventoryManager != null)
            {
                _inventoryManager = inventoryManager;
            }

            if (hungerManager != null)
            {
                _hungerManager = hungerManager;
            }

            if (staminaManager != null)
            {
                _staminaManager = staminaManager;
            }

            if (statusEffectManager != null)
            {
                _statusEffectManager = statusEffectManager;
            }

            if (interactionSystem != null)
            {
                _interactionSystem = interactionSystem;
            }

            if (timeManager != null)
            {
                _timeManager = timeManager;
            }

            if (saveManager != null)
            {
                _saveManager = saveManager;
            }

            if (GameBootstrap.Instance != null && GameBootstrap.Instance.EquipmentManager != null)
            {
                _equipmentManager = GameBootstrap.Instance.EquipmentManager;
            }

            Debug.Log("DebugHud: runtime references rebound.");
        }

        public void RebindCaveRuntime(CaveRunManager caveRunManager, CaveLevelRuntimeController caveLevelRuntimeController, CaveDebugLevelSkipController caveDebugLevelSkipController = null)
        {
            if (caveRunManager != null)
            {
                _caveRunManager = caveRunManager;
            }

            if (caveLevelRuntimeController != null)
            {
                _caveLevelRuntimeController = caveLevelRuntimeController;
            }

            if (caveDebugLevelSkipController != null)
            {
                _caveDebugLevelSkipController = caveDebugLevelSkipController;
            }

            Debug.Log("DebugHud: cave runtime references rebound.");
        }

        public static void RebindExisting(
            PlayerManager playerManager,
            InventoryManager inventoryManager,
            HungerManager hungerManager,
            StaminaManager staminaManager,
            StatusEffectManager statusEffectManager,
            InteractionSystem interactionSystem,
            TimeManager timeManager,
            SaveManager saveManager)
        {
            if (_instance != null)
            {
                _instance.RebindRuntimeReferences(playerManager, inventoryManager, hungerManager, staminaManager, statusEffectManager, interactionSystem, timeManager, saveManager);
            }
        }

        public static void RebindExistingCaveRuntime(CaveRunManager caveRunManager, CaveLevelRuntimeController caveLevelRuntimeController, CaveDebugLevelSkipController caveDebugLevelSkipController = null)
        {
            if (_instance != null)
            {
                _instance.RebindCaveRuntime(caveRunManager, caveLevelRuntimeController, caveDebugLevelSkipController);
            }
        }

        private static string ShortenMiddle(string value, int maxLength = 48)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
            {
                return value ?? string.Empty;
            }

            var keep = Mathf.Max(4, (maxLength - 3) / 2);
            return $"{value.Substring(0, keep)}...{value.Substring(value.Length - keep)}";
        }
    }
}
