using System.Collections;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// WAVE_INTEGRATION_16 — Cave Runtime Bridge.
    ///
    /// Thin bridge that connects the WAVE_INTEGRATION_13 scene transition system
    /// to the existing cave runtime (CaveRunManager / CaveLevelRuntimeController).
    ///
    /// Responsibilities:
    ///   1. RuntimeInitializeOnLoadMethod singleton (DontDestroyOnLoad).
    ///   2. Listen for SceneTransitionStartedEvent with source="CaveScene" to
    ///      publish CaveExitedEvent before the cave scene unloads.
    ///   3. Listen for CaveLevelEnteredEvent to log that the cave run is active.
    ///   4. Log a warning if CaveScene loads but no CaveRunManager is found on
    ///      the CaveRuntime object (wiring issue diagnostic).
    ///
    /// Does NOT create a parallel CaveRunManager.
    /// Does NOT touch combat, loot, or enemy AI (WAVE_INTEGRATION_17 scope).
    /// Does NOT alter CaveRunSeed (ADR-0005 / cave_rules.md compliant).
    ///
    /// Pattern: identical to QuestRuntimeBootstrap (WAVE_INTEGRATION_15) and
    ///          CraftingStationRuntimeBootstrap (WAVE_INTEGRATION_14).
    /// </summary>
    public sealed class CaveRuntimeBridge : MonoBehaviour
    {
        private const string GameObjectName = "CaveRuntimeBridge";
        private const int MaxBindAttempts = 120;

        private static CaveRuntimeBridge _instance;

        /// <summary>
        /// True after the bridge has found and validated the CaveRunManager in
        /// the current CaveScene load. Reset on each scene transition.
        /// </summary>
        public static bool IsCaveRuntimeValidated { get; private set; }

        public static void Install(Transform owner)
        {
            if (_instance != null) return;

            var go = new GameObject(GameObjectName);
            go.transform.SetParent(owner);
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<CaveRuntimeBridge>();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
        }

        // ------------------------------------------------------------------ //
        // Event handlers
        // ------------------------------------------------------------------ //

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt)
        {
            // Player is leaving CaveScene — publish CaveExitedEvent for surface return.
            if (evt.SourceSceneName == "CaveScene")
            {
                // Only fire for surface exits (not CaveScene → CaveScene level transitions).
                if (evt.TargetSceneName != "CaveScene")
                {
                    PublishCaveExitedEvent(evt.TargetSceneName, evt.TargetSpawnId);
                }

                // Reset validation state; will re-validate if player re-enters cave.
                IsCaveRuntimeValidated = false;
            }
        }

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            IsCaveRuntimeValidated = true;
            Debug.Log(
                $"[CaveRuntimeBridge] Cave level {evt.CaveLevel} entered. " +
                $"RunSeed={evt.CaveRunSeed} BiomeId={evt.BiomeId}. " +
                $"Cave runtime validated.",
                this);
        }

        // ------------------------------------------------------------------ //
        // Internal helpers
        // ------------------------------------------------------------------ //

        private void PublishCaveExitedEvent(string returnScene, string returnSpawnId)
        {
            // Read cave run state from CaveRunManager.Instance (static singleton).
            // Avoids all global scene searches — no FindAnyObjectByType / FindObjectOfType.
            var runManager = CaveRunManager.Instance;
            if (runManager == null)
            {
                Debug.LogWarning(
                    "[CaveRuntimeBridge] CaveRunManager.Instance is null when publishing CaveExitedEvent. " +
                    "Cave run state will be empty. Check that CaveScene has a CaveRunManager component.",
                    this);
            }

            var caveRunSeed = runManager != null ? runManager.CaveRunSeed : string.Empty;
            var caveLevel = runManager != null ? runManager.CurrentCaveLevel : 0;

            var evt = new CaveExitedEvent(
                caveRunSeed: caveRunSeed,
                exitedFromLevel: caveLevel,
                returnScene: returnScene,
                returnSpawnId: returnSpawnId);

            GameEventBus.Publish(evt);

            Debug.Log(
                $"[CaveRuntimeBridge] Player exiting cave to surface. " +
                $"RunSeed={caveRunSeed} Level={caveLevel} → {returnScene} @{returnSpawnId}",
                this);
        }

        // ------------------------------------------------------------------ //
        // Cave scene entry validation (diagnostic only, no game logic)
        // ------------------------------------------------------------------ //

        private IEnumerator ValidateCaveEntryState()
        {
            // Wait one frame for CaveScene's Awake/Start to complete.
            yield return null;

            // Use CaveRunManager.Instance (static singleton) — no global scene search.
            var runManager = CaveRunManager.Instance;
            if (runManager == null)
            {
                Debug.LogWarning(
                    "[CaveRuntimeBridge] CaveScene loaded but no CaveRunManager found. " +
                    "Check that the 'CaveRuntime' GameObject in CaveScene has a CaveRunManager component. " +
                    "WIRING_ISSUE: human Unity Editor action may be required.",
                    this);
                yield break;
            }

            Debug.Log(
                $"[CaveRuntimeBridge] CaveScene entry validated. " +
                $"CaveRunManager present. WorldSeed={runManager.CaveWorldSeed} " +
                $"RunSeed={runManager.CaveRunSeed} Level={runManager.CurrentCaveLevel}",
                this);
        }

        // Called from OnEnable when scene is CaveScene
        private void TryValidateCaveEntryIfNeeded()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "CaveScene")
            {
                IsCaveRuntimeValidated = false;
                StartCoroutine(ValidateCaveEntryState());
            }
        }

        private void Start()
        {
            TryValidateCaveEntryIfNeeded();

            // Subscribe to scene loaded events for future scene loads
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(
            UnityEngine.SceneManagement.Scene scene,
            UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "CaveScene")
            {
                IsCaveRuntimeValidated = false;
                StartCoroutine(ValidateCaveEntryState());
            }
        }
    }
}
