using System;
using System.Collections.Generic;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Craft
{
    [DisallowMultipleComponent]
    public sealed class CraftingRuntime : MonoBehaviour
    {
        private const string PocketStationId = "player_pocket";

        /// <summary>
        /// Static registry of active CraftingRuntime instances.
        /// Populated via OnEnable/OnDisable — avoids global scene searches.
        /// CraftingStationRuntimeBootstrap reads from this list instead of FindObjectsOfType.
        /// </summary>
        public static readonly List<CraftingRuntime> ActiveInstances = new List<CraftingRuntime>();

        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private RecipeDatabaseSO _recipeDatabase;
        [SerializeField] private StaminaManager _staminaManager;

        private readonly Dictionary<string, CraftingStation> _stations = new Dictionary<string, CraftingStation>();

        public bool IsInitialized { get; private set; }
        public RecipeDatabaseSO RecipeDatabase => _recipeDatabase;
        public InventoryManager InventoryManager => _inventoryManager;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!ActiveInstances.Contains(this))
                ActiveInstances.Add(this);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (_inventoryManager == null || _recipeDatabase == null)
            {
                Debug.LogError("CraftingRuntime requires InventoryManager and RecipeDatabaseSO.", this);
                return;
            }

            IsInitialized = true;
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
            Initialize();
        }

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        public CraftingStation GetOrCreateStation(string stationInstanceId, WorkshopType stationType)
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(stationInstanceId))
            {
                return null;
            }

            if (_stations.TryGetValue(stationInstanceId, out var station))
            {
                return station;
            }

            station = new CraftingStation(stationInstanceId, stationType);
            _stations.Add(stationInstanceId, station);
            return station;
        }

        public CraftingStation GetPocketStation()
        {
            return GetOrCreateStation(PocketStationId, WorkshopType.None);
        }

        public List<RecipeDataSO> GetRecipesForStation(WorkshopType stationType)
        {
            var recipes = new List<RecipeDataSO>();
            if (_recipeDatabase == null)
            {
                return recipes;
            }

            foreach (var recipe in _recipeDatabase.All)
            {
                if (recipe != null && recipe.IsUnlockedByDefault && recipe.RequiredStationType == stationType)
                {
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }

        public bool TryStartCraft(CraftingStation station, RecipeDataSO recipe, out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            return station.TryStartCraft(recipe, _inventoryManager, out failureReason, _staminaManager);
        }

        public bool TryCollect(CraftingStation station, out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            return station.TryCollectOutput(_inventoryManager, out failureReason);
        }

        public bool TryCancel(CraftingStation station, out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            return station.TryCancelJob(_inventoryManager, out failureReason);
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            foreach (var station in _stations.Values)
            {
                station.Update(Time.deltaTime);
            }
        }

        public CraftingRuntimeSaveData CaptureSaveData()
        {
            var data = new CraftingRuntimeSaveData();
            foreach (var station in _stations.Values)
            {
                data.Stations.Add(station.CaptureSaveData());
            }

            return data;
        }

        public void LoadFromSaveData(CraftingRuntimeSaveData saveData)
        {
            if (!IsInitialized || saveData?.Stations == null)
            {
                return;
            }

            foreach (var stationData in saveData.Stations)
            {
                var station = GetOrCreateStation(stationData.StationInstanceId, (WorkshopType)stationData.StationType);
                station?.LoadFromSaveData(stationData, _recipeDatabase);
            }
        }
    }

    [Serializable]
    public class CraftingRuntimeSaveData
    {
        public List<CraftingStationSaveData> Stations = new List<CraftingStationSaveData>();
    }
}
