using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Farm.Data;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Farm
{
    [DisallowMultipleComponent]
    public class FarmPlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _plotIndex;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private SeedDatabaseSO _seedDatabase;

        public FarmPlotState State { get; private set; }
        public string PlantedSeedId { get; private set; }
        public int DaysGrown { get; private set; }

        private int _currentDay = 1;

        public string InteractionPrompt
        {
            get
            {
                switch (State)
                {
                    case FarmPlotState.Empty:
                        return "Plantar";
                    case FarmPlotState.Growing:
                        return "Crescendo";
                    case FarmPlotState.Ready:
                        return "Colher";
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

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            SetState(State);
        }

        public void SetState(FarmPlotState state)
        {
            State = state;
            UpdateVisual();
        }

        public void ResetPlot()
        {
            PlantedSeedId = string.Empty;
            DaysGrown = 0;
            SetState(FarmPlotState.Empty);
        }

        public FarmPlotSaveData CaptureSaveData()
        {
            return new FarmPlotSaveData
            {
                PlotIndex = _plotIndex,
                State = State.ToString(),
                PlantedSeedId = PlantedSeedId,
                DaysGrown = DaysGrown
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
                ResetPlot();
                return;
            }

            PlantedSeedId = string.IsNullOrWhiteSpace(saveData.PlantedSeedId) ? string.Empty : saveData.PlantedSeedId;
            DaysGrown = Mathf.Max(0, saveData.DaysGrown);

            if (restoredState == FarmPlotState.Empty)
            {
                PlantedSeedId = string.Empty;
                DaysGrown = 0;
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

        public bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public void Interact(GameObject interactor)
        {
            switch (State)
            {
                case FarmPlotState.Empty:
                    TryPlantAvailableSeed();
                    break;
                case FarmPlotState.Growing:
                    Debug.Log($"FarmPlot {_plotIndex} is still growing. SeedId='{PlantedSeedId}', DaysGrown={DaysGrown}.", this);
                    break;
                case FarmPlotState.Ready:
                    TryHarvest();
                    break;
            }
        }

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateVisual();
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateVisual();
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            _currentDay = evt.DayNumber;

            if (State != FarmPlotState.Growing)
            {
                return;
            }

            DaysGrown++;

            if (!TryGetPlantedSeedData(out var seedData))
            {
                UpdateVisual();
                return;
            }

            if (DaysGrown >= seedData.GrowthDays)
            {
                SetState(FarmPlotState.Ready);
                GameEventBus.Publish(new CropReadyEvent(PlantedSeedId, GetTilePosition(), DaysGrown));
                Debug.Log($"FarmPlot {_plotIndex} crop '{PlantedSeedId}' is ready after {DaysGrown} day(s).", this);
                return;
            }

            UpdateVisual();
            Debug.Log($"FarmPlot {_plotIndex} crop '{PlantedSeedId}' grew to {DaysGrown}/{seedData.GrowthDays} day(s) on day {evt.DayNumber}.", this);
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null)
            {
                return;
            }

            switch (State)
            {
                case FarmPlotState.Empty:
                    _spriteRenderer.color = new Color(0.42f, 0.25f, 0.15f);
                    break;
                case FarmPlotState.Growing:
                    _spriteRenderer.color = new Color(0.22f, 0.55f, 0.22f);
                    break;
                case FarmPlotState.Ready:
                    _spriteRenderer.color = new Color(0.9f, 0.7f, 0.18f);
                    break;
            }
        }

        private void TryPlantAvailableSeed()
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot plant because InventoryManager is missing.", this);
                return;
            }

            if (_seedDatabase == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot plant because SeedDatabaseSO is missing.", this);
                return;
            }

            var seedId = GetSelectedHotbarSeedId();
            if (string.IsNullOrEmpty(seedId))
            {
                return;
            }

            if (!_seedDatabase.TryGetById(seedId, out var seedData) || seedData == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} could not resolve seed id '{seedId}' in SeedDatabaseSO.", this);
                return;
            }

            if (!_inventoryManager.HasItem(seedId))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Selected seed is not in inventory."));
                Debug.Log($"FarmPlot {_plotIndex} blocked planting because selected seed '{seedId}' is not in inventory.", this);
                return;
            }

            if (!_inventoryManager.RemoveItem(seedId, 1))
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} could not remove seed '{seedId}' from inventory.", this);
                return;
            }

            PlantedSeedId = seedId;
            DaysGrown = 0;
            SetState(FarmPlotState.Growing);

            GameEventBus.Publish(new SeedPlantedEvent(seedId, GetTilePosition(), _currentDay));
            Debug.Log($"FarmPlot {_plotIndex} planted seed '{seedId}'.", this);
        }

        private string GetSelectedHotbarSeedId()
        {
            var saveManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.SaveManager : null;
            var hotbarState = saveManager != null ? saveManager.HotbarState : null;
            if (hotbarState == null)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Select a seed in hotbar."));
                Debug.Log($"FarmPlot {_plotIndex} blocked planting because HotbarState is missing.", this);
                return string.Empty;
            }

            var selectedItemId = hotbarState.SelectedItemId;
            if (string.IsNullOrWhiteSpace(selectedItemId))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Select a seed in hotbar."));
                Debug.Log($"FarmPlot {_plotIndex} blocked planting because selected hotbar slot is empty.", this);
                return string.Empty;
            }

            if (!selectedItemId.StartsWith("seed_", System.StringComparison.Ordinal))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Selected hotbar item is not a seed."));
                Debug.Log($"FarmPlot {_plotIndex} blocked planting because selected hotbar item '{selectedItemId}' is not a seed.", this);
                return string.Empty;
            }

            return selectedItemId;
        }

        private Vector2Int GetTilePosition()
        {
            return Vector2Int.RoundToInt(transform.position);
        }

        private bool TryGetPlantedSeedData(out SeedDataSO seedData)
        {
            seedData = null;

            if (string.IsNullOrWhiteSpace(PlantedSeedId))
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} is growing without a planted seed id.", this);
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

        private void TryHarvest()
        {
            if (_inventoryManager == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest because InventoryManager is missing.", this);
                return;
            }

            if (!TryGetPlantedSeedData(out var seedData))
            {
                return;
            }

            if (seedData.HarvestItems == null || seedData.HarvestAmounts == null)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is missing.", this);
                return;
            }

            var pairCount = Mathf.Min(seedData.HarvestItems.Length, seedData.HarvestAmounts.Length);
            if (pairCount == 0)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} cannot harvest seed '{PlantedSeedId}' because harvest data is empty.", this);
                return;
            }

            if (seedData.HarvestItems.Length != seedData.HarvestAmounts.Length)
            {
                Debug.LogWarning($"FarmPlot {_plotIndex} harvest data length mismatch for seed '{PlantedSeedId}'. Harvesting {pairCount} valid pair(s).", this);
            }

            var harvestedAnyItem = false;
            var harvestedSeedId = PlantedSeedId;
            var tilePosition = GetTilePosition();

            for (var i = 0; i < pairCount; i++)
            {
                var harvestItem = seedData.HarvestItems[i];
                var amount = seedData.HarvestAmounts[i];

                if (harvestItem == null)
                {
                    Debug.LogWarning($"FarmPlot {_plotIndex} skipped null harvest item at index {i} for seed '{harvestedSeedId}'.", this);
                    continue;
                }

                if (amount <= 0)
                {
                    Debug.LogWarning($"FarmPlot {_plotIndex} skipped harvest item '{harvestItem.Id}' with invalid amount {amount}.", this);
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
                return;
            }

            ResetPlot();
        }
    }
}
