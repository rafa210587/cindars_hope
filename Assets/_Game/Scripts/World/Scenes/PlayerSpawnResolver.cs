using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// MonoBehaviour that resolves the pending spawn ID from SceneTransitionState
    /// and repositions the player when a scene loads.
    ///
    /// Resolution order:
    ///   1. Check SceneTransitionState.PendingSpawnId.
    ///   2. Find SceneSpawnAnchor with matching _spawnAnchorId in scene.
    ///   3. Fallback: find SceneSpawnPoint with matching SpawnId (legacy).
    ///   4. Fallback: use _defaultSpawnAnchorId on this component.
    ///   5. Fallback: log warning and leave player at current position.
    ///
    /// Publishes SceneTransitionCompletedEvent after successful spawn.
    /// Clears SceneTransitionState.PendingSpawnId after resolution.
    /// Resets SceneTransitionRouter guard after spawn completes.
    ///
    /// This component should be placed on a persistent scene-root GameObject
    /// in each gameplay scene (FarmScene, TownScene, CaveScene).
    ///
    /// No GameObject.Find at runtime — uses FindObjectsByType which is
    /// the allowed fallback per no-runtime-global-search rule.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerSpawnResolver : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private string _defaultSpawnAnchorId;

        private void Start()
        {
            ResolveSpawn();
        }

        private void ResolveSpawn()
        {
            var spawnId = SceneTransitionState.PendingSpawnId;
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                spawnId = _defaultSpawnAnchorId;
            }

            if (_playerTransform == null)
            {
                Debug.LogWarning(
                    $"[PlayerSpawnResolver] '{name}' has no player transform assigned. " +
                    "Wire the player transform in the Unity Editor.",
                    this);
                SceneTransitionState.ClearPendingSpawn();
                SceneTransitionRouter.ClearTransitionGuard();
                return;
            }

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                Debug.LogWarning(
                    $"[PlayerSpawnResolver] No pending or default spawn ID. Player stays at current position.",
                    this);
                SceneTransitionState.ClearPendingSpawn();
                SceneTransitionRouter.ClearTransitionGuard();
                return;
            }

            // Priority 1: SceneSpawnAnchor (WAVE_INTEGRATION_13 type)
            if (TryResolveFromSceneSpawnAnchor(spawnId, out var anchorPosition))
            {
                ApplySpawn(spawnId, anchorPosition);
                return;
            }

            // Priority 2: Legacy SceneSpawnPoint
            if (TryResolveFromSceneSpawnPoint(spawnId, out var spawnPointPosition))
            {
                ApplySpawn(spawnId, spawnPointPosition);
                return;
            }

            Debug.LogWarning(
                $"[PlayerSpawnResolver] Could not find spawn anchor or spawn point with id '{spawnId}' " +
                $"in scene '{SceneManager.GetActiveScene().name}'. " +
                "Player stays at current position.",
                this);
            SceneTransitionState.ClearPendingSpawn();
            SceneTransitionRouter.ClearTransitionGuard();
        }

        private bool TryResolveFromSceneSpawnAnchor(string spawnId, out Vector2 position)
        {
            position = Vector2.zero;
            var anchors = FindObjectsByType<SceneSpawnAnchor>(FindObjectsInactive.Exclude);
            foreach (var anchor in anchors)
            {
                if (anchor.SpawnAnchorId == spawnId)
                {
                    position = anchor.Position;
                    return true;
                }
            }
            return false;
        }

        private bool TryResolveFromSceneSpawnPoint(string spawnId, out Vector2 position)
        {
            position = Vector2.zero;
            var spawnPoints = FindObjectsByType<SceneSpawnPoint>(FindObjectsInactive.Exclude);
            foreach (var sp in spawnPoints)
            {
                if (sp.SpawnId == spawnId)
                {
                    position = sp.Position;
                    return true;
                }
            }
            return false;
        }

        private void ApplySpawn(string spawnId, Vector2 position)
        {
            _playerTransform.position = position;

            var sceneName = SceneManager.GetActiveScene().name;
            GameEventBus.Publish(new SceneTransitionCompletedEvent(sceneName, spawnId, position));

            Debug.Log(
                $"[PlayerSpawnResolver] Spawned player at '{spawnId}' → {position} in scene '{sceneName}'.");

            SceneTransitionState.ClearPendingSpawn();
            SceneTransitionRouter.ClearTransitionGuard();
        }
    }
}
