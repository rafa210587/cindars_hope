using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.SceneManagement;
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
    /// Does NOT do fading — FADE_LOADING_DEFERRED_WITH_REASON: no fade system.
    ///
    /// Transition guard: if a transition is already in progress this frame,
    /// subsequent calls are silently ignored.
    /// </summary>
    public static class SceneTransitionRouter
    {
        private static bool _transitionInProgress;

        /// <summary>
        /// Execute a scene transition from a SceneTransitionRequest.
        /// Returns a SceneTransitionResult indicating success or failure reason.
        /// </summary>
        public static SceneTransitionResult Execute(SceneTransitionRequest request)
        {
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

            // Note: _transitionInProgress is intentionally not reset here.
            // The scene will be destroyed/reloaded before any further calls
            // can happen in the new scene context.
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
