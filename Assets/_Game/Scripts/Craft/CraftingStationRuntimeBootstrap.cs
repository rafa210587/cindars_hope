using System.Collections;
using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Inventory;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Craft
{
    /// <summary>
    /// Runtime bootstrap for CraftingRuntime binding in scenes.
    ///
    /// Resolves CraftingRuntime from the active scene and binds it to
    /// InventoryManager and StaminaManager from GameBootstrap.
    ///
    /// Pattern: identical to PlayerMovementActionRuntimeBootstrap
    /// (MaxBindAttempts loop, DontDestroyOnLoad singleton, sceneLoaded rebind).
    ///
    /// Does NOT use GameObject.Find or any global scene search at runtime.
    /// Uses CraftingRuntime.ActiveInstances (static registry) — no FindObjectsOfType,
    /// no FindObjectsByType, no FindAnyObjectByType at any point.
    ///
    /// WAVE_INTEGRATION_14 — Crafting Station + Processing Jobs
    /// </summary>
    public sealed class CraftingStationRuntimeBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 120;
        private static CraftingStationRuntimeBootstrap _instance;

        public static CraftingStationRuntimeBootstrap Install(Transform owner)
        {
            if (_instance != null) return _instance;

            var go = new GameObject("CraftingStationRuntimeBootstrap");
            if (owner != null) go.transform.SetParent(owner, false);
            else DontDestroyOnLoad(go);
            _instance = go.AddComponent<CraftingStationRuntimeBootstrap>();
            return _instance;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            StartCoroutine(BindWhenReady());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(BindWhenReady());
        }

        private IEnumerator BindWhenReady()
        {
            for (var attempt = 0; attempt < MaxBindAttempts; attempt++)
            {
                var bootstrap = GameBootstrap.Instance;
                if (bootstrap == null)
                {
                    yield return null;
                    continue;
                }

                // Find CraftingRuntime instances via the static ActiveInstances registry.
                // CraftingRuntime registers itself in OnEnable and unregisters in OnDisable.
                // This avoids all global scene searches (no FindObjectsOfType / FindObjectsByType).
                var runtimes = CraftingRuntime.ActiveInstances;

                if (runtimes == null || runtimes.Count == 0)
                {
                    yield return null;
                    continue;
                }

                BindRuntimes(runtimes, bootstrap.InventoryManager, bootstrap.StaminaManager);
                yield break;
            }

            // No CraftingRuntime found in scene — not an error if scene has no crafting stations.
            Debug.Log("[CraftingStationRuntimeBootstrap] No CraftingRuntime found in scene. Crafting stations will not be active.");
        }

        private static void BindRuntimes(List<CraftingRuntime> runtimes, InventoryManager inventoryManager, StaminaManager staminaManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("[CraftingStationRuntimeBootstrap] InventoryManager is null from GameBootstrap. CraftingRuntime cannot be initialized.");
                return;
            }

            foreach (var runtime in runtimes)
            {
                if (runtime == null) continue;

                runtime.RebindInventoryManager(inventoryManager);

                if (staminaManager != null)
                    runtime.RebindStaminaManager(staminaManager);

                if (!runtime.IsInitialized)
                    runtime.Initialize();

                Debug.Log($"[CraftingStationRuntimeBootstrap] Bound CraftingRuntime '{runtime.name}' to InventoryManager and StaminaManager.");
            }
        }
    }
}
