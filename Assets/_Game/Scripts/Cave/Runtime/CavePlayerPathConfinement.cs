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
            var playerGridPos = WorldToGridPosition(playerWorldPos, generatedLevel);

            if (IsPositionWalkable(playerGridPos, generatedLevel))
            {
                _lastValidPosition = playerWorldPos;
                return;
            }

            _playerTransform.position = _lastValidPosition;

            if (Time.time - _lastLogTime > LogRateLimitSeconds)
            {
                Debug.Log($"CavePlayerPathConfinement: Confined player to last valid position {_lastValidPosition}.", this);
                _lastLogTime = Time.time;
            }
        }

        private Vector2Int WorldToGridPosition(Vector3 worldPos, CaveGeneratedLevel generatedLevel)
        {
            var offsetX = generatedLevel.Width * 0.5f;
            var offsetY = generatedLevel.Height * 0.5f;

            var gridX = Mathf.RoundToInt(worldPos.x + offsetX);
            var gridY = Mathf.RoundToInt(worldPos.y + offsetY);

            return new Vector2Int(gridX, gridY);
        }

        private bool IsPositionWalkable(Vector2Int gridPos, CaveGeneratedLevel generatedLevel)
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
