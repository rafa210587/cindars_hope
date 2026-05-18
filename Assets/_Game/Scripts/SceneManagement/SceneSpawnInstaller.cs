using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.SceneManagement
{
    [DisallowMultipleComponent]
    public sealed class SceneSpawnInstaller : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private SceneSpawnPoint[] _spawnPoints;
        [SerializeField] private string _defaultSpawnId;

        private void Start()
        {
            var spawnId = SceneTransitionState.PendingSpawnId;
            if (string.IsNullOrWhiteSpace(spawnId))
            {
                spawnId = _defaultSpawnId;
            }

            if (_playerTransform == null)
            {
                Debug.LogWarning($"{nameof(SceneSpawnInstaller)} on '{name}' has no player transform assigned.");
                SceneTransitionState.ClearPendingSpawn();
                return;
            }

            if (string.IsNullOrWhiteSpace(spawnId))
            {
                Debug.LogWarning($"{nameof(SceneSpawnInstaller)} on '{name}' has no pending or default spawn id.");
                SceneTransitionState.ClearPendingSpawn();
                return;
            }

            if (!TryGetSpawnPoint(spawnId, out var spawnPoint))
            {
                Debug.LogWarning($"{nameof(SceneSpawnInstaller)} on '{name}' could not find spawn id '{spawnId}'.");
                SceneTransitionState.ClearPendingSpawn();
                return;
            }

            _playerTransform.position = spawnPoint.Position;
            GameEventBus.Publish(new SceneTransitionCompletedEvent(
                SceneManager.GetActiveScene().name,
                spawnPoint.SpawnId,
                spawnPoint.Position));
            SceneTransitionState.ClearPendingSpawn();
        }

        private bool TryGetSpawnPoint(string spawnId, out SceneSpawnPoint spawnPoint)
        {
            spawnPoint = null;
            if (_spawnPoints == null)
            {
                return false;
            }

            for (var i = 0; i < _spawnPoints.Length; i++)
            {
                var candidate = _spawnPoints[i];
                if (candidate == null || candidate.SpawnId != spawnId)
                {
                    continue;
                }

                spawnPoint = candidate;
                return true;
            }

            return false;
        }
    }
}
