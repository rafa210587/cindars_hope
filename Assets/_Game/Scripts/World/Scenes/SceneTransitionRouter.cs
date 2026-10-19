using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// Static service that processes a SceneTransitionRequest.
    ///
    /// Responsibilities:
    ///   1. Validate the request.
    ///   2. Store the target spawn ID in SceneTransitionState (so
    ///      SceneSpawnInstaller / PlayerSpawnResolver can read it on load).
    ///   3. Publish SceneTransitionStartedEvent via GameEventBus.
    ///   4. Call SceneManager.LoadScene with the destination scene name.
    ///
    /// Does NOT create duplicate managers or use DontDestroyOnLoad.
    /// Fade overlay is provided by SceneFadeOverlayBootstrap (DontDestroyOnLoad, sortingOrder 9999).
    ///
    /// Transition guard: if a transition is already in progress this frame,
    /// subsequent calls are silently ignored.
    /// </summary>
    public static class SceneTransitionRouter
    {
        private static readonly ProfilerMarker ExecuteMarker =
            new ProfilerMarker("CindarsHope.SceneTransition.Execute");

        private static bool _transitionInProgress;
        private static ITownInteriorVisitAdmission _interiorAdmission;

        public static bool RegisterTownInteriorVisitAdmission(ITownInteriorVisitAdmission admission)
        {
            if (admission == null || (_interiorAdmission != null && !ReferenceEquals(_interiorAdmission, admission)))
                return false;
            _interiorAdmission = admission;
            return true;
        }

        public static void UnregisterTownInteriorVisitAdmission(ITownInteriorVisitAdmission admission)
        {
            if (ReferenceEquals(_interiorAdmission, admission)) _interiorAdmission = null;
        }

        // O guard precisa SEMPRE ser liberado quando a cena destino carrega. Antes so o
        // PlayerSpawnResolver/SceneSpawnInstaller (Farm/Town) chamavam ClearTransitionGuard; a
        // CaveScene usa spawn proprio e nunca limpava, deixando o guard preso 'true' -> toda
        // transicao seguinte falhava com "A transition is already in progress". Este hook estatico
        // (registrado no boot) zera o guard a cada carga de cena Single, valendo para qualquer destino.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InstallGuardReset()
        {
            _transitionInProgress = false; // reset defensivo (enter-playmode sem domain reload)
            _interiorAdmission = null;
            SceneManager.sceneLoaded -= OnSceneLoadedClearGuard;
            SceneManager.sceneLoaded += OnSceneLoadedClearGuard;
        }

        private static void OnSceneLoadedClearGuard(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                _transitionInProgress = false;
            }
        }

        /// <summary>
        /// Execute a scene transition from a SceneTransitionRequest.
        /// Returns a SceneTransitionResult indicating success or failure reason.
        /// </summary>
        public static SceneTransitionResult Execute(SceneTransitionRequest request)
        {
            using var profilerScope = ExecuteMarker.Auto();

            if (request == null)
            {
                Debug.LogError("[SceneTransitionRouter] Request is null.");
                return SceneTransitionResult.Failure("Request is null.");
            }

            if (!request.IsValid())
            {
                var reason = $"Invalid request: {request}";
                Debug.LogError($"[SceneTransitionRouter] {reason}");
                return SceneTransitionResult.Failure(reason);
            }

            if (_transitionInProgress)
            {
                var reason = "A transition is already in progress.";
                Debug.LogWarning($"[SceneTransitionRouter] {reason}");
                return SceneTransitionResult.Failure(reason);
            }

            if (request.Kind == SceneTransitionKind.TownInteriorVisit)
            {
                if (SceneManager.GetActiveScene().name != request.SourceSceneId || _interiorAdmission == null)
                    return SceneTransitionResult.Failure("Interior runtime is not ready in the active source scene.");
                if (!_interiorAdmission.CanBegin(request.InteriorVisit, out var reason))
                    return SceneTransitionResult.Failure(reason);
                GameEventBus.Publish(new TownInteriorVisitRequestedEvent(request.InteriorVisit));
                return SceneTransitionResult.Success; // Accepted, not visually completed.
            }

            _transitionInProgress = true;

            var destinationSceneName = request.DestinationSceneId;
            var sourceSceneName = SceneManager.GetActiveScene().name;

            // Store pending spawn so the destination scene can resolve it.
            SceneTransitionState.SetPendingSpawn(request.TargetSpawnAnchorId);

            // Publish transition started event for all subscribers
            // (HUD, cave manager, bestiary, etc.).
            GameEventBus.Publish(new SceneTransitionStartedEvent(
                sourceSceneName,
                destinationSceneName,
                request.TargetSpawnAnchorId));

            Debug.Log(
                $"[SceneTransitionRouter] Transitioning {sourceSceneName} → {destinationSceneName} " +
                $"@{request.TargetSpawnAnchorId} (gate: {request.InitiatingGateId})");

            LoadScene(destinationSceneName);

            // _transitionInProgress NAO e resetado aqui de proposito: o guard debounce dura ate a
            // cena destino terminar de carregar. O reset acontece em OnSceneLoadedClearGuard (todo
            // destino) e tambem em ClearTransitionGuard (Farm/Town, um pouco antes, no spawn).
            return SceneTransitionResult.Success;
        }

        private static void LoadScene(string sceneName)
        {
#if UNITY_EDITOR
            // In editor Play Mode, use EditorSceneManager to support
            // scenes not in Build Settings (same as ScenePortal).
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scenePath = $"Assets/_Game/Scenes/{sceneName}.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.LoadSceneInPlayMode(
                    scenePath,
                    new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Reset the transition guard. Called by PlayerSpawnResolver after
        /// spawn completes in the new scene. Guards against double-transition
        /// within the same play session.
        /// </summary>
        internal static void ClearTransitionGuard()
        {
            _transitionInProgress = false;
        }
    }
}
