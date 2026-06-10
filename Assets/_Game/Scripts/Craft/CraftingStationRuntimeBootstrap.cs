using System.Collections;
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
    /// Does NOT use GameObject.Find or deprecated FindObjectOfType at runtime.
    /// Uses Object.FindObjectsByType&lt;CraftingRuntime&gt;() once per scene load,
    /// in the coroutine bind loop — not in gameplay Update.
    ///
    /// WAVE_INTEGRATION_14 — Crafting Station + Processing Jobs
    /// </summary>
    public sealed class CraftingStationRuntimeBootstrap : MonoBehaviour
    {
        private const int MaxBindAttempts = 120;
        private static CraftingStationRuntimeBootstrap _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null) return;

            var go = new GameObject("CraftingStationRuntimeBootstrap");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<CraftingStationRuntimeBootstrap>();
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

                // Find CraftingRuntime instances in the active scene.
                // This is allowed in a bootstrap setup loop — not in gameplay Update.
                // Use FindObjectsByType (non-deprecated Unity 2023.1+ API, active-only).
#if UNITY_2023_1_OR_NEWER
                var runtimes = Object.FindObjectsByType<CraftingRuntime>(FindObjectsInactive.Exclude);
#else
                var runtimes = Object.FindObjectsOfType<CraftingRuntime>();
#endif

                if (runtimes == null || runtimes.Length == 0)
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

        private static void BindRuntimes(CraftingRuntime[] runtimes, InventoryManager inventoryManager, StaminaManager staminaManager)
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
