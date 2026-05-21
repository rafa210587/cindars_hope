using System.Collections.Generic;
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
        [SerializeField] private float _horizontalHalfWidth = 0.005f;
        [SerializeField] private float _verticalHalfHeight = 0.08f;
        [SerializeField] private bool _useLateralSamples = false;
        [SerializeField] private bool _useVerticalSamples = true;
        [SerializeField] private bool _useDiagonalSamples = false;
        [SerializeField] private bool _logFailedSample = false;

        private Vector3 _lastValidPosition;
        private float _lastLogTime;
        private int _lastLevelHash;
        private const float LogRateLimitSeconds = 1f;

        public void ResetLastValidPosition(Vector3 position)
        {
            _lastValidPosition = position;
            Debug.Log($"CavePlayerPathConfinement: Reset last valid position to {position}.", this);
        }

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
            Debug.Log($"CavePlayerPathConfinement: enabled. Player={_playerTransform.name}, LevelController={_levelController.name}, horizontalHalfWidth={_horizontalHalfWidth}, verticalHalfHeight={_verticalHalfHeight}, useLateralSamples={_useLateralSamples}, useVerticalSamples={_useVerticalSamples}, useDiagonalSamples={_useDiagonalSamples}.", this);
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

            // Reset last valid position if level changed
            var currentLevelHash = generatedLevel.GetHashCode();
            if (currentLevelHash != _lastLevelHash)
            {
                var playerWorldPos = _playerTransform.position;
                if (IsWorldPositionAllowed(playerWorldPos, generatedLevel))
                {
                    ResetLastValidPosition(playerWorldPos);
                }
                else
                {
                    Debug.LogWarning($"CavePlayerPathConfinement: Player spawned in invalid position after level load. Current={playerWorldPos}.", this);
                }
                _lastLevelHash = currentLevelHash;
            }

            var playerWorldPos2 = _playerTransform.position;
            var resolved = ResolveConstrainedPosition(playerWorldPos2, generatedLevel);

            if (resolved != playerWorldPos2)
            {
                _playerTransform.position = resolved;

                if (Time.time - _lastLogTime > LogRateLimitSeconds)
                {
                    var playerGridPos = WorldToGridPosition(playerWorldPos2, generatedLevel);
                    Debug.Log($"CavePlayerPathConfinement: Confined player. Current={playerWorldPos2}, Resolved={resolved}, Grid={playerGridPos}.", this);
                    _lastLogTime = Time.time;
                }
            }
        }

        private Vector3 ResolveConstrainedPosition(Vector3 currentPosition, CaveGeneratedLevel level)
        {
            if (IsWorldPositionAllowed(currentPosition, level))
            {
                _lastValidPosition = currentPosition;
                return currentPosition;
            }

            var xRollback = new Vector3(_lastValidPosition.x, currentPosition.y, currentPosition.z);
            if (IsWorldPositionAllowed(xRollback, level))
            {
                _lastValidPosition = xRollback;
                return xRollback;
            }

            var yRollback = new Vector3(currentPosition.x, _lastValidPosition.y, currentPosition.z);
            if (IsWorldPositionAllowed(yRollback, level))
            {
                _lastValidPosition = yRollback;
                return yRollback;
            }

            return _lastValidPosition;
        }

        private bool IsWorldPositionAllowed(Vector3 worldPos, CaveGeneratedLevel level)
        {
            var samples = new List<Vector3> { worldPos };

            if (_useLateralSamples)
            {
                samples.Add(worldPos + Vector3.left * _horizontalHalfWidth);
                samples.Add(worldPos + Vector3.right * _horizontalHalfWidth);
            }

            if (_useVerticalSamples)
            {
                samples.Add(worldPos + Vector3.up * _verticalHalfHeight);
                samples.Add(worldPos + Vector3.down * _verticalHalfHeight);
            }

            if (_useDiagonalSamples)
            {
                samples.Add(worldPos + new Vector3(-_horizontalHalfWidth, _verticalHalfHeight, 0));
                samples.Add(worldPos + new Vector3(_horizontalHalfWidth, _verticalHalfHeight, 0));
                samples.Add(worldPos + new Vector3(-_horizontalHalfWidth, -_verticalHalfHeight, 0));
                samples.Add(worldPos + new Vector3(_horizontalHalfWidth, -_verticalHalfHeight, 0));
            }

            foreach (var sample in samples)
            {
                var grid = WorldToGridPosition(sample, level);
                if (!IsGridWalkable(grid, level))
                {
                    if (_logFailedSample)
                    {
                        Debug.Log($"CavePlayerPathConfinement: Failed sample at world={sample}, grid={grid}, horizontalHalfWidth={_horizontalHalfWidth}, verticalHalfHeight={_verticalHalfHeight}.", this);
                    }
                    return false;
                }
            }

            return true;
        }

        private Vector2Int WorldToGridPosition(Vector3 worldPos, CaveGeneratedLevel generatedLevel)
        {
            var offsetX = generatedLevel.Width * 0.5f;
            var offsetY = generatedLevel.Height * 0.5f;

            var gridX = Mathf.RoundToInt(worldPos.x + offsetX);
            var gridY = Mathf.RoundToInt(worldPos.y + offsetY);

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
