using System.Collections.Generic;
using CindarsHope.Farm;
using CindarsHope.Save;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.SceneManagement
{
    public static class FarmSceneRuntimeStateCache
    {
        private static bool _hasCachedState;
        private static FarmSaveData _cachedFarmState;
        private static WorldSaveData _cachedWorldState;

        public static bool HasCachedState => _hasCachedState;

        public static void Capture(FarmPlotRegistry farmPlotRegistry, TreeRegistry treeRegistry, ItemPickupRegistry itemPickupRegistry)
        {
            if (farmPlotRegistry == null)
            {
                Debug.LogWarning("FarmSceneRuntimeStateCache.Capture: farmPlotRegistry is null, skipping capture.");
            }

            _cachedFarmState = farmPlotRegistry != null ? farmPlotRegistry.CaptureSaveData() : new FarmSaveData();

            _cachedWorldState = new WorldSaveData();
            if (treeRegistry != null)
            {
                _cachedWorldState.Trees = treeRegistry.CaptureSaveData();
            }

            if (itemPickupRegistry != null)
            {
                _cachedWorldState.Pickups = itemPickupRegistry.CaptureSaveData();
            }

            _hasCachedState = true;
            Debug.Log($"FarmSceneRuntimeStateCache captured state: {_cachedFarmState.Plots.Count} plots, {_cachedWorldState.Trees.Count} trees, {_cachedWorldState.Pickups.Count} pickups.");
        }

        public static bool TryRestore(FarmPlotRegistry farmPlotRegistry, TreeRegistry treeRegistry, ItemPickupRegistry itemPickupRegistry)
        {
            if (!_hasCachedState)
            {
                return false;
            }

            if (farmPlotRegistry != null)
            {
                farmPlotRegistry.RestoreFromSaveData(_cachedFarmState);
            }

            if (treeRegistry != null)
            {
                treeRegistry.RestoreFromSaveData(_cachedWorldState?.Trees ?? new List<TreeSaveData>());
            }

            if (itemPickupRegistry != null)
            {
                itemPickupRegistry.RestoreFromSaveData(_cachedWorldState?.Pickups);
            }

            Debug.Log($"FarmSceneRuntimeStateCache restored state: {_cachedFarmState?.Plots?.Count ?? 0} plots, {_cachedWorldState?.Trees?.Count ?? 0} trees, {_cachedWorldState?.Pickups?.Count ?? 0} pickups.");

            return true;
        }

        public static void Clear()
        {
            _hasCachedState = false;
            _cachedFarmState = null;
            _cachedWorldState = null;
        }
    }
}
