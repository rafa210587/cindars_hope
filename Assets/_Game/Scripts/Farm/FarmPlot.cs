using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
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
    public class FarmPlot : MonoBehaviour, IInteractable
    {
        private enum FarmMenuActionType
        {
            Till,
            Water,
            Plant,
            Harvest,
            Status
        }

        private readonly struct FarmMenuAction
        {
            public readonly FarmMenuActionType Type;
            public readonly string Label;
            public readonly string SeedId;

            public FarmMenuAction(FarmMenuActionType type, string label, string seedId = "")
            {
                Type = type;
                Label = label;
                SeedId = seedId ?? string.Empty;
            }
        }

        private static FarmPlot _activeMenuPlot;

        [SerializeField] private int _plotIndex;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SeedDatabaseSO _seedDatabase;
        [SerializeField] private Player.StaminaManager _staminaManager;

        private readonly List<FarmMenuAction> _menuActions = new List<FarmMenuAction>();
        private Sprite _baseSprite;
        private int _selectedMenuIndex;
        private int _currentDay = 1;
        private int _menuOpenedFrame = -1;
        private string _feedback = string.Empty;

        public static bool IsAnyActionMenuOpen => _activeMenuPlot != null;

        public FarmPlotState State { get; private set; }
        public string PlantedSeedId { get; private set; }
        public int DaysGrown { get; private set; }
        public bool IsWatered => State == FarmPlotState.TilledWet || State == FarmPlotState.PlantedWet;
        public int RegrowRemainingDays { get; private set; }

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
            SetState(State == FarmPlotState.Blocked ? FarmPlotState.Blocked : NormalizeState(State));
        }

        public void SetState(FarmPlotState state)
        {
            State = NormalizeState(state);
            if (State == FarmPlotState.Raw || State == FarmPlotState.TilledDry || State == FarmPlotState.TilledWet || State == FarmPlotState.Blocked)
            {
                PlantedSeedId = string.Empty;
                DaysGrown = 0;
                RegrowRemainingDays = 0;
            }

            UpdateVisual();
        }

        public void ResetPlot()
        {
            PlantedSeedId = string.Empty;
            DaysGrown = 0;
            RegrowRemainingDays = 0;
            SetState(FarmPlotState.TilledDry);
        }

        public FarmPlotSaveData CaptureSaveData()
        {
            return new FarmPlotSaveData
            {
                PlotIndex = _plotIndex,
                State = State.ToString(),
                PlantedSeedId = PlantedSeedId,
                DaysGrown = DaysGrown,
                GrowthProgressDays = DaysGrown,
                IsWatered = IsWatered,
                RegrowRemainingDays = RegrowRemainingDays,
                LastUpdatedDay = _currentDay
            };
        }

        public void RestoreFromSaveData(FarmPlotSaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot restore from null save data.", this);
                return;
            }

            if (!System.Enum.TryParse(saveData.State, out FarmPlotState restoredState))
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} received invalid saved state '{saveData.State}'. Resetting plot.", this);
                SetState(FarmPlotState.Raw);
                return;
            }

            restoredState = NormalizeState(restoredState);
            PlantedSeedId = string.IsNullOrWhiteSpace(saveData.PlantedSeedId) ? string.Empty : saveData.PlantedSeedId;
            DaysGrown = Mathf.Max(0, saveData.GrowthProgressDays > 0 ? saveData.GrowthProgressDays : saveData.DaysGrown);
            RegrowRemainingDays = Mathf.Max(0, saveData.RegrowRemainingDays);
            _currentDay = Mathf.Max(1, saveData.LastUpdatedDay);

            if (!IsPlantedState(restoredState) && restoredState != FarmPlotState.ReadyToHarvest)
            {
                PlantedSeedId = string.Empty;
                DaysGrown = 0;
                RegrowRemainingDays = 0;
            }
            else if (!TryGetPlantedSeedData(out _))
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} could not resolve saved seed '{PlantedSeedId}'. Resetting to tilled dry.", this);
                PlantedSeedId = string.Empty;
                DaysGrown = 0;
                RegrowRemainingDays = 0;
                restoredState = FarmPlotState.TilledDry;
            }

            SetState(restoredState);
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

        public bool CanInteract(GameObject interactor)
        {
            return _activeMenuPlot == null || _activeMenuPlot == this;
        }

        public void Interact(GameObject interactor)
        {
            if (_activeMenuPlot == this)
            {
                CloseMenu();
                return;
            }

            OpenMenu();
        }

        private void Reset()
        {
            EnsureRenderer();
        }

        private void Awake()
        {
            EnsureRenderer();
            State = NormalizeState(State);
            UpdateVisual();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            if (_activeMenuPlot == this)
            {
                _activeMenuPlot = null;
            }
        }

        private void OnValidate()
        {
            EnsureRenderer();
            UpdateVisual();
        }

        private void Update()
        {
            if (_activeMenuPlot != this)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                _selectedMenuIndex = Mathf.Max(0, _selectedMenuIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                _selectedMenuIndex = Mathf.Min(_menuActions.Count - 1, _selectedMenuIndex + 1);
            }
            else if (Time.frameCount != _menuOpenedFrame &&
                     (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
            {
                ExecuteSelectedMenuAction();
            }
        }

        private void OnGUI()
        {
            if (_activeMenuPlot != this)
            {
                return;
            }

            var screenPosition = GetMenuScreenPosition();
            var width = 260f;
            var height = Mathf.Clamp(70f + _menuActions.Count * 26f, 90f, 260f);
            var rect = new Rect(screenPosition.x - width * 0.5f, screenPosition.y - height, width, height);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label($"Plot {_plotIndex}: {State}");

            if (_menuActions.Count == 0)
            {
                GUILayout.Label(string.IsNullOrWhiteSpace(_feedback) ? "Sem acao disponivel." : _feedback);
            }
            else
            {
                for (var index = 0; index < _menuActions.Count; index++)
                {
                    GUILayout.Label(index == _selectedMenuIndex ? $"> {_menuActions[index].Label}" : $"  {_menuActions[index].Label}");
                }
            }

            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GUILayout.Space(4f);
                GUILayout.Label(_feedback);
            }

            GUILayout.EndArea();
        }

        private void OpenMenu()
        {
            BuildMenuActions();
            if (_menuActions.Count == 0)
            {
                PublishFeedback(string.IsNullOrWhiteSpace(_feedback) ? "No farm action available." : _feedback);
                return;
            }

            _activeMenuPlot = this;
            _selectedMenuIndex = 0;
            _menuOpenedFrame = Time.frameCount;
        }

        private void CloseMenu()
        {
            if (_activeMenuPlot == this)
            {
                _activeMenuPlot = null;
            }

            _menuActions.Clear();
            _selectedMenuIndex = 0;
        }

        private void BuildMenuActions()
        {
            _menuActions.Clear();
            _feedback = string.Empty;

            switch (State)
            {
                case FarmPlotState.Raw:
                    if (HasRequiredTool(ToolType.Hoe))
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Till, "Arar solo"));
                    }
                    else
                    {
                        _feedback = "Hoe required.";
                    }

                    break;
                case FarmPlotState.TilledDry:
                    if (HasRequiredTool(ToolType.WateringCan))
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Water, "Molhar solo"));
                    }

                    AddPlantActions();
                    break;
                case FarmPlotState.TilledWet:
                    AddPlantActions();
                    break;
                case FarmPlotState.PlantedDry:
                    if (HasRequiredTool(ToolType.WateringCan))
                    {
                        _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Water, "Molhar solo"));
                    }
                    else
                    {
                        _feedback = "Watering Can required.";
                    }

                    break;
                case FarmPlotState.PlantedWet:
                    _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Status, "Ja irrigado"));
                    break;
                case FarmPlotState.ReadyToHarvest:
                    _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Harvest, "Colher"));
                    break;
                case FarmPlotState.Blocked:
                    _feedback = "Plot blocked.";
                    break;
            }
        }

        private void AddPlantActions()
        {
            if (_inventoryManager == null || _seedDatabase == null)
            {
                _feedback = "Inventory or seed database missing.";
                return;
            }

            foreach (var item in _inventoryManager.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Key) || item.Value <= 0)
                {
                    continue;
                }

                if (!_seedDatabase.TryGetById(item.Key, out var seedData) || seedData == null)
                {
                    continue;
                }

                var label = seedData.SeedItem != null && !string.IsNullOrWhiteSpace(seedData.SeedItem.DisplayName)
                    ? $"Plantar {seedData.SeedItem.DisplayName}"
                    : $"Plantar {item.Key}";
                _menuActions.Add(new FarmMenuAction(FarmMenuActionType.Plant, label, item.Key));
            }

            if (_menuActions.Count == 0)
            {
                _feedback = "No seeds in inventory.";
            }
        }

        private void ExecuteSelectedMenuAction()
        {
            if (_selectedMenuIndex < 0 || _selectedMenuIndex >= _menuActions.Count)
            {
                return;
            }

            var action = _menuActions[_selectedMenuIndex];
            var closeAfterAction = true;
            switch (action.Type)
            {
                case FarmMenuActionType.Till:
                    TryTill();
                    break;
                case FarmMenuActionType.Water:
                    TryWater();
                    break;
                case FarmMenuActionType.Plant:
                    TryPlantSeed(action.SeedId);
                    break;
                case FarmMenuActionType.Harvest:
                    TryHarvest();
                    break;
                case FarmMenuActionType.Status:
                    PublishFeedback("Plot already watered.");
                    break;
            }

            if (closeAfterAction)
            {
                CloseMenu();
            }
        }

        private bool TryTill()
        {
            const int tillStaminaCost = 16;

            if (State != FarmPlotState.Raw || !HasRequiredTool(ToolType.Hoe))
            {
                PublishFeedback("Cannot till this plot.");
                return false;
            }

            if (!ValidateStamina(tillStaminaCost))
            {
                PublishFeedback("Not enough stamina to till.");
                return false;
            }

            if (!TrySpendStamina(tillStaminaCost))
            {
                PublishFeedback("Not enough stamina to till.");
                return false;
            }

            SetState(FarmPlotState.TilledDry);
            PublishFeedback("Soil tilled.");
            return true;
        }

        private bool TryWater()
        {
            const int waterStaminaCost = 8;

            if (!HasRequiredTool(ToolType.WateringCan))
            {
                PublishFeedback("Watering Can required.");
                return false;
            }

            if (!ValidateStamina(waterStaminaCost))
            {
                PublishFeedback("Not enough stamina to water.");
                return false;
            }

            if (State == FarmPlotState.TilledDry)
            {
                if (!TrySpendStamina(waterStaminaCost))
                {
                    PublishFeedback("Not enough stamina to water.");
                    return false;
                }
                SetState(FarmPlotState.TilledWet);
                PublishFeedback("Soil watered.");
                return true;
            }

            if (State == FarmPlotState.PlantedDry)
            {
                if (!TrySpendStamina(waterStaminaCost))
                {
                    PublishFeedback("Not enough stamina to water.");
                    return false;
                }
                SetState(FarmPlotState.PlantedWet);
                PublishFeedback("Crop watered.");
                return true;
            }

            PublishFeedback("Cannot water this plot.");
            return false;
        }

        private bool TryPlantSeed(string seedId)
        {
            const int plantStaminaCost = 4;

            if (State != FarmPlotState.TilledDry && State != FarmPlotState.TilledWet)
            {
                PublishFeedback("Plot is not plantable.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(seedId) || _inventoryManager == null || _seedDatabase == null)
            {
                PublishFeedback("Seed data unavailable.");
                return false;
            }

            if (!_seedDatabase.TryGetById(seedId, out var seedData) || seedData == null)
            {
                PublishFeedback("Seed not registered.");
                return false;
            }

            if (!_inventoryManager.HasItem(seedId))
            {
                PublishFeedback("Seed not in inventory.");
                return false;
            }

            if (!ValidateStamina(plantStaminaCost))
            {
                PublishFeedback("Not enough stamina to plant.");
                return false;
            }

            if (!_inventoryManager.RemoveItem(seedId, 1))
            {
                PublishFeedback("Could not consume seed.");
                return false;
            }

            if (!TrySpendStamina(plantStaminaCost))
            {
                _inventoryManager.AddItem(seedId, 1);
                PublishFeedback("Not enough stamina to plant.");
                return false;
            }

            PlantedSeedId = seedId;
            DaysGrown = 0;
            RegrowRemainingDays = 0;
            SetState(State == FarmPlotState.TilledWet ? FarmPlotState.PlantedWet : FarmPlotState.PlantedDry);

            GameEventBus.Publish(new SeedPlantedEvent(seedId, GetTilePosition(), _currentDay));
            Debug.Log($"FarmPlot {_plotIndex} planted seed '{seedId}'.", this);
            return true;
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;

            if (State == FarmPlotState.PlantedWet)
            {
                AdvanceGrowth();
                if (State == FarmPlotState.PlantedWet)
                {
                    SetState(FarmPlotState.PlantedDry);
                }
            }
            else if (State == FarmPlotState.TilledWet)
            {
                SetState(FarmPlotState.TilledDry);
            }
        }

        private void AdvanceGrowth()
        {
            if (!TryGetPlantedSeedData(out var seedData))
            {
                UpdateVisual();
                return;
            }

            DaysGrown++;
            if (DaysGrown >= seedData.GrowthDays)
            {
                SetState(FarmPlotState.ReadyToHarvest);
                GameEventBus.Publish(new CropReadyEvent(PlantedSeedId, GetTilePosition(), DaysGrown));
                Debug.Log($"FarmPlot {_plotIndex} crop '{PlantedSeedId}' is ready after {DaysGrown} watered day(s).", this);
                return;
            }

            UpdateVisual();
            Debug.Log($"FarmPlot {_plotIndex} crop '{PlantedSeedId}' grew to {DaysGrown}/{seedData.GrowthDays} on day {DaysGrown}.", this);
        }

        private bool TryHarvest()
        {
            const int harvestStaminaCost = 4;

            if (State != FarmPlotState.ReadyToHarvest)
            {
                PublishFeedback("Crop is not ready.");
                return false;
            }

            if (_inventoryManager == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest because InventoryManager is missing.", this);
                return false;
            }

            if (!ValidateStamina(harvestStaminaCost))
            {
                PublishFeedback("Not enough stamina to harvest.");
                return false;
            }

            if (!TryGetPlantedSeedData(out var seedData))
            {
                return false;
            }

            if (seedData.HarvestItems == null || seedData.HarvestAmounts == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is missing.", this);
                return false;
            }

            var pairCount = Mathf.Min(seedData.HarvestItems.Length, seedData.HarvestAmounts.Length);
            if (pairCount == 0)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is empty.", this);
                return false;
            }

            if (!TrySpendStamina(harvestStaminaCost))
            {
                PublishFeedback("Not enough stamina to harvest.");
                return false;
            }

            var harvestedAnyItem = false;
            var harvestedSeedId = PlantedSeedId;
            var tilePosition = GetTilePosition();

            for (var i = 0; i < pairCount; i++)
            {
                var harvestItem = seedData.HarvestItems[i];
                var amount = seedData.HarvestAmounts[i];

                if (harvestItem == null || amount <= 0)
                {
                    continue;
                }

                if (!_inventoryManager.AddItem(harvestItem.Id, amount))
                {
                    Debug.LogWarning($"FarmPlot {_plotIndex} could not add harvest item '{harvestItem.Id}' x{amount} to inventory.", this);
                    continue;
                }

                harvestedAnyItem = true;
                GameEventBus.Publish(new CropHarvestedEvent(harvestedSeedId, harvestItem.Id, amount, tilePosition));
                Debug.Log($"FarmPlot {_plotIndex} harvested '{harvestItem.Id}' x{amount} from seed '{harvestedSeedId}'.", this);
            }

            if (!harvestedAnyItem)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} harvest produced no items and plot will remain ready.", this);
                return false;
            }

            if (seedData.RegrowDays > 0)
            {
                DaysGrown = Mathf.Max(0, seedData.GrowthDays - seedData.RegrowDays);
                RegrowRemainingDays = seedData.RegrowDays;
                SetState(FarmPlotState.PlantedDry);
            }
            else
            {
                ResetPlot();
            }

            return true;
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
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
            if (!IsPlantedState(State) && State != FarmPlotState.ReadyToHarvest)
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

        private bool TryGetPlantedSeedData(out SeedDataSO seedData)
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

        private bool HasRequiredTool(ToolType toolType)
        {
            var equipmentManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.EquipmentManager : null;
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
            _feedback = message ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_feedback))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(_feedback));
            }
        }

        private static bool IsPlantedState(FarmPlotState state)
        {
            return state == FarmPlotState.PlantedDry || state == FarmPlotState.PlantedWet;
        }

        private static FarmPlotState NormalizeState(FarmPlotState state)
        {
            switch (state)
            {
                case FarmPlotState.Blocked:
                case FarmPlotState.Raw:
                case FarmPlotState.TilledDry:
                case FarmPlotState.TilledWet:
                case FarmPlotState.PlantedDry:
                case FarmPlotState.PlantedWet:
                case FarmPlotState.ReadyToHarvest:
                case FarmPlotState.Dead:
                    return state;
                default:
                    return FarmPlotState.Raw;
            }
        }

        private bool ValidateStamina(int requiredStamina)
        {
            if (_staminaManager == null)
                return true;

            return _staminaManager.CurrentStamina >= requiredStamina;
        }

        private bool TrySpendStamina(int requiredStamina)
        {
            if (_staminaManager == null)
                return true;

            return _staminaManager.TrySpendStamina(requiredStamina);
        }
    }
}
