using UnityEngine;

namespace CindarsHope.World.Scenes
{
    /// <summary>
    /// MonoBehaviour that marks a spawn position in a scene.
    /// The stable spawnAnchorId must match exactly what gates and transition
    /// requests specify as TargetSpawnAnchorId.
    ///
    /// This coexists with the legacy SceneSpawnPoint from SceneManagement;
    /// both work with SceneTransitionState.PendingSpawnId.
    /// PlayerSpawnResolver checks SceneSpawnAnchor first, then falls back to
    /// the legacy SceneSpawnInstaller's registered points.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneSpawnAnchor : MonoBehaviour
    {
        [SerializeField] private string _spawnAnchorId;

        [Tooltip("Optional: facing direction when spawned here. 1 = right, -1 = left, 0 = inherit.")]
        [SerializeField] private float _facingDirection = 0f;

        public string SpawnAnchorId => _spawnAnchorId;
        public Vector2 Position => transform.position;
        public float FacingDirection => _facingDirection;

        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(_spawnAnchorId))
            {
                _spawnAnchorId = _spawnAnchorId.Trim();
            }

            _facingDirection = _facingDirection < -1f ? -1f : _facingDirection > 1f ? 1f : _facingDirection;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            if (!string.IsNullOrWhiteSpace(_spawnAnchorId))
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, _spawnAnchorId);
#endif
            }
        }
    }
}
