using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CavePlayerPathConfinement : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField] private bool _enableConfinement = true;
        [SerializeField] private float _playerHalfWidth = 0.15f;
        [SerializeField] private float _playerHalfHeight = 0.25f;
        [SerializeField] private float _wallContactTolerance = 0.10f;

        private Vector3 _lastValidPosition;
        private float _lastLogTime;
        private const float LogRateLimitSeconds = 1f;

        private void Start()
        {
            if (_playerTransform == null)
            {
                Debug.LogWarning("CavePlayerPathConfinement: PlayerTransform not assigned.", this);
                enabled = false;
                return;
            }

            if (_levelController == null)
            {
                Debug.LogWarning("CavePlayerPathConfinement: LevelController not assigned.", this);
                enabled = false;
                return;
            }

            _lastValidPosition = _playerTransform.position;
            Debug.Log($"CavePlayerPathConfinement: enabled. Player={_playerTransform.name}, LevelController={_levelController.name}, halfWidth={_playerHalfWidth}, halfHeight={_playerHalfHeight}, tolerance={_wallContactTolerance}.", this);
        }

        private void LateUpdate()
        {
            if (!_enableConfinement || _playerTransform == null || _levelController == null)
            {
                return;
            }

            var generatedLevel = _levelController.CurrentGeneratedLevel;
            if (generatedLevel == null)
            {
                return;
            }

            var playerWorldPos = _playerTransform.position;

            if (IsWorldPositionAllowed(playerWorldPos, generatedLevel))
            {
                _lastValidPosition = playerWorldPos;
                return;
            }

            _playerTransform.position = _lastValidPosition;

            if (Time.time - _lastLogTime > LogRateLimitSeconds)
            {
                var playerGridPos = WorldToGridPosition(playerWorldPos, generatedLevel);
                Debug.Log($"CavePlayerPathConfinement: Confined player. Current={playerWorldPos}, LastValid={_lastValidPosition}, Grid={playerGridPos}, Reason=outside walkable samples.", this);
                _lastLogTime = Time.time;
            }
        }

        private bool IsWorldPositionAllowed(Vector3 worldPos, CaveGeneratedLevel level)
        {
            var xOffset = Mathf.Max(0f, _playerHalfWidth - _wallContactTolerance);
            var yOffset = Mathf.Max(0f, _playerHalfHeight - _wallContactTolerance);

            var samples = new[]
            {
                worldPos,
                worldPos + Vector3.left * xOffset,
                worldPos + Vector3.right * xOffset,
                worldPos + Vector3.up * yOffset,
                worldPos + Vector3.down * yOffset
            };

            foreach (var sample in samples)
            {
                var grid = WorldToGridPosition(sample, level);
                if (!IsGridWalkable(grid, level))
                {
                    return false;
                }
            }

            return true;
        }

        private Vector2Int WorldToGridPosition(Vector3 worldPos, CaveGeneratedLevel generatedLevel)
        {
            var offsetX = generatedLevel.Width * 0.5f;
            var offsetY = generatedLevel.Height * 0.5f;

            var gridX = Mathf.FloorToInt(worldPos.x + offsetX);
            var gridY = Mathf.FloorToInt(worldPos.y + offsetY);

            return new Vector2Int(gridX, gridY);
        }

        private bool IsGridWalkable(Vector2Int gridPos, CaveGeneratedLevel generatedLevel)
        {
            if (gridPos.x < 0 || gridPos.x >= generatedLevel.Width ||
                gridPos.y < 0 || gridPos.y >= generatedLevel.Height)
            {
                return false;
            }

            return generatedLevel.WalkableTiles.Contains(gridPos);
        }
    }
}
