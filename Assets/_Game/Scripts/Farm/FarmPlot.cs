using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Farm.Data;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Farm
{
    [DisallowMultipleComponent]
    public partial class FarmPlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _plotIndex;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SeedDatabaseSO _seedDatabase;
        [SerializeField] private Player.StaminaManager _staminaManager;
        // fable_55: calendÃ¡rio para o gate de estaÃ§Ã£o no plantio (ponto Ãºnico). Opcional: ausente
        // = sem informaÃ§Ã£o de estaÃ§Ã£o = plantio liberado (nÃ£o bloquear sem dado).
        [SerializeField] private World.Calendar.GameCalendarService _calendarService;
        // TODO_INTEGRATION_NOT_FINAL (RE-REGISTRADO fable_66, owner: kit inicial canÃ´nico em inventÃ¡rio):
        // o slice mode Ã© o ÃšNICO caminho jogÃ¡vel do loop de crop sem ferramentas/sementes no inventÃ¡rio.
        // NÃƒO REMOVER atÃ©: (1) o kit inicial canÃ´nico GRANTAR hoe + watering-can + sementes ao inventÃ¡rio
        // do player no new game (PlayerDataSO.StartingItems verificado no asset) E (2) CreateMvpFarmScene
        // ser regenerado no Unity Editor com o flag OFF. DecisÃ£o e condiÃ§Ã£o em
        // docs/validation/fable_66_spec_code_debt_cleanup_slice_mode_execution_report.md (CA-4, SEM gate).
        [SerializeField] private bool _temporarySequentialSliceMode;
        [SerializeField] private string _temporarySequentialSeedId = "seed_carrot";

        // Inicializado no campo (não só no Awake): scene creators/editor (AddComponent, ResetPlot,
        // Configure) rodam fora do Play Mode, onde Awake não é chamado — sem isto, _logic fica null e NRE.
        private FarmPlotLogic _logic = new FarmPlotLogic();
        private FarmPlotMenuController _menuController;
        private Sprite _baseSprite;

        public static bool IsAnyActionMenuOpen => FarmPlotMenuController.ActiveMenuPlot != null;

        public FarmPlotState State => _logic.State;
        public string PlantedSeedId => _logic.PlantedSeedId;
        public int DaysGrown => _logic.DaysGrown;
        public bool IsWatered => _logic.State == FarmPlotState.TilledWet || _logic.State == FarmPlotState.PlantedWet;
        public int RegrowRemainingDays => _logic.RegrowRemainingDays;
        public int DaysWithoutWater => _logic.DaysWithoutWater;
        public int LastProcessedDay => _logic.LastProcessedDay;
        public string FertilizerId => _logic.FertilizerId;
        public int WateredDaysCount => _logic.WateredDaysCount;

        public string PlotId => $"plot_{_plotIndex}";

        public string InteractionPrompt
        {
            get
            {
                switch (State)
                {
                    case FarmPlotState.Raw:
                        return "Arar";
                    case FarmPlotState.TilledDry:
                    case FarmPlotState.TilledWet:
                        return "Cultivar";
                    case FarmPlotState.PlantedDry:
                        return "Molhar";
                    case FarmPlotState.PlantedWet:
                        return "Irrigado";
                    case FarmPlotState.ReadyToHarvest:
                        return "Colher";
                    case FarmPlotState.Dead:
                        return "Limpar";
                    case FarmPlotState.Blocked:
                        return "Bloqueado";
                    default:
                        return "Interagir";
                }
            }
        }

        public void Configure(int plotIndex, InventoryManager inventoryManager, SeedDatabaseSO seedDatabase)
        {
            _plotIndex = plotIndex;
            _inventoryManager = inventoryManager;
            _seedDatabase = seedDatabase;
            EnsureRenderer();
            var normalized = State == FarmPlotState.Blocked ? FarmPlotState.Blocked : FarmPlotLogic.NormalizeState(State);
            _logic.SetStateInternal(normalized);
            UpdateVisual();
        }

        public void SetState(FarmPlotState state)
        {
            _logic.SetStateInternal(state);
            UpdateVisual();
        }

        public void ResetPlot()
        {
            _logic.ResetPlot();
            UpdateVisual();
        }

        public FarmPlotSaveData CaptureSaveData()
        {
            return _logic.CaptureSaveData(_plotIndex);
        }

        public void RestoreFromSaveData(FarmPlotSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot restore from null save data.", this);
                return;
            }

            if (!System.Enum.TryParse(saveData.State, out FarmPlotState _))
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} received invalid saved state '{saveData.State}'. Resetting plot.", this);
                _logic.SetStateInternal(FarmPlotState.Raw);
                UpdateVisual();
                return;
            }

            // Resolve seed before delegating so logic doesn't need a reference to seedDatabase
            var seedIsResolvable = true;
            if (!string.IsNullOrWhiteSpace(saveData.PlantedSeedId))
            {
                // Only relevant when state is planted/ready
                if (!System.Enum.TryParse(saveData.State, out FarmPlotState savedStateEnum) ||
                    (FarmPlotLogic.IsPlantedState(savedStateEnum) || savedStateEnum == FarmPlotState.ReadyToHarvest))
                {
                    seedIsResolvable = TryGetPlantedSeedDataById(saveData.PlantedSeedId, out _);
                    if (!seedIsResolvable)
                    {
                        Debug.LogWarning($"FarmPlot {_plotIndex} could not resolve saved seed '{saveData.PlantedSeedId}'. Resetting to tilled dry.", this);
                    }
                }
            }

            var restored = _logic.RestoreFromSaveData(saveData, seedIsResolvable);
            if (!restored)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} RestoreFromSaveData returned false.", this);
            }

            if (!string.IsNullOrEmpty(_logic.FertilizerId))
            {
                FarmFertilityRuntime.RestoreModifier(PlotId, _logic.FertilizerId, _logic.CurrentDay);
            }

            UpdateVisual();
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} received null InventoryManager for rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        public void RebindStaminaManager(Player.StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        // WAVE_INTEGRATION_11: Public bridge for skill effect executors to water this plot.
        public bool TryWaterViaSkill()
        {
            var success = _logic.TryWaterSilent();
            if (success)
            {
                UpdateVisual();
                PublishFeedback(State == FarmPlotState.TilledWet ? "Soil watered by skill." : "Crop watered by skill.");
            }
            else
            {
                PublishFeedback("Plot cannot be watered in current state.");
            }

            return success;
        }

        public bool CanBeWatered => State == FarmPlotState.TilledDry || State == FarmPlotState.PlantedDry;

        /// <summary>
        /// fable_37 â€” hook NOMEADO ÃšNICO do pico de Lua Verde.
        /// </summary>
        public bool TryAdvanceStageFromLunarPeak()
        {
            if (State != FarmPlotState.PlantedDry && State != FarmPlotState.PlantedWet)
            {
                return false;
            }

            if (!TryGetPlantedSeedData(out var seedData))
            {
                return false;
            }

            var seed = BuildSeedParams(seedData);
            var advanced = _logic.TryAdvanceStagePure(seed);
            if (advanced)
            {
                if (State == FarmPlotState.ReadyToHarvest)
                {
                    GameEventBus.Publish(new CropReadyEvent(_logic.PlantedSeedId, GetTilePosition(), _logic.DaysGrown));
                }

                UpdateVisual();
            }

            return advanced;
        }

        /// <summary>
        /// Rega silenciosa pela chuva (RainIrrigationRunner).
        /// </summary>
        public bool TryWaterFromRain()
        {
            var success = _logic.TryWaterSilent();
            if (success)
            {
                UpdateVisual();
            }

            return success;
        }

        /// <summary>
        /// Monta a entrada de qualidade da colheita a partir do histÃ³rico do canteiro.
        /// </summary>
        internal static Crops.CropQualityInput BuildQualityInput(int wateredDaysCount, int daysGrown, bool fertilizerApplied)
        {
            return FarmPlotLogic.BuildQualityInput(wateredDaysCount, daysGrown, fertilizerApplied);
        }

        /// <summary>Unidades extras por tier de qualidade.</summary>
        internal static int GetQualityBonusUnits(Crops.CropQualityTier tier)
        {
            return FarmPlotLogic.GetQualityBonusUnits(tier);
        }

        public bool CanInteract(GameObject interactor)
        {
            return FarmPlotMenuController.ActiveMenuPlot == null || FarmPlotMenuController.ActiveMenuPlot == this;
        }

        public void Interact(GameObject interactor)
        {
            if (FarmPlotMenuController.ActiveMenuPlot == this)
            {
                _menuController.CloseMenu();
                return;
            }

            _menuController.OpenMenu(State);
        }

        private void Reset()
        {
            EnsureRenderer();
        }

        private void Awake()
        {
            _logic = new FarmPlotLogic();
            _menuController = new FarmPlotMenuController(this);
            EnsureRenderer();
            _logic.SetStateInternal(FarmPlotLogic.NormalizeState(State));
            UpdateVisual();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            if (FarmPlotMenuController.ActiveMenuPlot == this)
            {
                _menuController.CloseMenu();
            }
        }

        private void OnValidate()
        {
            EnsureRenderer();
            UpdateVisual();
        }

        private void Update()
        {
            _menuController?.Update();
        }

        private void OnGUI()
        {
            _menuController?.OnGUI(_plotIndex, State);
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            if (_logic == null)
            {
                return;
            }

            var seedParams = ResolveSeedParams();
            var result = _logic.ProcessDay(evt.DayNumber, seedParams);

            if (result.BecameReady)
            {
                GameEventBus.Publish(new CropReadyEvent(_logic.PlantedSeedId, GetTilePosition(), _logic.DaysGrown));
                Debug.Log($"FarmPlot {_plotIndex} crop '{_logic.PlantedSeedId}' is ready after {_logic.DaysGrown} watered day(s).", this);
            }
            else if (result.CropAdvanced)
            {
                if (TryGetPlantedSeedData(out var sd))
                {
                    Debug.Log($"FarmPlot {_plotIndex} crop '{_logic.PlantedSeedId}' grew to {_logic.DaysGrown}/{sd.GrowthDays} on day {evt.DayNumber}.", this);
                }
            }
            else if (result.CropDied)
            {
                GameEventBus.Publish(new CropDiedEvent(_logic.PlantedSeedId, GetTilePosition(), evt.DayNumber));
                Debug.Log($"FarmPlot {_plotIndex} crop '{_logic.PlantedSeedId}' died after {_logic.DaysWithoutWater} days without water.", this);
            }

            UpdateVisual();
        }

        // â”€â”€ Internal bridge methods for FarmPlotMenuController â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        internal bool HasRequiredToolInternal(ToolType toolType)
        {
            return HasRequiredTool(toolType);
        }

        internal bool TemporarySliceModeInternal => _temporarySequentialSliceMode;
        internal string TemporarySequentialSeedIdInternal => _temporarySequentialSeedId;
        internal InventoryManager InventoryManagerInternal => _inventoryManager;
        internal SeedDatabaseSO SeedDatabaseInternal => _seedDatabase;

        internal bool TryResolveSeedDataForItemInternal(string seedItemId, out Data.SeedDataSO seedData)
        {
            return TryResolveSeedDataForItem(seedItemId, out seedData);
        }

        internal Vector2 GetMenuScreenPositionInternal()
        {
            return GetMenuScreenPosition();
        }

        internal void PublishFeedbackInternal(string message)
        {
            PublishFeedback(message);
        }

        internal void TryTillInternal()
        {
            TryTill();
        }

        internal void TryWaterInternal()
        {
            TryWater();
        }

        internal void TryPlantSeedInternal(string seedId, string seedItemId)
        {
            TryPlantSeed(seedId, seedItemId);
        }

        internal void TryHarvestInternal()
        {
            TryHarvest();
        }

        internal void TryAdvanceTemporaryGrowthInternal()
        {
            TryAdvanceTemporaryGrowth();
        }

        internal void TryClearDeadInternal()
        {
            TryClearDead();
        }

        internal void ExecuteAnalyzeInternal()
        {
            ExecuteAnalyze();
        }

        internal void TryFertilizeInternal(string fertilizerId)
        {
            TryFertilize(fertilizerId);
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            // _logic só é criado no Awake; OnValidate/edit-time pode chamar UpdateVisual antes disso.
            // Sem o core, não há estado de cultivo para exibir — mostra o sprite base e sai (evita NRE no editor).
            if (_logic == null)
            {
                _spriteRenderer.sprite = _baseSprite;
                return;
            }

            var stageSprite = GetCurrentStageSprite();
            _spriteRenderer.sprite = stageSprite != null ? stageSprite : _baseSprite;

            switch (State)
            {
                case FarmPlotState.Raw:
                    _spriteRenderer.color = new Color(0.35f, 0.24f, 0.16f);
                    break;
                case FarmPlotState.TilledDry:
                    _spriteRenderer.color = new Color(0.42f, 0.25f, 0.15f);
                    break;
                case FarmPlotState.TilledWet:
                    _spriteRenderer.color = new Color(0.23f, 0.20f, 0.16f);
                    break;
                case FarmPlotState.PlantedDry:
                    _spriteRenderer.color = stageSprite != null ? Color.white : new Color(0.22f, 0.55f, 0.22f);
                    break;
                case FarmPlotState.PlantedWet:
                    _spriteRenderer.color = stageSprite != null ? new Color(0.85f, 0.95f, 1f) : new Color(0.16f, 0.45f, 0.26f);
                    break;
                case FarmPlotState.ReadyToHarvest:
                    _spriteRenderer.color = stageSprite != null ? Color.white : new Color(0.9f, 0.7f, 0.18f);
                    break;
                case FarmPlotState.Blocked:
                    _spriteRenderer.color = Color.gray;
                    break;
                case FarmPlotState.Dead:
                    _spriteRenderer.color = new Color(0.16f, 0.16f, 0.16f);
                    break;
            }
        }

        private Sprite GetCurrentStageSprite()
        {
            if (!FarmPlotLogic.IsPlantedState(State) && State != FarmPlotState.ReadyToHarvest)
            {
                return null;
            }

            if (!TryGetPlantedSeedData(out var seedData) || seedData.GrowthStageSprites == null || seedData.GrowthStageSprites.Length == 0)
            {
                return null;
            }

            var index = State == FarmPlotState.ReadyToHarvest
                ? seedData.GrowthStageSprites.Length - 1
                : Mathf.Clamp(DaysGrown, 0, seedData.GrowthStageSprites.Length - 1);
            return seedData.GrowthStageSprites[index];
        }

        private bool TryGetPlantedSeedData(out Data.SeedDataSO seedData)
        {
            seedData = null;

            if (string.IsNullOrWhiteSpace(PlantedSeedId))
            {
                return false;
            }

            if (_seedDatabase == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot resolve planted seed '{PlantedSeedId}' because SeedDatabaseSO is missing.", this);
                return false;
            }

            if (!_seedDatabase.TryGetById(PlantedSeedId, out seedData) || seedData == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} could not resolve planted seed id '{PlantedSeedId}'.", this);
                return false;
            }

            return true;
        }

        private bool TryGetPlantedSeedDataById(string seedId, out Data.SeedDataSO seedData)
        {
            seedData = null;
            if (string.IsNullOrWhiteSpace(seedId) || _seedDatabase == null)
            {
                return false;
            }

            return _seedDatabase.TryGetById(seedId, out seedData) && seedData != null;
        }

        private bool TryResolveSeedDataForItem(string seedItemId, out Data.SeedDataSO seedData)
        {
            seedData = null;
            if (string.IsNullOrWhiteSpace(seedItemId) || _seedDatabase == null)
            {
                return false;
            }

            if (_seedDatabase.TryGetById(seedItemId, out seedData) && seedData != null)
            {
                return true;
            }

            foreach (var candidate in _seedDatabase.All)
            {
                if (candidate == null || candidate.SeedItem == null)
                {
                    continue;
                }

                if (candidate.SeedItem.Id == seedItemId)
                {
                    seedData = candidate;
                    return true;
                }
            }

            return false;
        }

        private bool IsSeasonAllowedForSeed(Data.SeedDataSO seedData)
        {
            var canOverride = false;
            var greenhouse = Watering.GreenhouseRuntimeHost.Instance;
            if (greenhouse != null)
            {
                canOverride = greenhouse.CanOverrideSeason(PlotId);
            }

            var currentSeason = ResolveCurrentSeasonName();
            return FarmSeasonGate.IsPlantingAllowed(seedData != null ? seedData.SeasonTags : null, currentSeason, canOverride);
        }

        private string ResolveCurrentSeasonName()
        {
            if (_calendarService == null || !_calendarService.IsInitialized)
            {
                return string.Empty;
            }

            return _calendarService.CurrentDate.CurrentSeason.ToString();
        }

        private bool HasRequiredTool(ToolType toolType)
        {
            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager
            // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            var equipmentManager = EquipmentManager.Instance;
            if (equipmentManager == null)
            {
                return false;
            }

            return equipmentManager.HasTool(toolType, ToolTier.Basic);
        }

        private void EnsureRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null && _baseSprite == null)
            {
                _baseSprite = _spriteRenderer.sprite;
            }
        }

        private Vector2Int GetTilePosition()
        {
            return Vector2Int.RoundToInt(transform.position);
        }

        private Vector2 GetMenuScreenPosition()
        {
            var worldPosition = transform.position + Vector3.up * 0.8f;
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            var screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        }

        private void PublishFeedback(string message)
        {
            var msg = message ?? string.Empty;
            _menuController?.SetFeedback(msg);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(msg));
            }
        }

        private SeedParams ResolveSeedParams()
        {
            if (!TryGetPlantedSeedData(out var seedData))
            {
                return new SeedParams { IsValid = false };
            }

            return BuildSeedParams(seedData);
        }

        private static SeedParams BuildSeedParams(Data.SeedDataSO seedData)
        {
            if (seedData == null)
            {
                return new SeedParams { IsValid = false };
            }

            var pairCount = seedData.HarvestItems != null && seedData.HarvestAmounts != null
                ? System.Math.Min(seedData.HarvestItems.Length, seedData.HarvestAmounts.Length)
                : 0;

            var pairs = new (string itemId, int baseAmount)[pairCount];
            for (var i = 0; i < pairCount; i++)
            {
                var item = seedData.HarvestItems[i];
                pairs[i] = (item != null ? item.Id : string.Empty, seedData.HarvestAmounts[i]);
            }

            return new SeedParams
            {
                SeedId = seedData.Id,
                GrowthDays = seedData.GrowthDays,
                RegrowDays = seedData.RegrowDays,
                HarvestPairs = pairs,
                IsValid = true
            };
        }
    }
}