using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Craft
{
    public class CraftingRuntime : MonoBehaviour
    {
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        [SerializeField] private RecipeDatabaseSO _recipeDatabase;

        private Dictionary<string, CraftingStation> _stations = new();
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_itemDatabase == null || _recipeDatabase == null)
            {
                Debug.LogError("CraftingRuntime: Missing database references");
                return;
            }

            _stations.Clear();
            _isInitialized = true;
        }

        public CraftingStation GetOrCreateStation(string stationInstanceId, WorkshopType stationType)
        {
            if (!_isInitialized)
            {
                Debug.LogError("CraftingRuntime not initialized");
                return null;
            }

            if (_stations.TryGetValue(stationInstanceId, out var station))
                return station;

            station = new CraftingStation(stationInstanceId, stationType, _itemDatabase, _recipeDatabase);
            _stations[stationInstanceId] = station;
            return station;
        }

        public CraftingStation GetStation(string stationInstanceId)
        {
            _stations.TryGetValue(stationInstanceId, out var station);
            return station;
        }

        public bool TryGetStation(string stationInstanceId, out CraftingStation station)
        {
            return _stations.TryGetValue(stationInstanceId, out station);
        }

        public void RemoveStation(string stationInstanceId)
        {
            _stations.Remove(stationInstanceId);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            foreach (var station in _stations.Values)
            {
                station.Update(Time.deltaTime);
            }
        }

        public CraftingRuntimeSaveData CaptureSaveData()
        {
            var data = new CraftingRuntimeSaveData();
            foreach (var kvp in _stations)
            {
                data.Stations.Add(kvp.Value.CaptureSaveData());
            }
            return data;
        }

        public void LoadFromSaveData(CraftingRuntimeSaveData saveData)
        {
            if (saveData?.Stations == null)
                return;

            foreach (var stationData in saveData.Stations)
            {
                var station = GetOrCreateStation(stationData.StationInstanceId, (WorkshopType)stationData.StationType);
                station.LoadFromSaveData(stationData, _recipeDatabase);
            }
        }
    }

    [System.Serializable]
    public class CraftingRuntimeSaveData
    {
        public List<CraftingStationSaveData> Stations = new();
    }
}
